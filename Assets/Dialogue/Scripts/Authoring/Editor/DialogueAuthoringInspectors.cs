using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectNTH.Dialogue.Authoring.Editor
{
    [CustomEditor(typeof(DialogueCharacter))]
    public sealed class DialogueCharacterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Create this character once, then select it in any conversation. Sounds below belong only to this character. Give each sound a recognizable label and choose Voice or Sound Effect.", MessageType.Info);
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            if (serializedObject.ApplyModifiedProperties()) DialogueConversationGenerator.Queue();
        }
    }

    [CustomEditor(typeof(DialogueConversation))]
    public sealed class DialogueConversationEditor : UnityEditor.Editor
    {
        private string generationMessage;
        private bool generationSucceeded;

        public override void OnInspectorGUI()
        {
            var conversation = (DialogueConversation)target;
            EditorGUILayout.HelpBox("Write the conversation here. Valid changes generate Yarn automatically after a short pause. Dialogue text is plain text; sound and scene commands are generated for you. This first version is linear.", MessageType.Info);
            serializedObject.Update();
            var steps = serializedObject.FindProperty("steps");
            for (int i = 0; i < steps.arraySize; i++)
            {
                var step = steps.GetArrayElementAtIndex(i);
                var kind = (DialogueStepKind)step.FindPropertyRelative("kind").enumValueIndex;
                var character = step.FindPropertyRelative("character").objectReferenceValue as DialogueCharacter;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                step.isExpanded = EditorGUILayout.Foldout(step.isExpanded,
                    (i + 1) + ". " + kind + (kind == DialogueStepKind.Line && character != null ? " — " + character.displayName : ""), true);
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUILayout.Button("↑", GUILayout.Width(25))) steps.MoveArrayElement(i, i - 1);
                using (new EditorGUI.DisabledScope(i == steps.arraySize - 1))
                    if (GUILayout.Button("↓", GUILayout.Width(25))) steps.MoveArrayElement(i, i + 1);
                bool remove = GUILayout.Button("×", GUILayout.Width(25));
                EditorGUILayout.EndHorizontal();
                if (!remove && step.isExpanded) DrawStep(step, kind);
                EditorGUILayout.EndVertical();
                if (remove) { steps.DeleteArrayElementAtIndex(i); break; }
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            foreach (DialogueStepKind kind in Enum.GetValues(typeof(DialogueStepKind)))
                if (GUILayout.Button("+ " + ObjectNames.NicifyVariableName(kind.ToString()))) AddStep(steps, kind);
            EditorGUILayout.EndHorizontal();
            if (serializedObject.ApplyModifiedProperties())
            {
                generationMessage = null;
                DialogueConversationGenerator.Queue();
            }
            EditorGUILayout.Space();
            var errors = DialogueYarnText.Validate(conversation);
            if (errors.Count > 0) EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Warning);
            else if (conversation.GeneratedHash != DialogueYarnText.Fingerprint(conversation))
                EditorGUILayout.HelpBox("Changes are waiting to be generated.", MessageType.Info);
            else EditorGUILayout.HelpBox("Yarn is up to date.", MessageType.Info);
            if (GUILayout.Button("Generate Yarn Now"))
                generationSucceeded = DialogueConversationGenerator.Generate(conversation, out generationMessage);
            if (!string.IsNullOrEmpty(generationMessage)) EditorGUILayout.HelpBox(generationMessage, generationSucceeded ? MessageType.Info : MessageType.Error);
            using (new EditorGUI.DisabledScope(errors.Count > 0))
                if (GUILayout.Button("Create Scene Player for This Conversation")) DialogueAuthoringMenus.CreatePlayer(conversation);
            if (GUILayout.Button("Select Generated Yarn File"))
                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(DialogueConversationGenerator.OutputPath(conversation)));
        }

        private static void DrawStep(SerializedProperty step, DialogueStepKind kind)
        {
            if (kind == DialogueStepKind.Line || kind == DialogueStepKind.SoundEffect)
                EditorGUILayout.PropertyField(step.FindPropertyRelative("character"), new GUIContent(kind == DialogueStepKind.Line ? "Character" : "Character library (optional)"));
            var character = step.FindPropertyRelative("character").objectReferenceValue as DialogueCharacter;
            if (kind == DialogueStepKind.Line)
            {
                EditorGUILayout.PropertyField(step.FindPropertyRelative("text"), new GUIContent("Dialogue"));
                DrawAudio(step.FindPropertyRelative("voice"), character, DialogueAudioKind.Voice, "Voice");
            }
            if (kind == DialogueStepKind.Line || kind == DialogueStepKind.SoundEffect)
                DrawAudio(step.FindPropertyRelative("soundEffect"), character, DialogueAudioKind.SoundEffect, "Sound effect");
            if (kind == DialogueStepKind.SceneAction)
            {
                EditorGUILayout.PropertyField(step.FindPropertyRelative("actionLabel"), new GUIContent("Action label"));
                EditorGUILayout.HelpBox("Describe the action, such as Open guard door. Connect its target using the Scene Player's Scene Actions list. No command or GameObject name is needed.", MessageType.None);
            }
            if (kind == DialogueStepKind.Wait)
                EditorGUILayout.PropertyField(step.FindPropertyRelative("waitSeconds"), new GUIContent("Wait (seconds)"));
        }

        private static void DrawAudio(SerializedProperty selection, DialogueCharacter character, DialogueAudioKind kind, string label)
        {
            var mode = selection.FindPropertyRelative("mode");
            var cueId = selection.FindPropertyRelative("cueId");
            var names = new List<string> { "None", "Choose an AudioClip…" };
            var ids = new List<string> { "", "" };
            if (character != null)
                foreach (var cue in character.sounds)
                {
                    if (cue == null || cue.kind != kind) continue;
                    names.Add(character.displayName + "/" + (string.IsNullOrWhiteSpace(cue.label) ? "Unnamed sound" : cue.label));
                    ids.Add(cue.id);
                }
            int selected = mode.enumValueIndex == (int)DialogueClipMode.AudioClip ? 1 : 0;
            if (mode.enumValueIndex == (int)DialogueClipMode.CharacterLibrary)
            {
                selected = ids.FindIndex(2, id => id == cueId.stringValue);
                if (selected < 0)
                {
                    names.Add("Missing character sound — choose another");
                    ids.Add(cueId.stringValue);
                    selected = names.Count - 1;
                }
            }
            int next = EditorGUILayout.Popup(label, selected, names.ToArray());
            if (next != selected)
            {
                mode.enumValueIndex = next == 0 ? (int)DialogueClipMode.None : next == 1 ? (int)DialogueClipMode.AudioClip : (int)DialogueClipMode.CharacterLibrary;
                cueId.stringValue = next >= 2 ? ids[next] : "";
            }
            if (mode.enumValueIndex == (int)DialogueClipMode.AudioClip)
                EditorGUILayout.PropertyField(selection.FindPropertyRelative("clip"), new GUIContent(label + " clip"));
        }

        private static void AddStep(SerializedProperty steps, DialogueStepKind kind)
        {
            int index = steps.arraySize++;
            var step = steps.GetArrayElementAtIndex(index);
            step.FindPropertyRelative("id").stringValue = Guid.NewGuid().ToString("N");
            step.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            step.FindPropertyRelative("character").objectReferenceValue = null;
            step.FindPropertyRelative("text").stringValue = "";
            step.FindPropertyRelative("actionLabel").stringValue = "Scene action";
            step.FindPropertyRelative("waitSeconds").floatValue = 1f;
            foreach (string field in new[] { "voice", "soundEffect" })
            {
                var selection = step.FindPropertyRelative(field);
                selection.FindPropertyRelative("mode").enumValueIndex = 0;
                selection.FindPropertyRelative("clip").objectReferenceValue = null;
                selection.FindPropertyRelative("cueId").stringValue = "";
            }
            step.isExpanded = true;
        }
    }

    [CustomEditor(typeof(DialogueAssetPlayer))]
    public sealed class DialogueAssetPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var player = (DialogueAssetPlayer)target;
            var previousConversation = player.Conversation;
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "sceneActions");
            serializedObject.ApplyModifiedProperties();
            if (previousConversation != player.Conversation) Sync(player);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Action List")) Sync(player);
            serializedObject.Update();
            var actions = serializedObject.FindProperty("sceneActions");
            for (int i = 0; i < actions.arraySize; i++)
            {
                var binding = actions.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(binding.FindPropertyRelative("action"), new GUIContent(binding.FindPropertyRelative("label").stringValue), true);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("Call PlayConversation() from a Timeline Signal, interaction or UnityEvent. For a trigger volume, enable Trigger On Player Enter and add an Is Trigger collider. Audio Sources should be dedicated to this conversation; empty fields create separate sources automatically.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Play Conversation")) player.PlayConversation();
                if (GUILayout.Button("Stop Conversation")) player.StopConversation();
            }
        }

        private static void Sync(DialogueAssetPlayer player)
        {
            Undo.RecordObject(player, "Update dialogue actions");
            player.SynchronizeSceneActions();
            EditorUtility.SetDirty(player);
        }
    }

    public static class DialogueAuthoringMenus
    {
        public static void CreatePlayer(DialogueConversation conversation)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var gameObject = new GameObject(conversation.name + " Player");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create dialogue player");
            var player = Undo.AddComponent<DialogueAssetPlayer>(gameObject);
            var serialized = new SerializedObject(player);
            serialized.FindProperty("conversation").objectReferenceValue = conversation;
            foreach (var system in UnityEngine.Object.FindObjectsOfType<global::DialogueSystemController>(true))
                if (system.gameObject.scene == SceneManager.GetActiveScene())
                {
                    serialized.FindProperty("dialogueSystem").objectReferenceValue = system;
                    break;
                }
            serialized.ApplyModifiedProperties();
            player.SynchronizeSceneActions();
            Selection.activeGameObject = gameObject;
        }
    }
}
