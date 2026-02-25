using System;
using Game.Object.Character;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class CharacterNeedsPanel : UIBehaviour
    {
        [SerializeField] private NeedView hungryView;
        [SerializeField] private NeedView fatigueView;
        [SerializeField] private NeedView toiletView;
        [SerializeField] private NeedView motivationNeedView;
        [SerializeField] private NeedView lonelinessView;
        [SerializeField] private NeedView romanceView;
        [SerializeField] private NeedView funView;
        [SerializeField] private NeedView hygieneView;


        protected override void Awake()
        {
            base.Awake();

            GameManager.Instance.OnSetPlayer += (c) => { gameObject.SetActive(c != null); };
        }

        protected override void Start()
        {
            base.Start();

            if (GameManager.Instance.Player == null)
            {
                gameObject.SetActive(false);
                return;
            }

            Draw();
        }

        private void LateUpdate()
        {
            Draw();
        }

        private void Draw()
        {
            hungryView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Hungry),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Hungry]
                );
            
            fatigueView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Fatigue),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Fatigue]
                );
            
            toiletView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Toilet),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Toilet]
                );
            
            motivationNeedView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Motivation),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Motivation]
                );
            
            lonelinessView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Loneliness),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Loneliness]
                );
            
            romanceView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.RLoneliness),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.RLoneliness]
            );
            
            funView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Fun),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Fun]
            );
            
            hygieneView.Set(
                Enum.GetName(typeof(CharacterStatus), CharacterStatus.Hygiene),
                GameManager.Instance.Player.Data[(CharacterStatus)CharacterStatus.Hygiene]
                );
        }
    }
}