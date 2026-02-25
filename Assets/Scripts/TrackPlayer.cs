using UnityEngine;

namespace Game
{
    public class TrackPlayer : MonoBehaviour
    {
        public Object.Character.Character Player => GameManager.Instance.Player;

        private void LateUpdate()
        {
            var pos = Player?.transform.position ?? Vector3.zero;
            pos.z = -10;

            transform.position = pos;
        }
    }
}