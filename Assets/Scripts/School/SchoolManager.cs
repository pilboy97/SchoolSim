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
            foreach (var school in schools)
            {
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
            }
        }
    }
}