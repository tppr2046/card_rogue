using System.IO;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Get list of saved games from persistent folder")]
    public partial class ListSaves : SaveLoadA
    {
        [ArrayEditor(VariableType.String)]
        public FsmArray fileNames;

        public override void Reset()
        {
            base.Reset();
            fileNames = null;
        }
        
        public override void OnEnter()
        {
            var files = Directory.GetFiles(Application.persistentDataPath, "*." + FileExt);
            var filesNoExtension = new object[files.Length];
            if (files.Length > 0)
            {
                for (var i = 0; i < files.Length; i++) filesNoExtension[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            // fileNames.stringValues = filesNoExtension;
            fileNames.Values = filesNoExtension;
            // fileNames.SaveChanges();
            
            if (IsDebugging)
            {
                if (files.Length > 0)
                {
                    var fileNamesBuilder = new StringBuilder();
                    foreach (var file in files) fileNamesBuilder.Append(Path.GetFileNameWithoutExtension(file)).Append(' ');
                    Log("$ saved files found in folder: $. Files are: $" , files.Length, Application.persistentDataPath, fileNamesBuilder.ToString());
                }
                else Log("No saved files found in folder: $" , Application.persistentDataPath);
            }
            
            Finish();
        }
    }
}