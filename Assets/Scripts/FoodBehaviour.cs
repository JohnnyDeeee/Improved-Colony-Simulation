﻿using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Assets.Scripts {
    public class FoodBehaviour : MonoBehaviour {
        // Start is called before the first frame update
        public void Start() { }

        // Update is called once per frame
        public void Update() { }

        public void OnCollisionEnter2D(Collision2D col) {
            if(col.collider.gameObject.GetComponent<CreatureBehaviour>() != null)
                Destroy(gameObject);
        }
    }
}