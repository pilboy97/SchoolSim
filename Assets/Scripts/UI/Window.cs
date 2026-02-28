using System;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class Window :　MonoBehaviour
    {
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Move(Vector2 delta)
        {
            var d = new Vector3(delta.x, delta.y, 0);

            transform.position += d;
        }

        public void OnDisable()
        {
            _rectTransform.anchoredPosition = Vector3.zero;
        }
    }
}