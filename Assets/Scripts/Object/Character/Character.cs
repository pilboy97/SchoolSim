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
using Unity.Mathematics;
using UnityEngine;
using Action = Game.Task.Action;

namespace Game.Object.Character
{
    public enum ControllerType
    {
        None,
        AI,
        AutoPilot,
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

        [SerializeField] private float curTime = 0;
        [SerializeField] private float coolTime = 0.5f;

        public bool Busy
        {
            get => busy;
            set => busy = value;
        }

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
        
        public Vector3[] Positions => new [] { Position };
        public Vector3 CenterPosition => Position;
        
        public Vector3Int CPosition
        {
            get => MapController.Instance.WorldToCell(Position);
            set => Position = MapController.Instance.CellToWorld(value);
        }

        public Direction Direction
        {
            get => dir;
            set
            {
                dir = value;
            }
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

                if (controller is PlayerControl p) p.OnDestroy();

                _controllerType = value;
                controller = _controllerType switch
                {
                    ControllerType.AutoPilot => new PlayerControl(this, true),
                    ControllerType.Player => new PlayerControl(this),
                    ControllerType.AI => new AIControl(this),
                    _ => null
                };
            }
        }

        public CharacterData Data => _data;
        public TaskQueue TaskQueue => taskQueue;

        public void Apply(CharacterStatus status, float value)
        {
            _data[status] += value;
        }
        
        private void Awake()
        {
            _view = GetComponent<ICharacterView>();
            _distribution = new DistributionUniformDistribution(0.3f, 0.7f);
        }

        public void Init()
        {
            _data.owner = this;
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

            gameObject.name = _data.charName;
            
            var initStatus = new CharacterStatusDeltaFactory(new Dictionary<CharacterStatus, float>()
            {
                {
                    CharacterStatus.Hungry,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {
                    CharacterStatus.Fatigue,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {
                    CharacterStatus.Toilet,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {

                    CharacterStatus.Hygiene,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {

                    CharacterStatus.Loneliness,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {

                    CharacterStatus.RLoneliness,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {

                    CharacterStatus.Fun,
                    new DistributionNormalDistribution(50, 10).Sample
                },
                {
                    CharacterStatus.Motivation,
                    new DistributionNormalDistribution(50, 10).Sample
                },
            });

            initStatus.Apply(this, false);
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
            _view.SetPosition(_data.position);
        }

        public void OnUpdate()
        {
            curTime += UnityEngine.Time.deltaTime;

            if (curTime < coolTime) return;

            var delta = curTime;
            
            curTime = 0;
            coolTime = _distribution.Sample;
            
            _view?.SetVisible(IsVisible);
            
            var deltas = new CharacterStatusDeltaFactory(new Dictionary<CharacterStatus, float>
            {
                { CharacterStatus.Hungry, -0.1f },
                { CharacterStatus.Fatigue, -0.1f },
                { CharacterStatus.Motivation, -0.1f },
                { CharacterStatus.Toilet, -0.1f },
                { CharacterStatus.Loneliness, -0.1f },
                { CharacterStatus.RLoneliness, -0.1f },
                { CharacterStatus.Fun, -0.1f },
                { CharacterStatus.Hygiene, -0.1f }
            }) * delta;

            deltas.Apply(this);
            
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
                        effect = new TractTargetEffect {
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
        
        private float CalcENeedScoreMultiplier(float val)
        {
            return Mathf.Pow(0.5f, (val - 64f) / 3);
        }

        private float CalcRNeedScoreMultiplier(float val)
        {
            return Mathf.Max(0, 1.2f * (100f - val));
        }
        
        private float CalcGNeedScoreMultiplier(float val)
        {
            // 1. 기본적으로는 숙련도가 낮을 때 아주 최소한의 '배워야겠다'는 의지가 있음 (15점)
            float baseWill = 15f; 

            // 2. 숙련도(val)가 높을수록 점수가 가파르게 상승 (양성 피드백)
            // 0~100 사이의 val을 0~1로 정규화해서 계산
            float ratio = val / 100f;
            float passion = Mathf.Pow(ratio, 2.5f) * 120f; // 2.5제곱으로 전문가일수록 몰입도 폭발

            return baseWill + passion;
        }

        public float CalcScore()
        {
            if (taskQueue.Current != null) return taskQueue.Current.CalcScore();

            return 0;
        }
        
        public float CalcScore(CharacterStatusDeltaFactory deltas, IInteractable o)
        {
            var eNeedScore = 0f;
            for (var need = CharacterStatus.ENeedsBegin; need < CharacterStatus.ENeedsEnd; need++)
            {
                eNeedScore += CalcENeedScoreMultiplier(
                                  Data[need]
                              ) *
                              deltas[need];
            }

            var rNeedScore = 0f;
            for (var need = CharacterStatus.RNeedsBegin; need < CharacterStatus.RNeedsEnd; need++)
            {
                rNeedScore += CalcRNeedScoreMultiplier(
                                  Data[need]
                              ) *
                              deltas[need];
            }

            var gNeedScore = 0f;
            for (var need = CharacterStatus.GNeedsBegin; need < CharacterStatus.GNeedsEnd; need++)
            {
                gNeedScore += CalcGNeedScoreMultiplier(
                                  Data[need]
                              ) *
                              deltas[need];
            }

            var score = eNeedScore * Data.eModifier +
                        rNeedScore * Data.rModifier +
                        gNeedScore * Data.gModifier;
            
            var dist = NavManager.Instance.FindPathAround(Position, o?.Positions ?? new Vector3[] {
                Position
            }).Item2;

            return score / math.max(dist, 1f);
        }

        public async UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who, bool forced = false)
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
            
            return true;
        }

        public float GetVar(string name) => _data.GetVar(name);
        public void SetVar(string name, float val) => _data.SetVar(name, val);
        
        public float PersonalAttractionFrom(Character other)
        {
            const float M = 1.1f;
            
            if (other == null) return 0;
            
            var angle = Vector3.Angle(_data.beauty, other._data.beauty) * Mathf.PI / 180;
            var b = Mathf.Pow(M,  other._data.attraction);

            var strikeZone = new StrikeZoneDistribution()
            {
                angle = angle,
                b = b,
                e = _data.e,
            };
            var ret = strikeZone.Priority;
            if (other._data.gender == _data.gender) ret *= 0.5f;

            return ret;
        }
        
        public CharacterStatusDeltaFactory Receive(CharacterStatus s, float v)
        {
            var ret = new CharacterStatusDeltaFactory();
            ret.Add(s, v);

            return ret;
        }

        public CharacterStatusDeltaFactory Receive(CharacterStatusDeltaFactory delta)
        {
            var ret = new CharacterStatusDeltaFactory();
            foreach (var (s ,v) in delta.dict)
            {
                ret.Add(Receive(s, v));
            }

            return ret;
        }
    }
}