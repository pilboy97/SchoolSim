using System.Collections.Generic;
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
        }

        private void LateUpdate()
        {
            if (talkEvent == null) return; 
            talkEvent.selected.TryAdd(character.ID, TalkEvent.Topics[0]);

            var mod = talkEvent.shareOfInfluence.GetValueOrDefault(character.ID);
            mod /= talkEvent.DesiredShare;
            
            nameText.text = $"{character.Name} x{mod:F2}";
            topicText.text = talkEvent.selected[character.ID].ToString();
        }
    }
}