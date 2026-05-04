using UnityEngine;
using MinesServer.Data;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fodinae.Assets.Scripts.Game.Managers;

namespace Fodinae.Assets.Scripts.Game
{
    public class Pack : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _clanRenderer;
        private PackType _packType;
        private byte _variant;
        private byte _linkedClan;
        private CancellationTokenSource _cts;
        private Sprite _packSprite;
        private Sprite _clanSprite;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            var clanGo = new GameObject("ClanIcon");
            clanGo.transform.SetParent(transform);
            clanGo.transform.localPosition = new Vector3(0.6f, -0.5f, 0);
            _clanRenderer = clanGo.AddComponent<SpriteRenderer>();
            _clanRenderer.sortingOrder = 10; // Ensure it's on top of the pack
        }

        public void Initialize(PackType packType, byte variant, byte linkedClan)
        {
            _packType = packType;
            _variant = variant;
            _linkedClan = linkedClan;

            LoadAssets();
        }

        private void LoadAssets()
        {
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            LoadAssetsAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid LoadAssetsAsync(CancellationToken token)
        {
            string packName = _packType.ToString().ToLowerInvariant();
            string packPath = $"pack/{packName}/{_variant}.png";

            var packTask = ClientAssetLoader.Instance.GetTextureAsync(packPath, token);
            var clanTask = _linkedClan == 0 ? UniTask.FromResult<Texture2D>(null) : ClientAssetLoader.Instance.GetTextureAsync($"clan/{_linkedClan}.png", token);

            var (packTexture, clanTexture) = await UniTask.WhenAll(packTask, clanTask);

            if (token.IsCancellationRequested) return;

            if (packTexture != null && _spriteRenderer != null)
            {
                if (_packSprite != null) Destroy(_packSprite);
                // 32 pixels per unit as requested
                _packSprite = Sprite.Create(packTexture, new Rect(0, 0, packTexture.width, packTexture.height), new Vector2(0.5f, 0.5f), 16);
                _spriteRenderer.sprite = _packSprite;
            }

            if (clanTexture != null && _clanRenderer != null)
            {
                if (_clanSprite != null) Destroy(_clanSprite);
                // Robot uses clanTexture.width as PPU and 0.8 scale.
                // Let's match robot's logic for consistency.
                // Use left-aligned pivot (0, 0.5) to position relative to the edge.
                _clanSprite = Sprite.Create(clanTexture, new Rect(0, 0, clanTexture.width, clanTexture.height), new Vector2(0f, 0.5f), clanTexture.width);
                _clanRenderer.sprite = _clanSprite;
                _clanRenderer.transform.localScale = Vector3.one * 0.8f;

                // Position to the right and slightly below the center
                float packWidth = packTexture != null ? packTexture.width : 16;
                float xOffset = (packWidth / 32f) + 0.1f; // Right edge + 0.1 gap
                _clanRenderer.transform.localPosition = new Vector3(xOffset, -0.5f, 0);
            }
            else if (_clanRenderer != null)
            {
                _clanRenderer.sprite = null;
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (_packSprite != null) Destroy(_packSprite);
            if (_clanSprite != null) Destroy(_clanSprite);
        }
    }
}
