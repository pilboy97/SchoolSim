using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Object.Character;

namespace Game.Task
{
    public interface ITask: IHasID
    {
        public bool Busy { get; set; }
        public Character Sub { get; }
        string Desc { get; }
        ITask Prev { get; set; }
        UniTask DoAsync(CancellationToken token);
        float CalcScore();

        float GetVar(string name) => GameManager.Instance.GetVar($"{ID}|{name}");
        void SetVar(string name, float val) => GameManager.Instance.SetVar($"{ID}|{name}", val);
    }
}