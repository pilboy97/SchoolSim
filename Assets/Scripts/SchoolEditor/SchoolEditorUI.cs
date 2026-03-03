using System;
using System.Collections.Generic;
using System.Linq;
using Game.Object.Character;
using Game.School;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SchoolEditor
{
    public class SchoolEditorUI : Singleton<SchoolEditorUI>
    {
        [SerializeField] private TMP_InputField schoolNameField;
        [SerializeField] private RectTransform classRoot;
        [SerializeField] private RectTransform studentRoot;

        [SerializeField] private Button backToTitleButton;
        [SerializeField] private Button playButton;

        [SerializeField] private Button addNewStudentButton;

        [SerializeField] private ListElem elemPrefab;
        
        [SerializeField] public Class selectedClass;
        [SerializeField] public CharacterData selectedStudent;

        public Action<string> OnInputSchoolNameHandler = (_) => { };
        public Action<Class> OnSelectClassHandler = (_) => { };
        public Action<CharacterData> OnSelectStudentHandler = (_) => { };
        public Action OnAddNewStudentHandler = () => { };
        public Action OnLoadTitleSceneHandler = () => { };
        public Action OnLoadPlaySceneHandler = () => { };

        private void Awake()
        {
            schoolNameField.onValueChanged.AddListener((str) => OnInputSchoolNameHandler(str));
            
            playButton.onClick.AddListener(() => OnLoadPlaySceneHandler());
            backToTitleButton.onClick.AddListener(()=>OnLoadTitleSceneHandler());
        }

        private void Start()
        {
            OnSelectClassHandler(null);
            OnSelectStudentHandler(null);
        }

        public void SetSchoolName(string schoolName)
        {
            schoolNameField.text = schoolName;
        }

        public void SetClasses(List<Class> classes)
        {
            classRoot.PurgeChild();
            
            if (classes == null) return;

            foreach (var cl in classes)
            {
                var elem = Instantiate(elemPrefab,classRoot);

                elem.cl = cl;
                elem.Init(cl.className);
                elem.OnClick += () =>
                {
                    selectedClass = cl;
                    OnSelectClass(cl);
                    
                };
            }
        }

        public void SetClassFocus()
        {
            foreach (Transform child in classRoot)
            {
                var elem = child.GetComponent<ListElem>();
                
                if (elem == null) continue;
                
                elem.Focus(selectedClass == elem.cl);
            }
        }

        public void SetStudentFocus()
        {
            foreach (Transform child in studentRoot)
            {
                var elem = child.GetComponent<ListElem>();
                
                if (elem == null) continue;
                
                elem.Focus(selectedStudent == elem.ch);
            }
        }

        public void SetStudents(List<CharacterData> characters)
        {
            studentRoot.PurgeChild();

            if (characters == null) return;
            
            var e = Instantiate(addNewStudentButton, studentRoot);
            e.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 150);
            e.onClick.AddListener(()=>OnAddNewStudentHandler());
            
            foreach (var ch in characters)
            {
                var elem = Instantiate(elemPrefab, studentRoot);
                
                elem.ch = ch;
                elem.Init(ch.genData.charName);
                elem.OnClick += () =>
                {
                    selectedStudent = ch;
                    
                    OnSelectStudent(ch);
                };
                elem.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 150);
            }

        }
        
        private void OnSelectClass(Class classData)
        {
            selectedClass = classData;
            OnSelectClassHandler(classData);
            
            SetStudents(SchoolDataController.Instance.data.characters.Where(x=>x.classroom == selectedClass).ToList());
            
            SetClassFocus();
        }
        
        private void OnSelectStudent(CharacterData student)
        {
            selectedStudent = student;
            OnSelectStudentHandler(student);
            
            SetStudentFocus();
        }
    }
}