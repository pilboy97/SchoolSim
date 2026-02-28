using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class CharacterInfoPanel : Singleton<CharacterInfoPanel>
    {
        [SerializeField] private Character _target;
        [SerializeField] private CharacterMoreInfoPanel moreInfo;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI clubText;
        [SerializeField] private TextMeshProUGUI MBTIText;
        [SerializeField] private Button returnToPlayerBtn;

        public Action OnChangeTarget = () => { };

        public Character Target
        {
            get => _target;
            set
            {
                _target = value;
                
                OnChangeTarget.Invoke();
            }
        }

        protected void Awake()
        {
            GameManager.Instance.OnSetPlayer += (c) => {
                gameObject.SetActive(c != null);
                if (c != null) _target = c;
                
                Init(c);
            };
            
            moreInfo.gameObject.SetActive(false); 
        }

        public void Init(Character who)
        {
            _target = who;
            
            if (Target == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            nameText.text = Target.charName;
            MBTIText.text = Target.Data.mbti.ToString();
        }
        
        
        private void LateUpdate()
        {
            nameText.text = Target.charName;
            MBTIText.text = Target.Data.mbti.ToString();
            
            returnToPlayerBtn.gameObject.SetActive(Target.ID != GameManager.Instance.Player.ID);
        }

        private void OnClick()
        {
            moreInfo.ToggleShow();
        }

        public void ReturnToPlayer()
        {
            Target = GameManager.Instance.Player;
        }
    }
}