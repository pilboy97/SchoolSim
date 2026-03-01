// using System.Linq;

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Event;
using Game.Map;
using UnityEngine;

namespace Game.Object.Character
{
    public static class CharacterAsyncHelper
    {
        public static async UniTask InviteAsync(this Character character, CancellationToken token, Character who,
            Event.Event e)
        {
            if (token.IsCancellationRequested) return;
            if (who == null) return;

            var temp = character.Busy;
            character.Busy = true;
            
            if (who.Data.eventID == "")
            {
                EventManager.Instance.AddEvent(e);

                if (!(await character.TryInviteMeAsync(token, e, who, true)))
                {
                    e.Finish();
                    
                    character.Busy = temp;
                    return;
                }

                using var cts = new CancellationTokenSource();
                if (!(await who.TryInviteMeAsync(cts.Token, e, character)))
                {
                    e.Finish();
                }

                character.Busy = temp;
                return;
            }

            Event.Event curEvent = EventManager.Instance.Find(who.Data.eventID);

            if (curEvent == null)
            {
                who.Data.eventID = "";
                who.TaskQueue.Cancel();

                await character.TryInviteMeAsync(token, e, who);
                character.Busy = temp;
                return;
            }

            if (curEvent.Equals(e))
            {
                await character.TryInviteMeAsync(token, curEvent, who, true);
            }
            else
            {
                EventManager.Instance.AddEvent(e);
                if (!await who.TryInviteMeAsync(token, e, character))
                {
                    e.Finish();
                    character.Busy = temp;
                    return;
                }

                if (!await character.TryInviteMeAsync(token, e, who, true))
                {
                    e.Finish();
                }
            }
            
            character.Busy = temp;
        }

        public static async UniTask TrackTargetAsync(
            this Character character, 
            CancellationToken token,
            string targetID)
        {
            if (token.IsCancellationRequested) return;
            
            if (targetID == null) return;
            var target = ObjectManager.Instance.Find(targetID);
            
            if (target == null) return;

            var cp = target.CenterPosition;
            var routine = NavManager.Instance.FindPathAround(character.Position, target.Positions);
            var idx = 0;
            while (target.Distance(character) > 1.3f)
            {
                if (token.IsCancellationRequested) return;

                if (target.CenterPosition != cp)
                {
                    bool isAlreadyInRoute = false;
                    foreach (var pos in routine.Item1)
                    {
                        var cPos = MapController.Instance.WorldToCell(target.CenterPosition);
                        if (cPos == pos)
                        {
                            isAlreadyInRoute = true;
                            break;
                        }
                    }
                    
                    if (!isAlreadyInRoute) 
                    {
                        idx = 0;
                        cp = target.CenterPosition;
                        routine = NavManager.Instance.FindPath(character.Position, target.Positions);
                    }
                }

                if (routine.Item1 == null) return;
                if (routine.Item1.Length <= idx + 1) return;
                
                var next = routine.Item1[++idx];
                await character.WalkRouteAsync(token, new[] { next });
            }
        }

        public static async UniTask AddStatusAsync(this Character character, CancellationToken token, CharacterStats delta)
        {
            await UniTask.NextFrame(token);
            if (token.IsCancellationRequested) return;
            
            character.Receive(delta);
        }

        public static async UniTask WalkAsync(this Character character,CancellationToken token, Vector2[] dest)
        {
            if (token.IsCancellationRequested) return;
            if (dest == null) return;
            
            var moveDestCPos = new Vector3Int[dest.Length];
            for(int i = 0;i < dest.Length;i++)
            {
                moveDestCPos[i] = MapController.Instance.WorldToCell(dest[i]);
            }
            
            await WalkAsync(character, token, moveDestCPos);
        }

        public static async UniTask WalkAsync(this Character character,CancellationToken token, Vector3Int[] dest)
        {
            if (token.IsCancellationRequested) return;
            if (dest == null) return;

            var (route, dist) = NavManager.Instance.FindPath(character.CPosition,dest);
            if (dist == 0)
            {
                return;
            }
            
            await WalkRouteAsync(character, token, route);
        }

        public static async UniTask WalkRouteAsync(this Character character,CancellationToken token, Vector3Int[] route)
        {
            if (token.IsCancellationRequested) return;
            if (route == null) return;

            foreach (var cPos in route)
            {
                if (token.IsCancellationRequested) return;

                if (cPos.z != character.CPosition.z)
                {
                    character.CPosition = cPos;
                    await UniTask.NextFrame();
                    continue;
                }
                
                var pos = MapController.Instance.CellToWorld(cPos);
                await WalkNeighborCellAsync(character, token, pos);
            }
        }
        
        public static async UniTask WalkNeighborCellAsync(this Character character, CancellationToken token, Vector3 pos)
        {
            if (token.IsCancellationRequested) return;
            var dist = Vector2.Distance(character.Position, pos);
            if (dist > 2)
            {
                UnityEngine.Debug.Log($"{character.Position} {character.transform.position} {pos} {dist}");
            }

            var lp = MapController.Instance.WorldToCell(pos);
            if (!NavManager.Instance.WalkableCells.Contains(lp)) return;

            var delta = pos - character.Position;
            if (delta == Vector3.zero) return; 
            
            var angle = Vector3.SignedAngle(Vector3.right, delta, Vector3.up);

            character.Direction = angle switch
            {
                >= -45 and < 45 => Direction.Right,
                >= 45 and < 135 => Direction.Up,
                _ => angle is >= 135 or < -135 ? Direction.Left : Direction.Down
            };

            using var t = CancellationTokenSource.CreateLinkedTokenSource(token);
            await DOTween
                .To(() => character.Position, v => character.Position = v, pos, 0.2f)
                .SetEase(Ease.Linear)
                .SetLink(character.gameObject)
                .WithCancellation(t.Token);
        }
    }
}