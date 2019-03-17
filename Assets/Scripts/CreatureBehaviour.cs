using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class Genome {
        [ExposeProperty] [SerializeField] public float BaseMass { get; set; }
        [ExposeProperty] [SerializeField] public float VisionLength { get; set; }
        [ExposeProperty] [SerializeField] public NeuralNetwork Brain { get; set; }

        public BitArray GetGenome() {
            byte[] baseMassBytes = BitConverter.GetBytes(BaseMass);
            byte[] visionLengthBytes = BitConverter.GetBytes(VisionLength);
            byte[] brainInputBytes = Brain.GetInputWeights().SelectMany(x => BitConverter.GetBytes(x)).ToArray();

            List<byte> bytes = new List<byte>(baseMassBytes.Concat(visionLengthBytes).Concat(brainInputBytes));

            return new BitArray(bytes.ToArray());
        }

        public void SetGenome(BitArray genome) {
            // Bits to bytes
            byte[] bytes = new byte[genome.Length / 8];
            genome.CopyTo(bytes, 0);
            List<byte> byteList = bytes.ToList();

            // BaseMass - float (4 bytes)
            byte[] baseMassBytes = byteList.GetRange(0, 4).ToArray();
            byteList.RemoveRange(0, 4);
            BaseMass = BitConverter.ToSingle(baseMassBytes, 0);
            BaseMass = Mathf.Clamp(BaseMass, 1f, 50f);

            // VisionLength - float (4 bytes)
            byte[] visionLengthBytes = byteList.GetRange(0, 4).ToArray();
            byteList.RemoveRange(0, 4);
            VisionLength = BitConverter.ToSingle(visionLengthBytes, 0);
            VisionLength = Mathf.Clamp(VisionLength, 0.1f, 25f);

            // Brain Inputs - double[] (8 bytes per double)
            byte[] brainBytes = byteList.GetRange(0, Brain.GetInputWeights().Length * 8).ToArray();
            byteList.RemoveRange(0, Brain.GetInputWeights().Length * 8);
            double[] inputWeights = new double[brainBytes.Length / 8];
            for (int i = 0; i < inputWeights.Length; i++) {
                inputWeights[i] = BitConverter.ToDouble(brainBytes, i * 8);
            }
            Brain = new NeuralNetwork(Brain.GetInputLayerSize(), Brain.GetHiddenLayerSize(), Brain.GetOutputLayerSize(), inputWeights);
        }

        public override string ToString() {
            return $"BaseMass: {BaseMass}, VisionLength: {VisionLength}, BrainInputs: {Brain.ToString()}";
        }
    }

    public class CreatureBehaviour : ExposableMonobehaviour {
        private const int ChanceToFollowOrAvoidIndex = 0; // Output 0 of the brain // TODO: Put the output in a sep class
        private readonly Vector2? _target = null;
        private Rigidbody2D _body;
        private Manager _manager;
        [SerializeField] private double[] _brainOutput;
        [SerializeField] public readonly Genome Genome = new Genome();
        [ExposeProperty] [SerializeField] public float Mass => Genome.BaseMass + Food;

        [ExposeProperty] [SerializeField] public float MassDebug {
            get { return Mass; }
            set { }
        }

        [ExposeProperty] [SerializeField] public float Rotation { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Acceleration { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Velocity { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Position { get; set; }
        [ExposeProperty] [SerializeField] public bool Dead { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Target { get; set; }
        [ExposeProperty] [SerializeField] public int Age { get; private set; }
        [ExposeProperty] [SerializeField] public float Food { get; private set; }
        [ExposeProperty] [SerializeField] public float FoodDepletionMultiplier { get; private set; }
        [ExposeProperty] [SerializeField] public float MaxSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float MaxForce { get; set; }
        [ExposeProperty] [SerializeField] public float Fitness => Age;
        [ExposeProperty] [SerializeField] public float NormalizedFitness { get; set; }
        [ExposeProperty] [SerializeField] public float AccumulatedNormalizedFitness { get; set; }

        // Start is called before the first frame update
        private void Awake() {
            // Defaults
            Age = 1;
            FoodDepletionMultiplier = 0.2f;
            _body = GetComponent<Rigidbody2D>();
            MaxSpeed = 0.3f;
            MaxForce = 0.01f;
            Genome.Brain = new NeuralNetwork(5, 5, 1);
        }

        private void Start() {
            Position = _body.position;
            transform.position = _body.position;

            Rotation = _body.rotation;
            transform.Rotate(0f, 0f, _body.rotation);

            transform.localScale = new Vector3(Mass, Mass, 0);

            Target = GetForwardPosition(Genome.VisionLength);
            _manager = FindObjectOfType<Manager>();
            Food = Genome.BaseMass;
        }

        // Update is called once per frame
        private void Update() {
            if (Dead) {
                enabled = false; // This disables the Update() and FixedUpdate() function
                return;
            }

            if (_target.HasValue)
                ApplyForce(_target.Value);

            Age = Mathf.FloorToInt(_manager.GameTime);

            if (Food <= 0)
                Die();

            // Display
            transform.localScale = new Vector3(Mass, Mass, 0);
        }

        private void FixedUpdate() {
            if (Dead) {
                enabled = false; // This disables the Update() and FixedUpdate() function
                return;
            }

            RaycastHit2D? vision = Vision(); // Always register target

            if (vision.HasValue) { // If we see something, go follow/avoid
                SpriteRenderer sprite = vision.Value.collider.GetComponent<SpriteRenderer>();
                if (!sprite)
                    return;

                double r = sprite.color.r;
                double g = sprite.color.g;
                double b = sprite.color.b;

                _brainOutput = Genome.Brain.FeedForward(new[] {r, g, b, 1, Mass}); // Input [0-255 Red, 0-255 Green, 0-255 Blue, 0-1 Bool whether we see something or not, ~ Mass amount]

                // Choose to avoid or follow what we see
                if (_brainOutput[ChanceToFollowOrAvoidIndex] > 0.5f)
                    ApplyForce(AvoidForce(vision.Value.collider.transform.position));
                else
                    ApplyForce(SeekForce(vision.Value.collider.transform.position));

                // DEBUG - Show if creature is avoiding or following
                GetComponent<SpriteRenderer>().color = _brainOutput[ChanceToFollowOrAvoidIndex] > 0.5f ? Color.red : Color.green;
            } else { // If we see nothing, go wander
                ApplyForce(WanderForce());

                // DEBUG - Show that creature is wandering
                GetComponent<SpriteRenderer>().color = Color.white;
            }

            // Physics Movement
            Velocity += Acceleration;
            Velocity = Vector2.ClampMagnitude(Velocity, MaxSpeed / Mass); // Max speed, impacted by mass because otherwise everyone would move at the same speed (when reaching maxSpeed)
            if (Velocity.magnitude == 0)
                Velocity = Vector2.ClampMagnitude(Velocity, 0.01f); // Min speed (to prevent creatures from being unable to move)
            Position += Velocity;
            Acceleration *= 0; // Clear acceleration for the next frame (otherwise it will build up)

            _body.position = Position;

            // Rotation
            Vector2 newPosition = Position + Velocity;
            Rotation = (float) (Mathf.Atan2(Position.y - newPosition.y, Position.x - newPosition.x) + Math.PI / 2);

            _body.rotation = Rotation * Mathf.Rad2Deg;

            // Food depletion
            float distanceTraveled = Vector2.Distance(_body.position, Position - Velocity);
            Food -= distanceTraveled * FoodDepletionMultiplier;
        }

        public void ApplyForce(Vector2 force) {
            force = force / Mass; // Size matters (bigger = slower, smaller = faster)
            Acceleration += force;
        }

        public Vector2 SeekForce(Vector2 targetPosition) {
            Vector2 direction = (targetPosition - Position).normalized;
            direction *= MaxSpeed;
            Vector2 steering = (direction - Velocity).normalized;
            steering = Vector2.ClampMagnitude(steering, MaxForce);

            return steering;
        }

        public Vector2 AvoidForce(Vector2 avoidPosition) {
            Vector2 ahead = GetForwardPosition(Genome.VisionLength);
            Vector2 avoidanceForce = Vector2.ClampMagnitude((ahead - avoidPosition).normalized, MaxForce);

            return avoidanceForce;
        }

        public Vector2 WanderForce() {
            // We can go to the "random" target that was found within vision()
            return SeekForce(Target);
        }

        public RaycastHit2D? Vision() {
            // Vision boundary
            Vector2 ahead = GetForwardPosition(Genome.VisionLength);
            Vector2 leftEdge = ahead + GetLeftPosition(Genome.VisionLength, false);
            Vector2 rightEdge = ahead + GetRightPosition(Genome.VisionLength, false);
            Vector2 rightVisionEdge = (leftEdge - rightEdge) / 3 + rightEdge;
            Vector2 leftVisionEdge = (rightEdge - leftEdge) / 3 + leftEdge;

            // Get a point in between the vision edges
            float distanceBetweenLeftAndTarget = Vector2.Distance(leftVisionEdge, Target);
            distanceBetweenLeftAndTarget = Mathf.Clamp01(distanceBetweenLeftAndTarget); // 0f is all the way left, 1f is all the way right
            distanceBetweenLeftAndTarget -= Random.Range(-1.0f, 1.0f); // Go in random directions
            distanceBetweenLeftAndTarget = Mathf.Clamp01(distanceBetweenLeftAndTarget);

            // Get the actual point
            Target = (1f - distanceBetweenLeftAndTarget) * leftVisionEdge + distanceBetweenLeftAndTarget * rightVisionEdge;

            // DEBUG
            //DebugExtensions.DrawLine(1, _body.position, Target, 0.15f, Color.white);

            // Check if there is something at that point
            RaycastHit2D? hit = null;
            RaycastHit2D[] hits = Physics2D.RaycastAll(_body.position, Target - _body.position, Genome.VisionLength);

            // If there is, go validate that hit
            if (hits.Length > 0)
                foreach (RaycastHit2D _hit in hits) {
                    if (_hit.collider.gameObject == gameObject) // Ignore our own collider
                        continue;

                    hit = _hit;
                    break; // Only handle the first 'valid' hit
                }

            return hit;
        }

        public void Die() {
            Dead = true;
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0.2f);
        }

        public void AddFood(int amount) {
            Food += amount;
        }

        private Vector2 GetForwardDirection() {
            return transform.TransformDirection(Vector2.up);
        }

        private Vector2 GetLeftDirection() {
            return transform.TransformDirection(Vector2.left);
        }

        private Vector2 GetRightDirection() {
            return transform.TransformDirection(Vector2.right);
        }

        private Vector2 GetForwardPosition(float distance, bool fromBodyOrigin = true) {
            if (fromBodyOrigin)
                return _body.position + GetForwardDirection() * distance;
            return GetForwardDirection() * distance;
        }

        private Vector2 GetLeftPosition(float distance, bool fromBodyOrigin = true) {
            if (fromBodyOrigin)
                return _body.position + GetLeftDirection() * distance;
            return GetLeftDirection() * distance;
        }

        private Vector2 GetRightPosition(float distance, bool fromBodyOrigin = true) {
            if (fromBodyOrigin)
                return _body.position + GetRightDirection() * distance;
            return GetRightDirection() * distance;
        }
    }
}