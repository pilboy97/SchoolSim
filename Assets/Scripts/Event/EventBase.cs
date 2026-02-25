using Game.Object.Character;
using Game.Task;
using UnityEngine;

namespace Game.Event
{
    [CreateAssetMenu(menuName = "Event/Event Data")]
    public class EventBase : ScriptableObject
    {
        [SerializeField] public string eventName;
        [SerializeReference] public Effect effect;
        [SerializeReference] public Effect onStart;
        [SerializeReference] public Effect onEnd;
        [SerializeReference] public Effect onEnter;
        [SerializeReference] public Effect onLeave;
        [SerializeReference] public Condition inviteCond;
        [SerializeReference] public Condition runCond;
        [SerializeField] public bool zombie;
        [SerializeField] public bool busy;
        [SerializeField] public bool repeat;
    }
}