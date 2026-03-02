using System;
using Game.Object;
using Game.Room;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class UIManager : Singleton<UIManager>
    {
        public enum UIState
        {
            Default,
            Move
        }

        [SerializeField] private ReturnToPlayerButton resetCameraBtn;
        [SerializeField] private Button changePlayerBtn;
        [SerializeField] private Button changeRoomBtn;
        [SerializeField] private MainCamera playerCamera;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] public UIState state = UIState.Default;

        public void Init()
        {
            resetCameraBtn.gameObject.SetActive(state == UIState.Move);

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

        private void LateUpdate()
        {
            var isDefault = state != UIState.Move;
            
            playerCamera.GetComponent<TrackPlayer>().enabled = isDefault;

            resetCameraBtn.gameObject.SetActive(!isDefault);
            changeRoomBtn.gameObject.SetActive(!isDefault);

            if (isDefault)
            {
                RoomManager.Instance.LoadRoomData(
                    RoomManager.Instance.CurrentRoom,
                    RoomManager.Instance.currentRoomIndex);
            }
        }

        public void ChangePlayer()
        {
            int idx = 0;

            if (GameManager.Instance.Player == null)
            {
                if (ObjectManager.Instance.Characters.Length > 0)
                    GameManager.Instance.SetPlayer(ObjectManager.Instance.Characters[idx]);
                
                return;
            }

            for (idx = 0; idx < ObjectManager.Instance.Characters.Length; idx++)
            {
                var ch = ObjectManager.Instance.Characters[idx];

                if (ch == GameManager.Instance.Player) break;
            }

            idx = (idx + 1) % ObjectManager.Instance.Characters.Length;
            
            GameManager.Instance.SetPlayer(ObjectManager.Instance.Characters[idx]);
        }
        public void ChangeRoom()
        {
            if(UIManager.Instance.state != UIManager.UIState.Move) return;
            
            int idx = RoomManager.Instance.currentRoomIndex;
            idx = (idx + 1) % RoomManager.Instance.roomDatas.Count;

            var data = RoomManager.Instance.roomDatas[idx];

            MainCamera.Instance.transform.position = new Vector3(0,0,-10);
            RoomManager.Instance.LoadRoomData(data, idx);
        }
    }
}