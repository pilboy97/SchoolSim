using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Event;
using Game.Map;
using Game.Object.Character.AI;
using Game.Object.Character.Player;
using Game.Room;
using Game.Task;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Action = Game.Task.Action;

namespace Game.Object.Character
{
    public enum ControllerType
    {
        None,
        AI,
        Player
    }

    [RequireComponent(typeof(TaskQueue))]
    public class Character : MonoBehaviour, IInteractable
    {
        [ShowInInspector] private CharacterData _data;
        [SerializeField] private TextMeshPro nameLabel;
        [SerializeField] private bool busy;
        [SerializeField] private Direction dir = Direction.Right;
        [SerializeField] public Action[] actions = Array.Empty<Action>();
        public bool IsVisible => RoomManager.Instance.currentRoomIndex == CPosition.z;

        public bool Busy
        {
            get => busy;
            set
            {
                busy = value;
                curTime = 0;
            }
        }

        public AIControl AI { get; private set; }
        
        [SerializeField] public float curTime = 0;
        [SerializeField] public float coolTime = 0.5f;

        public Action[] Actions => actions;

        public Event.Event CurEvent => EventManager.Instance.Find(_data.eventID);

        public void OnLeaveEvent()
        {
            _data.eventID = "";
        }

        [SerializeField] private string id;
        public string ID => id;

        public int ZIndex
        {
            get => CPosition.z;
            set => Position = new Vector3(Position.x, Position.y, value);
        }

        public Vector3 Position
        {
            get => _data.position;
            set => _data.position = value;
        }

        public string Name => _data.charName;
        public string Desc => $"{Name}:{ID}";

        public Vector3[] Positions => new[] { Position };
        public Vector3 CenterPosition => Position;

        public Vector3Int CPosition
        {
            get => MapController.Instance.WorldToCell(Position);
            set => Position = MapController.Instance.CellToWorld(value);
        }

        public Direction Direction
        {
            get => dir;
            set => dir = value;
        }

        [SerializeReference] public IController controller;
        private ControllerType _controllerType = ControllerType.AI;
        private ICharacterView _view;

        private IDistribution _distribution;

        [SerializeField] public TaskQueue taskQueue;

        [ShowInInspector]
        public ControllerType ControllerType
        {
            get => _controllerType;
            set
            {
                if (_controllerType == value) return;

                taskQueue.Cancel();
                taskQueue.Clear();

                if (controller is PlayerControl p) p.OnDestroy();

                _controllerType = value;
                controller = _controllerType switch
                {
                    ControllerType.Player => new PlayerControl(this),
                    ControllerType.AI => new AIControl(this),
                    _ => null
                };
            }
        }

        public CharacterData Data => _data;
        public TaskQueue TaskQueue => taskQueue;

        public float Receive(
            DeltaResult result,
            bool perSec = true, bool withSideEffect = true)
        {
            if (perSec)
            {
                var deltaTime = UnityEngine.Time.deltaTime;

                result.Stats *= deltaTime;
                
                if (result.Relation != null)
                {
                    RelationFloatDict r = new RelationFloatDict();

                    foreach (var (k, v) in result.Relation)
                    {
                        r.TryAdd(k, v * deltaTime);
                    }

                    result.Relation = r;
                }
            }
            
            return Receive(result.Relation, withSideEffect) + Receive(result.Stats, withSideEffect);
        }
        
        public float Receive(CharacterStats stats, bool withSideEffect = true)
        {
            if (withSideEffect)
            {
                _result.Reset();
                CalcPersonalizedStatsDeltaOnReceive(stats, ref _result);

                stats += _result.Stats;
            }

            _data.Receive(stats);

            return AI.CalcScore(stats, null, ref _result);
        }

        public float Receive(RelationFloatDict relations, bool withSideEffect = true)
        {
            float sum = 0;
            if (relations == null) return sum;

            foreach (var (k,v) in relations)
            {
                sum += Receive(k, v, withSideEffect);
            }

            return sum;
        }

        public bool IsRival(Character x)
        {
            foreach (var rival in _data.rivals)
            {
                if (x.id == rival.ID) return true;
            }

            return false;
        }
        public bool IsRival(string id)
        {
            foreach (var rival in _data.rivals)
            {
                if (id == rival.ID) return true;
            }

            return false;
        }
        public bool IsFriend(Character x)
        {
            foreach (var friend in _data.friends)
            {
                if (x.id == friend.ID) return true;
            }

            return false;
        }
        public bool IsFriend(string id)
        {
            foreach (var friend in _data.friends)
            {
                if (id == friend.ID) return true;
            }

            return false;
        }
        
        public float Receive(CharacterRelation rel, float value, bool withSideEffect = true)
        {
            float sum = 0;

            if (IsRival(rel.ID))
            {
                if (rel.relType == CharacterRelation.Type.Friend && value > 0)
                    value *= 0.1f;
                else if (rel.relType == CharacterRelation.Type.Romance && value > 0)
                    value = 0;
            }
            else if (IsFriend(rel.ID))
            {
                if (rel.relType == CharacterRelation.Type.Friend && value < 0) value *= 0.1f;
            }
            
            _data.Receive(rel, value);
            
            if (withSideEffect)
            {
                _result.Reset();
                
                CalcPersonalizedStatsDeltaOnReceive(rel, value, ref _result);
                
                _data.Receive(_result);
                sum += AI.CalcScore(_result.Stats, null, ref _result) + AI.CalcScore(_result.Relation, null, ref _result);
            }

            return sum;
        }

        private void Awake()
        {
            _view = GetComponent<ICharacterView>();
            _distribution = new DistributionNormalDistribution(0.5f, 0.1f);
        }

        public void Init()
        {
            _data.Init();
            id = _data.ID;

            var classroom = _data.classroom;
            classroom.grp.Add(_data);

            taskQueue = GetComponent<TaskQueue>();

            controller = _controllerType switch
            {
                ControllerType.AI => new AIControl(this),
                ControllerType.Player => new PlayerControl(this),
                _ => null
            };

            if (_controllerType == ControllerType.AI) AI = controller as AIControl;
            else AI = ((PlayerControl)controller).Autopilot;

            gameObject.name = _data.charName;

            var initStatus = new CharacterStats()
            {
                hungry =
                    new DistributionNormalDistribution(50, 10).Sample,
                fatigue =
                    new DistributionNormalDistribution(50, 10).Sample,
                toilet =
                    new DistributionNormalDistribution(50, 10).Sample,
                hygiene =
                    new DistributionNormalDistribution(50, 10).Sample,
                loneliness =
                    new DistributionNormalDistribution(50, 10).Sample,
                rLoneliness =
                    new DistributionNormalDistribution(50, 10).Sample,
                fun =
                    new DistributionNormalDistribution(50, 10).Sample,
                motivation =
                    new DistributionNormalDistribution(50, 10).Sample,
            };

            Receive(initStatus, false);
        }

        public void Init(CharacterData data)
        {
            _data = data;
            Init();
        }

        private void Start()
        {
            _view.SetVisible(true);
            Draw();
        }

        private void Draw()
        {
            _view.SetGender(_data.gender);
            _view.SetPosition(Position);
            _view.SetDirection(Direction);
            _view.SetName(_data.charName);
        }

        private void LateUpdate()
        {
            _view?.SetVisible(IsVisible);
            Draw();
        }
        
        private DeltaResult _result = new DeltaResult()
        {
            Stats = default,
            Relation = new RelationFloatDict()
        };
        
        public void OnUpdate()
        {
            var decay = ConfigData.Instance.statsDecay;
            float delta = UnityEngine.Time.deltaTime;
            
            Receive(new CharacterStats()
            {
                hungry = -decay,
                fatigue = -decay,
                toilet = -decay,
                hygiene = -decay,
                loneliness = -decay,
                rLoneliness = -decay,
                fun = -decay,
                motivation = -decay
            } * delta);
            
            _data.stats = _data.stats.Clamp(0, 100);
            foreach (var ch in ObjectManager.Instance.Characters)
            {
                var friendRel = new CharacterRelation()
                {
                    relType = CharacterRelation.Type.Friend,
                    ID = ch.id
                };
                var romanceRel = new CharacterRelation()
                {
                    relType = CharacterRelation.Type.Romance,
                    ID = ch.id
                };

                _data[friendRel] = Mathf.Clamp(_data[friendRel], -100, 100);
                var friendshipVal = _data[friendRel];
                
                if (friendshipVal <= 0)
                    _data[romanceRel] = 0;
                else
                    _data[romanceRel] = Mathf.Clamp(_data[romanceRel], 0, friendshipVal);

            }
            
            UpdateRelations();
            
            curTime += delta;

            if (curTime < coolTime) return;

            delta = curTime;

            curTime = 0;
            coolTime = _distribution.Sample;

            if (Busy) return;
            if (controller == null) return;

            var next = controller.Select();
            if (next == null)
            {
                return;
            }
            
            if (next is ActionTask { action: { indirect: false } } actionTask)
            {
                actionTask.Prev = new ActionTask(next.Sub,
                    null,
                    new Action()
                    {
                        actionName = $"Move closer To ${actionTask.Obj.Name}",
                        indirect = false,
                        busy = true,
                        effect = new TractTargetEffect
                        {
                            targetID = actionTask.Obj.ID
                        }
                    }
                );
            }

            taskQueue.Clear();
            taskQueue.PushFront(next);
        }

        public Vector3Int[] CPositions => new[]
        {
            MapController.Instance.WorldToCell(Position)
        };

        public async UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who,
            bool forced = false)
        {
            if (token.IsCancellationRequested) return false;
            if (!await controller.TryInviteMeAsync(token, e, who, forced)) return false;

            if (_data.eventID != "")
            {
                CurEvent?.Leave(this);
            }

            if (e.Status == EventStatus.Done)
            {
                return false;
            }

            _data.eventID = e.ID;

            taskQueue.Clear();
            taskQueue.PushFront(e, who);

            curTime = 0;

            return true;
        }

        public float GetVar(string name) => _data.GetVar(name);
        public void SetVar(string name, float val) => _data.SetVar(name, val);


        private readonly Dictionary<string, float> _attrCache = new();
        public float PersonalAttractionFrom(Character other)
        {
            const float M = 1.1f;

            if (other == null) return 0;

            if (_attrCache.TryGetValue(other.id, out var value)) return value;
            
            var angle = Vector3.Angle(_data.beauty, other._data.beauty) * Mathf.PI / 180;
            var b = Mathf.Pow(M, other.Data.attraction);

            var strikeZone = new StrikeZoneDistribution()
            {
                angle = angle,
                b = b,
                e = _data.e,
            };
            var ret = strikeZone.Priority;
            if (other.Data.gender == _data.gender) ret *= 0.5f;

            _attrCache[other.id] = ret;
            
            return ret;
        }

        public float AttractionModifier(Character other)
        {
            var attr = PersonalAttractionFrom(other);

            return 2 * attr - 1;
        }
        
        public void CalcPersonalizedStatsDeltaOnReceive(CharacterStats s, ref DeltaResult result)
        {
            s.motivation += s.SumSkill() + s.SumSubject();
            result.Stats += s;
        }

        public void CalcPersonalizedStatsDeltaOnReceive(CharacterRelation s, float v, ref DeltaResult result)
        {
            ref var ret = ref result.Stats;
            var other = ObjectManager.Instance.Find(s.ID) as Character;
            
            if (other == null) return ;

            var mbti = Data.mbti;
            var attr = PersonalAttractionFrom(other);

            var socialEfficiency = 1;

            int mod = 0;
            if (IsRival(s.ID))
                mod -= 3;
            else if (IsFriend(s.ID))
                mod += 3;
            
            foreach (var rival in _data.rivals)
            {
                if (rival.friends.Contains(other._data)) mod -= 1;
                if (rival.rivals.Contains(other._data)) mod += 1;
            }
            
            foreach (var friend in _data.friends)
            {
                if (friend.friends.Contains(other._data)) mod += 1;
                if (friend.rivals.Contains(other._data)) mod -= 1;
            }

            if (mod == 0) mod = 1;

            socialEfficiency *= mod;

            switch (s.relType)
            {
                case CharacterRelation.Type.Friend:
                    ret += new CharacterStats()
                    {
                        fun = attr * v * socialEfficiency,
                        loneliness = attr * v * socialEfficiency,
                    };
                    
                    break;

                case CharacterRelation.Type.Romance:
                    ret += new CharacterStats()
                    {
                        fun = attr * v * socialEfficiency,
                        rLoneliness = v * attr * socialEfficiency
                    };
                    
                    break;
            }
        }

        public void CalcPersonalizedStatsDeltaOnReceive(RelationFloatDict rel, ref DeltaResult result)
        {
            if (result.Relation != null)
            
             foreach (var (s, v) in result.Relation)
             {
                CalcPersonalizedStatsDeltaOnReceive(s, v, ref result);
             }
        }

        public void CalcPersonalizedStatsDeltaOnReceive(ref DeltaResult result)
        {
            if (result.Relation != null)
            {
                foreach (var (s, v) in result.Relation)
                {
                    CalcPersonalizedStatsDeltaOnReceive(s, v, ref result);
                }

                CalcPersonalizedStatsDeltaOnReceive(result.Stats, ref result);
            }
        }

        public void UpdateRelations()
        {
            foreach (var (k, v) in _data.relations)
            {
                if (k.relType == CharacterRelation.Type.Romance) continue;

                var other = ObjectManager.Instance.Find(k.ID) as Character;
                if (other == null) continue;
                
                if (v > 20 && IsRival(other))
                {
                    _data.rivals.Remove(other._data);
                }
                else if (v < -20 && IsFriend(other))
                {
                    _data.friends.Remove(other._data);
                }
                else if (v > 50 && !IsFriend(other))
                {
                    _data.friends.Add(other._data);
                }
                else if (v < -50 && !IsRival(other))
                {
                    _data.rivals.Add(other._data);
                }
            }
        }
    }
}