using Game.Map;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Room
{
    [CreateAssetMenu(menuName = "Map/RoomData")]
    public class RoomData : ScriptableObject, IHasID
    {
        [SerializeField] public int zIndex;
        [SerializeField] public string roomName;
        [SerializeField] public RectFilledTile[] rects;
        [SerializeField] public MapObjectInit[] objects;
        [SerializeField] public Portal[] portals;
        [ShowInInspector] public string ID { get; private set; }

        private void OnEnable()
        {
            ID = IHasID.GenerateID();
        }
    }
}