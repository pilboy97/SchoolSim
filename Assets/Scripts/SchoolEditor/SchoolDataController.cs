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
            data = Game.ConfigData.Instance.schoolData;
            
            if (data == null) return;
            
            var d = ScriptableObject.CreateInstance<School.School>();
            d.schoolName = data.schoolName;
            
            d.grp = new List<CharacterData>();
            
            d.maps = data.maps.ToList();
            d.timeTable = data.timeTable.ToList();
            
            d.characters = new List<CharacterData>();
            
            foreach (var ch in data.characters)
            {
                var c = ScriptableObject.CreateInstance<CharacterData>();
                c.genData = ch.genData;
                
                c.genData.rivals = new List<CharacterData>();
                c.genData.friends = new List<CharacterData>();
                
                c.Init();
                
                d.characters.Add(c);
            }

            d.classes = new List<Class>();
            foreach (var cl in data.classes)
            {
                var c = ScriptableObject.CreateInstance<Class>();

                c.map = cl.map;
                c.className = cl.className;
                c.grp = new List<CharacterData>();

                foreach (var ch in d.characters)
                {
                    ch.classroom = c;
                }
                
                d.classes.Add(c);
            }
            
            d.grp.Clear();

            data = d;
            Game.ConfigData.Instance.schoolData = data;
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