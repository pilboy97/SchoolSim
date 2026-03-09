using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ConfigPanel : MonoBehaviour
    {
        [SerializeField] private Button openBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private RectTransform root;
        
        [SerializeField] private TogglePanel debugMode;
        [SerializeField] private SliderPanel statsDecay;
        [SerializeField] private SliderPanel needsDecayMod;
        [SerializeField] private SliderPanel subjectDecayMod;
        [SerializeField] private SliderPanel skillDecayMod;
        [SerializeField] private SliderPanel intDecayMod;
        [SerializeField] private SliderPanel relationDecayMod;
        [SerializeField] private SliderPanel inertia;
        [SerializeField] private SliderPanel E_Needs;
        [SerializeField] private SliderPanel R_Needs;
        [SerializeField] private SliderPanel G_Needs;
        [SerializeField] private SliderPanel I_E;
        [SerializeField] private SliderPanel N_S;
        [SerializeField] private SliderPanel F_T;
        [SerializeField] private SliderPanel P_J;
        [SerializeField] private SliderPanel talk_baseFun;
        [SerializeField] private SliderPanel talk_baseLoneliness;
        [SerializeField] private SliderPanel talk_baseRLoneliness;
        [SerializeField] private SliderPanel talk_baseMotivation;
        [SerializeField] private SliderPanel talk_baseInfluence;
        [SerializeField] private SliderPanel talk_baseTeach;

        private void Awake()
        {
            closeBtn.onClick.AddListener(() =>
            {
                root.gameObject.SetActive(false);
                openBtn.gameObject.SetActive(true);
            });
            openBtn.onClick.AddListener(() =>
            {
                root.gameObject.SetActive(true);
                openBtn.gameObject.SetActive(false);
            });
            
            closeBtn.onClick.Invoke();
        }

        private void Start()
        {
            debugMode.Init("Debug", ConfigData.Instance.isLogEnabled);
            debugMode.OnValueChanged += b => ConfigData.Instance.isLogEnabled = b;
            
            statsDecay.Init("decay of stats", ConfigData.Instance.statsDecay);
            statsDecay.OnValueChanged += x => ConfigData.Instance.statsDecay = Mathf.Exp(x);
            
            intDecayMod.Init("decay modifier of intelligence stats", ConfigData.Instance.intDecayMod);
            intDecayMod.OnValueChanged += x => ConfigData.Instance.intDecayMod = Mathf.Exp(x);
            skillDecayMod.Init("decay modifier of skill stats", ConfigData.Instance.skillDecayMod);
            skillDecayMod.OnValueChanged += x => ConfigData.Instance.skillDecayMod = Mathf.Exp(x);
            subjectDecayMod.Init("decay modifier of subject stats", ConfigData.Instance.subjectDecayMod);
            subjectDecayMod.OnValueChanged += x => ConfigData.Instance.subjectDecayMod = Mathf.Exp(x);
            needsDecayMod.Init("decay modifier of need stats", ConfigData.Instance.needsDecayMod);
            needsDecayMod.OnValueChanged += x => ConfigData.Instance.needsDecayMod = Mathf.Exp(x);
            relationDecayMod.Init("decay modifier of relation", ConfigData.Instance.relationDecayMod);
            relationDecayMod.OnValueChanged += x => ConfigData.Instance.relationDecayMod = Mathf.Exp(x);
            
            inertia.Init("Inertia", ConfigData.Instance.inertia);
            inertia.OnValueChanged += x => ConfigData.Instance.inertia = Mathf.Exp(x);
            
            E_Needs.Init("E Needs", ConfigData.Instance.eModifier);
            E_Needs.OnValueChanged += x => ConfigData.Instance.eModifier = Mathf.Exp(x);
            R_Needs.Init("R Needs", ConfigData.Instance.rModifier);
            R_Needs.OnValueChanged += x => ConfigData.Instance.rModifier = Mathf.Exp(x);
            G_Needs.Init("G Needs", ConfigData.Instance.gModifier);
            G_Needs.OnValueChanged += x => ConfigData.Instance.gModifier = Mathf.Exp(x);
            
            I_E.Init("I_E", ConfigData.Instance.I_E_modifier);
            I_E.OnValueChanged += x => ConfigData.Instance.I_E_modifier = Mathf.Exp(x);
            N_S.Init("N_S", ConfigData.Instance.N_S_modifier);
            N_S.OnValueChanged += x => ConfigData.Instance.N_S_modifier = Mathf.Exp(x);
            F_T.Init("F_T", ConfigData.Instance.F_T_modifier);
            F_T.OnValueChanged += x => ConfigData.Instance.F_T_modifier = Mathf.Exp(x);
            P_J.Init("P_J", ConfigData.Instance.P_J_modifier);
            P_J.OnValueChanged += x => ConfigData.Instance.P_J_modifier = Mathf.Exp(x);
            
            talk_baseFun.Init("talk_base_fun", ConfigData.Instance.talk_baseFun);
            talk_baseFun.OnValueChanged += x => ConfigData.Instance.talk_baseFun = Mathf.Exp(x);
            talk_baseLoneliness.Init("talk_base_loneliness", ConfigData.Instance.talk_baseLoneliness);
            talk_baseLoneliness.OnValueChanged += x => ConfigData.Instance.talk_baseLoneliness = Mathf.Exp(x);
            talk_baseRLoneliness.Init("talk_base_RLoneliness", ConfigData.Instance.talk_baseRLoneliness);
            talk_baseRLoneliness.OnValueChanged += x => ConfigData.Instance.talk_baseRLoneliness = Mathf.Exp(x);
            talk_baseMotivation.Init("talk_base_motivation", ConfigData.Instance.talk_baseMotivation);
            talk_baseMotivation.OnValueChanged += x => ConfigData.Instance.talk_baseMotivation = Mathf.Exp(x);
            talk_baseTeach.Init("talk_base_teach", ConfigData.Instance.talk_baseTeach);
            talk_baseTeach.OnValueChanged += x => ConfigData.Instance.talk_baseTeach = Mathf.Exp(x);
            talk_baseInfluence.Init("talk_base_influence", ConfigData.Instance.talk_baseInfluence);
            talk_baseInfluence.OnValueChanged += x => ConfigData.Instance.talk_baseInfluence = Mathf.Exp(x);
        }
    }
}