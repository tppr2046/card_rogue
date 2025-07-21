using HutongGames.PlayMaker;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Resize the table to provided rows size")]
    public class ResizeTable : FsmStateAction
    {
        public FsmString tableName;
        public FsmInt size;

        public override void Reset()
        {
            tableName = null;
            size = null;
        }

        public override void OnEnter()
        {
            var sizeValue = size.Value;
            if (sizeValue < 0)
            {
                Log("size should be >=0");
            }
            else
            {
                var metaName = tableName.Value;
                if (string.IsNullOrEmpty(metaName))
                {
                    Log("tableName is not set");
                }
                else
                {
                    var meta = BGRepo.I[metaName];
                    if (meta == null)
                    {
                        Log("Can not find table with name " + metaName);
                    }
                    else
                    {
                        if (meta.CountEntities != sizeValue)
                        {
                            if (meta.CountEntities > sizeValue)
                            {
                                //delete rows
                                meta.DeleteEntities(meta.FindEntities(entity => entity.Index >= sizeValue));
                            }
                            else
                            {
                                //add rows
                                for (var i = meta.CountEntities; i < sizeValue; i++) meta.NewEntity();
                            }
                        }
                    }
                }
            }

            Finish();
        }
    }
}