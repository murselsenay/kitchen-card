using System;
using System.IO;
using UnityEngine;
using Modules.Logger;

namespace Modules.SaveSystem
{
    /// <summary>
    /// All files are written under Application.persistentDataPath/Saves/.
    /// The path parameter can include subfolders: "TangramLevels/level_001"
    /// </summary>
    public static class SaveManager
    {
        #region Private Fields
        private static string Root => Path.Combine(Application.persistentDataPath, "Saves");
        #endregion

        #region Public Methods
        public static void Save<T>(T data, string path)
        {
            string fullPath = FullPath(path);
            EnsureDirectory(fullPath);

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(fullPath, json);

            DebugLogger.Log($"Saved -> {fullPath}");
        }

        public static T Load<T>(string path)
        {
            string fullPath = FullPath(path);

            if (!File.Exists(fullPath))
            {
                DebugLogger.LogWarning($"Save file not found: {fullPath}");
                return default;
            }

            string json = File.ReadAllText(fullPath);
            return JsonUtility.FromJson<T>(json);
        }
        #endregion

        #region Helpers
        public static bool Exists(string path) => File.Exists(FullPath(path));

        public static void Delete(string path)
        {
            string fullPath = FullPath(path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public static string[] GetAllPaths(string folder = "")
        {
            string dir = string.IsNullOrEmpty(folder)
                ? Root
                : Path.Combine(Root, folder);

            if (!Directory.Exists(dir))
                return Array.Empty<string>();

            return Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
        }

        private static string FullPath(string path) =>
            Path.Combine(Root, $"{path}.json");

        private static void EnsureDirectory(string fullPath)
        {
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        #endregion
    }
}
