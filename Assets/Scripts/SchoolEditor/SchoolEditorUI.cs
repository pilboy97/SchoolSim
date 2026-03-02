using System;
using System.Collections.Generic;
using Game.Object.Character;
using Game.School;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SchoolEditor
{
    public class SchoolEditorUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField schoolNameField;
        [SerializeField] private RectTransform classRoot;
        [SerializeField] private RectTransform studentRoot;

        [SerializeField] private ListElem elemPrefab;

        [SerializeField] private Class selectedClass;
        [SerializeField] private CharacterData selectedCharacter;

        public Action<string> OnInputSchoolNameHandler = (_) => { };

        private void Awake()
        {
            schoolNameField.onValueChanged.AddListener((str) => OnInputSchoolNameHandler(str));
        }

        public void SetSchoolName(string schoolName)
        {
            schoolNameField.text = schoolName;
        }

        public void SetClasses(List<Class> classes)
        {
            classRoot.PurgeChild();

            foreach (var cl in classes)
            {
                var elem = Instantiate(elemPrefab,classRoot);
                
                elem.Init(cl.className);
                elem.OnClick += () =>
                {
                    selectedClass = cl;
                };
            }
        }

        public void SetStudents(List<CharacterData> characters)
        {
            studentRoot.PurgeChild();
            
            foreach (var ch in characters)
            {
                var elem = Instantiate(elemPrefab, studentRoot);
                
                elem.Init(ch.charName);
                elem.OnClick += () =>
                {
                    selectedCharacter = ch;
                };
            }
        }

        public void OnInputSchoolName(string input)
        {
            OnInputSchoolNameHandler(input);
        }
    }
}