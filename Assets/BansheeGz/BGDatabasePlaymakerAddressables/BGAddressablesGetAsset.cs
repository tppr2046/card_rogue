/*
<copyright file="BGAddressablesGetAsset.cs" company="BansheeGz">
    Copyright (c) 2018-2020 All Rights Reserved
</copyright>
*/

using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Fetch Unity asset from database using addressables system")]
    public class BGAddressablesGetAsset : FsmStateAction
    {
        public enum EntitySourceEnum
        {
            Index,
            Id,
            Name
        }

        public string MetaId;
        public string FieldId;

        public EntitySourceEnum EntitySource;

        public FsmInt EntityIndex;
        public FsmString EntityId;
        public FsmString EntityName;


        public FsmObject Result;
        public FsmEvent LoadedEvent;


        public BGMetaEntity Meta => BGRepo.I[BGId.Parse(MetaId)];

        public BGField Field
        {
            get
            {
                var meta = Meta;
                if (meta == null) return null;
                var field = meta.GetField(BGId.Parse(FieldId), false);
                if (field == null) return null;
                if (!(field is BGAddressablesAssetI) || !(field is BGAssetLoaderA.WithLoaderI withLoaderI)) return null;
                var loader = withLoaderI.AssetLoader;
                if (!(loader is BGAssetLoaderAddressables)) return null;
                return field;
            }
        }

        public override void Reset()
        {
            MetaId = null;
            FieldId = null;
            EntitySource = EntitySourceEnum.Index;
            EntityIndex = 0;
            EntityId = null;
            EntityName = null;
            Result = null;
            LoadedEvent = null;
        }

        public override void OnEnter()
        {
            var callbackAttached = false;
            var field = Field;
            if (field != null)
            {
                var addressablesField = field as BGAddressablesAssetI;
                string address = null;
                var entityIndex = -1;
                switch (EntitySource)
                {
                    case EntitySourceEnum.Index:
                        if (!EntityIndex.IsNone)
                        {
                            entityIndex = EntityIndex.Value;
                        }

                        break;
                    case EntitySourceEnum.Id:
                        if (!EntityId.IsNone)
                        {
                            var entity = field.Meta.GetEntity(BGId.Parse(EntityId.Value));
                            if (entity != null) entityIndex = entity.Index;
                        }

                        break;
                    case EntitySourceEnum.Name:
                        if (!EntityName.IsNone)
                        {
                            var entity = field.Meta.GetEntity(EntityName.Value);
                            if (entity != null) entityIndex = entity.Index;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException("EntitySource");
                }

                if (entityIndex >= 0)
                {
                    address = addressablesField.GetAddressablesAddress(entityIndex);
                    if (!string.IsNullOrEmpty(address))
                    {
                        var valueType = field.ValueType;
                        if (valueType == typeof(Sprite))
                        {
                            Addressables.LoadAssetAsync<Sprite>(address).Completed += LoadedSprite;
                        }
                        else
                        {
                            Addressables.LoadAssetAsync<Object>(address).Completed += Loaded;
                        }

                        callbackAttached = true;
                    }
                }
            }

            if (!callbackAttached) Done();
        }

        private void LoadedSprite(AsyncOperationHandle<Sprite> obj)
        {
            if (!Result.IsNone) Result.Value = obj.Result;
            Done();
        }

        private void Loaded(AsyncOperationHandle<Object> obj)
        {
            if (!Result.IsNone) Result.Value = obj.Result;
            Done();
        }

        private void Done()
        {
            Fsm.Event(LoadedEvent);
            Finish();
        }
    }
}