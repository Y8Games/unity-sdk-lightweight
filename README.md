# Y8 SDK 2.0 — Unity Bridge

## Live demo

https://storage-direct.y8.com/bitlaslt/unity_webgl/unity-sdk-lightweight_0_2_9/index.html

---

## Setup

1. Download the UnityPackage and import it into your project
   (https://github.com/Y8Games/unity-sdk-lightweight/releases)
2. Drag the prefab from `Assets > Y8 > Y8Root` into your main scene
   (e.g. preloader or splash screen)
3. Create an application at https://account.y8.com/applications
4. Paste the **App ID** into the Y8Root prefab inspector
   (contact Y8 support for an **Ads Game ID** if you are a partner)
5. When building WebGL, select the `Y8_Responsive` template under
   `Project Settings > Player > Resolution and Presentation`

---

## Basic usage

```csharp
using Y8API;

// All async methods return JsResponse<T>
JsResponse<Y8User> response = await Y8.Instance.LoginAsync();
if (response.IsSuccess)
{
    Debug.Log(response.Data.nickname);
}
```

---

## Banners

Banners are display ads that stay on screen during play. Sizes: 728x90, 300x250,
320x50, 468x60, 320x100 (`BannerSize`). Requires Y8 SDK 2.13.0, which `Y8Root` loads
from the CDN.

In-game banner ads need Y8 approval for each game. While the game is in draft or in
review, banners show test ads, so you can build the placement first; once it is
released, they appear only if Y8 approved them (otherwise `bannersUnavailable`). See
[Banners](https://docs.y8.com/sdk/advertising/#banners) for what Y8 approves.

1. Add an empty UI element where the banner should go, sized at least as large as the
   banner **on screen** (with a Canvas Scaler, check at the smallest window size you
   support). The banner is drawn by the page over the Unity canvas, centred on it.
2. Add a `Y8BannerSlot` component to it and pick the `Size`.
3. Request the banner:

```csharp
JsResponse<BannerResult> response = await bannerSlot.RequestAsync();
if (!response.IsSuccess)
{
    Debug.Log(response.Data.code);   // bannerCooldown, unfilled, invalidSize, notVisible, ...
}
```

While a banner shows, `Y8BannerSlot` moves it with the element, and disabling the
element clears it. Without the component, use `Y8.Instance.RequestBannerAsync(id, size,
rectTransform)`, `MoveBanner(id, rectTransform)`, `ClearBanner(id)` and
`ClearAllBanners()`.

Each size gets at most one new banner every 180 seconds by default (never under 30),
whether you request it or it refreshes on its own. A request that comes too soon fails
with `bannerCooldown` and the current banner stays. Keep game UI you need clear of the
banner area: the banner sits on top of the canvas and takes clicks there.
