using UnityEditor;
using UnityEngine;
namespace Utilities.Editor
{
    [InitializeOnLoad]
    public class HierarchyHighlighter
    {
        static HierarchyHighlighter()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HighlightHierarchyItem;
        }

        private static void HighlightHierarchyItem(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj != null && obj.name.StartsWith("#"))
            {
                EditorGUI.DrawRect(selectionRect, new Color(0.15f, 0.15f, 0.15f, 1f));

                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                style.normal.textColor = Color.yellow;

                string cleanName = obj.name.Substring(1).ToUpper();
                EditorGUI.LabelField(selectionRect, cleanName, style);
            }
        }
    }
}