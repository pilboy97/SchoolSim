using System;
using Game.Object.Character;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class Portrait : MonoBehaviour
    {
        [SerializeField] private Image img;
        [SerializeField] private CharacterData who;

        public void Init(CharacterData who)
        {
            this.who = who;
            img.color = who.gender == Gender.Male ? Color.blue : Color.magenta;
        }
    }
}