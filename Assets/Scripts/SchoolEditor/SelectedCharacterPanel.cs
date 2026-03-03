using System;
using Game.Object.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SchoolEditor
{
    public class SelectedCharacterPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Slider genderSlider;
        [SerializeField] private Slider attractionLevelSlider;
        [SerializeField] private RelationSelector friendSelector;
        [SerializeField] private RelationSelector rivalSelector;
        [SerializeField] private Slider MBTI_I_E_Slider;
        [SerializeField] private Slider MBTI_N_S_Slider;
        [SerializeField] private Slider MBTI_F_T_Slider;
        [SerializeField] private Slider MBTI_P_J_Slider;

        private CharacterData target;

        private void Awake()
        {
            SchoolEditorUI.Instance.OnSelectStudentHandler += Init;
            
            nameInput.onValueChanged.AddListener(_ => OnSetName());
            
            genderSlider.onValueChanged.AddListener(_ => OnSetGender());
            
            attractionLevelSlider.onValueChanged.AddListener(_ => OnSetAttr());
            
            MBTI_I_E_Slider.onValueChanged.AddListener(_ => OnSetMBTI_I_E());
            MBTI_N_S_Slider.onValueChanged.AddListener(_ => OnSetMBTI_N_S());
            MBTI_F_T_Slider.onValueChanged.AddListener(_ => OnSetMBTI_F_T());
            MBTI_P_J_Slider.onValueChanged.AddListener(_ => OnSetMBTI_P_J());
            
            friendSelector.OnValueChangedHandler += OnSetFriends;
            rivalSelector.OnValueChangedHandler += OnSetRivals;
        }

        public void Init(CharacterData character)
        {
            target = character;
            if (character == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            nameInput.text = character.genData.charName;
            genderSlider.value = (character.genData.gender == Gender.Male) ? 0 : 1;
            attractionLevelSlider.value = character.genData.attractionLevel;

            int I_E = 0;
            int N_S = 0;
            int F_T = 0;
            int P_J = 0;

            foreach (var cond in character.genData.mbtiCond)
            {
                switch (cond)
                {
                    case MBTIComponent.E:
                        I_E = 1;
                        break;
                    case MBTIComponent.I:
                        I_E = -1;
                        break;
                    case MBTIComponent.S:
                        N_S = 1;
                        break;
                    case MBTIComponent.N:
                        N_S = 1;
                        break;
                    case MBTIComponent.T:
                        F_T = 1;
                        break;
                    case MBTIComponent.F:
                        F_T = -1;
                        break;
                    case MBTIComponent.J:
                        P_J = 1;
                        break;
                    case MBTIComponent.P:
                        P_J = -1;
                        break;
                }

                MBTI_I_E_Slider.value = I_E;
                MBTI_N_S_Slider.value = N_S;
                MBTI_F_T_Slider.value = F_T;
                MBTI_P_J_Slider.value = P_J;
            }

            friendSelector.Init(character,SchoolDataController.Instance.data.characters, character.genData.friends);
            rivalSelector.Init(character,SchoolDataController.Instance.data.characters, character.genData.rivals);
        }

        public void OnSet()
        {
            target?.Init();
        }

        public void OnSetName()
        {
            target.genData.charName = nameInput.text;
            OnSet();
        }

        public void OnSetGender()
        {
            target.genData.gender = (genderSlider.value == 0) ? Gender.Male : Gender.Female;
            OnSet();
        }

        public void OnSetAttr()
        {
            target.genData.attractionLevel = (int)(attractionLevelSlider.value + 0.01f);
            OnSet();
        }

        public void OnSetMBTI_I_E()
        {
            if (!Mathf.Approximately(MBTI_I_E_Slider.value, 0))
                target.genData.mbtiCond[0] = (MBTI_I_E_Slider.value < 0) ? MBTIComponent.I : MBTIComponent.E;
            else
                target.genData.mbtiCond[0] = MBTIComponent.None;
            OnSet();
        }
        public void OnSetMBTI_N_S()
        {
            if (!Mathf.Approximately(MBTI_N_S_Slider.value, 0))
                target.genData.mbtiCond[1] = (MBTI_N_S_Slider.value < 0) ? MBTIComponent.N : MBTIComponent.S;
            else
                target.genData.mbtiCond[1] = MBTIComponent.None;
            OnSet();
        }
        public void OnSetMBTI_F_T()
        {
            if (!Mathf.Approximately(MBTI_F_T_Slider.value, 0))
                target.genData.mbtiCond[2] = (MBTI_F_T_Slider.value < 0) ? MBTIComponent.F : MBTIComponent.T;
            else
                target.genData.mbtiCond[2] = MBTIComponent.None;
            OnSet();
        }
        public void OnSetMBTI_P_J()
        {
            if (!Mathf.Approximately(MBTI_P_J_Slider.value, 0))
                target.genData.mbtiCond[3] = (MBTI_P_J_Slider.value < 0) ? MBTIComponent.P : MBTIComponent.J;
            else
                target.genData.mbtiCond[3] = MBTIComponent.None;
            OnSet();
        }

        public void OnSetFriends()
        {
            target.genData.friends = friendSelector.Selected;
            OnSet();
        }

        public void OnSetRivals()
        {
            target.genData.rivals = rivalSelector.Selected;
            OnSet();
        }
    }
}