using UnityEngine;
using System.Collections.Generic;
using MinesServer.Data;
using Fodinae.Assets.Scripts.Game;

namespace Fodinae.Assets.Scripts.Game.Managers
{
    public class PackManager : MonoBehaviour
    {
        private static PackManager _instance;
        public static PackManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PackManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[PackManager]");
                        _instance = go.AddComponent<PackManager>();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<Vector2Int, Pack> _packs = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Subscribe to WorldInitialized is problematic because it might trigger after packets are processed.
            // However, MapManager.LoadWorldInit calls MapStorage.InitWorld which is the best time to clear packs.
        }

        private void OnDestroy()
        {
        }

        public void AddOrUpdatePack(ushort x, ushort y, PackType packType, byte variant, byte linkedClan)
        {
            if (MapManager.Instance == null) return;

            var pos = new Vector2Int(x, y);
            if (_packs.TryGetValue(pos, out var pack))
            {
                pack.Initialize(packType, variant, linkedClan);
                return;
            }

            var go = new GameObject($"Pack_{x}_{y}");
            go.transform.SetParent(transform);

            // Centered at the target cell.
            // Formula for UnityY: (MapManager.Instance.WorldHeight - 1 - y) + 0.5f
            float unityY = (MapManager.Instance.WorldHeight - 1 - y) + 0.5f;
            go.transform.position = new Vector3(x + 0.5f, unityY, 0);

            pack = go.AddComponent<Pack>();
            pack.Initialize(packType, variant, linkedClan);
            _packs[pos] = pack;
        }

        public void RemovePack(ushort x, ushort y)
        {
            var pos = new Vector2Int(x, y);
            if (_packs.TryGetValue(pos, out var pack))
            {
                Destroy(pack.gameObject);
                _packs.Remove(pos);
            }
        }

        public void ClearAllPacks()
        {
            foreach (var pack in _packs.Values)
            {
                if (pack != null) Destroy(pack.gameObject);
            }
            _packs.Clear();
        }
    }
}
