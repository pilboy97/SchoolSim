using System;
using Game.Object.Character;
using Game.Task;
using Game.Time;
using UnityEngine;

namespace Game.School
{
    [Serializable]
    public class Session : RepeatSchedule
    {
        [SerializeField] public School school;

        private void OnEnable()
        {
            busy = true;
        }
    }
}