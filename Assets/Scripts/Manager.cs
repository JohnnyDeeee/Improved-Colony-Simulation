using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public static class Events {
        public static int CREATURE_DIES = 1;
    }

    public class Manager : ExposableMonobehaviour {
        private bool _isReady;
        private readonly List<GameObject> _parents = new List<GameObject>();
        private readonly List<CreatureBehaviour> _creatures = new List<CreatureBehaviour>();
        private int _deathCount;
        [SerializeField] [ExposeProperty] public int CreatureAmount { get; private set; }
        [SerializeField] [ExposeProperty] public int FoodAmount { get; private set; }
        [SerializeField] [ExposeProperty] public int TrapAmount { get; private set; }
        [SerializeField] [ExposeProperty] public int Height { get; private set; }       
        [SerializeField] [ExposeProperty] public int Width { get; private set; }
        [SerializeField] [ExposeProperty] public float GameTime { get; private set; }
        [SerializeField] [ExposeProperty] public int GameLoopInSeconds { get; private set; }
        [SerializeField] [ExposeProperty] public int GameLoopAmount { get; private set; }
        [SerializeField] [ExposeProperty] public Random.State Seed { get; private set; }

        private void Awake() {
            CreatureAmount = 100;
            FoodAmount = 400;
            TrapAmount = 300;
            Width = 800;
            Height = 600;
            GameLoopInSeconds = 30;
            GameLoopAmount = 1;
            Seed = Random.state; // Keep the seed the same for the whole game
            Time.timeScale = 1f;

            // Add origin point for graph
            FindObjectOfType<Graph>().AddPoints(new Vector2[1] { new Vector2(0,0) });
        }

        // Start is called before the first frame update
        private void Start() {
            SpawnWrapper();
        }

        // Update is called once per frame
        private void Update() {
            if (!_isReady)
                return;

            GameTime += Time.deltaTime;

            if ((int) GameTime >= GameLoopInSeconds || _deathCount == _creatures.Count) {
                GameLoopAmount += 1;
                GameTime = 0;
                _isReady = false;
                _deathCount = 0;

                // Genetic Algorithm()
                CreatureBehaviour[] parents = GeneticAlgorithm.GetParents(_creatures).ToArray();
                BitArray newGenome = GeneticAlgorithm.Crossover(parents[0].Genome.GetGenome(), parents[1].Genome.GetGenome());

                // Add fitness value to graph
                float pointScale = 1.0f;
                FindObjectOfType<Graph>().AddPoints(new Vector2[1] { new Vector2(GameLoopAmount * pointScale, parents[0].Fitness * pointScale) });

                Cleanup();
                SpawnWrapper(newGenome);
            }
        }

        private void SpawnWrapper(BitArray newGenome = null) {
            Random.state = Seed;

            SpawnBorders();
            SpawnFood();
            SpawnTraps();
            SpawnCreatures(newGenome);

            _isReady = true;
        }

        private void Cleanup() {
            foreach (GameObject parent in _parents) Destroy(parent);
            _parents.Clear();
            _creatures.Clear();
        }

        private void SpawnCreatures(BitArray newGenome) {
            GameObject creaturesParent = new GameObject("creatures");
            _parents.Add(creaturesParent);
            for (int i = 0; i < CreatureAmount; i++) {
                CreatureBehaviour creature = Instantiate(Resources.Load("Prefabs/creature") as GameObject).GetComponent<CreatureBehaviour>();

                if (newGenome != null) {
                    newGenome = GeneticAlgorithm.Mutation(newGenome);
                    creature.Genome.SetGenome(newGenome);
                } else {
                    creature.Genome.BaseMass = Random.Range(1f, 10f);
                    creature.Genome.VisionLength = 2f;
                }

                Vector2 screenBounds = new Vector2(Width, Height);
                creature.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y)));
                creature.transform.Rotate(0, 0, Random.Range(0, 360));

                creature.transform.SetParent(creaturesParent.transform, true);

                _creatures.Add(creature);
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

        public void SendEvent(int _event) {
            if (_event == Events.CREATURE_DIES)
                _deathCount += 1;
        }
    }
}