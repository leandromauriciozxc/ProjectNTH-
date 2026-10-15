using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectNTH.Dialogue.Authoring
{
    public enum DialogueAudioKind { Voice, SoundEffect }

    [Serializable]
    public sealed class CharacterAudioCue
    {
        [HideInInspector] public string id;
        public string label = "New sound";
        public DialogueAudioKind kind;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [CreateAssetMenu(menuName = "Project NTH/Dialogue/Character", fileName = "New Character")]
    public sealed class DialogueCharacter : ScriptableObject
    {
        public string displayName = "Character";
        [Range(0f, 1f)] public float voiceVolume = 1f;
        public List<CharacterAudioCue> sounds = new List<CharacterAudioCue>();

        public CharacterAudioCue FindCue(string id, DialogueAudioKind kind)
        {
            return sounds.Find(cue => cue != null && cue.id == id && cue.kind == kind);
        }

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            foreach (var cue in sounds)
            {
                if (cue == null) continue;
                if (string.IsNullOrEmpty(cue.id) || !ids.Add(cue.id))
                {
                    cue.id = Guid.NewGuid().ToString("N");
                    ids.Add(cue.id);
                }
            }
        }
    }
}
