using System;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts {
    public class CreatureBehaviour : ExposableMonobehaviour {
        private Rigidbody2D _body;
        [ExposeProperty] [SerializeField] public float Mass { get; set; }
        [ExposeProperty] [SerializeField] public float MaxSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float MaxForce { get; set; }
        [ExposeProperty] [SerializeField] public float Rotation { get; private set; }
        [ExposeProperty] [SerializeField] public float VisionLength { get; private set; }
        [ExposeProperty] [SerializeField] public Vector2 Acceleration { get; private set; }
        [ExposeProperty] [SerializeField] public Vector2 Velocity { get; private set; }
        [ExposeProperty] [SerializeField] public Vector2 Position { get; private set; }

        // Start is called before the first frame update
        private void Awake() {
            // Defaults
            Mass = 0.1f;
            MaxSpeed = 0.01f;
            MaxForce = 0.0005f;
            VisionLength = 0.2f;
            _body = GetComponent<Rigidbody2D>();
        }

        private void Start() {
            Position = _body.position;
        }

        // Update is called once per frame
        private void Update() {
            // Physics Movement
            Velocity += Acceleration;
            Velocity = Vector2.ClampMagnitude(Velocity, MaxSpeed / Mass); // Max speed, impacted by mass because otherwise everyone would move at the same speed (when reaching maxSpeed)
            if (Velocity.magnitude == 0)
                Velocity = Vector2.ClampMagnitude(Velocity, MaxSpeed / 10); // Min speed (to prevent creatures from being unable to move)
            Position += Velocity;
            Acceleration *= 0; // Clear acceleration for the next frame (otherwise it will build up)

            _body.position = Position;

            // Rotation
            var newPosition = Position + Velocity;
            Rotation = (float) (Mathf.Atan2(Position.y - newPosition.y, Position.x - newPosition.x) + Math.PI / 2);

            _body.rotation = Rotation * Mathf.Rad2Deg;

            // Display
            transform.localScale = new Vector3(Mass, Mass, 0) * 0.01f;
        }

        public void ApplyForce(Vector2 force) {
            force = force / Mass; // Size matters (bigger = slower, smaller = faster)
            Acceleration += force;
        }

        public Vector2 SeekForce(Vector2 targetPosition) {
            var direction = (targetPosition - Position).normalized;
            direction *= MaxSpeed;
            var steering = (direction - Velocity).normalized;
            steering = Vector2.ClampMagnitude(steering, MaxForce);

            return steering;
        }

        public RaycastHit2D? Vision() {
            RaycastHit2D? hit = null;
            var hits = Physics2D.RaycastAll(_body.position, transform.TransformDirection(Vector2.up), VisionLength);

            // Validate hits
            if (hits.Length > 0)
                foreach (var _hit in hits) {
                    if (_hit.collider.gameObject == gameObject)
                        continue;

                    hit = _hit;
                    Debug.DrawLine(_body.position, _hit.collider.transform.position, Color.red); // DEBUG Target
                    break; // Only handle the first 'valid' hit
                }

            if(hit == null)
                Debug.DrawRay(_body.position, transform.TransformDirection(Vector2.up) * VisionLength, Color.green); // DEBUG Vision range

            return hit;
        }
    }
}