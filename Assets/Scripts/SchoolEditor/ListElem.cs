using System;
using Game.Object.Character;
using Game.School;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.SchoolEditor
{
    public class ListElem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI txt;
        [SerializeField] private Image background;

        public CharacterData ch;
        public Class cl;

        public Action OnClick = () => { };
        public Action OnRightClick = () => { };
        
        public void SetName(string str)
        {
            txt.text = str;
        }

        public void Focus(bool focused)
        {
            background.color = focused ? Color.skyBlue : Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right) OnRightClick();
            else OnClick();
        }
    }
}