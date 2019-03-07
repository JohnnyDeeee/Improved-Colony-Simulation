﻿using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class Manager : ExposableMonobehaviour {
        private bool _isReady;
        private readonly List<GameObject> _parents = new List<GameObject>();
        [SerializeField] [ExposeProperty] public int CreatureAmount { get; private set; }
        [SerializeField] [ExposeProperty] public int FoodAmount { get; private set; }
        [SerializeField] [ExposeProperty] public int Height { get; private set; }
        [SerializeField] [ExposeProperty] public int TrapAmount { get; private set; }
        [SerializeField] [ExposeProperty] public int Width { get; private set; }
        [SerializeField] [ExposeProperty] public float GameTime { get; private set; }
        [SerializeField] [ExposeProperty] public int GameLoopInSeconds { get; private set; }
        [SerializeField] [ExposeProperty] public int Seed { get; private set; }

        private void Awake() {
            CreatureAmount = 100;
            FoodAmount = 60;
            TrapAmount = 60;
            Width = 800;
            Height = 600;
            GameLoopInSeconds = 60;
            Seed = (int)DateTime.Now.Ticks; // Keep the seed the same for the whole game
        }

        // Start is called before the first frame update
        private void Start() {
            Random.InitState(Seed);

            SpawnBorders();
            SpawnFood();
            SpawnTraps();
            SpawnCreatures();

            _isReady = true;
        }

        // Update is called once per frame
        private void Update() {
            if (!_isReady)
                return;

            GameTime += Time.deltaTime;

            if ((int) GameTime == GameLoopInSeconds) {
                GameTime = 0;
                _isReady = false;
                Cleanup();
                // TODO: Genetic Algorithm()
                // TODO: Start() with new genomes
                Start();
            }
        }

        private void Cleanup() {
            foreach (GameObject parent in _parents) Destroy(parent);
        }

        private void SpawnCreatures() {
            GameObject creaturesParent = new GameObject("creatures");
            _parents.Add(creaturesParent);
            for (int i = 0; i < CreatureAmount; i++) {
                CreatureBehaviour creature = Instantiate(Resources.Load("Prefabs/creature") as GameObject).GetComponent<CreatureBehaviour>();
                creature.Mass = Random.Range(1f, 10f);

                Vector2 screenBounds = new Vector2(Width, Height);
                creature.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y)));

                creature.transform.SetParent(creaturesParent.transform, true);
            }
        }

        private void SpawnFood() {
            GameObject foodParent = new GameObject("food");
            _parents.Add(foodParent);
            for (int i = 0; i < FoodAmount; i++) {
                FoodBehaviour food = Instantiate(Resources.Load("Prefabs/food") as GameObject).GetComponent<FoodBehaviour>();

                Vector2 screenBounds = new Vector2(Width, Height);
                food.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y)));

                food.transform.SetParent(foodParent.transform, true);
            }
        }

        // DEBUG - Spawn border (as trap) to see how creatures react on different colors
        private void SpawnTraps() {
            GameObject trapParent = new GameObject("traps");
            _parents.Add(trapParent);
            for (int i = 0; i < TrapAmount; i++) {
                BorderBehaviour trap = Instantiate(Resources.Load("Prefabs/border") as GameObject).GetComponent<BorderBehaviour>();

                Vector2 screenBounds = new Vector2(Width, Height);
                trap.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y)));

                trap.GetComponent<SpriteRenderer>().color = Color.red; // Make it red

                trap.transform.SetParent(trapParent.transform, true);
            }
        }

        private void SpawnBorders() {
            GameObject bordersParent = new GameObject("borders");
            _parents.Add(bordersParent);
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
    }
}