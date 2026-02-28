using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class GraphBar : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI txt;
        [SerializeField] public Image img;

        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}