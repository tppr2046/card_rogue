using HutongGames.PlayMaker;
using UnityEngine;

namespace BansheeGz.BGDatabase
{
    /// <summary>
    /// Abstract action for all SaveLoad related actions
    /// </summary>
    public abstract partial class SaveLoadA : FsmStateAction
    {
        protected const string FileExt = "sav";

        public FsmBool debug;

        protected bool IsDebugging => !debug.IsNone && debug.Value;

        public override void Reset() => debug = null;

        protected void Log(string message, params object[] parameters)
        {
            if (!IsDebugging) return;
            Debug.Log("Debug [" + GetType().Name + "]: " + BGUtil.Format(message, parameters));
        }
    }
}