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