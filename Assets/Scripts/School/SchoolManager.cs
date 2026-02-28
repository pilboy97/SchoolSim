using System.Collections.Generic;
using Game.Object;
using Game.Object.Character;
using Game.Room;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Game.School
{
    public class SchoolManager : Singleton<SchoolManager>
    {
        [SerializeField] private Character characterPrefab;
        [SerializeField] public List<School> schools = new();

        public void Init()
        {
            foreach (var s in schools)
            {
                var school = Instantiate(s);
                school.Init();

                for (int i = 0; i < s.randomGenCharacter; i++)
                {
                    var character = Instantiate(characterPrefab);
                    var data = ScriptableObject.CreateInstance<CharacterData>();
                    data.Init();
                    data.classroom = Random.Choose(school.classes);
                    
                    character.Init(data);
                    character.CPosition = NavManager.Instance.RandomCPos;
                    
                    s.Enter(character);
                    
                    ObjectManager.Instance.Add(character);
                }

                foreach (var d in school.characters)
                {
                    var data = Instantiate(d);
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
}