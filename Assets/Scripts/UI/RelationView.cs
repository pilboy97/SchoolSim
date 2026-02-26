using Game.Object;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class RelationView : UIBehaviour
    {
        private Character _target;
        [SerializeField] private string ID;
        [SerializeField] private ProgressBar fbar;
        [SerializeField] private ProgressBar rbar;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI fvalueText;
        [SerializeField] private TextMeshProUGUI rvalueText;
        [SerializeField] private TextMeshProUGUI attractionText;

        public void Init(string id)
        {
            ID = id;
            _target = ObjectManager.Instance.Find(ID) as Character;

            Draw();
        }

        private void LateUpdate()
        {
            Draw();
        }

        private void Draw()
        {
            var player = GameManager.Instance.Player;
            var fvalue = _target.Data[new CharacterRelation()
            {
                relType = CharacterRelation.Type.Friend,
                ID = player.ID
            }];
            var rvalue = _target.Data[new CharacterRelation()
            {
                relType = CharacterRelation.Type.Romance,
                ID = player.ID
            }];

            nameText.text = _target.Data.charName;
            fvalueText.text = $"{fvalue:000} / 100";
            fbar.value = fvalue;

            rvalueText.text = $"{rvalue:000} / 100";
            rbar.value = rvalue;

            attractionText.text = $"{_target.PersonalAttractionFrom(player)}";
        }
    }
}