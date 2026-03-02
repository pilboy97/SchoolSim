using System;
using UnityEngine;

namespace Game
{
    public class MainCamera : Singleton<MainCamera>
    {
        public int zIndex;

        private void LateUpdate()
        {
            var player = GameManager.Instance.Player;
            
            if (player == null || UIManager.Instance.state == UIManager.UIState.Move) return;

            zIndex = player.CPosition.z;
        }
    }
}