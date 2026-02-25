using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    
    [RequireComponent(typeof(Image))]
    public class BorderSpriteImage : UIBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] public BorderSpriteStyle style;
        [SerializeField] public BorderDirection border;

        public BorderDirection Direction
        {
            get => border;
            set
            {
                border = value;
                Draw();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            Draw();
        }

        protected void Draw()
        {
            image.sprite = style.Select(border);
        }
    }
}