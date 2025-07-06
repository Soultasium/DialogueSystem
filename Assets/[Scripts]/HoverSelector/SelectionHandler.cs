using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulpace.Inputs
{
    public class SelectionHandler : MonoBehaviour
    {
        private const float ClickDelayLock = 0.1f;
            
        [SerializeField] private InputActionReference _clickAction;
        [SerializeField] private LayerMask _layerMask;
        
        private Camera _camera;
        private IHoverable _lastHoverable = null;
        private float _lastClickTime = float.MinValue;

        private void Start()
        {
            _camera = CameraHelper.CurrentCamera;
            _clickAction.action.performed += OnClicked;
            enabled = false;
        }

        private void Update()
        {
            UpdateHover();
        }

        private void UpdateHover()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
            {
                IHoverable.AllHoverables.TryGetValue(hit.colliderInstanceID, out var hoverable);
                
                if (hoverable != _lastHoverable && _lastHoverable != null)
                {
                    _lastHoverable.OnHoverExit();
                    _lastHoverable = null;
                }
                
                if (hoverable == _lastHoverable)
                    return;
                
                _lastHoverable = hoverable;
                hoverable.OnHoverEnter();
            }
            else
            {
                if (_lastHoverable == null)
                    return;
                
                _lastHoverable.OnHoverExit();
                _lastHoverable = null;
            }
        }

        private void OnClicked(InputAction.CallbackContext obj)
        {
            if (!enabled || _lastHoverable == null || _lastHoverable is not ISelectable selectable)
                return;

            if (Time.time - _lastClickTime < ClickDelayLock)
                return;

            _lastClickTime = Time.time;
            
            selectable.OnClicked();
        }
    }
}
