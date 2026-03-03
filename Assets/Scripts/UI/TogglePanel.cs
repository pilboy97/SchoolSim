using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TogglePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelTxt;
        [SerializeField] private Toggle toggle;

        [SerializeField] private string text;
        [SerializeField] private bool value;

        public Action<bool> OnValueChanged = (_) => { };

        public void Init(string txt, bool value = false)
        {
            labelTxt.text = txt;
            toggle.isOn = value;
            
            toggle.onValueChanged.AddListener(Set);
        }

        public void Set(bool x)
        {
            value = x;

            OnValueChanged(x);
        }
    }
}