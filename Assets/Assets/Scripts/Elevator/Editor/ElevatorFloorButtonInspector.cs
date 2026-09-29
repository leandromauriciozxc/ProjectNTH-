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
                EditorGUILayout.HelpBox("Select one button to see its current automatic Hallway Floor. Floor Index is used only for fixed hallway panels and cabin destinations.", MessageType.Info);
                return;
            }
            serializedObject.Update();
            int action = serializedObject.FindProperty("action").enumValueIndex;
            bool isCall = action == (int)ElevatorFloorButton.ButtonAction.CallUp
                || action == (int)ElevatorFloorButton.ButtonAction.CallDown;
            SerializedProperty property = serializedObject.GetIterator();
            bool enter = true;
            while (property.NextVisible(enter))
            {
                enter = false;
                if (property.name == "followElevatorFloor")
                {
                    if (isCall) EditorGUILayout.PropertyField(property);
                }
                else if (property.name == "floorIndex")
                {
                    if (action == (int)ElevatorFloorButton.ButtonAction.OpenDoors || action == (int)ElevatorFloorButton.ButtonAction.CloseDoors) continue;
                    var controller = serializedObject.FindProperty("elevator").objectReferenceValue as ElevatorController;
                    if (isCall && serializedObject.FindProperty("followElevatorFloor").boolValue)
                    {
                        EditorGUILayout.LabelField("Hallway Floor", controller == null ? "Assign an elevator"
                            : controller.GetFloorLabel(controller.LandingFloorIndex) + " (automatic)");
                        continue;
                    }
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

        public override bool RequiresConstantRepaint() => Application.isPlaying;
    }
}
