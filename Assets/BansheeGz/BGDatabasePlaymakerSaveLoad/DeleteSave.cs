using System.IO;
using HutongGames.PlayMaker;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Delete save file in persistent folder")]
    public partial class DeleteSave : SaveLoadWithNameA
    {
        public override void OnEnter()
        {
            var fullFileName = FullFileName;

            if (File.Exists(fullFileName))
            {
                File.Delete(fullFileName);
                Log("File $ deleted", fullFileName);
            }
            else Log("File not found: $", fullFileName);

            Finish();
        }
    }
}