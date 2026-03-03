using Game.Object.Character;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class PortraitWithName : Portrait
    {
        [SerializeField] private TextMeshProUGUI nameText;

        public override void Init(CharacterData who)
        {
            base.Init(who);

            nameText.text = who.charName;
        }
    }
}