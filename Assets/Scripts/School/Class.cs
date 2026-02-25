using System;
using System.Collections.Generic;
using Game.Object.Character;
using Game.Room;
using UnityEngine;

namespace Game.School
{
    [CreateAssetMenu(menuName = "School/Class")]
    public class Class : ScriptableObject
    {
        public RoomData map;
        public List<CharacterData> grp = new();
    }
}