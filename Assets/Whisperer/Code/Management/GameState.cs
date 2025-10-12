#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

using UnityEngine;
using Whisperer.Pooling;

namespace Whisperer.Management
{
    public static class GameState
    {
        private static GenericPool<MeshFilter> _grassPoolInstance;

        public static GenericPool<MeshFilter> GrassPool
        {
            get
            {
                if (_grassPoolInstance == null)
                {
                    var settings = GameConfig.Instance.GrassPoolSettings;
                    if (settings == null)
                    {
                        Debug.LogError($"{nameof(GameConfig.GrassPoolSettings)} not found");
                        return null;
                    }
                    _grassPoolInstance = GenericPool<MeshFilter>.Create<MeshFilter>(GameConfig.Instance.GrassPoolSettings, nameof(GrassPool));
                }

                return _grassPoolInstance;
            }
        }

#if UNITY_EDITOR
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (GrassPool != null)
                GrassPool.Recompiled();
        }
#endif
    }
}