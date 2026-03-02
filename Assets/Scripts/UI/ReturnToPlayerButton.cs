using System;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class ReturnToPlayerButton : UIBehaviour
    {
        public void ReturnToPlayer()
        {
            UIManager.Instance.state = UIManager.UIState.Default;
        }
    }
}