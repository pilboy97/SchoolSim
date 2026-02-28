using Game.Task;
using UnityEngine;

namespace Game.Object
{
    [CreateAssetMenu(menuName = "Object/Object Data")]
    public class ObjectData : ScriptableObject
    {
        [SerializeField] public string objectName;
        [SerializeField] public Action[] actions;
    }
}