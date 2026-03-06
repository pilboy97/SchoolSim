using System;
using Game.Object;
using Game.Object.Character.Player;
using Game.Task;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = Game.Task.Action;

namespace Game.UI
{
    public class ContextMenuItem : UIBehaviour
    {
        [SerializeField] private ContextMenu menu;
        [SerializeField] private ContextMenu.Item item;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button btn;
        private IInteractable _obj;
        [SerializeField] private Action onClick;

        protected override void Awake()
        {
            base.Awake();
            
            btn.onClick.AddListener(() =>
            {
                var player = GameManager.Instance.Player;
                var task = new ActionTask(player, _obj, onClick);
                var c = player.controller as PlayerControl;
                if (c == null)
                {
                    return;
                }
                
                if (!onClick.indirect)
                    task.Prev = new ActionTask(player, null, new Action()
                    {
                        actionName = $"track to {_obj.Name}",
                        effect = new TractTargetEffect()
                        {
                            targetID = _obj.ID
                        },
                        indirect = true
                    });

                c.Character.TaskQueue.Cancel();
                c.SetNext(task);
                
                menu.Init(Vector2.zero, null);
            });
        }

        public void Init(ContextMenu menu,ContextMenu.Item item, IInteractable obj)
        {
            this.menu = menu;
            this.item = item;
            onClick = item.onClick;
            _obj = obj;

            text.text = $"{_obj?.Name ?? ""} {item.name}";
        }
    }
}