using TMPro;
using UnityEngine;

namespace Game.Object.Character
{
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        private Transform _transform;
        private SpriteRenderer _renderer;

        [SerializeField] private TextMeshPro nameLableText;

        private void Awake()
        {
            _transform = transform;
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void SetGender(Gender gender)
        {
            if (gender == Gender.Male) _renderer.color = Color.blue;
            else _renderer.color = Color.magenta;
        }

        public void SetDirection(Direction dir)
        {
        }

        public void SetPosition(Vector2 pos)
        {
            _transform.position = pos;
        }

        public void SetVisible(bool visible)
        {
            _renderer.enabled = visible;
            nameLableText.gameObject.SetActive(visible);
        }

        public void SetName(string name)
        {
            nameLableText.text = name;
        }
    }
}