using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class InfoIcons : UIBehaviour
    {
        public Action OnLateUpdate;

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
    }
}