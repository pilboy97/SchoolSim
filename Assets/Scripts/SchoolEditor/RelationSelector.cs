using System;
using System.Collections.Generic;
using Game.Object.Character;
using Game.UI;
using UnityEngine;

namespace Game.SchoolEditor
{
    public class RelationSelector : MonoBehaviour
    {
        [SerializeField] private PortraitWithName portraitPrefab;
        
        [SerializeField] private List<CharacterData> candidate = new();
        [SerializeField] private List<CharacterData> selected = new();

        [SerializeField] private CharacterData target;
        
        [SerializeField] private RectTransform selRoot;
        [SerializeField] private RectTransform canRoot;

        public List<CharacterData> Candidate => candidate;

        public List<CharacterData> Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnValueChangedHandler();
            }
        }

        public Action OnValueChangedHandler = () => { };
        
        public void Init(CharacterData target, List<CharacterData> c, List<CharacterData> s)
        {
            this.target = target;
            candidate = c;
            selected = s;
            
            Draw();
        }

        public void Draw()
        {
            canRoot.PurgeChild();
            foreach (var ch in candidate)
            {
                if (target == ch) continue;
                
                var p = Instantiate(portraitPrefab, canRoot);
                p.Init(ch);
                p.OnClickHandler = () =>
                {
                    Add(ch);
                };
            }
            
            selRoot.PurgeChild();
            foreach (var ch in selected)
            {
                if (target == ch) continue;

                var p = Instantiate(portraitPrefab, selRoot);
                p.Init(ch);
                p.OnClickHandler = () =>
                {
                    Remove(ch);
                };
            }
        }

        public void Add(CharacterData x)
        {
            selected.Add(x);
            Draw();
            
            OnValueChangedHandler();
        }

        public void Remove(CharacterData x)
        {
            selected.Remove(x);
            Draw();
            
            OnValueChangedHandler();
        }
    }
}