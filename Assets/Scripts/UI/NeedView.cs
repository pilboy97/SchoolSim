using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class NeedView : UIBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private ProgressBar progressBar;

        public void Set(string _name, float value)
        {
            nameText.text = _name ?? "???";
            progressBar.value = value;
        }
    }
}