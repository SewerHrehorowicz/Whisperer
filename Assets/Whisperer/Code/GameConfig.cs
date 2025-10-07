using UnityEngine;
using Whisperer.VirtualTerrain;

namespace Whisperer
{
    [CreateAssetMenu(menuName = "Whisperer/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private VTSettings _vtSettings;
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