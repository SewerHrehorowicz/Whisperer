using UnityEngine;
using Whisperer.VirtualTerrain;
using Whisperer.Pooling;

namespace Whisperer
{
    [CreateAssetMenu(menuName = "Whisperer/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private bool _debugMode;
        [SerializeField] private ObjectPoolSettings _grassPoolSettings;
        [SerializeField] private VTSettings _vtSettings;
        private static GameConfig _instance;

        public bool DebugMode => _debugMode;
        public ObjectPoolSettings GrassPoolSettings => _grassPoolSettings;
        public VTSettings VTSettings => _vtSettings;

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
    }
}