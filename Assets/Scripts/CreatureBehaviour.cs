using System;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class CreatureBehaviour : ExposableMonobehaviour {
        private Rigidbody2D _body;
        private LineRenderer _lineRenderer;
        private Vector2? _target = null;
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

            // DEBUG
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
        }

        // Update is called once per frame
        private void Update() {
            if(_target.HasValue)
                ApplyForce(_target.Value);

            // Display
            transform.localScale = new Vector3(Mass, Mass, 0) * 0.01f;
        }

        private void FixedUpdate() {
            RaycastHit2D? vision = Vision(); // Always register vision
            if ((Time.fixedTime) % 0.5f == 0) // Only act on vision every 500ms
                SetTarget(vision);

            // Physics Movement
            Velocity += Acceleration;
            Velocity = Vector2.ClampMagnitude(Velocity, MaxSpeed / Mass); // Max speed, impacted by mass because otherwise everyone would move at the same speed (when reaching maxSpeed)
            if (Velocity.magnitude == 0)
                Velocity = Vector2.ClampMagnitude(Velocity, MaxSpeed / 10); // Min speed (to prevent creatures from being unable to move)
            Position += Velocity;
            Acceleration *= 0; // Clear acceleration for the next frame (otherwise it will build up)

            _body.position = Position;

            // Rotation
            Vector2 newPosition = Position + Velocity;
            Rotation = (float)(Mathf.Atan2(Position.y - newPosition.y, Position.x - newPosition.x) + Math.PI / 2);

            _body.rotation = Rotation * Mathf.Rad2Deg;
        }

        private void SetTarget(RaycastHit2D? vision) {
            // Seek for food, or move to a random position
            RaycastHit2D? hit = vision;
            int radius = 10;
            if (hit.HasValue) {
                if (hit.Value.collider.GetComponent<FoodBehaviour>() == true)
                    _target = SeekForce(hit.Value.collider.transform.position);
                else
                    _target = SeekForce(Random.insideUnitCircle * radius); // TODO: Get random position in range of current position
            } else {
                _target = SeekForce(Random.insideUnitCircle * radius); // TODO: Get random position in range of current position
            }

            // DEBUG - Draw circle around creature
            //int segments = 8;
            //int index = 0;
            //_lineRenderer.startWidth = 0.01f;
            //_lineRenderer.endWidth = 0.01f;
            //_lineRenderer.positionCount = segments + 1;
            //for (int i = 0; i <= 360; i+= (360/segments)) {
            //    float x = Mathf.Sin(Mathf.Deg2Rad * i) * radius;
            //    float y = Mathf.Cos(Mathf.Deg2Rad * i) * radius;
            //    _lineRenderer.SetPosition(index, new Vector2(x, y));
            //    index++;
            //}
        }

        public void ApplyForce(Vector2 force) {
            force = force / Mass; // Size matters (bigger = slower, smaller = faster)
            Acceleration += force;

            // DEBUG
            Debug.DrawLine(Position, Position + (Velocity + Acceleration), Color.yellow);
        }

        public Vector2 SeekForce(Vector2 targetPosition) {
            Vector2 direction = (targetPosition - Position).normalized;
            direction *= MaxSpeed;
            Vector2 steering = (direction - Velocity).normalized;
            steering = Vector2.ClampMagnitude(steering, MaxForce);

            return steering;
        }

        public RaycastHit2D? Vision() {
            RaycastHit2D? hit = null;
            RaycastHit2D[] hits = Physics2D.RaycastAll(_body.position, transform.TransformDirection(Vector2.up), VisionLength);

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
                Debug.DrawRay(_body.position, transform.TransformDirection(Vector2.up) * VisionLength, Color.green);

            return hit;
        }
    }
}