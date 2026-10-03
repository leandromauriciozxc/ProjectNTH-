using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectNTH.SceneFlow.Editor
{
    [CustomEditor(typeof(SceneTransitionLoader))]
    public sealed class SceneTransitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var destination = serializedObject.FindProperty("destinationScene");
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(destination.stringValue);
            EditorGUI.BeginChangeCheck();
            scene = (SceneAsset)EditorGUILayout.ObjectField("Destination Scene", scene, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck()) destination.stringValue = scene == null ? string.Empty : AssetDatabase.GetAssetPath(scene);
            DrawPropertiesExcluding(serializedObject, "m_Script", "destinationScene");
            serializedObject.ApplyModifiedProperties();

            string path = destination.stringValue;
            bool included = false;
            foreach (var entry in EditorBuildSettings.scenes)
                if (entry.path == path && entry.enabled) included = true;
            if (scene == null)
                EditorGUILayout.HelpBox("Choose the scene to load when the Timeline Signal fires.", MessageType.Info);
            else if (!included)
            {
                EditorGUILayout.HelpBox("This scene must be enabled in Build Settings for asynchronous loading.", MessageType.Warning);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("Add destination to Build Settings")) AddToBuildSettings(path);
            }
            EditorGUILayout.HelpBox("Bind a Timeline Signal Track to this object's Signal Receiver, then place the Fade And Load Scene signal where the fade should begin.", MessageType.Info);
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var scene in scenes)
                if (scene.path == path)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        [MenuItem("Tools/Project NTH/Create Scene Transition")]
        private static void Create()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var existing in Object.FindObjectsOfType<SceneTransitionLoader>(true))
                if (existing.gameObject.scene == scene)
                {
                    Selection.activeGameObject = existing.gameObject;
                    EditorGUIUtility.PingObject(existing);
                    return;
                }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Prefabs/Scene Transition.prefab");
            if (prefab == null) { Debug.LogError("Scene Transition prefab could not be found."); return; }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Create scene transition");
            Selection.activeGameObject = instance;
        }

        [MenuItem("Tools/Project NTH/Create Scene Transition", true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
