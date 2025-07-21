/*
<copyright file="BGAddressablesSetAsset.cs" company="BansheeGz">
    Copyright (c) 2018-2020 All Rights Reserved
</copyright>
*/

using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Set address value to specified database entity and field")]
    public class BGAddressablesSetAsset : FsmStateAction
    {
        public string MetaId;
        public string FieldId;

        public FsmInt EntityIndex;

        public FsmString Address;

        public BGMetaEntity Meta => BGRepo.I[BGId.Parse(MetaId)];

        public BGField Field
        {
            get
            {
                var meta = Meta;
                if (meta == null) return null;
                var field = meta.GetField(BGId.Parse(FieldId), false);
                if (field == null) return null;
                if (!(field is BGAddressablesAssetI) || !(field is BGAssetLoaderA.WithLoaderI)) return null;
                var loader = (field as BGAssetLoaderA.WithLoaderI).AssetLoader;
                if (loader.GetType() != typeof(BGAssetLoaderAddressables)) return null;
                return field;
            }
        }

        public override void Reset()
        {
            MetaId = null;
            FieldId = null;
            EntityIndex = null;
            Address = null;
        }

        public override void OnEnter()
        {
            var field = Field;
            if (field == null) Debug.LogException(new Exception("BGAddressablesSetAsset: Field is not set!"));
            else
            {
                var entityIndex = EntityIndex.Value;
                if (entityIndex < 0 || entityIndex >= field.Meta.CountEntities) Debug.LogException(new Exception("BGAddressablesSetAsset: wrong entity index =" + entityIndex));
                else
                {
                    var addressablesField = field as BGAddressablesAssetI;
                    addressablesField.SetAssetPath(entityIndex, Address.Value);
                }
            }

            Finish();
        }
    }
}