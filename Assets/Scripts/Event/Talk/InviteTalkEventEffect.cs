using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Object;
using Game.Object.Character;
using Game.Task;
using UnityEngine;

namespace Game.Event.Talk
{
    [Serializable]
    public class InviteTalkEventEffect : InviteEventEffect
    {
        public override Event TargetEvent
        {
            get
            {
                var ret = new TalkEvent();
                ret.Init();
                return ret;
            }
        }


        public override bool Equals(Effect other)
        {
            return other is InviteTalkEventEffect;
        }
    }
}