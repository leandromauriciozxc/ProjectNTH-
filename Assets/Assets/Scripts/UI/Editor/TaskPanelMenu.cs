using UnityEditor;
using UnityEngine;

namespace ProjectNTH.UI.Editor
{
    public static class TaskPanelMenu
    {
        [MenuItem("Tools/Project NTH/Create Task Panel")]
        private static void Create()
        {
            // One reminder per scene: selecting the existing one avoids overlapping HUDs.
            foreach (var panel in Object.FindObjectsOfType<TaskPanelUI>(true))
            {
                if (panel.gameObject.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene()) continue;
                Selection.activeGameObject = panel.gameObject;
                EditorGUIUtility.PingObject(panel);
                return;
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Prefabs/Task Panel.prefab");
            if (prefab == null)
            {
                Debug.LogError("Task Panel prefab could not be found.");
                return;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Create task panel");
            Selection.activeGameObject = instance;
        }

        [MenuItem("Tools/Project NTH/Create Task Panel", true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
