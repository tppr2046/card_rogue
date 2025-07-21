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
    [CustomActionEditor(typeof(BGAddressablesGetAsset))]
    public class BGAddressablesGetAssetEditor : CustomActionEditor
    {
        private const int labelWidth = 80;
        private bool changed;
        
        public override bool OnGUI()
        {
            var action = target as BGAddressablesGetAsset;

            var meta = action.Meta;

            BGEditorUtility.Horizontal(() =>
            {
                BGEditorUtility.Label("Meta", labelWidth);
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
            BGEditorUtility.Horizontal(() =>
            {
                var field = action.Field;
                BGEditorUtility.Label("Field", labelWidth);
                if (!BGEditorUtility.Button(field == null ? "None" : field.Name)) return;

                var fields = meta.FindFields(null, f => f is BGAddressablesAssetI && ((BGAssetLoaderA.WithLoaderI)f).AssetLoader is BGAssetLoaderAddressables);
                BGPopup.Popup("Select field",450, 400, popup =>
                {
                    if (fields.Count==0)
                    {
                        BGEditorUtility.HelpBox("This meta does not have any Unity asset field with Addressables loader", MessageType.Info);
                    }
                    else
                    {
                        foreach (var f in fields)
                        {
                            if(!BGEditorUtility.Button(f.Name)) continue;
                            action.FieldId = f.Id.ToString();
                            changed = true;
                            popup.Close();
                        }
                    }
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
            EditField("Result");
            EditField("LoadedEvent");
            

/*

            // If you need to reference the action directly:
            var action = target as BGAddressablesGetAsset;

            // You can draw the full default inspector.

            GUILayout.Label("Default Inspector:", EditorStyles.boldLabel);

            var isDirty = DrawDefaultInspector();

            // Or draw individual controls

            GUILayout.Label("Field Controls:", EditorStyles.boldLabel);

            EditField("logLevel");
            EditField("floatVariable");

            // Or add your own controls using any GUILayout method

            GUILayout.Label("Your Controls:", EditorStyles.boldLabel);

            if (GUILayout.Button("Press Me"))
            {
                EditorUtility.DisplayDialog("My Dialog", "Hello", "OK");
                isDirty = true; // e.g., if you change action data
            }

            // OnGUI should return true if you change action data!

*/
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