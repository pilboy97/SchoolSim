using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Event.Talk
{
    public class ParticipantPanel : MonoBehaviour
    {
        [SerializeField] private TalkEvent talkEvent;
        [SerializeField] private Character character;
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI topicText;

        public string CharID => character.ID;
        
        public void Init(TalkEvent talkEvent, Character character)
        {
            this.talkEvent = talkEvent;
            this.character = character;

            nameText.text = character.Name;
        }

        private void LateUpdate()
        {
            if (talkEvent == null) return; 
            talkEvent.selected.TryAdd(character.ID, TalkEvent.Topics[0]);
            
            topicText.text = talkEvent.selected[character.ID].ToString();
        }
    }
}