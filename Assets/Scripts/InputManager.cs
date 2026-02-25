using System;
using Game.Time;
using UnityEngine;

namespace Game
{
    public class InputManager : Singleton<InputManager>, IClickable
    {
        public Action<Vector2> OnClickHandler = _ => { };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Equals)) TimeManager.Instance.Faster();

            if (Input.GetKeyDown(KeyCode.Minus)) TimeManager.Instance.Slower();
        }

        public void OnClick(Vector2 pos)
        {
            var wPos = GameManager.Instance.mainCamera.ScreenToWorldPoint(pos);

            OnClickHandler(wPos);
        }

        public void Init()
        {
            
        }
    }
}