using System.Collections.Generic;
using Game.Core.Enums;
using Game.Models.Recipes;

namespace Game.Core.Extensions
{
    public static class RecipeScriptableExtensions
    {

        // Simple CSV parser for quoted fields
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var field = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            result.Add(field.ToString());
            return result;
        }

        public static void FillIngredientsFromCsv(this RecipeScriptable recipe)
        {
#if UNITY_EDITOR
            string csvPath = "Assets/CSV/Recipes.csv";
            if (!System.IO.File.Exists(csvPath))
            {
                UnityEngine.Debug.LogError($"CSV file not found at {csvPath}");
                return;
            }
            var lines = System.IO.File.ReadAllLines(csvPath);
            int ingredientsCol = -1;
            int nameCol = -1;
            var headers = ParseCsvLine(lines[0]);
            for (int i = 0; i < headers.Count; i++)
            {
                if (headers[i].Trim().Equals("Ingredients", System.StringComparison.OrdinalIgnoreCase))
                    ingredientsCol = i;
                if (headers[i].Trim().Equals("Name", System.StringComparison.OrdinalIgnoreCase))
                    nameCol = i;
            }
            if (ingredientsCol == -1 || nameCol == -1)
            {
                UnityEngine.Debug.LogError("CSV must have 'Name' and 'Ingredients' columns.");
                return;
            }
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                if (cols.Count <= ingredientsCol || cols.Count <= nameCol)
                    continue;
                string csvName = cols[nameCol].Replace("_", " ").Trim();
                if (csvName.Equals(recipe.GetName(), System.StringComparison.OrdinalIgnoreCase))
                {
                    var ingredientStrs = cols[ingredientsCol].Split(new[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    recipe.GetIngredients().Clear();
                    var missingEnums = new List<string>();
                    foreach (var ing in ingredientStrs)
                    {
                        string ingTrim = ing.Trim(' ', '"', '\r', '\n');
                        string normalized = ingTrim.Replace(" ", "_");
                        if (System.Enum.TryParse<EIngredientType>(normalized, true, out var ingEnum))
                        {
                            recipe.GetIngredients().Add(ingEnum);
                        }
                        else
                        {
                            missingEnums.Add(ingTrim);
                        }
                    }
                    if (missingEnums.Count > 0)
                    {
                        UnityEngine.Debug.LogWarning($"Missing EIngredientType enums: {string.Join(", ", missingEnums)}");
                    }
                    UnityEditor.EditorUtility.SetDirty(recipe);
                    UnityEditor.AssetDatabase.SaveAssets();
                    break;
                }
            }
#endif
        }
    }
}