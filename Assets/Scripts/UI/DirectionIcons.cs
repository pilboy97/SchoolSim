using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DirectionIcons : MonoBehaviour
    {
        public enum Status
        {
            None,
            Increase,
            Decrease,
        }

        private RectTransform _rectTransform;
        
        [SerializeField] private Image[] children;

        public Status status;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            switch (status)
            {
                case Status.None:
                    foreach (var obj in children)
                    {
                        obj.enabled = false;
                    }

                    break;
                case Status.Increase:
                    foreach (var obj in children)
                    {
                        obj.enabled = true;
                        obj.rectTransform.localRotation = Quaternion.AngleAxis(-90, Vector3.forward);
                    }

                    break;
                case Status.Decrease:
                    foreach (var obj in children)
                    {
                        obj.enabled = true;
                        obj.rectTransform.localRotation = Quaternion.AngleAxis(90, Vector3.forward);
                    }

                    break;
            }
        }
    }
}