using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BansheeGz.BGDatabase.Editor;
using UnityEngine;

namespace BansheeGz.BGDatabase
{
    public class BGGoogleSheetsManagerGo : MonoBehaviour
    {
        //=============================================================================================
        //                         static
        //=============================================================================================
        private static readonly object lastRunLogMutex = new object();
        private const string OkMessage = "Ok.";
        private const int GoogleSheetsButtonWidth = 140;
        private const char EncoderSeparator = '|';

        //=============================================================================================
        //                         serializable
        //=============================================================================================
        [Header("GUI")] public bool DisableGUI;
        public Vector2 GUIPosition;
        public bool HideExportButton;
        public bool HideImportButton;

        [Header("DataSource")] public BGDsGoogleSheets.DataSourceTypeEnum DataSourceType;
        public string ClientId;
        public string ClientSecret;
        public string ApplicationName;
        public string SpreadSheetId;
        public string AccessToken;
        public string RefreshToken;
        public string APIKey;
        public string ClientEmail;
        [TextArea] public string PrivateKey;

        //settings
        [Header("Settings")] public bool TransferRowsOrder;
        public bool UpdateIdsOnImport;
        public bool EnableSettings;
        public string RowHandlerClass;
        public BGMergeModeEnum Mode;
        public bool GlobalAddMissing;
        public bool GlobalRemoveOrphaned;
        public bool GlobalUpdateMatching;
        public List<string> MetaSettingsList;

        //=============================================================================================
        //                         not serializable
        //=============================================================================================
        private Thread worker;
        private string lastRunLog;
        private BGLogger lastRunLogger;
        private Texture2D blackTexture;
        private bool guiExpanded;
        private Vector2 scrollPosition;

        //=============================================================================================
        //                         Properties
        //=============================================================================================
        private bool IsRunning
        {
            get { return worker != null; }
        }

        public BGDsGoogleSheets DataSource
        {
            get
            {
                return new BGDsGoogleSheets
                {
                    AccessToken = AccessToken,
                    ApplicationName = ApplicationName,
                    ClientEmail = ClientEmail,
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    PrivateKey = PrivateKey,
                    RefreshToken = RefreshToken,
                    DataSourceType = DataSourceType,
                    SpreadSheetId = SpreadSheetId,
                    APIKey = APIKey,
                };
            }
        }

        public string LastRunLog
        {
            get
            {
                lock (lastRunLogMutex) return lastRunLog;
            }
            set
            {
                lock (lastRunLogMutex) lastRunLog = value;
            }
        }

        public bool LastRunOk
        {
            get { return string.Equals(LastRunLog, OkMessage); }
        }


        public BGLogger LastRunLogger
        {
            get
            {
                lock (lastRunLogMutex) return lastRunLogger;
            }
            set
            {
                lock (lastRunLogMutex) lastRunLogger = value;
            }
        }

        public BGMergeSettingsEntity Settings
        {
            get
            {
                if (!EnableSettings)
                {
                    return new BGMergeSettingsEntity()
                    {
                        Mode = BGMergeModeEnum.Merge,
                        AddMissing = true,
                        RemoveOrphaned = true,
                        UpdateMatching = true,
                    };
                }
                else
                {
                    var settings = new BGMergeSettingsEntity
                    {
                        Mode = Mode,
                        AddMissing = GlobalAddMissing,
                        RemoveOrphaned = GlobalRemoveOrphaned,
                        UpdateMatching = GlobalUpdateMatching,
                    };
                    if (MetaSettingsList != null)
                    {
                        foreach (var setting in MetaSettingsList)
                        {
                            BGId metaId;
                            var decoded = Decode(setting, out metaId);
                            if (decoded == null) continue;
                            settings.Ensure(metaId).CopyFrom(decoded);
                        }
                    }

                    return settings;
                }
            }
        }

        //=============================================================================================
        //                         Unity Callbacks
        //=============================================================================================
        void OnGUI()
        {
            if (DisableGUI) return;

            if (guiExpanded)
            {
                Area(new Rect(GUIPosition, new Vector2(500, 240)), () =>
                {
                    if (GUILayout.Button("GoogleSheets<<", GUILayout.Width(GoogleSheetsButtonWidth))) guiExpanded = false;
                    if (IsRunning) GUILayout.Label("GoogleSheets GUI: Job is running, please wait...");
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("GoogleSheets operations:");
                        if (!HideExportButton && GUILayout.Button("Export", GUILayout.Width(80))) Export();
                        if (!HideImportButton && GUILayout.Button("Import", GUILayout.Width(80))) Import();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Label("Last run log: " + (LastRunLog ?? "N/A"));
                    }

                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    var lastLogger = LastRunLogger;
                    GUILayout.TextArea(lastLogger == null ? "" : lastLogger.Log, new GUIStyle {richText = true}, GUILayout.ExpandHeight(true));
                    GUILayout.EndScrollView();
                });
            }
            else
            {
                if (GUI.Button(new Rect(GUIPosition, new Vector2(GoogleSheetsButtonWidth, 22)), "GoogleSheets>>")) guiExpanded = true;
            }
        }

        //=============================================================================================
        //                         Import
        //=============================================================================================
        public void Import()
        {
            var repo = Settings.NewRepo(BGRepo.I, true);
            Run((service, settings, logger) =>
            {
                ImportInternal(service, settings, repo, logger);
            }, (service, settings) => StartCoroutine(CheckWorkerImport(repo, settings)));
        }

        private void ImportInternal(BGGoogleSheetServiceI service, BGMergeSettingsEntity settings, BGRepo repo, BGLogger logger)
        {
            try
            {
                service.Import(logger, repo, settings, new BGMergeSettingsMeta(), UpdateIdsOnImport, TransferRowsOrder);
                LastRunLog = OkMessage;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                LastRunLog = "Error: " + e.Message;
            }
            finally
            {
                LastRunLogger = logger;
            }
        }

        private IEnumerator CheckWorkerImport(BGRepo repo, BGMergeSettingsEntity settings)
        {
            yield return null;
            while (worker.IsAlive) yield return null;
            worker = null;
            if (LastRunOk)
            {
                //we must access main database (BGRepo.I) only from the main thread
                new BGMergerEntity(new BGLogger(), repo, BGRepo.I, settings).Merge();
            }
        }

        //=============================================================================================
        //                         Export
        //=============================================================================================
        public void Export()
        {
            if (DataSourceType == BGDsGoogleSheets.DataSourceTypeEnum.Anonymous || DataSourceType == BGDsGoogleSheets.DataSourceTypeEnum.APIKey)
            {
                LastRunLog = "Error: Your datasource type does not support exporting. Use OAuth or Service datasource.";
            }
            else
            {
                Run((service, settings, logger) =>
                {
                    ExportInternal(service, settings, settings.NewRepo(BGRepo.I, true), logger);
                }, (service, settings) => StartCoroutine(CheckWorkerExport()));
            }
        }

        //This is running in another thread : could we use  await/async?
        public void ExportInternal(BGGoogleSheetServiceI service, BGMergeSettingsEntity settings, BGRepo repo, BGLogger logger)
        {
            try
            {
                service.Export(logger, repo, settings, new BGMergeSettingsMeta(), TransferRowsOrder);
                LastRunLog = OkMessage;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                LastRunLog = "Error: " + e.Message;
            }
            finally
            {
                LastRunLogger = logger;
            }
        }

        private IEnumerator CheckWorkerExport()
        {
            yield return null;
            while (worker.IsAlive) yield return null;
            worker = null;
        }

        //=============================================================================================
        //                         encoding/decoding custom settings
        //=============================================================================================
        public static BGMergeSettingsEntity.MetaSettings Decode(string setting, out BGId metaId)
        {
            metaId = BGId.Empty;
            var decoded = new BGMergeSettingsEntity.MetaSettings();
            try
            {
                const int minTokens = 6;
                if (string.IsNullOrEmpty(setting)) return null;
                var tokens = setting.Split(EncoderSeparator);
                if (tokens.Length < minTokens) return null;
                metaId = BGId.Parse(tokens[0]);
                if (metaId.IsEmpty) return null;
                if (!BGRepo.I.HasMeta(metaId)) return null;
                decoded.AddMissing = ParseBool(tokens[1]);
                decoded.RemoveOrphaned = ParseBool(tokens[2]);
                decoded.UpdateMatching = ParseBool(tokens[3]);
                decoded.UseIncludedFields = ParseBool(tokens[4]);
                var fieldsCount = ParseInt(tokens[5]);
                if (fieldsCount > 0)
                {
                    var meta = BGRepo.I.GetMeta(metaId);
                    if (tokens.Length < minTokens + fieldsCount) return null;
                    for (var i = 0; i < fieldsCount; i++)
                    {
                        var idStr = tokens[minTokens + i];
                        var fieldId = ParseId(idStr);
                        if (!meta.HasField(fieldId)) continue;
                        decoded.AddField(fieldId);
                    }
                }
            }
            catch
            {
                return null;
            }

            return decoded;
        }

        public static string Encode(BGMergeSettingsEntity.MetaSettings setting, BGId metaId)
        {
            if (setting == null || metaId.IsEmpty) return null;
            var result = new StringBuilder();
            result.Append(metaId.ToString());
            AddSpace(result);
            AddBool(result, setting.AddMissing);
            AddBool(result, setting.RemoveOrphaned);
            AddBool(result, setting.UpdateMatching);
            AddBool(result, setting.UseIncludedFields);
            if (!setting.UseIncludedFields) AddInt(result, 0);
            else
            {
                AddInt(result, setting.CountFields);
                setting.ForEachField(id => AddId(result, id));
            }

            return result.ToString();
        }

        private static void AddId(StringBuilder result, BGId value)
        {
            result.Append(value);
            AddSpace(result);
        }

        private static void AddInt(StringBuilder result, int value)
        {
            result.Append(value);
            AddSpace(result);
        }

        private static void AddBool(StringBuilder result, bool value)
        {
            result.Append(value);
            AddSpace(result);
        }

        private static void AddSpace(StringBuilder result)
        {
            result.Append(EncoderSeparator);
        }

        private static BGId ParseId(string idStr)
        {
            return BGId.Parse(idStr);
        }

        private static bool ParseBool(string stringValue)
        {
            bool result;
            if (!bool.TryParse(stringValue, out result)) throw new Exception("Oops, string " + stringValue + " is not a valid bool value");
            return result;
        }

        private static int ParseInt(string stringValue)
        {
            int result;
            if (!int.TryParse(stringValue, out result)) throw new Exception("Oops, string " + stringValue + " is not a valid int value");
            return result;
        }

        //=============================================================================================
        //                         unclassified methods
        //=============================================================================================
        private void Run(Action<BGGoogleSheetServiceI, BGMergeSettingsEntity, BGLogger> action, Action<BGGoogleSheetServiceI, BGMergeSettingsEntity> startMonitor)
        {
            try
            {
                CheckRunning();
                CheckSettings();
                var settings = Settings;
                if (!settings.HasAny(BGRepo.I)) throw new Exception("Your settings does not have any table to update");
                LastRunLogger = null;
                LastRunLog = "running...";

                var dataSource = DataSource;
                var logger = new BGLogger();
                var service = dataSource.TryToCreateService(logger);
                worker = new Thread(() => action(service, settings, logger));
                worker.Start();
                startMonitor(service, settings);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                LastRunLog = "Error: " + e.Message;
            }
        }


        private void CheckRunning()
        {
            if (IsRunning) throw new Exception("Job is already running");
        }


        private void CheckSettings()
        {
            ThrowIfEmpty(SpreadSheetId, "SpreadSheetId is not set");
            switch (DataSourceType)
            {
                case BGDsGoogleSheets.DataSourceTypeEnum.OAuth:
                    ThrowIfEmpty(ClientId, "ClientId is not set");
                    ThrowIfEmpty(ClientSecret, "ClientSecret is not set");
                    ThrowIfEmpty(ApplicationName, "ApplicationName is not set");
                    ThrowIfEmpty(AccessToken, "AccessToken is not set");
                    ThrowIfEmpty(RefreshToken, "RefreshToken is not set");
                    break;
                case BGDsGoogleSheets.DataSourceTypeEnum.Service:
                    ThrowIfEmpty(ClientEmail, "ClientEmail is not set");
                    ThrowIfEmpty(PrivateKey, "PrivateKey is not set");
                    break;
                case BGDsGoogleSheets.DataSourceTypeEnum.APIKey:
                    ThrowIfEmpty(APIKey, "APIKey is not set");
                    break;
                case BGDsGoogleSheets.DataSourceTypeEnum.Anonymous:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("DataSourceType");
            }
        }

        private void Area(Rect rect, Action action)
        {
            if (blackTexture == null)
            {
                blackTexture = new Texture2D(1, 1);
                blackTexture.SetPixel(0, 0, new Color(0, 0, 0, .4f));
                blackTexture.Apply();
            }

            GUI.DrawTexture(rect, blackTexture);
            GUILayout.BeginArea(rect);
            action();
            GUILayout.EndArea();
        }

        private static void ThrowIfEmpty(string parameter, string message)
        {
            if (!string.IsNullOrEmpty(parameter)) return;
            throw new Exception(message);
        }
    }
}