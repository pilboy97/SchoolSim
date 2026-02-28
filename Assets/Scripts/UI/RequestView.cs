using UnityEngine;
using UnityEngine.Pool;

namespace Game.UI
{
    public class RequestView : Singleton<RequestView>
    {
        [SerializeField] private RequestPanel requestPanelPrefab;
        [SerializeField] private RectTransform root;
        
        private ObjectPool<RequestPanel> _requestPanelPool;

        private void Awake()
        {
            _requestPanelPool = new ObjectPool<RequestPanel>(
                createFunc: () => Instantiate(requestPanelPrefab, GameManager.TEMP),
                actionOnGet: (x) =>
                {
                    x.gameObject.SetActive(true);
                    x.transform.SetParent(root); 
                },
                actionOnRelease: (x) =>
                {
                    x.gameObject.SetActive(false);
                    x.transform.SetParent(GameManager.TEMP); 
                }
            );
            
            Instance.gameObject.SetActive(false);
        }
        
        public RequestPanel Get()
        {
            gameObject.SetActive(true);
            return _requestPanelPool.Get();
        }

        public void Release(RequestPanel obj)
        {
            _requestPanelPool.Release(obj);
            
            if (root.childCount == 0) gameObject.SetActive(false);
        }
    }
}