using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Game.Object.Character;
using Game.UI;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Event.Talk
{
    public class TalkWindow : Singleton<TalkWindow>
    {
        [SerializeField] private TalkEvent target;
        public bool isChoose = false;
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform participant;
        [SerializeField] private Window window;
        [SerializeField] public int select = 0;
        private List<Topic> Topics => TalkEvent.Topics;
        [SerializeField] private TopicButton topicButtonPrefab;
        [SerializeField] private ParticipantPanel participantPanelPrefab;
        [SerializeField] private ProgressBar timer;
        [SerializeField] private TalkShareGraph graph;

        private ObjectPool<ParticipantPanel> _participantPanelPool;
        
        public string EventID => target?.ID ?? "";
        public Topic Selected => Topics[select];

        public void Awake()
        {
            _participantPanelPool = new ObjectPool<ParticipantPanel>(
                createFunc: () =>
                {
                    var panel = Instantiate(participantPanelPrefab, GameManager.TEMP);
                    panel.gameObject.SetActive(false);
                    return panel;
                },
                actionOnGet: panel =>
                {
                    panel.gameObject.SetActive(true);
                    panel.transform.SetParent(participant);
                },
                actionOnRelease: panel =>
                {
                    panel.gameObject.SetActive(false);
                    panel.transform.SetParent(GameManager.TEMP);
                }
            );
        }

        public void Init()
        {
            TalkEvent.StaticInit();
                
            for (var i = 0; i < Topics.Count; i++)
            {
                var btn = Instantiate(topicButtonPrefab, content);
                btn.Init(i, Topics[i]);
            }
            
            Instance.Close();
        }

        private void Update()
        {
            List<ParticipantPanel> childs = new List<ParticipantPanel>();

            foreach (Transform transform in participant)
            {
                var panel = transform.GetComponent<ParticipantPanel>();
                childs.Add(panel);
            }

            foreach (var panel in childs)
            {
                _participantPanelPool.Release(panel);
            }
            
            foreach (var ch in target.members)
            {
                var panel = _participantPanelPool.Get();
                panel.Init(target, ch);
            }
            
            timer.value = target.curTime;
        }

        private void LateUpdate()
        {
            var player = GameManager.Instance.Player;
            target = player.CurEvent as TalkEvent;
            if (target is { Status: EventStatus.Run })
            {
                var selected = target.selected[player.ID];

                int index = 0;
                for (index = 0; index < TalkEvent.Topics.Count; index++)
                {
                    var topic = TalkEvent.Topics[index];
                    if (selected.type != topic.type) continue;

                    if (selected.type == Topic.Type.General || selected.type == Topic.Type.Romance) break;
                    if (selected.type == Topic.Type.Teach && selected.knowledge == topic.knowledge) break;
                    if (
                        (selected.type == Topic.Type.RelationUp ||
                            selected.type == Topic.Type.RelationDown) &&
                        selected.target == topic.target
                        ) break;
                }

                select = index;
                
                return;
            }

            Close();
        }

        public void Init(TalkEvent e)
        {
            target = e;
            if (target == null) return;
            
            timer.min = 0;
            timer.max = target.selectTime;

            isChoose = false;
            
            graph.Init(target);
            
            window.Open();
        }

        public void Select(int x)
        {
            select = x;
            
            var topic = TalkEvent.Topics[x];
            topic.speaker = GameManager.Instance.Player;
            target.selected[GameManager.Instance.Player.ID] = topic;
            
            isChoose = true;
        }

        public void Close()
        {
            window?.Close();
        }
    }
}