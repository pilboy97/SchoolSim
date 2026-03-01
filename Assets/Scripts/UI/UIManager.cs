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
        [SerializeField] private MainCamera playerCamera;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private UIState state = UIState.Default;

        public UIState State
        {
            get => state;
            set
            {
                var isDefault = value != UIState.Move;
                playerCamera.GetComponent<TrackPlayer>().enabled = isDefault;

                btn.gameObject.SetActive(!isDefault);

                if (isDefault)
                {
                    RoomManager.Instance.LoadRoomData(
                        RoomManager.Instance.CurrentRoom,
                        RoomManager.Instance.currentRoomIndex);
                }
            }
        }

        public void Init()
        {
            btn.gameObject.SetActive(state == UIState.Move);

            RoomManager.Instance.OnRoomLoad += OnRoomLoad;
        }

        private void Start()
        {
            playerCamera = MainCamera.Instance;
        }

        private void OnRoomLoad(int x)
        {
            if (state == UIState.Move) return;
            
            var pos = MainCamera.Instance.transform.position;

            GameManager.Instance.mainCamera.transform.position = new Vector3(pos.x, pos.y, -10);
        }
    }
}