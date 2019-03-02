using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts {
    public class CameraBehaviour : ExposableMonobehaviour {
        private Camera _camera;

        [ExposeProperty] [SerializeField] public float DragSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float ZoomSpeed { get; set; }
        [ExposeProperty] [SerializeField] public float MinZoom { get; set; }
        [ExposeProperty] [SerializeField] public float MaxZoom { get; set; }
        [ExposeProperty] [SerializeField] public float CurrentZoom { get; set; }

        public void Start() {
            _camera = GetComponent<Camera>();
            MinZoom = 0.1f;
            MaxZoom = 200f;
            CurrentZoom = _camera.orthographicSize;
        }

        public void Update() {
            // Dragging right mouse button
            if (Input.GetMouseButton(1))
                transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * DragSpeed * -1,
                    Input.GetAxisRaw("Mouse Y") * Time.deltaTime * DragSpeed * -1, 0f);

            // Scroll forward
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && CurrentZoom > MinZoom)
                ZoomToPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), ZoomSpeed);

            // Scroll back
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && CurrentZoom < MaxZoom)
                ZoomToPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), ZoomSpeed * -1f);

            DragSpeed = CurrentZoom / 0.2f;
            ZoomSpeed = CurrentZoom * 0.1f;

            transform.position = new Vector3(transform.position.x, transform.position.y, -10f); // Lock z-axis
        }

        public void ZoomToPosition(Vector3 zoomTowards, float amount = 0f) {
            // Calculate how much we will have to move towards the zoomTowards position
            var multiplier = 1.0f / _camera.orthographicSize * amount;

            // Move camera
            transform.position += (zoomTowards - transform.position) * multiplier;

            var newZoom = _camera.orthographicSize - amount;
            if (amount == 0f) // Zoom all the way in
                newZoom = MinZoom;

            // Zoom camera
            CurrentZoom = Mathf.Clamp(newZoom, MinZoom, MaxZoom);
            _camera.orthographicSize = CurrentZoom;
        }
    }
}