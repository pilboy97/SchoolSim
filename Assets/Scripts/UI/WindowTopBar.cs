using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class WindowTopBar : UIBehaviour, IDragHandler
    {
        [SerializeField] private Window window;
        [SerializeField] private Vector3 beginPoint;

        public void OnDrag(PointerEventData eventData)
        {
            window.Move(eventData.delta);
        }
    }
}