using System;
using UnityEngine;

namespace Game.UI
{
    public class ProgressBarWithNegative : ProgressBar
    {
        [SerializeField] private float zero = 0;
        [SerializeField] private bool isPositive;

        protected override void LateUpdate()
        {
            var w = max - min;
            var width = background.rectTransform.rect.width;
            
            if (value >= zero)
            {
                var pZero = (zero - min) / w;
                var p = (value - zero) / w;
                
                foreground.rectTransform.anchorMin = new Vector2(pZero, 0);
                foreground.rectTransform.anchorMax = new Vector2(pZero, 1);

                foreground.rectTransform.pivot = new Vector2(1, 0);

                foreground.rectTransform.anchoredPosition = new Vector2(p * width, 0);
                foreground.rectTransform.sizeDelta = new Vector2(p * width, 1);
            }
            else
            {
                var pZero = (zero - min) / w;
                var p = (value - zero) / w;
                
                foreground.rectTransform.anchorMin = new Vector2(pZero, 0);
                foreground.rectTransform.anchorMax = new Vector2(pZero, 1);

                foreground.rectTransform.pivot = new Vector2(0, 0);

                foreground.rectTransform.anchoredPosition = new Vector2(p * width, 0);
                foreground.rectTransform.sizeDelta = new Vector2(-p * width, 1);
            }

            foreground.color = GetColor();
        }
    }
}