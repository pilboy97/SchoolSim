using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class ProgressBar : UIBehaviour
    {
        [SerializeField] public float value;
        [SerializeField] public float min;
        [SerializeField] public float max = 100f;

        [SerializeField] protected Image background;
        [SerializeField] protected Image foreground;
        [SerializeField] protected float[] colorPivot;
        [SerializeField] protected Color[] colors;

        protected virtual void LateUpdate()
        {
            var w = background.rectTransform.rect.width;
            var p = GetRatio();
            
            Draw(w, p);
        }

        protected void Draw(float w, float p)
        {
            var size = foreground.rectTransform.sizeDelta;

            foreground.rectTransform.sizeDelta = new Vector2(p * w, size.y);
            foreground.color = GetColor();
        }

        protected float GetRatio()
        {
            return (value - min) / (max - min);
        }

        protected Color GetColor()
        {
            if (colorPivot == null || colorPivot.Length == 0) return Color.green;

            var c = colors[0];
            for (var i = 0; i < colorPivot.Length; i++)
            {
                if (colorPivot[i] > value) break;
                if (i + 1 >= colors.Length) break;

                c = colors[i + 1];
            }

            return c;
        }
    }
}