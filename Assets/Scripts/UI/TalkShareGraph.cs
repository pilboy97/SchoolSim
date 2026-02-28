using System;
using System.Collections.Generic;
using Game.Event.Talk;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class TalkShareGraph : MonoBehaviour
    {
        private RectTransform _rectTransform;

        [SerializeField] private GraphBar rectPrefab;

        private TalkEvent _event;
        private readonly List<GraphBar> _activated = new();
        
        private ObjectPool<GraphBar> _pool;
        private readonly Dictionary<string, Color> _colors = new();

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _pool =new ObjectPool<GraphBar>(
                createFunc: () => Instantiate(rectPrefab, GameManager.TEMP),
                actionOnGet: (x) =>
                {
                    x.gameObject.SetActive(true);
                    x.transform.SetParent(transform); 
                },
                actionOnRelease: (x) =>
                {
                    x.gameObject.SetActive(false);
                    x.transform.SetParent(GameManager.TEMP); 
                }
            );
        }

        public void Init(TalkEvent target)
        {
            _event = target;
            
            Draw();
        }
        
        private void Draw()
        {
            while (_activated.Count < _event.members.Count)
            {
                _activated.Add(_pool.Get());
            }
            
            while (_activated.Count > _event.members.Count)
            {
                _pool.Release(_activated[^1]);
                _activated.RemoveAt(_activated.Count - 1);
            }

            var width = _rectTransform.rect.width;
            for (var i = 0; i < _event.members.Count; i++)
            {
                var member = _event.members[i];

                if (!_colors.TryGetValue(member.ID, out var value))
                {
                    value = RandomColor();
                    
                    _colors.TryAdd(member.ID, value);
                }

                var w = _event.shareOfInfluence[member.ID] * width;
                var mod = _event.shareOfInfluence[member.ID] / _event.DesiredShare;
                
                _activated[i].img.color = value;
                _activated[i].RectTransform.sizeDelta = new Vector2(w, 0);

                _activated[i].txt.text = $"{member.charName}";
            }
        }

        private Color RandomColor()
        {
            return UnityEngine.Random.ColorHSV(0,1,0.5f,0.6f,0.4f,0.5f);
        }

        private void LateUpdate()
        {
            Draw();
        }
    }
}