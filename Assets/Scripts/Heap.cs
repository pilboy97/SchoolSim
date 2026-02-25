using System.Collections.Generic;

namespace Game
{
    public class Heap<TElem, TPriority> where TElem : new() where TPriority : new()
    {
        private readonly IComparer<TPriority> _comparer;
        private readonly List<(TElem, TPriority)> _nodes;

        public Heap(IComparer<TPriority> comparer = null)
        {
            _nodes = new List<(TElem, TPriority)> { (new TElem(), new TPriority()) };
            _comparer = comparer;
        }

        public IComparer<TPriority> Comparer => _comparer ?? Comparer<TPriority>.Default;

        public int Size => _nodes.Count - 1;

        public void Enqueue((TElem, TPriority) val)
        {
            _nodes.Add(val);
            var idx = Size;

            while (idx != 1)
            {
                var pal = idx >> 1;
                var (pElem, pPriority) = _nodes[pal];
                var (e, p) = _nodes[idx];

                if (Comparer.Compare(p, pPriority) <= 0) break;

                (_nodes[pal], _nodes[idx]) = (_nodes[idx], _nodes[pal]);
                idx = pal;
            }
        }

        public TElem Dequeue()
        {
            TryDequeue(out var ret);

            return ret;
        }

        public bool TryDequeue(out TElem res)
        {
            res = new TElem();
            if (Size == 0) return false;


            res = _nodes[1].Item1;
            (_nodes[1], _nodes[^1]) = (_nodes[^1], _nodes[1]);
            _nodes.RemoveAt(_nodes.Count - 1);

            var idx = 1;
            while (true)
            {
                var (l, r) = (idx * 2, idx * 2 + 1);
                if (l > Size) break;

                var parPriority = _nodes[idx].Item2;

                var lNode = _nodes[l];
                var lPriority = lNode.Item2;

                if (r > Size)
                {
                    if (Comparer.Compare(lPriority, parPriority) > 0)
                    {
                        (_nodes[idx], _nodes[l]) = (_nodes[l], _nodes[idx]);
                        idx = l;
                    }

                    break;
                }

                var rNode = _nodes[r];
                var rPriority = rNode.Item2;

                if (Comparer.Compare(lPriority, parPriority) > 0)
                {
                    (_nodes[idx], _nodes[l]) = (_nodes[l], _nodes[idx]);
                    idx = l;

                    continue;
                }

                if (Comparer.Compare(rPriority, parPriority) > 0)
                {
                    (_nodes[idx], _nodes[r]) = (_nodes[r], _nodes[idx]);
                    idx = r;

                    continue;
                }

                break;
            }

            return true;
        }
    }
}