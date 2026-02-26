using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class SkillView : UIBehaviour
    {
        [SerializeField] private CharacterStatus stype;
        [SerializeField] private ProgressBar bar;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI valueText;

        protected override void Awake()
        {
            base.Awake();
            
            Init(stype);
        }

        private void LateUpdate()
        {
            Draw();
        }

        public void Init(CharacterStatus stype)
        {
            this.stype = stype;
            
            Draw();
        }

        private void Draw()
        {
            var value = GameManager.Instance.Player.Data[stype];
            nameText.text = Enum.GetName(typeof(CharacterStatus),stype);
            valueText.text = $"{value:F2} / 100";
            bar.value = value;
        }
    }
}