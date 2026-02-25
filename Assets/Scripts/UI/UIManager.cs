using System;
using Game.Room;
using Game.UI;
using UnityEngine;

namespace Game
{
    public class UIManager : Singleton<UIManager>
    {
        public enum UIState
        {
            Default,
            Move
        }

        [SerializeField] private ReturnToPlayerButton btn;
        [SerializeField] private TrackPlayer playerCamera;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private UIState state = UIState.Default;

        public UIState State
        {
            get => state;
            set
            {
                var isDefault = value != UIState.Move;
                playerCamera.enabled = isDefault;
                
                btn.gameObject.SetActive(!isDefault);
            }
        }

        private void Awake()
        {
            btn.gameObject.SetActive(state == UIState.Move);

            RoomManager.Instance.OnRoomLoad += OnRoomLoad;
        }

        private void OnRoomLoad(int x)
        {
            var cpos = GameManager.Instance.Player?.CenterPosition;
            if (cpos == null) return;

            var value = cpos.Value;
            var z = GameManager.Instance.mainCamera.transform.position.z;

            GameManager.Instance.mainCamera.transform.position = new Vector3(value.x, value.y, z);
        }
    }
}