using System.Threading.Tasks;
using UnityEngine;

namespace Y8API
{
    /// <summary>
    /// Put this on an empty UI element (RectTransform) where a banner should go, sized at
    /// least as large as the banner on screen. Request() shows the banner over the Unity
    /// canvas, centred on it; while it shows, the banner follows the element when it moves,
    /// and it is cleared when the element is disabled or destroyed.
    ///
    /// Usage:
    ///   JsResponse&lt;BannerResult&gt; r = await bannerSlot.RequestAsync();
    ///   if (!r.IsSuccess) Debug.Log(r.Data.code);   // e.g. bannerCooldown, unfilled
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class Y8BannerSlot : MonoBehaviour
    {
        [Tooltip("Name of this banner slot. Defaults to the GameObject's name.")]
        public string BannerId = "";

        public BannerSize Size = BannerSize.Leaderboard_728x90;

        [Tooltip("Request a banner as soon as this element is enabled.")]
        public bool RequestOnEnable = false;

        private RectTransform rectTransform;
        private bool showing;
        private RectInt lastPlacement;
        private int lastScreenWidth;
        private int lastScreenHeight;

        public string Id => string.IsNullOrEmpty(BannerId) ? gameObject.name : BannerId;

        /// <summary>True while a banner requested through this slot is on screen.</summary>
        public bool IsShowing => showing;

        private void Awake() => rectTransform = (RectTransform)transform;

        private async void OnEnable()
        {
            if (RequestOnEnable)
            {
                await RequestAsync();
            }
        }

        private void OnDisable() => Clear();

        /// <summary>Requests a banner of Size for this slot. See Y8.RequestBannerAsync.</summary>
        public async Task<JsResponse<BannerResult>> RequestAsync()
        {
            if (Y8.Instance == null)
            {
                return new JsResponse<BannerResult>(
                    false,
                    new BannerResult { code = "bannersUnavailable", message = "No Y8Root in the scene" }
                );
            }

            JsResponse<BannerResult> response = await Y8.Instance.RequestBannerAsync(Id, Size, rectTransform);
            // A failed request leaves an earlier banner in this slot on screen
            showing = showing || response.IsSuccess;
            if (response.IsSuccess)
            {
                RememberPlacement();
            }

            return response;
        }

        /// <summary>Removes this slot's banner.</summary>
        public void Clear()
        {
            if (!showing || Y8.Instance == null)
            {
                return;
            }

            showing = false;
            Y8.Instance.ClearBanner(Id);
        }

        // The page only re-lays banners out on a browser resize; a UI that re-flows or
        // animates needs telling, so follow the element whenever it lands somewhere else.
        private void LateUpdate()
        {
            if (!showing || !Y8.TryGetScreenPlacement(rectTransform, out RectInt placement))
            {
                return;
            }

            if (placement.Equals(lastPlacement)
                && Screen.width == lastScreenWidth
                && Screen.height == lastScreenHeight)
            {
                return;
            }

            Y8.Instance.MoveBanner(Id, rectTransform);
            RememberPlacement();
        }

        private void RememberPlacement()
        {
            Y8.TryGetScreenPlacement(rectTransform, out lastPlacement);
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }
}
