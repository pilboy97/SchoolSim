using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Event.Talk
{
    public class TopicButton : MonoBehaviour
    {
        [SerializeField] private int idx;
        [SerializeField] private Button btn;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Topic topic;
        [SerializeField] private Image background;

        public void Init(int idx, Topic topic)
        {
            this.idx = idx;
            this.topic = topic;
            text.text = topic.ToString();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                TalkWindow.Instance.Select(idx);
            });
        }

        private void LateUpdate()
        {
            background.color = new Color(0, 0, 0, 0);
            if (TalkWindow.Instance.select == idx)
            {
                background.color = Color.black;
            }
        }
    }
}