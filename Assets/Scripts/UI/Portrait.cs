using System;
using Game.Object.Character;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class Portrait : MonoBehaviour , IPointerClickHandler
    {
        [SerializeField] private Image img;
        [SerializeField] private CharacterData who;

        public Action OnClickHandler = () => { };

        public virtual void Init(CharacterData who)
        {
            this.who = who;
            img.color = who.gender == Gender.Male ? Color.blue : Color.magenta;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickHandler();
        }
    }
}