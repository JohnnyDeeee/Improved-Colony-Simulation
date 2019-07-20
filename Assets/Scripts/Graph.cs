using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Assets.Scripts {
    public class Graph : MonoBehaviour {
        public UILineRenderer LineRenderer;
        public List<Vector2> points = new List<Vector2>();

        public void AddPoints(Vector2[] points) {
            foreach (Vector2 point in points) {
                this.points.Add(point);
            }

            LineRenderer.Points = this.points.ToArray();
        }
    }
}