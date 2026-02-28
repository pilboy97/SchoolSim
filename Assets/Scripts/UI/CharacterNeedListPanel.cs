using System;
using Game.Object.Character;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class CharacterNeedListPanel : UIBehaviour
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
                CharacterStatsType.Hungry,
                GameManager.Instance.Player.Data[CharacterStatsType.Hungry]
            );

            fatigueView.Set(CharacterStatsType.Fatigue,
                GameManager.Instance.Player.Data[CharacterStatsType.Fatigue]
            );

            toiletView.Set(CharacterStatsType.Toilet,
                GameManager.Instance.Player.Data[CharacterStatsType.Toilet]
            );

            motivationNeedView.Set(
                CharacterStatsType.Motivation,
                GameManager.Instance.Player.Data[CharacterStatsType.Motivation]
            );

            lonelinessView.Set(
                CharacterStatsType.Loneliness,
                GameManager.Instance.Player.Data[CharacterStatsType.Loneliness]
            );

            romanceView.Set(
                CharacterStatsType.RLoneliness,
                GameManager.Instance.Player.Data[CharacterStatsType.RLoneliness]
            );

            funView.Set(
                CharacterStatsType.Fun,
                GameManager.Instance.Player.Data[CharacterStatsType.Fun]
            );

            hygieneView.Set(
                CharacterStatsType.Hygiene,
                GameManager.Instance.Player.Data[CharacterStatsType.Hygiene]
            );
        }
    }
}