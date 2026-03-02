using Game.Object.Character;
using Game.School;
using UnityEngine;

namespace Game.SchoolEditor
{
    public class SchoolDataController : Singleton<SchoolDataController>
    {
        [SerializeField] private School.School data;
        [SerializeField] private SchoolEditorUI viewer;

        private void Start()
        {
            viewer.SetSchoolName(data.schoolName);
            viewer.OnInputSchoolNameHandler += OnInputSchoolName;
            
            viewer.SetClasses(data.classes);
            viewer.SetStudents(data.characters);
        }

        private void SetSchoolName(string name)
        {
            data.schoolName = name;
        }
        
        private void OnInputSchoolName(string input)
        {
            SetSchoolName(input);
        }

        private void OnSelectClass(Class classData)
        {
            
        }
        
        private void OnSelectStudent(CharacterData classData)
        {
            
        }
    }
}