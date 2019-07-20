using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils {
    public static class DebugExtensions {
        private static readonly List<LineRenderer> LineRenderers = new List<LineRenderer>();

        public static void DrawLine(int id, Vector2 start, Vector2 end, float width, Color color) {
            LineRenderer lineRenderer;

            if (LineRenderers.Count >= id)
                lineRenderer = LineRenderers[id-1];
            else {
                Manager manager = GameObject.FindObjectOfType<Manager>();
                GameObject child = new GameObject("LineRenderer");
                child.transform.SetParent(manager.transform);
                lineRenderer = child.gameObject.AddComponent<LineRenderer>();
                LineRenderers.Add(lineRenderer);
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
}
