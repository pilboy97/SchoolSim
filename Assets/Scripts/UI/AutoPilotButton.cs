using System;
using Game.Object.Character;
using Game.Object.Character.Player;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class AutoPilotButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txt;

        private bool _wannaChange = false;

        private void Start()
        {
            GameManager.Instance.OnSetPlayer += ch =>
            {
                gameObject.SetActive(ch != null);
            };
            
            gameObject.SetActive(false);
        }

        public void LateUpdate()
        {
            var player = GameManager.Instance?.Player;

            if (player.controller is not PlayerControl playerController) return;

            txt.text = (playerController.isAutoPilot) ? "Auto Pilot is ON" : "Auto Pilot is OFF";
            
            if (!_wannaChange) return;
            
            playerController?.ToggleAutoPilot();

            _wannaChange = false;
        }

        public void OnClick()
        {
            var player = GameManager.Instance.Player;
            var controllerType = player.ControllerType;

            _wannaChange = true;
        }
    }
}