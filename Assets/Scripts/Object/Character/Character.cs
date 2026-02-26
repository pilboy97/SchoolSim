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

        public void Apply(CharacterStatus status, float value)
        {
            _data[status] += value;
        }

        public void Apply(CharacterRelation status, float value)
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
            _data.Init();
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

            foreach (var ch in ObjectManager.Instance.Characters)
            {
                var friendRel = new CharacterRelation()
                {
                    relType = CharacterRelation.Type.Friend,
                    ID = ch.id
                };
                var romanceRel = new CharacterRelation()
                {
                    relType = CharacterRelation.Type.Friend,
                    ID = ch.id
                };
                var friendshipVal = _data[friendRel];

                if (friendshipVal <= 0)
                    _data[romanceRel] = 0;
                else
                    _data[romanceRel] = Mathf.Clamp(_data[romanceRel], 0, friendshipVal);

            }

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

            return true;
        }

        public float GetVar(string name) => _data.GetVar(name);
        public void SetVar(string name, float val) => _data.SetVar(name, val);


        public float PersonalAttractionFrom(Character other)
        {
            const float M = 1.1f;

            if (other == null) return 0;

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

            return ret;
        }
        //
        // public CharacterStatusDeltaFactory CalcPersonalizedStatsDeltaOnReceiveStatsDelta(CharacterStatus s, float v)
        // {
        //     var ret = new CharacterStatusDeltaFactory();
        //
        //     if ((s > CharacterStatus.SubjectBegin && s < CharacterStatus.SubjectEnd) ||
        //         (s > CharacterStatus.IntBegin && s < CharacterStatus.IntEnd) ||
        //         (s > CharacterStatus.SkillBegin && s < CharacterStatus.SkillEnd)
        //         )
        //     {
        //         ret.Add(CharacterStatus.Motivation, v);
        //     }
        //     
        //     ret.Add(s, v);  
        //
        //     return ret;
        // }
        //
        // public CharacterStatusDeltaFactory CalcPersonalizedStatsDeltaOnReceiveStatsDelta(CharacterRelation s, float v)
        // {
        //     var ret = new CharacterStatusDeltaFactory();
        //
        //     var other = ObjectManager.Instance.Find(s.ID) as Character;
        //     if (other == null) return ret;
        //
        //     var attr = PersonalAttractionFrom(other);
        //     float funModifier = 0;
        //     float fun = v;
        //     
        //     switch (s.relType)
        //     {
        //         case CharacterRelation.Type.Friend :
        //             funModifier = (attr * 2);
        //             fun *= funModifier;
        //             var loneliness = v;
        //
        //             ret.Add(CharacterStatus.Fun, fun);
        //             ret.Add(CharacterStatus.Loneliness, loneliness);
        //             
        //             break;
        //         case CharacterRelation.Type.Romance :
        //             funModifier = Mathf.Max(0, attr * 2 - 1);
        //             fun = funModifier * v;
        //             
        //             var rLoneliness = v;
        //             
        //             ret.Add(CharacterStatus.Fun, fun);
        //             ret.Add(CharacterStatus.RLoneliness, rLoneliness);
        //
        //             break;
        //     }
        //
        //     return ret;
        // }

        // Character 클래스 내부 메서드라고 가정
        public CharacterStatusDeltaFactory CalcPersonalizedStatsDeltaOnReceiveStatsDelta(CharacterStatus s, float v)
        {
            var ret = new CharacterStatusDeltaFactory();
            var mbti = Data.mbti; // CharacterData에서 가져옴
            float mbtiModifier = 1.0f;

            // 1. S(감각) vs N(직관) : 무엇을 배울 때 더 몰입하는가?
            if (s > CharacterStatus.SkillBegin && s < CharacterStatus.SkillEnd)
            {
                // 실질적 기술(S) 선호
                if (mbti.CheckComponent(MBTIComponent.S)) mbtiModifier = 1.2f;
                ret.Add(CharacterStatus.Motivation, v * mbtiModifier);
            }
            else if (s > CharacterStatus.SubjectBegin && s < CharacterStatus.SubjectEnd)
            {
                // 추상적 이론(N) 선호
                if (mbti.CheckComponent(MBTIComponent.N)) mbtiModifier = 1.2f;
                ret.Add(CharacterStatus.Motivation, v * mbtiModifier);
            }

            // 2. T(사고) : 성취(Motivation)에서 즐거움을 얻음
            if (mbti.CheckComponent(MBTIComponent.T) && s == CharacterStatus.Motivation)
            {
                ret.Add(CharacterStatus.Fun, v * 0.3f); // 무언가 배우는 것 자체가 즐거움
            }

            ret.Add(s, v * mbtiModifier);
            return ret;
        }

        public CharacterStatusDeltaFactory CalcPersonalizedStatsDeltaOnReceiveStatsDelta(CharacterRelation s, float v)
        {
            var ret = new CharacterStatusDeltaFactory();
            var other = ObjectManager.Instance.Find(s.ID) as Character;
            if (other == null) return ret;

            var mbti = Data.mbti;
            var attr = PersonalAttractionFrom(other);

            // 3. E(외향) vs I(내향) : 사회적 활동의 에너지 효율
            // 외향인은 사교로 고독감이 더 빨리 해소됨(1.2x), 내향인은 효율이 낮음(0.8x)
            float socialEfficiency = mbti.CheckComponent(MBTIComponent.E) ? 1.2f : 0.8f;

            float funModifier = 0;
            float fun = v;

            switch (s.relType)
            {
                case CharacterRelation.Type.Friend:
                    funModifier = (attr * 2);
                    // 4. F(감정) : 관계 형성에 대해 더 큰 정서적 만족(Fun)을 느낌
                    if (mbti.CheckComponent(MBTIComponent.F)) funModifier *= 1.25f;

                    fun *= (funModifier * socialEfficiency);
                    ret.Add(CharacterStatus.Fun, fun);
                    ret.Add(CharacterStatus.Loneliness, v * socialEfficiency);
                    break;

                case CharacterRelation.Type.Romance:
                    funModifier = Mathf.Max(0, attr * 2 - 1);
                    if (mbti.CheckComponent(MBTIComponent.F)) funModifier *= 1.4f;

                    ret.Add(CharacterStatus.Fun, funModifier * v * socialEfficiency);
                    ret.Add(CharacterStatus.RLoneliness, v * socialEfficiency);
                    break;
            }

            return ret;
        }

        public CharacterStatusDeltaFactory CalcPersonalizedStatsDeltaOnReceiveStatsDelta(
            CharacterStatusDeltaFactory delta)
        {
            var ret = new CharacterStatusDeltaFactory();
            foreach (var (s, v) in delta.dict ?? new())
            {
                ret.Add(CalcPersonalizedStatsDeltaOnReceiveStatsDelta(s, v));
            }

            return ret;
        }

        public CharacterStatusDeltaFactory CalcPersonalizedStatsDeltaOnReceiveStatsDelta(RelationFloatDict delta)
        {
            var ret = new CharacterStatusDeltaFactory();
            foreach (var (s, v) in delta ?? new())
            {
                ret.Add(CalcPersonalizedStatsDeltaOnReceiveStatsDelta(s, v));
            }

            return ret;
        }
    }
}