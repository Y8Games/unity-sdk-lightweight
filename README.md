# Lightweight Y8 SDK

## How to set up 
1) Download the UnityPackage and import it into your project (https://github.com/Y8Games/unity-sdk-lightweight/releases)
2) Drag and drop the prefab from Assets > Y8 > Y8Root into your main scene (for example preloader or splash screen scene)
3) go to https://account.y8.com/applications and create new application
4) Copy paste the application ID into the Y8Root prefab in your scene (also ask support for Ads Id if you are partner)
5) When building WebGL, use the Y8 template `Y8_2020_LTS` 

## How to use Y8 functions
1) Add `using Y8API;` to your using directives
2) Call awaitable methods on Y8, for example `var loginInfo = await Y8.Instance.LoginAsync();` 
3) Check *[TestScene](https://github.com/Y8Games/unity-sdk-lightweight/blob/main/Assets/Y8/TestScene/Scripts/TestWrapper.cs)* for all available methods and usage 

## Ads guidelines
- You will need to be approved as a Y8 partner first, please contact the support to receive AdsID
- Always **pause the game (including all sounds)** before playing the ads

Example:
```
public async void ShowAd()
{
    PauseGame(); // Should pause all sounds as well
    await Y8.Instance.ShowAdAsync();
    UnpauseGame();     
}
```
