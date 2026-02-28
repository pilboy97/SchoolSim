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

        public void Set(CharacterStatsType name, float value)
        {
            nameText.text = name.ToString();
            progressBar.value = value;
        }
    }
}