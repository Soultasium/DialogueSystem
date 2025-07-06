using UnityEngine;

namespace Soulpace
{
    public class CameraHelper : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        
        public static Camera CurrentCamera 
        {
            get
            {
                if (_currentCamera == null)
                {
                    _currentCamera = FindFirstObjectByType<Camera>();
                }

                return _currentCamera;
            }
            private set => _currentCamera = value;
        }

        public static Transform CurrentCameraTransform
        {
            get
            {
                if (_currentCameraTransform == null && CurrentCamera != null)
                {
                    _currentCameraTransform = CurrentCamera.transform;
                }
                
                return _currentCameraTransform;
            }
            set => _currentCameraTransform = value;
        }

        private static Camera _currentCamera;
        private static Transform _currentCameraTransform;
        
        private void OnEnable()
        {
            CurrentCamera = _camera;
            CurrentCameraTransform = _camera.transform;
        }

        private void OnDisable()
        {
            if (CurrentCamera == _camera)
            {
                CurrentCamera = null;
            }

            if (CurrentCameraTransform == _camera.transform)
            {
                CurrentCameraTransform = null;
            }
        }
    }
}
