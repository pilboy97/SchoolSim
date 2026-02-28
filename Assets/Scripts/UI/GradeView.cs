using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class GradeView : UIBehaviour
    {
        [SerializeField] private CharacterStatsType stype;
        [SerializeField] private ProgressBar bar;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI valueText;

        protected override void Awake()
        {
            base.Awake();
            
            Init(stype);
        }

        public void Init(CharacterStatsType stype)
        {
            this.stype = stype;
            
            Draw();
        }
        
        private void LateUpdate()
        {
            Draw();
        }

        private void Draw()
        {
            var value = GameManager.Instance.Player.Data[stype];
            nameText.text = Enum.GetName(typeof(CharacterStats),stype);
            valueText.text = $"{value:F2} / 100";
            bar.value = value;
        }
    }
}