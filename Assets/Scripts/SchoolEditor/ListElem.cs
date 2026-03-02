using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.SchoolEditor
{
    public class ListElem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI txt;

        public Action OnClick = () => { };
        public Action OnRightClick = () => { };
        
        public void Init(string str)
        {
            txt.text = str;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right) OnRightClick();
            else OnClick();
        }
    }
}