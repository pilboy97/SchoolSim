using Game.Time;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class TimeText : UIBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        protected override void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        private void LateUpdate()
        {
            text.text = Calendar.CalendarString(TimeManager.Instance.Ticks);
        }
    }
}