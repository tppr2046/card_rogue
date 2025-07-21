/*
<copyright file="BGAddressablesGetAssets.cs" company="BansheeGz">
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
    [HutongGames.PlayMaker.Tooltip("Fetch Unity assets from database using addressables system")]
    public class BGAddressablesGetAssets : FsmStateAction
    {
        public string MetaId;

        [RequiredField] [ArrayEditor(VariableType.String)]
        public FsmArray FieldNames;

        public BGAddressablesGetAsset.EntitySourceEnum EntitySource;

        public FsmInt EntityIndex;
        public FsmString EntityId;
        public FsmString EntityName;


        [RequiredField] [ArrayEditor(VariableType.Object)] [UIHint(UIHint.Variable)]
        public FsmArray Result;

        [RequiredField] 
        public FsmEvent LoadedEvent;


        public BGMetaEntity Meta => BGRepo.I[BGId.Parse(MetaId)];

        private int counter;

        public BGAddressablesAssetI GetField(BGMetaEntity meta, string fieldName)
        {
            var field = meta.GetField(fieldName, false);
            if (field == null) return null;
            if (!(field is BGAddressablesAssetI) || !(field is BGAssetLoaderA.WithLoaderI withLoaderI)) return null;
            var loader = withLoaderI.AssetLoader;
            if (!(loader is BGAssetLoaderAddressables)) return null;
            return withLoaderI as BGAddressablesAssetI;
        }

        public override void Reset()
        {
            MetaId = null;
            FieldNames = null;
            EntitySource = BGAddressablesGetAsset.EntitySourceEnum.Index;
            EntityIndex = 0;
            EntityId = null;
            EntityName = null;
            Result = null;
            LoadedEvent = null;
            counter = 0;
        }

        public override void OnEnter()
        {
            Result.Resize(FieldNames.Length);
            for (var i = 0; i < FieldNames.Length; i++) Result.Set(i, null);

            counter = 0;
            var meta = Meta;
            if (meta != null && !Result.IsNone)
            {
                var entityIndex = -1;
                switch (EntitySource)
                {
                    case BGAddressablesGetAsset.EntitySourceEnum.Index:
                        if (!EntityIndex.IsNone)
                        {
                            entityIndex = EntityIndex.Value;
                        }

                        break;
                    case BGAddressablesGetAsset.EntitySourceEnum.Id:
                        if (!EntityId.IsNone)
                        {
                            var entity = meta.GetEntity(BGId.Parse(EntityId.Value));
                            if (entity != null) entityIndex = entity.Index;
                        }

                        break;
                    case BGAddressablesGetAsset.EntitySourceEnum.Name:
                        if (!EntityName.IsNone)
                        {
                            var entity = meta.GetEntity(EntityName.Value);
                            if (entity != null) entityIndex = entity.Index;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException("EntitySource");
                }

                if (entityIndex >= 0)
                {
                    for (var i = 0; i < FieldNames.Length; i++)
                    {
                        var fieldName = FieldNames.stringValues[i];
                        var field = GetField(meta, fieldName);
                        if (field == null) continue;

                        var address = field.GetAddressablesAddress(entityIndex);
                        if (string.IsNullOrEmpty(address)) continue;

                        counter++;
                        var index = i;
                        var valueType = (field as BGField).ValueType;
                        if (valueType == typeof(Sprite))
                        {
                            var operationHandle = Addressables.LoadAssetAsync<Sprite>(address);
                            if (operationHandle.Result != null) LoadedSprite(operationHandle, index);
                            else operationHandle.Completed += handle => LoadedSprite(handle, index);
                        }
                        else
                        {
                            var operationHandle = Addressables.LoadAssetAsync<Object>(address);
                            if (operationHandle.Result != null) Loaded(operationHandle, index);
                            else operationHandle.Completed += handle => Loaded(handle, index);
                        }
                    }
                }
            }

            if (counter <= 0) Done();
        }

        private void LoadedSprite(AsyncOperationHandle<Sprite> obj, int index)
        {
            if (!Result.IsNone) Result.Set(index, obj.Result);
            Done();
        }

        private void Loaded(AsyncOperationHandle<Object> obj, int index)
        {
            if (!Result.IsNone) Result.Set(index, obj.Result);
            Done();
        }

        private void Done()
        {
            counter--;
            if (counter > 0) return;
            Fsm.Event(LoadedEvent);
            Finish();
        }
    }
}