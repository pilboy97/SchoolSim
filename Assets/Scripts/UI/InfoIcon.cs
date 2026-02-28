using System;
using Game.Event.Talk;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class InfoIcon : UIBehaviour
    {
        [SerializeField] private InfoIcons _icons;

        protected override void Awake()
        {
            _icons.OnLateUpdate += OnUpdate;
            
            gameObject.SetActive(false);
        }

        private void OnUpdate()
        {
            if (GameManager.Instance.Player?.CurEvent is not TalkEvent t)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
        }

        public void OnClick()
        {
            var t = GameManager.Instance.Player.CurEvent as TalkEvent;
            TalkWindow.Instance.Init(t);
        }
    }
}