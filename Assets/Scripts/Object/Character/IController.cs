using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Task;

namespace Game.Object.Character
{
    public interface IController
    {
        Character Character { get; }

        ITask Select();
        bool TryInterrupt();
        UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who, bool forced = false);

        void OnDestroy()
        {
            Character.TaskQueue.Clear();
        }
    }
}