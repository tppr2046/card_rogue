using HutongGames.PlayMaker;

namespace BansheeGz.BGDatabase
{
    /// <summary>
    /// Abstract action for SaveLoad related actions which has config name as a parameter
    /// </summary>
    public class SaveLoadWithConfigA : SaveLoadWithNameA
    {
        public FsmString configName;

        public string ConfigNameValue => configName.Value;
        public override void Reset()
        {
            base.Reset();
            configName = null;
        }
    }
}