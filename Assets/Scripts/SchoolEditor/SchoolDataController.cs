using System;
using System.Collections.Generic;
using System.Linq;
using Game.Object.Character;
using Game.School;
using UnityEngine;

namespace Game.SchoolEditor
{
    public class SchoolDataController : Singleton<SchoolDataController>
    {
        [SerializeField] public School.School data;
        [SerializeField] private SchoolEditorUI viewer;
        

        private void Awake()
        {
            data = ConfigData.Instance.schoolData;
        }

        private void Start()
        {
            viewer.SetSchoolName(data.schoolName);
            
            viewer.SetClasses(data.classes);
            viewer.SetStudents(null);
                                              
            viewer.OnInputSchoolNameHandler += OnInputSchoolName;
            viewer.OnAddNewStudentHandler += AddNewStudent;

            viewer.OnLoadPlaySceneHandler += () =>
            {
                SceneLoader.Instance.LoadPlay();
            };
            viewer.OnLoadTitleSceneHandler += () =>
            {
                SceneLoader.Instance.LoadTitle();
            };
        }

        private void SetSchoolName(string name)
        {
            data.schoolName = name;
        }
        
        private void OnInputSchoolName(string input)
        {
            SetSchoolName(input);
        }


        public void AddNewStudent()
        {
            var ch = ScriptableObject.CreateInstance<CharacterData>();
            var genData = new CharacterGenData();
            
            genData.Init();

            ch.genData = genData;
            ch.classroom = viewer.selectedClass;
            
            data.characters.Add(ch);
            viewer.selectedStudent = ch;

            viewer.OnSelectStudentHandler(ch);
            
            viewer.SetStudents(data.characters.Where(x=>x.classroom == viewer.selectedClass).ToList());
            viewer.SetStudentFocus();
        }
    }
}