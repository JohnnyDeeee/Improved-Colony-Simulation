using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
    private List<CreatureBehaviour> creatures = new List<CreatureBehaviour>();

    // Start is called before the first frame update
    void Start() {
        // Spawn creature
        for (int i = 0; i < 10; i++) {
            CreatureBehaviour creature = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/creature") as GameObject).GetComponent<CreatureBehaviour>();
            creature.mass = Random.Range(1f, 10f);
            Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            creature.transform.position = new Vector2(Random.Range(0f, screenBounds.x), Random.Range(0f, screenBounds.y));
            this.creatures.Add(creature);
        }
    }

    // Update is called once per frame
    void Update() {
        // Let creature move
        foreach (CreatureBehaviour creature in this.creatures) {
            Vector2 force = creature.SeekForce(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            creature.GetComponent<CreatureBehaviour>().ApplyForce(force);
        }
    }
}
