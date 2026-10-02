using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectNTH.Dialogue.Editor
{
    public static class ThoughtDialogueMenu
    {
        [MenuItem("Tools/Project NTH/Create Thought Dialogue")]
        private static void Create()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var existing in Object.FindObjectsOfType<ThoughtDialogueSystem>(true))
                if (existing.gameObject.scene == scene)
                {
                    Selection.activeGameObject = existing.gameObject;
                    EditorGUIUtility.PingObject(existing);
                    return;
                }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Prefabs/Thought Dialogue.prefab");
            if (prefab == null) { Debug.LogError("Thought Dialogue prefab could not be found."); return; }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Create thought dialogue");
            var settings = new SerializedObject(instance.GetComponent<ThoughtDialogueSystem>());
            foreach (var controller in Object.FindObjectsOfType<DialogueSystemController>(true))
                if (controller.gameObject.scene == scene && controller.DialogueRunner != null)
                {
                    settings.FindProperty("conversationRunner").objectReferenceValue = controller.DialogueRunner;
                    settings.ApplyModifiedProperties();
                    break;
                }
            Selection.activeGameObject = instance;
        }

        [MenuItem("Tools/Project NTH/Create Thought Dialogue", true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
