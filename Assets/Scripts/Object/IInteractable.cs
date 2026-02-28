using Game.Task;
using UnityEngine;

namespace Game.Object
{
    public interface IInteractable : IHasID
    {
        string charName { get; }
        Vector3[] Positions { get; }
        Vector3Int[] CPositions { get; }
        Vector3 CenterPosition { get; }
        Action[] Actions { get; }

        void Init();
        int ZIndex { get; set; }

        public void OnUpdate() { }
        
        float Distance(IInteractable x)
        {
            if (x == null) return 0;
            var pos = Positions;
            var xpos = x.Positions;

            float ret = float.PositiveInfinity;
            foreach (var p in pos)
            {
                foreach (var p2 in xpos)
                {
                    var dist = Mathf.Approximately(p.z, p2.z) ?Vector2.Distance(p, p2) : float.MaxValue;

                    ret = Mathf.Min(ret, dist);
                }
            }

            return ret;
        }
    }
}