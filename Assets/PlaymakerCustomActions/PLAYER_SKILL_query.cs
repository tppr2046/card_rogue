using System;
using System.Collections.Generic;
using BansheeGz.BGDatabase;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using UnityEngine;



namespace HutongGames.PlayMaker.Actions


{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Query a column to fetch some rows from PLAYER_SKILL and insert into a array")]
    public partial class PLAYER_SKILL_query : FsmStateAction
    {
        private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
        public static BansheeGz.BGDatabase.BGMetaRow MetaDefault => _metaDefault ?? (_metaDefault = BGCodeGenUtils.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(5111002365953763867UL, 9604015388000749242UL), () => _metaDefault = null));
        private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_name;
        public static BansheeGz.BGDatabase.BGFieldEntityName __generatedField___name => _ufle12jhs77_name ?? (_ufle12jhs77_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEntityName>(MetaDefault, new BGId(4696525671901230681UL, 4747355040025001656UL), () => _ufle12jhs77_name = null));
        private static BansheeGz.BGDatabase.BGFieldString _ufle12jhs77_skill_name_text;
        public static BansheeGz.BGDatabase.BGFieldString __generatedField___skill_name_text => _ufle12jhs77_skill_name_text ?? (_ufle12jhs77_skill_name_text = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldString>(MetaDefault, new BGId(5708405205065270187UL, 956990015764170666UL), () => _ufle12jhs77_skill_name_text = null));
        private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_card_1;
        public static BansheeGz.BGDatabase.BGFieldInt __generatedField___card_1 => _ufle12jhs77_card_1 ?? (_ufle12jhs77_card_1 = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5583162343687847488UL, 11076744086059702451UL), () => _ufle12jhs77_card_1 = null));
        private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_card_2;
        public static BansheeGz.BGDatabase.BGFieldInt __generatedField___card_2 => _ufle12jhs77_card_2 ?? (_ufle12jhs77_card_2 = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(4747808216075346042UL, 7617119778186150069UL), () => _ufle12jhs77_card_2 = null));
        private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_attribute;
        public static BansheeGz.BGDatabase.BGFieldInt __generatedField___attribute => _ufle12jhs77_attribute ?? (_ufle12jhs77_attribute = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5635041549529652593UL, 12620969153320959930UL), () => _ufle12jhs77_attribute = null));
        private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_atk;
        public static BansheeGz.BGDatabase.BGFieldInt __generatedField___atk => _ufle12jhs77_atk ?? (_ufle12jhs77_atk = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(4915576252160005527UL, 7004473572119264183UL), () => _ufle12jhs77_atk = null));
        private static BansheeGz.BGDatabase.BGFieldBool _ufle12jhs77_available;
        public static BansheeGz.BGDatabase.BGFieldBool __generatedField___available => _ufle12jhs77_available ?? (_ufle12jhs77_available = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldBool>(MetaDefault, new BGId(5743820445426647001UL, 8184059962662492570UL), () => _ufle12jhs77_available = null));

        [HutongGames.PlayMaker.Tooltip("Query Column")]
        public FsmString _QueryColumn;

        public FsmBool _QueryAll;


        public FsmBool _QueryValue;

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
                    if (entity.Get<bool>(_QueryColumn.Value) == _QueryValue.Value)
                    {
                        _InsertIntoArray.Resize(_InsertIntoArray.Length + 1);
                        _InsertIntoArray.Set(_InsertIntoArray.Length - 1, entity.Index);
                    }
                });
            }
        }






    }


 
        
        
        
}