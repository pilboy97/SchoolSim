using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class NeedView : UIBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private DirectionIcons direction;

        private float _prev = 0;
        
        public void Set(CharacterStatsType name, float value)
        {
            nameText.text = name.ToString();
            progressBar.value = value;

            if (value.CompareTo(_prev) > 0)
                direction.status = DirectionIcons.Status.Increase;
            else if (value.CompareTo(_prev) < 0)
                direction.status = DirectionIcons.Status.Decrease;
            else
                direction.status = DirectionIcons.Status.None;

            _prev = value;
        }
    }
}