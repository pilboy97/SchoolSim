using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class MainCanvas : UIBehaviour
    {
        private Canvas _canvas;

        protected override void Awake()
        {
            base.Awake();

            _canvas = GetComponent<Canvas>();
        }
    }
}