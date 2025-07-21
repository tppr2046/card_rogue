using System;
using System.Collections.Generic;
using System.IO;
using HutongGames.PlayMaker;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Load database state from a file in persistent folder")]
    public partial class LoadGame : SaveLoadWithConfigA
    {
        public FsmString preserveConfigs;
        public FsmBool skipDatabaseReloading;

        public override void OnEnter()
        {
            var fullFileName = FullFileName;
            if (File.Exists(fullFileName))
            {
                var data = File.ReadAllBytes(fullFileName);
                if (data.Length > 0)
                {
                    var configNameValue = ConfigNameValue;
                    var addon = BGRepo.I.Addons.Get<BGAddonSaveLoad>();
                    var configNameFinal = string.IsNullOrEmpty(configNameValue) ? BGAddonSaveLoad.DefaultSettingsName : configNameValue;

                    var context = new BGSaveLoadAddonLoadContext(
                        new BGSaveLoadAddonLoadContext.LoadRequest(configNameFinal, data))
                    {
                        PreserveRequests = GetPreserveRequests()
                    };

                    if (skipDatabaseReloading.Value) context.ReloadDatabase = false; 
                    
                    addon.Load(context);

                    Log("Loaded OK. File at: $", fullFileName);
                }
                else Log("Can not load: file at: $ has no data", fullFileName);
            }
            else Log("Can not load: file is not found: $", fullFileName);

            Finish();
        }

        public override void Reset()
        {
            base.Reset();
            preserveConfigs = null;
        }

        private List<BGSaveLoadAddonLoadContext.PreserveRequest> GetPreserveRequests()
        {
            var preserveConfigsValue = preserveConfigs.Value;
            if (string.IsNullOrEmpty(preserveConfigsValue)) return null;
            var result = new List<BGSaveLoadAddonLoadContext.PreserveRequest>();
            var requests = preserveConfigsValue.Split(',');
            foreach (var request in requests) result.Add(new BGSaveLoadAddonLoadContext.PreserveRequest(request));
            return result;
        }
    }
}