﻿using UnityEngine;

namespace Assets.Scripts {
    public class Manager : MonoBehaviour {
        [SerializeField] public int CreatureAmount = 100;
        [SerializeField] public int FoodAmount = 60;
        [SerializeField] public int Height = 600;
        [SerializeField] public int Width = 800;

        // Start is called before the first frame update
        private void Start() {
            // Spawn creatures
            GameObject createParent = new GameObject("creatures");
            for (int i = 0; i < CreatureAmount; i++) {
                CreatureBehaviour creature = Instantiate(Resources.Load("Prefabs/creature") as GameObject).GetComponent<CreatureBehaviour>();
                creature.Mass = Random.Range(1f, 10f);

                Vector2 screenBounds = new Vector2(Width, Height);
                creature.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y)));

                creature.transform.SetParent(createParent.transform, true);
            }

            // Spawn food
            GameObject foodParent = new GameObject("food");
            for (int i = 0; i < FoodAmount; i++) {
                FoodBehaviour food = Instantiate(Resources.Load("Prefabs/food") as GameObject).GetComponent<FoodBehaviour>();

                Vector2 screenBounds = new Vector2(Width, Height);
                food.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y)));

                food.transform.SetParent(foodParent.transform, true);
            }

            // Create borders
            GameObject bordersParent = new GameObject("borders");
            const float borderOffset = 5f / 2f;
            for (float x = 0; x <= Width; x += borderOffset) {
                for (float y = 0; y <= Height; y += borderOffset) {
                    if (x > 0 && x < Width - borderOffset && y > 0 && y < Height - borderOffset)
                        continue;

                    BorderBehaviour border = Instantiate(Resources.Load("Prefabs/border") as GameObject).GetComponent<BorderBehaviour>();
                    border.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(x, y));

                    border.transform.SetParent(bordersParent.transform, true);
                }
            }
        }

        // Update is called once per frame
        private void Update() { }
    }
}