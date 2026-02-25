using Game.Object;
using Game.Object.Character;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class CharacterMoreInfoPanel : UIBehaviour
    {
        public enum Mode
        {
            Skill,
            Relation,
            Grade
        }

        [SerializeField] public Mode mode = Mode.Skill;

        [SerializeField] private GradeView gradeViewPrefab;
        [SerializeField] private RelationView relationViewPrefab;
        [SerializeField] private SkillView skillViewPrefab;

        [SerializeField] private Transform content;

        public void ToggleShow()
        {
            var e = gameObject.activeSelf;

            gameObject.SetActive(!e);
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

        public void Draw()
        {
            content.PurgeChild();
            
            switch (mode)
            {
                case Mode.Skill:
                    DrawSkill();
                    break;
                case Mode.Grade:
                    DrawGrade();
                    break;
                case Mode.Relation:
                    DrawRelation();
                    break;
            }
        }

        public void SetSkill()
        {
            mode = Mode.Skill;
            Draw();
        }

        public void SetRelation()
        {
            mode = Mode.Relation;
            Draw();
        }

        public void SetGrade()
        {
            mode = Mode.Grade;
            Draw();
        }

        private void DrawSkill()
        {
            for (var it = CharacterStatus.SkillBegin + 1; it < CharacterStatus.SkillEnd; it++)
            {
                var view = Instantiate(skillViewPrefab, content);
                view.Init(it);
            }
        }

        private void DrawRelation()
        {
            var characters = ObjectManager.Instance.Characters;
            foreach (var target in characters)
            {
                if (target.ID == GameManager.Instance.Player.ID) continue;
                
                var view = Instantiate(relationViewPrefab, content);
                view.Init(target.ID);
            }
        }

        private void DrawGrade()
        {
            for (var it = CharacterStatus.SubjectBegin + 1; it < CharacterStatus.SubjectEnd; it++)
            {
                var view = Instantiate(skillViewPrefab, content);
                view.Init(it);
            }
        }
    }
}