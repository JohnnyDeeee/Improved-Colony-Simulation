using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Assets.Scripts {
    public class Manager : MonoBehaviour {
        private readonly List<CreatureBehaviour> _creatures = new List<CreatureBehaviour>();

        // Start is called before the first frame update
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            // Spawn creature
            for (var i = 0; i < 10; i++) {
                var creature = Instantiate(Resources.Load("Prefabs/creature") as GameObject)
                    .GetComponent<CreatureBehaviour>();
                creature.Mass = Random.Range(1f, 10f);
                Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
                creature.transform.position =
                    new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y));
                _creatures.Add(creature);
            }
        }

        // Update is called once per frame
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            // Let creature move
            foreach (var creature in _creatures) {
                var force = creature.SeekForce(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                creature.GetComponent<CreatureBehaviour>().ApplyForce(force);
            }
        }
    }
}