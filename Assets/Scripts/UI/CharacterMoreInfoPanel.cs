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

        protected override void Awake()
        {
            base.Awake();
            
            CharacterInfoPanel.Instance.OnChangeTarget += Draw;
        }

        protected override void Start()
        {
            base.Start();

            if (CharacterInfoPanel.Instance.Target == null)
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
            for(var skill = CharacterStatsType.SkillBegin + 1;skill < CharacterStatsType.SkillEnd;skill++)
            {
                var view = Instantiate(skillViewPrefab, content);
                view.Init(skill);
            }
        }

        private void DrawRelation()
        {
            var characters = ObjectManager.Instance.Characters;
            foreach (var target in characters)
            {
                if (target.ID == CharacterInfoPanel.Instance.Target.ID) continue;
                
                var view = Instantiate(relationViewPrefab, content);
                view.Init(target.ID);
            }
        }

        private void DrawGrade()
        {
            for(var subject = CharacterStatsType.SubjectBegin + 1;subject < CharacterStatsType.SubjectEnd;subject++)
            {
                var view = Instantiate(skillViewPrefab, content);
                view.Init(subject);
            }
        }
    }
}