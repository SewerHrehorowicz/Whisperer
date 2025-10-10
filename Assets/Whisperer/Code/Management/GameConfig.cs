#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using Whisperer.VirtualTerrain;

namespace Whisperer
{
    [CreateAssetMenu(menuName = "Whisperer/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private bool _debugMode;
        public bool DebugMode => _debugMode;

        [SerializeField] private ObjectPoolSettings _grassPoolSettings;
        [SerializeField] private VTSettings _vtSettings;

        private ObjectPool _grassPoolInstance;

        public ObjectPool GrassPool
        {
            get
            {
                if (_grassPoolInstance == null)
                    _grassPoolInstance = ObjectPool.Create(_grassPoolSettings, nameof(GrassPool));
                    
                return _grassPoolInstance;
            }
        }
        public VTSettings VTSettings => _vtSettings;

        private static GameConfig _instance;
        public static GameConfig Instance
        {
            get
            {
                if (!_instance)
                    _instance = Resources.Load<GameConfig>(nameof(GameConfig));
                if (!_instance)
                {
                    Debug.LogError("Config not found");
                    return null;
                }
                return _instance;
            }
        }

#if UNITY_EDITOR
        

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (Instance?.GrassPool != null)
                Instance.GrassPool.Recompiled();
        }
#endif
    }
}