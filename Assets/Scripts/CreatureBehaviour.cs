using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : ExposableMonobehaviour {
    [ExposeProperty, SerializeField] public float mass { get; set; }
    [ExposeProperty, SerializeField] public float maxSpeed { get; set; }
    [ExposeProperty, SerializeField] public float maxForce { get; set; }
    [ExposeProperty, SerializeField] public Vector2 acceleration { get; private set; }
    [ExposeProperty, SerializeField] public Vector2 velocity { get; private set; }
    [ExposeProperty, SerializeField] public Vector2 position { get; private set; }

    // Start is called before the first frame update
    void Awake() {
        // Defaults
        this.mass = 1f;
        this.maxSpeed = 0.01f;
        this.maxForce = 0.0005f;
    }

    void Start() {
        this.position = this.transform.position;
    }

    // Update is called once per frame
    void Update() {
        // Physics
        this.velocity += this.acceleration;
        this.velocity = Vector2.ClampMagnitude(this.velocity, this.maxSpeed); // Max speed
        if (this.velocity.magnitude == 0)
            this.velocity = Vector2.ClampMagnitude(this.velocity, this.maxSpeed / 10); // Min speed (to prevent creatures from being unable to move)
        this.position += this.velocity;
        this.acceleration *= 0; // Clear acceleration for the next frame (otherwise it will build up)

        this.transform.position = this.position;

        // Display
        this.transform.localScale = new Vector3(this.mass, this.mass, 0) * 0.1f;
    }

    public void ApplyForce(Vector2 force) {
        Vector2 _force = force / this.mass; // Size matters (bigger = slower, smaller = faster)
        this.acceleration += _force;
    }

    public Vector2 SeekForce(Vector2 targetPosition) {
        Vector2 direction = (targetPosition - this.position).normalized;
        direction *= this.maxSpeed;
        Vector2 steering = (direction - this.velocity).normalized;
        steering = Vector2.ClampMagnitude(steering, this.maxForce);

        return steering;
    }
}
