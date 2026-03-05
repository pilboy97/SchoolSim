using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField] private float secToOpen = 0.5f;
        [TextArea] [SerializeField] private string message;

        private float _hoverTimer;
        private bool _isHovering;
        private bool _isDisplayed;
        private Vector2 _activationMousePos;

        private TooltipBox Box => TooltipBox.Instance;

        private void Update()
        {
            if (!_isHovering) return;

            // If the mouse moves significantly, reset the timer (optional "isStop" logic)
            if (Input.mousePositionDelta.sqrMagnitude > 0.1f && !_isDisplayed)
            {
                _hoverTimer = 0;
                return;
            }

            if (!_isDisplayed)
            {
                _hoverTimer += UnityEngine.Time.unscaledDeltaTime;
                if (_hoverTimer >= secToOpen)
                {
                    ShowTooltip();
                }
            }
        }

        private void ShowTooltip()
        {
            if (Box == null || !Box.TryOpen(this)) return;

            _isDisplayed = true;
            _activationMousePos = Input.mousePosition;
            Box.Text = message;
            
            PositionTooltip();
        }

        private void PositionTooltip()
        {
            RectTransform rect = Box.rectTransform;
            Vector2[] pivots = {
                new Vector2(0, 1), // Top Left
                new Vector2(1, 1),  // Top Right
                new Vector2(0, 0), // Bottom Left
                new Vector2(1, 0), // Bottom Right
            };

            foreach (var pivot in pivots)
            {
                rect.pivot = pivot;
                rect.position = _activationMousePos;

                // Check if this pivot keeps the box on screen
                if (IsInsideScreen(rect)) break;
            }
        }

        private bool IsInsideScreen(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            foreach (var corner in corners)
            {
                if (corner.x < 0 || corner.x > Screen.width || corner.y < 0 || corner.y > Screen.height)
                    return false;
            }
            return true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            _hoverTimer = 0;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            _isDisplayed = false;
            _hoverTimer = 0;
            TooltipBox.Instance.TryClose(this);
        }

        private void OnDisable()
        {
            _isHovering = false;
            _isDisplayed = false;
            if (TooltipBox.Instance != null) TooltipBox.Instance.TryClose(this);
        }
    }
}