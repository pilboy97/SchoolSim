using Game.Object;
using Game.Object.Character;
using UnityEngine;

namespace Game.School
{
    public class SchoolManager : Singleton<SchoolManager>
    {
        [SerializeField] private Character characterPrefab;
        [SerializeField] private School school;

        public School School => school;

        public void Init()
        {
            school = ConfigData.Instance.schoolData;
            
            school.Init();

            for (int i = 0; i < school.randomGenCharacter; i++)
            {
                var character = Instantiate(characterPrefab);
                var data = ScriptableObject.CreateInstance<CharacterData>();
                data.Init();
                data.classroom = Random.Choose(school.classes);

                character.Init(data);
                character.CPosition = NavManager.Instance.RandomCPos;

                school.Enter(character);

                ObjectManager.Instance.Add(character);
            }

            foreach (var data in school.characters)
            {
                data.GenerateCharacter();

                var character = Instantiate(characterPrefab);
                character.Init(data);
                character.CPosition = NavManager.Instance.RandomCPos;

                school.Enter(character);

                ObjectManager.Instance.Add(character);
            }

            foreach (var ch in school.grp)
            {
                ch.relations ??= new RelationFloatDict();

                foreach (var ch2 in school.grp)
                {
                    ch.relations.TryAdd(new CharacterRelation()
                    {
                        ID = ch2.ID,
                        relType = CharacterRelation.Type.Friend
                    }, 0);
                    ch.relations.TryAdd(new CharacterRelation()
                    {
                        ID = ch2.ID,
                        relType = CharacterRelation.Type.Romance
                    }, 0);
                }
            }

        }
    }
}