using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SliderPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelTxt;
        [SerializeField] private TextMeshProUGUI valueTxt;
        [SerializeField] private Slider slider;

        [SerializeField] private string text;
        [SerializeField] private float value;

        public Action<float> OnValueChanged = (_) => { };

        
        public void Init(string txt, float val, float min = 0f, float max = 10f)
        {
            text = txt;
            value = val;
            
            slider.maxValue = max;
            slider.minValue = min;
            slider.value = value;
            
            Set(value);

            labelTxt.text = text;
            valueTxt.text = $"{val:f2}";

            slider.onValueChanged.AddListener(Set);
        }

        public void Set(float x)
        {
            value = x;
            valueTxt.text = $"{value:f2}";

            OnValueChanged(x);
        }
    }
}