using System.Diagnostics.CodeAnalysis;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts {
    public class CreatureBehaviour : ExposableMonobehaviour {
        [ExposeProperty] [SerializeField] public float Mass { get; set; }
        [ExposeProperty] [SerializeField] public float MaxSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float MaxForce { get; set; }
        [ExposeProperty] [SerializeField] public Vector2 Acceleration { get; private set; }
        [ExposeProperty] [SerializeField] public Vector2 Velocity { get; private set; }
        [ExposeProperty] [SerializeField] public Vector2 Position { get; private set; }

        // Start is called before the first frame update
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            // Defaults
            Mass = 0.1f;
            MaxSpeed = 0.01f;
            MaxForce = 0.0005f;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            Position = transform.position;
        }

        // Update is called once per frame
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            // Physics
            Velocity += Acceleration;
            Velocity =
                Vector2.ClampMagnitude(Velocity,
                    MaxSpeed /
                    Mass); // Max speed, impacted by mass because otherwise everyone would move at the same speed (when reaching maxSpeed)
            if (Velocity.magnitude == 0)
                Velocity =
                    Vector2.ClampMagnitude(Velocity,
                        MaxSpeed / 10); // Min speed (to prevent creatures from being unable to move)
            Position += Velocity;
            Acceleration *= 0; // Clear acceleration for the next frame (otherwise it will build up)

            transform.position = Position;

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
    }
}