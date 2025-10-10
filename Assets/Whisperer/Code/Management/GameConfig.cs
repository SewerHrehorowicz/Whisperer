using UnityEngine;
using Whisperer.VirtualTerrain;

namespace Whisperer
{
    [CreateAssetMenu(menuName = "Whisperer/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private ObjectPool _grassLinesPool;
        [SerializeField] private VTSettings _vtSettings;

        private ObjectPool _grassLinesPoolInstance;

        public ObjectPool GrassLinesPool
        {
            get
            {
                if (_grassLinesPoolInstance == null)
                {
                    _grassLinesPoolInstance = Instantiate(_grassLinesPool);
                    _grassLinesPoolInstance.name = $"{_grassLinesPool.name}_Instance";
                    _grassLinesPoolInstance.transform.position = Vector3.zero;
                }
                    
                if (_grassLinesPoolInstance == null)
                {
                    Debug.LogError($"{nameof(GameConfig)} is missing {nameof(_grassLinesPool)} reference");
                    return null;
                }

                return _grassLinesPoolInstance;
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
    }
}