using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class SkillView : UIBehaviour
    {
        [SerializeField] private CharacterStatsType statsType;
        [SerializeField] private ProgressBar bar;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private DirectionIcons direction;

        private float _prev;

        protected override void Awake()
        {
            base.Awake();

            Init(statsType);
        }

        private void LateUpdate()
        {
            Draw();
        }

        public void Init(CharacterStatsType statsType)
        {
            this.statsType = statsType;

            Draw();
        }

        private void Draw()
        {
            var value = CharacterInfoPanel.Instance.Target.Data[statsType];

            nameText.text = Enum.GetName(typeof(CharacterStatsType), statsType);
            valueText.text = $"{value:F2} / 100";
            bar.value = value;

            if (value > _prev) direction.status = DirectionIcons.Status.Increase;
            else if (value < _prev) direction.status = DirectionIcons.Status.Decrease;
            else direction.status = DirectionIcons.Status.None;

            _prev = value;
        }
    }
}