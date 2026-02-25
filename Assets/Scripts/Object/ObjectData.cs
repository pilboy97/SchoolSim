using Game.Task;
using UnityEngine;

namespace Game.Object
{
    [CreateAssetMenu(menuName = "Object/Object Data")]
    public class ObjectData : ScriptableObject, IHasID
    {
        [SerializeField] public string objectName;
        [SerializeField] public Action[] actions;
        [SerializeField] private string objID;

        public string ID => objID;
    }
}