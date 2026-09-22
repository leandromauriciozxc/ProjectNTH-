using UnityEditor;
using UnityEngine;

namespace ProjectNTH.Elevators.Editor
{
    [CustomEditor(typeof(ElevatorFloorButton)), CanEditMultipleObjects]
    public sealed class ElevatorFloorButtonInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            bool enter = true;
            while (property.NextVisible(enter))
            {
                enter = false;
                if (property.name == "floorIndex")
                {
                    int action = serializedObject.FindProperty("action").enumValueIndex;
                    if (action == (int)ElevatorFloorButton.ButtonAction.OpenDoors || action == (int)ElevatorFloorButton.ButtonAction.CloseDoors) continue;
                    var controller = serializedObject.FindProperty("elevator").objectReferenceValue as ElevatorController;
                    SerializedProperty floors = controller == null ? null : new SerializedObject(controller).FindProperty("floors");
                    ElevatorInspectorFields.FloorPopup(property, floors,
                        action == (int)ElevatorFloorButton.ButtonAction.SelectFloor ? "Destination Floor" : "Hallway Floor");
                }
                else
                {
                    using (new EditorGUI.DisabledScope(property.name == "m_Script"))
                        EditorGUILayout.PropertyField(property, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
