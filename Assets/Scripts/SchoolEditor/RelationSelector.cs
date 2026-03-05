using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Game.Object.Character;
using Game.UI;
using UnityEngine;

namespace Game.SchoolEditor
{
    public class RelationSelector : MonoBehaviour
    {
        [SerializeField] private PortraitWithName portraitPrefab;

        private HashSet<CharacterData> _candidate = new();
        private HashSet<CharacterData> _selected = new();

        [SerializeField] private CharacterData target;

        [SerializeField] private RectTransform selRoot;
        [SerializeField] private RectTransform canRoot;

        public HashSet<CharacterData> Candidate => _candidate;
        public HashSet<CharacterData> Selected => _selected;

        public Action OnValueChangedHandler = () => { };

    public void Init(CharacterData target, HashSet<CharacterData> c, HashSet<CharacterData> s)
        {
            this.target = target;
            _candidate = c;
            _selected = s;

            OnValueChangedHandler += Draw;

            OnValueChangedHandler();
        }

        public void Draw()
        {
            canRoot.PurgeChild();
            foreach (var ch in _candidate)
            {
                if (target == ch) continue;
                
                var p = Instantiate(portraitPrefab, canRoot);
                p.Init(ch);
                p.OnClickHandler = () =>
                {
                    Select(ch);
                };
            }
            
            selRoot.PurgeChild();
            foreach (var ch in _selected)
            {
                if (target == ch) continue;

                var p = Instantiate(portraitPrefab, selRoot);
                p.Init(ch);
                p.OnClickHandler = () =>
                {
                    UnSelect(ch);
                };
            }
        }

        public void Select(CharacterData x)
        {
            if (!_selected.Add(x)) return;
            _candidate.Remove(x);
            
            OnValueChangedHandler();
        }
        public void UnSelect(CharacterData x)
        {
            if (!_selected.Contains(x)) return;
            
            _candidate.Add(x);
            _selected.Remove(x);
            OnValueChangedHandler();
        }

        public void AddCandidate(CharacterData x)
        {
            if (_selected.Contains(x) || !_candidate.Add(x)) return;
            
            OnValueChangedHandler();
        }

        public void Remove(CharacterData x)
        {
            if (!_selected.Contains(x) && !_candidate.Contains(x)) return;
            
            _selected.Remove(x);
            _candidate.Remove(x);
            OnValueChangedHandler();
        }
    }
}