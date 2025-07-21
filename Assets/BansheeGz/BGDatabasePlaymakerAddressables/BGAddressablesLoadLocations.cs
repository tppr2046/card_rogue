using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Load addresses from Addressables system using a label and [optional] type")]
    public class BGAddressablesLoadLocations : FsmStateAction
    {
        public FsmString Label;
        public FsmString AssetType;

        [UIHint(UIHint.Variable)]
        [ArrayEditor(VariableType.String)] public FsmArray Result;

        public override void Reset()
        {
            Label = null;
            AssetType = null;
            Result = null;
        }

        public override void OnEnter() => Addressables.LoadResourceLocationsAsync(Label.Value, null).Completed += LocationsLoaded;

        private void LocationsLoaded(AsyncOperationHandle<IList<IResourceLocation>> obj)
        {
            var result = obj.Result;
            if (result != null && result.Count > 0)
            {
                var values = new List<string>();
                var assetTypeValue = AssetType.Value;
                var checkingType = !string.IsNullOrEmpty(assetTypeValue);
                foreach (var location in result)
                {
                    if (checkingType && !string.Equals(assetTypeValue, location.ResourceType.Name)) continue;
                    values.Add(location.PrimaryKey);
                }

                Result.stringValues = values.ToArray();
            }

            Done();
        }

        private void Done() => Finish();
    }
}