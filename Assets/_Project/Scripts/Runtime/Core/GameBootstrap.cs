using UnityEngine;

namespace ShadowSupply.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [SerializeField] private int targetFrameRate = 120;
        [SerializeField] private bool keepRunningInBackground = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = targetFrameRate;
            Application.runInBackground = keepRunningInBackground;

            Debug.Log($"[Shadow Supply] Runtime initialized. Unity {Application.unityVersion}, Build {Application.version}.");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
