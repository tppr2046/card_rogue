using System.IO;
using HutongGames.PlayMaker;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Save database state to a file in persistent folder")]
    public partial class SaveGame : SaveLoadWithConfigA
    {
       
        public override void OnEnter()
        {
            var fullFileName = FullFileName;
            var configNameValue = ConfigNameValue;
            var addon = BGRepo.I.Addons.Get<BGAddonSaveLoad>();
            
            var bytes = string.IsNullOrEmpty(configNameValue)
                ?addon.Save()
                :addon.Save(new BGSaveLoadAddonSaveContext(configNameValue));
            
            File.WriteAllBytes(fullFileName, bytes);
            Log("Saved OK. File at: $", fullFileName);
            Finish();
        }

    }
}