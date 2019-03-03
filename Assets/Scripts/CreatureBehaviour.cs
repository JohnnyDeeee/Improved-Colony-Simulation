using System;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class CreatureBehaviour : ExposableMonobehaviour {
        private readonly Vector2? _target = null;
        private Rigidbody2D _body;
        private NeuralNetwork _brain;
        [ExposeProperty] [SerializeField] public float Mass { get; set; }
        [ExposeProperty] [SerializeField] public float MaxSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float MaxForce { get; set; }
        [ExposeProperty] [SerializeField] public float Rotation { get; set; }
        [ExposeProperty] [SerializeField] public float VisionLength { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Acceleration { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Velocity { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Position { get; set; }
        [ExposeProperty] [SerializeField] public bool Dead { get; set; }

        // Start is called before the first frame update
        private void Awake() {
            // Defaults
            Mass = 1f;
            MaxSpeed = 0.1f;
            MaxForce = 0.005f;
            VisionLength = 2f;
            _body = GetComponent<Rigidbody2D>();
            _brain = new NeuralNetwork(3, 3, 1);
            ApplyForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f))); // Random starting direction
        }

        private void Start() {
            Position = _body.position;
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
            if (vision.HasValue) {
                SpriteRenderer sprite = vision.Value.collider.GetComponent<SpriteRenderer>();

                double r = sprite ? sprite.color.r : 0;
                double g = sprite ? sprite.color.g : 0;
                double b = sprite ? sprite.color.b : 0;

                double[] result = _brain.FeedForward(new[] {r, g, b});

                // DEBUG
                //Debug.Log($"In[0-2]: {r}, {g}, {b}");
                //Debug.Log($"Out[0] : {result[0]}");

                // Choose to avoid or follow what we see
                if (result[0] > 0.5f)
                    ApplyForce(AvoidForce(vision.Value.collider.transform.position));
                else
                    ApplyForce(SeekForce(vision.Value.collider.transform.position));
            }

            // TODO: If we see nothing, go Wander()
            WanderForce();

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
            Vector2 ahead = (Vector2) transform.position + Velocity.normalized * VisionLength;
            Vector2 avoidanceForce = Vector2.ClampMagnitude((ahead - Velocity).normalized, MaxForce);

            return avoidanceForce;
        }

        public Vector2 WanderForce() {           
            Vector2 ahead = GetForwardPosition(2);

            // DEBUG - show ahead point
            DebugExtensions.DrawLine(1, _body.position, ahead, 0.15f, Color.red);
            DebugExtensions.DrawLine(2, _body.position, ahead + GetLeftPosition(2, false), 0.15f, Color.blue);
            DebugExtensions.DrawLine(3, _body.position, ahead + GetRightPosition(2, false), 0.15f, Color.blue);
            
            // TODO: Get a point in between the blue lines
            // TODO: ApplyForce towards that point
            // TODO: On each frame make a SMALL adjustment to this steering

            return Vector2.zero; // Return nothing for now
        }

        public RaycastHit2D? Vision() {
            RaycastHit2D? hit = null;
            RaycastHit2D[] hits = Physics2D.RaycastAll(_body.position, GetForwardDirection(), VisionLength);

            // Validate hits
            if (hits.Length > 0)
                foreach (RaycastHit2D _hit in hits) {
                    if (_hit.collider.gameObject == gameObject)
                        continue;

                    hit = _hit;
                    Debug.DrawLine(_body.position, _hit.collider.transform.position, Color.red); // DEBUG Target
                    break; // Only handle the first 'valid' hit
                }

            // DEBUG - Vision range
            if (!hit.HasValue)
                Debug.DrawRay(_body.position, GetForwardDirection() * VisionLength, Color.green);

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