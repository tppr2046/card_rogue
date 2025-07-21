using System;
using UnityEditor;
using UnityEngine;

namespace BansheeGz.BGDatabase.Editor
{
    [CustomEditor(typeof(BGGoogleSheetsManagerGo))]
    public class BGGoogleSheetsManagerGoEditor : UnityEditor.Editor
    {
        private BGGoogleSheetsManagerGo manager;

        private GUILayoutOption heightOption;

        private GUILayoutOption HeightOptions
        {
            get { return heightOption ?? (heightOption = GUILayout.Height(BGEditorUtility.MinRowHeight)); }
        }

        private void OnEnable()
        {
            manager = (BGGoogleSheetsManagerGo) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //GUI
            Property("DisableGUI");
            Property("GUIPosition");
            Property("HideExportButton");
            Property("HideImportButton");

            //datasource
            Property("DataSourceType");
            switch (manager.DataSourceType)
            {
                case BGDsGoogleSheets.DataSourceTypeEnum.OAuth:
                    Property("ClientId");
                    Property("ClientSecret");
                    Property("ApplicationName");
                    Property("AccessToken");
                    Property("RefreshToken");
                    break;
                case BGDsGoogleSheets.DataSourceTypeEnum.Service:
                    Property("ClientEmail");
                    Property("PrivateKey");
                    break;
                case BGDsGoogleSheets.DataSourceTypeEnum.APIKey:
                    Property("APIKey");
                    break;
                case BGDsGoogleSheets.DataSourceTypeEnum.Anonymous:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Property("SpreadSheetId");

            //settings
            Property("TransferRowsOrder");
            Property("UpdateIdsOnImport");
            Property("EnableSettings");
            if (manager.EnableSettings)
            {
                Property("Mode");
                Property("GlobalAddMissing");
                Property("GlobalRemoveOrphaned");
                Property("GlobalUpdateMatching");

                var settings = manager.Settings;
                EditorGUILayout.LabelField("Meta list:", EditorStyles.boldLabel);
                BGRepo.I.ForEachMeta(meta =>
                {
                    BGEditorUtility.Horizontal(() =>
                    {
                        var included = settings.IsMetaIncluded(meta.Id);
                        GUILayout.Label(meta.Name, included ? BGStyle.CellIncluded : BGStyle.Cell, HeightOptions);
                        var hasCustomSettings = settings.Has(meta.Id);
                        if (hasCustomSettings)
                        {
                            if (GUILayout.Button("Delete custom settings", BGStyle.CellButton, GUILayout.Width(150), HeightOptions))
                            {
                                settings.Remove(meta.Id);
                                Encode(settings);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Create custom settings", BGStyle.CellButton, GUILayout.Width(150), HeightOptions))
                            {
                                settings.Ensure(meta.Id);
                                Encode(settings);
                            }
                        }
                    }, HeightOptions);
                    var metaIncluded = settings.Has(meta.Id);
                    if (metaIncluded)
                    {
                        var setting = settings.GetSettings(meta.Id);
                        BGEditorUtility.Horizontal(() =>
                        {
                            GUILayout.Space(16);
                            BoolField(settings, "Add missing", setting.AddMissing, b => setting.AddMissing = b);
                            BoolField(settings, "Remove orphaned", setting.RemoveOrphaned, b => setting.RemoveOrphaned = b);
                            BoolField(settings, "Update matching", setting.UpdateMatching, b => setting.UpdateMatching = b);
                            BoolField(settings, "Chose fields to update", setting.UseIncludedFields, b => setting.UseIncludedFields = b);
                        }, HeightOptions);

                        if (setting.UseIncludedFields)
                        {
                            meta.ForEachField(field =>
                            {
                                BGEditorUtility.Horizontal(() =>
                                {
                                    GUILayout.Space(32);
                                    var hasCustom = setting.HasField(field.Id);
                                    GUILayout.Label(field.Name, settings.IsFieldIncluded(field) ? BGStyle.CellIncluded : BGStyle.Cell, HeightOptions);
                                    var newHasCustom = GUILayout.Toggle(hasCustom, "", BGStyle.CellToggle, GUILayout.Width(30), HeightOptions);
                                    if (hasCustom != newHasCustom)
                                    {
                                        if (newHasCustom) setting.AddField(field.Id);
                                        else setting.RemoveField(field.Id);
                                        Encode(settings);
                                    }
                                }, HeightOptions);
                            });
                        }
                    }
                });
            }


            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }

        private void BoolField(BGMergeSettingsEntity settings, string message, bool value, Action<bool> setter)
        {
            var newVal = GUILayout.Toggle(value, message, BGStyle.CellToggle, HeightOptions);
            if (value == newVal) return;
            setter(newVal);
            Encode(settings);
        }

        private void Encode(BGMergeSettingsEntity settings)
        {
            var metaList = Find("MetaSettingsList");
            metaList.ClearArray();
            BGRepo.I.ForEachMeta(meta =>
            {
                var setting = settings.GetSettings(meta.Id);
                if (setting == null) return;
                metaList.InsertArrayElementAtIndex(0);
                var prop = metaList.GetArrayElementAtIndex(0);
                prop.stringValue = BGGoogleSheetsManagerGo.Encode(setting, meta.Id);
            });
        }

        private void Property(string name)
        {
            EditorGUILayout.PropertyField(Find(name));
        }

        private SerializedProperty Find(string name)
        {
            return serializedObject.FindProperty(name);
        }
    }
}