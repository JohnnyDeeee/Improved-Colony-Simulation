using UnityEngine;

namespace Assets.Scripts {
    public class BorderBehaviour : MonoBehaviour {
        // Start is called before the first frame update
        private void Start() { }

        // Update is called once per frame
        private void Update() { }

        public void OnCollisionEnter2D(Collision2D col) {
            CreatureBehaviour creature = col.collider.gameObject.GetComponent<CreatureBehaviour>();
            if (creature != null)
                creature.Die();
        }
    }
}