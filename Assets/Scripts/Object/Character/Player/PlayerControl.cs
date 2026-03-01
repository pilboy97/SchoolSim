using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Map;
using Game.Object.Character.AI;
using Game.Room;
using Game.Task;
using Game.Time;
using Game.UI;
using UnityEngine;
using Action = Game.Task.Action;
using ContextMenu = Game.UI.ContextMenu;

namespace Game.Object.Character.Player
{
    [Serializable]
    public class PlayerControl : IController
    {
        [SerializeField] private Character character;
        [SerializeReference] private ITask next;

        [SerializeReference] private AIControl autoPilot;
        public AIControl Autopilot => autoPilot;
        [SerializeField] public bool isAutoPilot;

        private void OnClickMap(Vector2 pos)
        {
            var player = GameManager.Instance.Player;
            if (player == null) return;
            
            var cpos = MapController.Instance.WorldToCell(new Vector3(pos.x, pos.y,
                RoomManager.Instance.currentRoomIndex));
            var objs = ObjectManager.Instance.GetObjectAt(cpos);
            List<UI.ContextMenu.Item> items = new();

            if (objs.Length == 0)
            {
                if (NavManager.Instance.WalkableCells.Contains(cpos))
                {
                    items.Add(new ContextMenu.Item()
                    {
                        name = "Move",
                        onClick = new Action()
                        {
                            indirect =  true,
                            effect = new WalkToEffect()
                            {
                                cpos= cpos
                            }
                        }
                    });
                    
                    UI.ContextMenu.Instance.Init(pos, items.ToArray());
                    return;
                }
                
                UI.ContextMenu.Instance.Init(Vector2.zero, null);
                return;
            }
            
            foreach (var obj in objs)
            {
                if (obj is MapObject o)
                {
                    foreach (var action in o.Actions)
                    {
                        if (!action.cond?.Check(null, action, player, obj) ?? true) continue;
                        
                        items.Add(new UI.ContextMenu.Item()
                        {
                            name = action.actionName,
                            onClick = action,
                            target = o
                        });
                    }
                }

                if (obj is Character ch)
                {
                    foreach (var action in ch.Actions)
                    {
                        if (!action.cond?.Check(null, action, player, obj) ?? false) continue;
                        if (ch.ID == player.ID) continue;

                        items.Add(new UI.ContextMenu.Item()
                        {
                            target = ch,
                            name = action.actionName,
                            onClick = action
                        });
                    }
                }
                if (obj is Portal portal)
                {
                    items.Add(new UI.ContextMenu.Item()
                    {
                        target = obj,
                        name = $"Goto {portal.Dest}",
                        onClick = new Action()
                        {
                            effect = new ListEffect()
                            {
                                list = new List<Effect>()
                                {
                                    new TractTargetEffect()
                                    {
                                        targetID = portal.ID
                                    },
                                    new TractTargetEffect()
                                    {
                                        targetID = portal.dest.ID,
                                    }
                                }
                            }
                        }
                    });
                }
            }
            
            ContextMenu.Instance.Init(pos, items.ToArray());
        }

        public PlayerControl(Character character, bool isAutoPilot = false)
        {
            this.character = character;
            this.isAutoPilot = isAutoPilot;
            
            autoPilot = new AIControl(character);
            
            MapController.Instance.groundMap.OnClickCPosHandler += OnClickCPos;
            InputManager.Instance.OnClickHandler += OnClickMap;
        }

        public void ToggleAutoPilot()
        {
            isAutoPilot = !isAutoPilot;
        }

        public void SetNext(ITask task)
        {
            next = task;
        }
        
        public ITask Select()
        {
            if (next == null) return isAutoPilot ? autoPilot.Select() : null;
            
            var ret = next;
            ret.Busy = true;

            var p = ret;
            while (p != null)
            {
                p.Busy = true;
                p = p.Prev;
            }
                
            next = null;
            return ret;

        }

        public Character Character => character;

        public async UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who,
            bool forced = false)
        {
            if (token.IsCancellationRequested) return false;
            if (character.Data.eventID != "") return false;
            
            if (!forced)
            {
                if (isAutoPilot)
                {
                    return await autoPilot.TryInviteMeAsync(token, e, who, false);
                }
                
                var front = character.TaskQueue.Front;
                if (front is ActionTask { action: { effect: InviteSimpleEventEffect frontEffect } } && 
                    e.Equals(frontEffect.eventData)) return true;
                
                string str = $"{who.charName} asked {character.charName} to join {e.eventName}";
                
                var panel = RequestView.Instance.Get();
                panel.Init(str);

                var (t1,t2) = (character.Busy, who.Busy);
                character.Busy = true;
                who.Busy = true;
                
                var isYes = await panel.WaitForAnswer(token);
                
                character.Busy = t1;
                who.Busy = t2;
                RequestView.Instance.Release(panel);
                
                TimeManager.Instance.SetTimeScale(timescale);

                if (!isYes) return false;

                character.TaskQueue.Cancel();
                character.TaskQueue.Clear();
            }
            
            return e.TryInvite(character, forced);
        }

        public void OnDestroy()
        {
            MapController.Instance.groundMap.OnClickCPosHandler -= OnClickCPos;
            InputManager.Instance.OnClickHandler -= OnClickMap;
        }

        public bool TryInterrupt()
        {
            return !character.Busy;
        }

        private void OnClickCPos(Vector3Int cPos)
        {
        }
    }
}