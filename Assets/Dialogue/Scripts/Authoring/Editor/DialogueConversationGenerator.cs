using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Yarn.Compiler;

namespace ProjectNTH.Dialogue.Authoring.Editor
{
    [InitializeOnLoad]
    public static class DialogueConversationGenerator
    {
        public const string OutputFolder = "Assets/Dialogue/Generated";
        private static double generateAfter;
        private static bool queued;
        private static bool generating;

        static DialogueConversationGenerator()
        {
            EditorApplication.update += Update;
            Undo.undoRedoPerformed += Queue;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode) GenerateAll(false);
            };
        }

        public static void Queue()
        {
            queued = true;
            generateAfter = EditorApplication.timeSinceStartup + 0.7;
        }

        private static void Update()
        {
            if (!queued || generating || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup < generateAfter) return;
            queued = false;
            GenerateAll(false);
        }

        public static string OutputPath(DialogueConversation conversation)
        {
            return OutputFolder + "/" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(conversation)) + ".yarn";
        }

        public static bool Generate(DialogueConversation conversation, out string message)
        {
            if (conversation == null) { message = "Choose a Conversation asset."; return false; }
            string assetPath = AssetDatabase.GetAssetPath(conversation);
            if (string.IsNullOrEmpty(assetPath)) { message = "Save the conversation in the Project window first."; return false; }
            var serialized = new SerializedObject(conversation);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (serialized.FindProperty("assetId").stringValue != guid)
            {
                serialized.FindProperty("assetId").stringValue = guid;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var errors = DialogueYarnText.Validate(conversation);
            if (errors.Count > 0) { message = string.Join("\n", errors); return false; }
            string source = DialogueYarnText.Generate(conversation);
            string output = OutputPath(conversation);
            try
            {
                // Compile before replacing a valid file: a typo in a draft must not break the shared Yarn Project.
                var result = Compiler.Compile(CompilationJob.CreateFromString(Path.GetFileName(output), source));
                if (result.Program == null)
                {
                    message = "Yarn validation failed:\n" + string.Join("\n", result.Diagnostics.Select(d => d.ToString()));
                    return false;
                }
                string existing = File.Exists(output) ? File.ReadAllText(output).Replace("\r\n", "\n") : null;
                if (existing != null && !existing.Contains("// Source asset: " + guid + "\n"))
                {
                    message = "The output file is not owned by this conversation. Move it before generating: " + output;
                    return false;
                }
                if (!AssetDatabase.IsValidFolder(OutputFolder)) AssetDatabase.CreateFolder("Assets/Dialogue", "Generated");
                if (existing != source)
                {
                    File.WriteAllText(output, source, new UTF8Encoding(false));
                    AssetDatabase.ImportAsset(output, ImportAssetOptions.ForceUpdate);
                }
                serialized.Update();
                serialized.FindProperty("generatedHash").stringValue = DialogueYarnText.Fingerprint(conversation);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(conversation);
                foreach (var character in conversation.steps.Where(s => s.character != null).Select(s => s.character).Distinct())
                    AssetDatabase.SaveAssetIfDirty(character);
                message = "Yarn is up to date. Use this asset on a Dialogue Asset Player.";
                return true;
            }
            catch (Exception exception)
            {
                message = "Could not generate Yarn: " + exception.Message;
                return false;
            }
        }

        [MenuItem("Tools/Project NTH/Dialogue/Generate All Conversations")]
        public static void GenerateAllMenu() => GenerateAll(true);

        public static bool GenerateAll(bool reportErrors)
        {
            if (generating) return true;
            generating = true;
            bool success = true;
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:DialogueConversation"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<DialogueConversation>(AssetDatabase.GUIDToAssetPath(guid));
                    if (Generate(asset, out string message)) continue;
                    success = false;
                    if (reportErrors) Debug.LogError(asset.name + ": " + message, asset);
                }
            }
            finally { generating = false; }
            return success;
        }
    }

    public sealed class DialogueAuthoringImports : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] oldPaths)
        {
            foreach (string path in imported.Concat(moved))
            {
                if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) continue;
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == typeof(DialogueConversation) || type == typeof(DialogueCharacter))
                {
                    DialogueConversationGenerator.Queue();
                    return;
                }
            }
        }
    }

    public sealed class DialogueAuthoringBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!DialogueConversationGenerator.GenerateAll(true))
                throw new BuildFailedException("Fix the authored conversation errors in the Console before building. Existing handwritten Yarn files were not changed.");
        }
    }
}
