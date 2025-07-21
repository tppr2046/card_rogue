using HutongGames.PlayMaker;
using UnityEngine;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Reload database")]
    public partial class BGReloadDatabase : FsmStateAction
    {
        public override void OnEnter()
        {
            Debug.Log("Reloading!");
            BGRepo.Load();
            Finish();
        }
    }
}