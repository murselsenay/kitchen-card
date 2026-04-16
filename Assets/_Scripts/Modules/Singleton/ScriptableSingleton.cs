using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.Singleton
{
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private const string ResourcesRoot = "Scriptables/Singletons";
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadOrCreateInstance();
                }

                return _instance;
            }
        }

        private static T LoadOrCreateInstance()
        {
            var resourcePath = $"{ResourcesRoot}/{typeof(T).Name}";
            var instance = Resources.Load<T>(resourcePath);

#if UNITY_EDITOR
            WarnIfDuplicateAssetsExist();

            if (instance == null)
            {
                instance = LoadOrCreateEditorAsset();
            }
#endif

            if (instance == null)
            {
                instance = CreateInstance<T>();
                instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
                Debug.LogWarning($"[{typeof(T).Name}] Resources asset not found. Created a temporary in-memory instance.", instance);
            }

            return instance;
        }

#if UNITY_EDITOR
        private static void WarnIfDuplicateAssetsExist()
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length <= 1)
            {
                return;
            }

            var assetPaths = new System.Text.StringBuilder();
            for (int i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                if (assetPaths.Length > 0)
                {
                    assetPaths.Append(", ");
                }

                assetPaths.Append(assetPath);
            }

            Debug.LogWarning($"[{typeof(T).Name}] Multiple singleton assets found. Runtime loads Assets/Resources/Scriptables/Singletons/{typeof(T).Name}.asset. Found: {assetPaths}");
        }

        private static T LoadOrCreateEditorAsset()
        {
            var assetDirectory = Path.Combine("Assets", "Resources", "Scriptables", "Singletons");
            var assetPath = Path.Combine(assetDirectory, $"{typeof(T).Name}.asset").Replace("\\", "/");

            Directory.CreateDirectory(assetDirectory);

            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            return asset;
        }
#endif
    }
}