/*
<copyright file="BGAddressablesGetAssetEditor.cs" company="BansheeGz">
    Copyright (c) 2018-2020 All Rights Reserved
</copyright>
*/
using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMakerEditor;
using UnityEditor;
using UnityEngine;

namespace BansheeGz.BGDatabase.Editor
{
    [CustomActionEditor(typeof(BGAddressablesGetAssets))]
    public class BGAddressablesGetAssetsEditor : CustomActionEditor
    {
        private const int labelWidth = 80;
        private bool changed;
        
        public override bool OnGUI()
        {
            var action = target as BGAddressablesGetAssets;

            var meta = action.Meta;

            BGEditorUtility.Horizontal(() =>
            {
                GUILayout.Label("Meta", GUILayout.Width(labelWidth));
                if (!BGEditorUtility.Button(meta == null ? "None" : meta.Name)) return;

                var tree = new BGTreeViewMeta(null);

                BGPopup.Popup("Select meta",450, 400, popup => { tree.Gui(); }, popup =>
                {
                    tree.OnSelect = m =>
                    {
                        var newMeta = (BGMetaEntity) m;
                        action.MetaId = newMeta.Id.ToString();
                        changed = true;
                        popup.Close();
                    };
                });
            });

            EditField("EntitySource");
            switch (action.EntitySource)
            {
                case BGAddressablesGetAsset.EntitySourceEnum.Index:
                    EditField("EntityIndex");
                    break;
                case BGAddressablesGetAsset.EntitySourceEnum.Id:
                    EditField("EntityId");
                    break;
                case BGAddressablesGetAsset.EntitySourceEnum.Name:
                    EditField("EntityName");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action.EntitySource");
            }
            EditField("FieldNames");

            EditField("Result");
            EditField("LoadedEvent");

            try
            {
                return changed || GUI.changed;
            }
            finally
            {
                changed = false;
            }
        }
    }
}