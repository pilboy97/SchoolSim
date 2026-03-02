using Game.Room;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MapText : UIBehaviour
    {
        private TextMeshProUGUI _text;

        protected override void Start()
        {
            base.Awake();

            _text = GetComponent<TextMeshProUGUI>();

            RoomManager.Instance.OnRoomLoad += OnLoadRoom;
        }

        private void OnLoadRoom(int idx)
        {
            _text.text = RoomManager.Instance.CurrentRoom.roomName;
        }
    }
}