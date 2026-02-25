using UnityEngine;

namespace Game.UI
{
    public class Window :　MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Move(Vector2 delta)
        {
            var d = new Vector3(delta.x, delta.y, 0);

            transform.position += d;
        }
    }
}