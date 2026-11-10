using UnityEngine;

namespace CottageMagic
{
    public sealed class PixelCameraRig : MonoBehaviour
    {
        private const int SourcePixelsHigh = 180;
        private const float PixelsPerUnit = 16f;
        private int _lastHeight;

        private void OnEnable() { UpdatePixelScale(); }

        private void LateUpdate()
        {
            if (_lastHeight != Screen.height) UpdatePixelScale();
        }

        private void UpdatePixelScale()
        {
            var camera = GetComponent<Camera>();
            if (camera == null || Screen.height <= 0) return;
            _lastHeight = Screen.height;
            int integerScale = Mathf.Max(1, Screen.height / SourcePixelsHigh);
            camera.orthographicSize = Screen.height / (2f * PixelsPerUnit * integerScale);
        }
    }
}
