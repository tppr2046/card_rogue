using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Release Unity asset using addressables system")]
    public class BGAddressablesReleaseAsset : FsmStateAction
    {
        [UIHint(UIHint.Variable)] public FsmObject Asset;


        public override void Reset() => Asset = null;

        public override void OnEnter()
        {
            if (Asset == null || Asset.IsNone || Asset.Value == null) Debug.LogError("BGAddressablesReleaseAsset: Can not release asset- asset is null");
            else Addressables.Release(Asset.Value);

            Finish();
        }
    }
}