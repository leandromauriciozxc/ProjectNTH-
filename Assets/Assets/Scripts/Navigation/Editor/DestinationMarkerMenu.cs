using UnityEditor;
using UnityEngine;

namespace ProjectNTH.Navigation.Editor
{
    public static class DestinationMarkerMenu
    {
        [MenuItem("Tools/Project NTH/Create Destination Marker")]
        private static void Create()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Prefabs/Destination Marker.prefab");
            if (prefab == null)
            {
                Debug.LogError("Destination Marker prefab could not be found.");
                return;
            }
            Transform selected = Selection.activeTransform;
            var marker = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(marker, "Create destination marker");
            if (selected != null && selected.gameObject.scene.IsValid()) marker.transform.position = selected.position;
            else if (SceneView.lastActiveSceneView != null) marker.transform.position = SceneView.lastActiveSceneView.pivot;
            PrefabUtility.RecordPrefabInstancePropertyModifications(marker.transform);
            Selection.activeGameObject = marker;
        }

        [MenuItem("Tools/Project NTH/Create Destination Marker", true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
