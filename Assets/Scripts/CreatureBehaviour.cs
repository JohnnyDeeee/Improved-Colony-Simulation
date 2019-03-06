using System;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class CreatureBehaviour : ExposableMonobehaviour {
        private const int ChanceToFollowOrAvoidIndex = 0; // Output 0 of the brain
        private const int DirectionAdjustmentIndex = 1; // Output 1 of the brain
        private readonly Vector2? _target = null;
        private Rigidbody2D _body;
        private NeuralNetwork _brain;
        [SerializeField] private double[] _brainOutput;
        [ExposeProperty] [SerializeField] public float Mass { get; set; }
        [ExposeProperty] [SerializeField] public float MaxSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float MaxForce { get; set; }
        [ExposeProperty] [SerializeField] public float Rotation { get; set; }
        [ExposeProperty] [SerializeField] public float VisionLength { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Acceleration { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Velocity { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Position { get; set; }
        [ExposeProperty] [SerializeField] public bool Dead { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Target { get; set; }

        // Start is called before the first frame update
        private void Awake() {
            // Defaults
            Mass = 1f;
            MaxSpeed = 0.1f;
            MaxForce = 0.005f;
            VisionLength = 2f;
            _body = GetComponent<Rigidbody2D>();
            _brain = new NeuralNetwork(4, 4, 1);
        }

        private void Start() {
            Position = _body.position;
            Target = GetForwardPosition(VisionLength);
        }

        // Update is called once per frame
        private void Update() {
            if (Dead)
                enabled = false; // This disables the Update() and FixedUpdate() function

            if (_target.HasValue)
                ApplyForce(_target.Value);

            // Display
            transform.localScale = new Vector3(Mass, Mass, 0);
        }

        private void FixedUpdate() {
            RaycastHit2D? vision = Vision(); // Always register target

            if (vision.HasValue) { // If we see something, go follow/avoid
                SpriteRenderer sprite = vision.Value.collider.GetComponent<SpriteRenderer>();
                if (!sprite)
                    return;

                double r = sprite.color.r;
                double g = sprite.color.g;
                double b = sprite.color.b;

                _brainOutput = _brain.FeedForward(new[] {r, g, b, 1}); // Input [0-255 Red, 0-255 Green, 0-255 Blue, 0-1 Bool whether we see something or not]

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
            Vector2 ahead = GetForwardPosition(VisionLength);
            Vector2 avoidanceForce = Vector2.ClampMagnitude((ahead - avoidPosition).normalized, MaxForce);

            return avoidanceForce;
        }

        public Vector2 WanderForce() {
            // We can go to the "random" target that was found within vision()
            return SeekForce(Target);
        }

        public RaycastHit2D? Vision() {
            // Vision boundary
            Vector2 ahead = GetForwardPosition(VisionLength);
            Vector2 leftEdge = ahead + GetLeftPosition(VisionLength, false);
            Vector2 rightEdge = ahead + GetRightPosition(VisionLength, false);
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
            RaycastHit2D[] hits = Physics2D.RaycastAll(_body.position, Target - _body.position, VisionLength);

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