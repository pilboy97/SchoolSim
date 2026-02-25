using System;
using Game.Task;
using UnityEngine.EventSystems;
using Action = System.Action;

namespace Game.UI
{
    public class UIBackground : UIBehaviour, IPointerDownHandler, IDragHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            InputManager.Instance.OnClick(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var delta = eventData.delta;

            UIManager.Instance.State = UIManager.UIState.Move;
            var pos = GameManager.Instance.mainCamera.transform.position;
            var screenPos = GameManager.Instance.mainCamera.WorldToScreenPoint(pos);
            
            screenPos.x += delta.x;
            screenPos.y += delta.y;

            var nextPos = GameManager.Instance.mainCamera.ScreenToWorldPoint(screenPos);
            delta = -(nextPos - pos);

            pos.x += delta.x;
            pos.y += delta.y;

            GameManager.Instance.mainCamera.transform.position = pos;
        }
    }
}