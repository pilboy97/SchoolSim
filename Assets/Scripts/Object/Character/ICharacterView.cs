using UnityEngine;

namespace Game.Object.Character
{
    public interface ICharacterView
    {
        void SetGender(Gender gender);
        void SetVisible(bool visible);
        void SetPosition(Vector2 pos);
        void SetDirection(Direction dir);
        void SetName(string name);
        void SetTask(string task);
    }
}