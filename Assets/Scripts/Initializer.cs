using UnityEngine;

namespace Game
{
    public class Initializer : MonoBehaviour
    {
        [SerializeField] private GameObject neverDie;

        private void Awake()
        {
            if (ConfigData.Instance != null) return;

            Instantiate(neverDie);
            Destroy(gameObject);
        }
    }
}