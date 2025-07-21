using System.IO;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BansheeGz.BGDatabase
{
    /// <summary>
    /// Abstract action for all SaveLoad related actions which has filename as a parameter
    /// </summary>
    public abstract partial class SaveLoadWithNameA : SaveLoadA
    {
        private const string DefaultFileName = "game";
        
        public FsmString fileName;

        protected string FullFileName
        {
            get
            {
                var name = fileName.Value;
                if (string.IsNullOrEmpty(name)) name = DefaultFileName;
                return Path.ChangeExtension(Path.Combine(Application.persistentDataPath, name), "." + FileExt);
            }
        }
        
        public override void Reset()
        {
            base.Reset();
            fileName = null;
        }

    }
}