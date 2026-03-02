using Game.Object;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class RelationView : UIBehaviour, IPointerClickHandler
    {
        private Character who;
        
        [SerializeField] private string ID;
        [SerializeField] private ProgressBar fbar;
        [SerializeField] private ProgressBar rbar;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI fvalueText;
        [SerializeField] private TextMeshProUGUI rvalueText;
        [SerializeField] private Portrait portrait;

        public void Init(string id)
        {
            ID = id;
            who = ObjectManager.Instance.Find(ID) as Character;

            if (who == null) return;
            
            Draw();
        }

        private void LateUpdate()
        {
            Draw();
        }

        private void Draw()
        {
            var target = CharacterInfoPanel.Instance.Target;
            var fvalue = who.Data[new CharacterRelation()
            {
                relType = CharacterRelation.Type.Friend,
                ID = target.ID
            }];
            var rvalue = who.Data[new CharacterRelation()
            {
                relType = CharacterRelation.Type.Romance,
                ID = target.ID
            }];

            nameText.text = who.Data.charName;
            fvalueText.text = $"{fvalue:000} / 100";
            fbar.value = fvalue;

            rvalueText.text = $"{rvalue:000} / 100";
            rbar.value = rvalue;
            
            portrait.Init(who.Data);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CharacterInfoPanel.Instance.Target = who;
        }
    }
}