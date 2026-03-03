using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ExitButton : MonoBehaviour
    {
        [SerializeField] public Button btn;
        
        private void Awake()
        {
            btn.onClick.AddListener(Exit);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}