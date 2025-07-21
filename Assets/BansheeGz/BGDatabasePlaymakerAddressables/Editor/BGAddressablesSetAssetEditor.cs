/*
<copyright file="BGAddressablesSetAssetEditor.cs" company="BansheeGz">
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
    [CustomActionEditor(typeof(BGAddressablesSetAsset))]
    public class BGAddressablesSetAssetEditor : CustomActionEditor
    {
        private const int labelWidth = 80;
        private bool changed;
        public override bool OnGUI()
        {
            var action = target as BGAddressablesSetAsset;

            var meta = action.Meta;

            BGEditorUtility.Horizontal(() =>
            {
                BGEditorUtility.Label("Meta", labelWidth);
                if (!BGEditorUtility.Button(meta == null ? "None" : meta.Name)) return;

                var tree = new BGTreeViewMeta(null);

                BGPopup.Popup("Select meta", 450, 400, popup => { tree.Gui(); }, popup =>
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
            BGEditorUtility.Horizontal(() =>
            {
                var field = action.Field;
                BGEditorUtility.Label("Field", labelWidth);
                if (!BGEditorUtility.Button(field == null ? "None" : field.Name)) return;

                var fields = meta.FindFields(null, f => f is BGAddressablesAssetI && ((BGAssetLoaderA.WithLoaderI) f).AssetLoader.GetType() == typeof(BGAssetLoaderAddressables));
                BGPopup.Popup("Select field", 450, 400, popup =>
                {
                    if (fields.Count == 0)
                    {
                        BGEditorUtility.HelpBox("This meta does not have any Unity asset field with Addressables loader", MessageType.Info);
                    }
                    else
                    {
                        foreach (var f in fields)
                        {
                            if (!BGEditorUtility.Button(f.Name)) continue;
                            action.FieldId = f.Id.ToString();
                            changed = true;
                            popup.Close();
                        }
                    }
                });
            });

            EditField("EntityIndex");
            EditField("Address");

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