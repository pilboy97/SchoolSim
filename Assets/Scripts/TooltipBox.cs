using System;
using TMPro;
using UnityEngine;

namespace Game
{
    
    public class TooltipBox : Singleton<TooltipBox>
    {
        [SerializeField] private TextMeshProUGUI textComp;
        [SerializeField] public RectTransform rectTransform;

        [SerializeField] private string text;
        
        private Tooltip _target;

        private void Awake()
        {
            _target = null;
            Instance.gameObject.SetActive(false);
        }

        public string Text
        {
            get => text;
            set
            {
                text = value;
                textComp.text = text;
            }
        }
        
        public bool TryOpen(Tooltip x)
        {
            if (_target != null) return false;
            if (_target == x) return true;

            _target = x;
            gameObject.SetActive(true);
            return true;
        }

        public bool TryClose(Tooltip x)
        {
            if (_target == null) return false;
            if (_target != x) return false;
            
            _target = null;
            gameObject.SetActive(false);
            return true;
        }
    }
}