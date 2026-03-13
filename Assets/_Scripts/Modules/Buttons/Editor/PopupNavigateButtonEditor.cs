#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
namespace Modules.Buttons.Editor
{

    [CustomEditor(typeof(PopupNavigateButton))]
    public class PopupNavigateButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("PopupNavigateButton opens the selected EPopup when clicked.", MessageType.Info);
        }
    }
#endif
}