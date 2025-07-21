#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace BansheeGz.BGDatabase
{
    [ActionCategory("BansheeGz")]
    [HutongGames.PlayMaker.Tooltip("Load sprite addresses from Addressables system using a label. This class works inside Editor ONLY!")]
    public class BGAddressablesLoadSpriteLocations : FsmStateAction
    {
        public FsmString Label;

        [UIHint(UIHint.Variable)] [ArrayEditor(VariableType.String)]
        public FsmArray Result;

        public override void Reset()
        {
            Label = null;
            Result = null;
        }

        public override void OnEnter()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings != null)
            {
                var groups = settings.groups;
                if (groups != null)
                {
                    var result = new List<string>();
                    var label = Label.Value;
                    foreach (var addressableAssetGroup in groups)
                    {
                        var entries = addressableAssetGroup.entries;
                        if (entries == null) continue;

                        foreach (var entry in entries)
                        {
                            if (entry.TargetAsset == null) continue;
                            if (!string.IsNullOrEmpty(label) && (entry.labels == null || !entry.labels.Contains(label))) continue;

                            switch (entry.TargetAsset)
                            {
                                case SpriteAtlas _:
                                {
                                    var spriteAtlas = entry.TargetAsset as SpriteAtlas;
                                    var spritesArray = new Sprite[spriteAtlas.spriteCount];
                                    spriteAtlas.GetSprites(spritesArray);
                                    foreach (var sprite in spritesArray)
                                    {
                                        var subAssetPath = sprite.name;
                                        if (string.IsNullOrEmpty(subAssetPath)) continue;
                                        if (subAssetPath.EndsWith("(Clone)")) subAssetPath = subAssetPath.Substring(0, subAssetPath.Length - "(Clone)".Length);
                                        result.Add(new BGFieldUnitySprite.SpriteLocation(BGFieldUnitySprite.LocationEnum.SpriteAtlas, entry.address, subAssetPath).FullPath);
                                    }

                                    break;
                                }
                                case Texture2D _:
                                {
                                    var path = AssetDatabase.GetAssetPath(entry.TargetAsset);
                                    var importer = (TextureImporter) TextureImporter.GetAtPath(path);
                                    switch (importer.spriteImportMode)
                                    {
                                        case SpriteImportMode.None:
                                        case SpriteImportMode.Single:
                                        case SpriteImportMode.Polygon:
                                            result.Add(entry.address);
                                            break;
                                        case SpriteImportMode.Multiple:
                                            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                                            foreach (var sprite in sprites)
                                            {
                                                result.Add(new BGFieldUnitySprite.SpriteLocation(BGFieldUnitySprite.LocationEnum.Multiple, entry.address, sprite.name).FullPath);
                                            }

                                            result.Sort((s1, s2) =>
                                            {
                                                if (s1 == null || s2 == null) return string.CompareOrdinal(s1, s2);
                                                var i1 = s1.LastIndexOf('_');
                                                var i2 = s2.LastIndexOf('_');
                                                if (i1 == -1 || i2 == -1) return string.CompareOrdinal(s1, s2);
                                                try
                                                {
                                                    var index1 = int.Parse(s1.Substring(i1 + 1));
                                                    var index2 = int.Parse(s2.Substring(i2 + 1));
                                                    return index1.CompareTo(index2);
                                                }
                                                catch
                                                {
                                                    return string.CompareOrdinal(s1, s2);
                                                }
                                            });
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }

                                    break;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    Result.stringValues = result.ToArray();
                }
            }

            Finish();
        }
    }
}
#endif