using System;
using System.Collections.Generic;
using System.Linq;
using Game.Map;
using Game.Object;
using Game.Room;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class NavManager : Singleton<NavManager>
    {
        const int INF = 1 << 20;
        
        private Map.Map GroundMap => MapController.Instance.groundMap;
        private Map.Map WallMap => MapController.Instance.wallMap;
        
        public HashSet<Vector3Int> WalkableCells { get; private set; } = new();

        private HashSet<(Vector3Int, Vector3Int)> _jumpPoints = new();
        private HashSet<Vector3Int> _jumpPointsPos = new();

        private readonly Dictionary<int, Vector3Int[]> _jumpPointsCache = new();
        
        private Dictionary<(Vector3Int, Vector3Int), int> _distJP = new();
        private Dictionary<(Vector3Int, Vector3Int), Vector3Int> _cacheJP = new();

        private Dictionary<(Vector3Int, Vector3Int), (Vector3Int[] path, int dist)> _pathCache = new();

        public Vector3Int RandomPos => Random.Choose(WalkableCells.ToList());
        public Vector3Int RandomCPos => MapController.Instance.WorldToCell(RandomPos);

        public void Init()
        {
            _jumpPoints.Clear();
            _jumpPointsPos.Clear();
            _pathCache.Clear();

            foreach (var room in RoomManager.Instance.roomDatas)
            {
                foreach (var jp in room.portals)
                {
                    _jumpPointsPos.Add(jp.Dest);
                    foreach (var pos in jp.Positions)
                    {
                        var cp = MapController.Instance.WorldToCell(pos);
                        _jumpPoints.Add((cp, jp.Dest));
                        _jumpPointsPos.Add(cp);
                    }
                }
            }

            foreach (var jp in _jumpPointsPos)
            {
                _jumpPointsCache[jp.z] = _jumpPointsPos.Where(x=>x.z == jp.z).ToArray();
            }

            SetWalkableCell();

            FindInterRoom();
        }

        public void UpdateObstacles()
        {
            SetWalkableCell();
            ClearCache();
        }

        public void ClearCache()
        {
            _pathCache.Clear();
            FindInterRoom();
        }

        public void SetWalkableCell()
        {
            HashSet<Vector3Int> groundCells = new();
            HashSet<Vector3Int> otherCells = new();

            for (var i = 0; i < RoomManager.Instance.roomDatas.Count; i++)
            {
                var data = RoomManager.Instance.roomDatas[i];

                foreach (var rect in data.rects)
                {
                    HashSet<Vector3Int> target = otherCells;

                    if (rect.targetMapID == GroundMap.mapID) target = groundCells;
                    if (rect.targetMapID == MapController.Instance.jumpMap.mapID) continue;
                    
                    foreach (var pos in rect.rect.allPositionsWithin)
                    {
                        target.Add(new Vector3Int(pos.x, pos.y, i));
                    }
                }
            }

            foreach (var t in ObjectManager.Instance.occupied)
            {
                otherCells.Add(t);
            }

            WalkableCells = groundCells;
            WalkableCells.ExceptWith(otherCells);
        }

        public void FindInterRoom()
        {
            _distJP.Clear();
            _cacheJP.Clear();

            foreach (var x in _jumpPointsPos)
            {
                foreach (var y in _jumpPointsPos)
                {
                    _distJP[(x, y)] = INF;
                }
            }

            foreach (var x in _jumpPoints)
            {
                _distJP[(x.Item1, x.Item1)] = 0;
                _distJP[(x.Item1, x.Item2)] = 0;
                _cacheJP[(x.Item1, x.Item2)] = x.Item2;
                _distJP[(x.Item2, x.Item2)] = 0;
            }

            for (int i = 0; i < RoomManager.Instance.roomDatas.Count; i++)
            {
                var _d = _jumpPointsPos.Where(x => x.z == i).ToArray();

                foreach (var X in _d)
                {
                    foreach (var Y in _d)
                    {
                        _distJP[(X, Y)] = FindPathSameMap(X, Y).Item2;
                        _cacheJP[(X, Y)] = Y;
                    }
                }
            }

            foreach (var dot in _jumpPointsPos)
            {
                foreach (var ed in _jumpPointsPos)
                {
                    foreach (var st in _jumpPointsPos)
                    {
                        if (_distJP[(st, dot)] < INF && _distJP[(dot, ed)] < INF && _distJP[(st, dot)] + _distJP[(dot, ed)] < _distJP[(st, ed)])
                        {
                            _distJP[(st, ed)] = _distJP[(st, dot)] + _distJP[(dot, ed)];
                            _cacheJP[(st, ed)] = dot;
                        }
                    }
                }
            }
        }

        private Vector3Int[] GetJump(Vector3Int s, Vector3Int e)
        {
            var P = s;
            var route = new List<Vector3Int>() { s };

            while (P != e)
            {
                P = _cacheJP[(P, e)];
                route.Add(P);
            }

            return route.ToArray();
        }

        public (Vector3Int[], int) FindPath(Vector3 st, Vector3 ed)
        {
            return FindPath(MapController.Instance.WorldToCell(st), MapController.Instance.WorldToCell(ed));
        }

        public (Vector3Int[], int) FindPath(Vector3Int st, Vector3Int ed)
        {
            if (st == ed) return (new[] { st }, 0);
            if (!WalkableCells.Contains(st) || !WalkableCells.Contains(ed)) return (Array.Empty<Vector3Int>(), INF);

            if (_pathCache.TryGetValue((st, ed), out var mem))
            {
                return mem;
            }
            
            int sId = st.z;
            int eId = ed.z;

            if (sId == eId) return FindPathSameMap(st, ed);

            var sjp = _jumpPointsCache[sId];
            var ejp = _jumpPointsCache[eId];

            Vector3Int S = new Vector3Int();
            Vector3Int E = new Vector3Int();
            int dist = INF;
            
            foreach (var s in sjp)
            {
                foreach (var e in ejp)
                {
                    if (!_distJP.TryGetValue((s,e),out var distjp)) continue;
                    if (distjp == INF) continue;

                    var sdist = FindPathSameMap(st, s).Item2;
                    var edist = FindPathSameMap(e, ed).Item2;
                    
                    if (sdist == INF || edist == INF) continue;
                    
                    var d = sdist + distjp + edist;
                    if (dist > d) {
                        dist = d;
                        S = s;
                        E = e;
                    }
                }
            }

            if (dist == INF) return (null, INF);
            
            var jRoute = GetJump(S, E);
            var subPaths = new Vector3Int[jRoute.Length + 2][];

            int len = 0;
            
            subPaths[0] = FindPathSameMap(st, S).Item1;
            len += subPaths[0].Length;
            
            for (int i = 0; i < jRoute.Length - 1; i++)
            {
                if (jRoute[i].z != jRoute[i + 1].z) continue;
                subPaths[i + 1] = FindPathSameMap(jRoute[i], jRoute[i + 1]).Item1;
                len += subPaths[i + 1].Length;
            }

            subPaths[jRoute.Length + 1] = FindPathSameMap(E, ed).Item1;
            len += subPaths[jRoute.Length + 1].Length;

            var ret = new Vector3Int[len];
            int cnt = 0;
            foreach (var path in subPaths)
            {
                if (path == null) continue;
                foreach (var dot in path)
                {
                    ret[cnt++] = dot;
                }
            }

            _pathCache[(st, ed)] = (ret, dist);
            
            return (ret, dist);
        }

        private (Vector3Int[], int) FindPathSameMap(Vector3Int st, Vector3Int ed)
        {
            if (st == ed) return (new[] { ed }, 0);
            if (st.z != ed.z) return (null, INF);

            var key = (st, ed);

            if (_pathCache.TryGetValue(key, out var cachedData))
            {
                return cachedData;
            }
            
            var result = RunAStar(st, ed);

            _pathCache[key] = result;

            return result;
        }
        
        private (Vector3Int[] path, int dist) RunAStar(Vector3Int st, Vector3Int ed)
        {
            var openSet = new List<Vector3Int> { st };
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            
            var gScore = new Dictionary<Vector3Int, int> { [st] = 0 };
            var fScore = new Dictionary<Vector3Int, int> { [st] = Heuristic(st, ed) };

            var dir = new[]
            {
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(1, 0, 0)
            };

            while (openSet.Count > 0)
            {
                var minScore = INF;
                Vector3Int next = default;
                foreach (var dot in openSet)
                {
                    var score = fScore.GetValueOrDefault(dot, INF);
                    if (score < minScore)
                    {
                        minScore = score;
                        next = dot;
                    }
                }
                
                if (next == ed)
                {
                    return ReconstructPath(cameFrom, next, gScore[next]);
                }

                openSet.RemoveAt(0);

                foreach (var d in dir)
                {
                    var neighbor = next + d;

                    if (!WalkableCells.Contains(neighbor)) continue;

                    int tentative_gScore = gScore.GetValueOrDefault(next, INF) + 1;

                    if (tentative_gScore < gScore.GetValueOrDefault(neighbor, INF))
                    {
                        cameFrom[neighbor] = next;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = tentative_gScore + Heuristic(neighbor, ed);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return (null, INF);
        }

        public (Vector3Int[], int) FindPath(Vector3 src, Vector3[] dst)
        {
            int minDist = INF;
            Vector3Int[] minPath = null;

            foreach (var d in dst)
            {
                var (path, dist) = FindPath(src, d);

                if (dist < minDist)
                {
                    minDist = dist;
                    minPath = path;
                }
            }

            return (minPath, minDist);
        }
        public (Vector3Int[], int) FindPath(Vector3Int src, Vector3Int[] dst)
        {
            int minDist = INF;
            Vector3Int[] minPath = null;

            foreach (var d in dst)
            {
                var (path, dist) = FindPath(src, d);

                if (dist < minDist)
                {
                    minDist = dist;
                    minPath = path;
                }
            }

            return (minPath, minDist);
        }

        private static Vector3Int[] _neighborVecs = new[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };
        public (Vector3Int[], int) FindPathAround(Vector3Int src, Vector3Int dst)
        {
            int minDist = INF;
            Vector3Int[] minPath = null;

            foreach (var vec in _neighborVecs)
            {
                var (path, dist) = FindPath(src, dst + vec);

                if (dist < minDist)
                {
                    minDist = dist;
                    minPath = path;
                }
            }

            return (minPath, minDist);
        }
        
        public (Vector3Int[], int) FindPathAround(Vector3 src, Vector3[] dst)
        {
            int minDist = INF;
            Vector3Int[] minPath = null;

            if (dst == null) return (Array.Empty<Vector3Int>(), 0);

            foreach (var d in dst)
            {
                foreach (var vec in _neighborVecs)
                {
                    var (path, dist) = FindPath(src, d + vec);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        minPath = path;
                    }
                }
            }

            return (minPath, minDist);
        }
        
        public (Vector3Int[], int) FindPathAround(Vector3 src, Vector3 dst)
        {
            int minDist = INF;
            Vector3Int[] minPath = null;

            foreach (var vec in _neighborVecs)
            {
                var (path, dist) = FindPath(src, dst + vec);

                if (dist < minDist)
                {
                    minDist = dist;
                    minPath = path;
                }
            }

            return (minPath, minDist);
        }
        
        public (Vector3Int[], int) FindPathAround(Vector3Int src, Vector3Int[] dst)
        {
            int minDist = INF;
            Vector3Int[] minPath = null;

            foreach (var d in dst)
            {
                foreach (var vec in _neighborVecs)
                {
                    var (path, dist) = FindPath(src, d + vec);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        minPath = path;
                    }
                }
            }

            return (minPath, minDist);
        }

        private int Heuristic(Vector3Int a, Vector3Int b)
        {
            return math.abs(a.x - b.x) + math.abs(a.y - b.y);
        }

        private (Vector3Int[] path, int dist) ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current, int dist)
        {
            var path = new List<Vector3Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                if(current != path.Last())
                    path.Add(current);
            }
            path.Reverse();
            return (path.ToArray(), dist);
        }
    }
}