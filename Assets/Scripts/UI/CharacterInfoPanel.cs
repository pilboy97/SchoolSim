using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class CharacterInfoPanel : UIBehaviour
    {
        [SerializeField] private Object.Character.Character target;
        [SerializeField] private CharacterMoreInfoPanel moreInfo;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI clubText;
        [SerializeField] private TextMeshProUGUI MBTIText;

        protected override void Awake()
        {
            base.Awake();

            GameManager.Instance.OnSetPlayer += (c) => {
                gameObject.SetActive(c != null);
                if (c != null) target = c;
                
                Init(c);
            };
            
            moreInfo.gameObject.SetActive(false); 
        }

        public void Init(Character who)
        {
            target = who;
            
            if (target == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            nameText.text = target.Name;
            MBTIText.text = target.Data.mbti.ToString();
        }

        private void OnClick()
        {
            moreInfo.ToggleShow();
        }
    }
}