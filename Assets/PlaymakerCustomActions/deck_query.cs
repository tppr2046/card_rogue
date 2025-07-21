using System;
using System.Collections.Generic;
using BansheeGz.BGDatabase;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using UnityEngine;



namespace HutongGames.PlayMaker.Actions


{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Query a column to fetch some rows from DECK and insert into a array")]
    public partial class deck_query : FsmStateAction
    {
        private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
        public static BansheeGz.BGDatabase.BGMetaRow MetaDefault => _metaDefault ?? (_metaDefault = BGCodeGenUtils.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(4848574864863409645UL, 15512298332687844745UL), () => _metaDefault = null));
        private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_name;
        public static BansheeGz.BGDatabase.BGFieldEntityName __generatedField___name => _ufle12jhs77_name ?? (_ufle12jhs77_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEntityName>(MetaDefault, new BGId(5342201965021109602UL, 5818923539317291693UL), () => _ufle12jhs77_name = null));
        private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_card_id;
        public static BansheeGz.BGDatabase.BGFieldInt __generatedField___card_id => _ufle12jhs77_card_id ?? (_ufle12jhs77_card_id = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5009978643930814691UL, 16827428176520195756UL), () => _ufle12jhs77_card_id = null));


        [HutongGames.PlayMaker.Tooltip("Query Column")]
        public FsmString _QueryColumn;

        public FsmBool _QueryAll;

        [UIHint(UIHint.Variable)]
        public FsmInt _QueryValue;

        [UIHint(UIHint.Variable)]
        public FsmArray _InsertIntoArray;

        [HutongGames.PlayMaker.Tooltip("Event is fired if entity was not found")]
        public FsmEvent _NotFoundEvent;


        public override void Reset()
        {

            _QueryColumn = null;
            _QueryValue = null;
            _InsertIntoArray = null;
            _QueryAll = false;

        }
        public override void OnEnter()
        {

            Query();
            Finish();
        }

        public void Query()
        {
            // 若為查詢全部
            if (_QueryAll.Value)
            {
                // 無條件篩選，全部加入
                MetaDefault.ForEachEntity(entity =>
                {
                    _InsertIntoArray.Resize(_InsertIntoArray.Length + 1);
                    _InsertIntoArray.Set(_InsertIntoArray.Length - 1, entity.Index);
                });
            }
            else
            {
                // 加入條件篩選
                MetaDefault.ForEachEntity(entity =>
                {
                    if (entity.Get<int>(_QueryColumn.Value) == _QueryValue.Value)
                    {
                        _InsertIntoArray.Resize(_InsertIntoArray.Length + 1);
                        _InsertIntoArray.Set(_InsertIntoArray.Length - 1, entity.Index);
                    }
                });
            }
        }


    }


}