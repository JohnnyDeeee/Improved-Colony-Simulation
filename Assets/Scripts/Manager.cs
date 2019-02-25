using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public class Manager : MonoBehaviour {
        private readonly List<CreatureBehaviour> _creatures = new List<CreatureBehaviour>();

        // Start is called before the first frame update
        private void Start() {
            // Spawn creatures
            for (var i = 0; i < 100; i++) {
                var creature = Instantiate(Resources.Load("Prefabs/creature") as GameObject)
                    .GetComponent<CreatureBehaviour>();
                creature.Mass = Random.Range(1f, 10f);
                Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
                creature.GetComponent<Rigidbody2D>().position =
                    new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y));
                _creatures.Add(creature);
            }

            // Spawn food
            for (var i = 0; i < 60; i++) {
                var food = Instantiate(Resources.Load("Prefabs/food") as GameObject)
                    .GetComponent<FoodBehaviour>();
                Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
                food.transform.position =
                    new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y));
            }
        }

        // Update is called once per frame
        private void Update() {
            // Let creatures move
            foreach (var creature in _creatures) {
                var force = creature.SeekForce(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                creature.ApplyForce(force);

                // DEBUG
                creature.Vision();
            }
        }
    }
}