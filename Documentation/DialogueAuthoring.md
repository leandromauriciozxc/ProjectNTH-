# Dialogue authoring in Unity

Create characters once, select them when writing conversations, and choose voice recordings or sound effects in the Inspector. The editor generates the Yarn file. The existing Yarn Runner and dialogue UI still present the conversation.

## Try the example

1. Let Unity finish compiling.
2. Select `Assets/Dialogue/Authoring/Conversations/Jan Conversation Example.asset`.
3. Expand the three entries. They use the existing Jan and Rosetta recordings and dialogue text.
4. Click **Generate Yarn Now**, then **Create Scene Player for This Conversation**.
5. On the new scene object, check **Dialogue System** references your scene's existing `DialogueSystemController`. Its runner must use `Assets/Dialogue/Yarn Project.yarnproject` and the normal dialogue presenters.
6. Enter Play Mode and click **Play Conversation** on that component. Exit Play Mode when finished.

This example is not connected to any existing trigger. To test it repeatedly during one Play session, turn off **Trigger Once**. A rejected start (for example, another conversation is playing) does not consume the trigger.

## Create reusable characters

In the Project window, right-click → **Create → Project NTH → Dialogue → Character**.

- **Display Name** is the speaker name in dialogue.
- **Voice Volume** adjusts that character's recordings before the Dialogue volume setting is applied.
- **Sounds** is this character's reusable library. Add an entry, give it a descriptive label, choose **Voice** or **Sound Effect**, and assign an AudioClip. Its volume is an optional adjustment for that recording.

Only the selected character's sounds appear in a conversation's audio dropdowns. Renaming a sound or moving it within the list preserves its links. Deleting a used sound produces a validation warning; select a replacement or None in affected conversations.

## Write a conversation

Right-click → **Create → Project NTH → Dialogue → Conversation**. Add and reorder entries using its Inspector:

| Entry | What to fill in |
| --- | --- |
| Line | Select a character, write dialogue, and choose optional voice and sound effect. |
| Sound Effect | Choose an AudioClip directly, or pick from a character's sound-effect library. |
| Scene Action | Give the action a descriptive label, then connect it on the scene player. |
| Wait | Set the delay in seconds. It pauses when the game time scale is zero. |

Audio can come from the reusable character library or **Choose an AudioClip…**. Voice and effects can play together. A new step stops the previous voice; effects can overlap. Completing or stopping the conversation stops its remaining audio.

The dialogue field is plain text. Yarn commands and markup are treated as literal text; line breaks become spaces within one dialogue line. This first version supports linear conversations, not choices, conditions, variables, or a node graph. Existing handwritten branching conversations continue to use their current workflow.

Valid edits generate Yarn automatically after a short pause. **Generate Yarn Now** is also available, and all conversations are checked before entering Play Mode and before building. If a draft has a missing clip or other error, its last valid file is retained and the player refuses to start the invalid/outdated conversation. Fix the Inspector warnings before building.

## Connect a scene action

Assets cannot hold references to scene objects. Those references belong on the **Dialogue Asset Player**:

1. Add a Scene Action entry, such as `Open guard door`, to the conversation.
2. Select its scene player and click **Refresh Action List**.
3. Under that action, press **+**, drag in the target scene object, and select its public method, just as with a Timeline Signal Receiver.

The action is called at that position in the conversation. The next entry starts immediately after the call returns. Add a Wait entry if you want a timed gap for an animation. Renaming or reordering a step preserves its event connections when the list is refreshed. Deleting an action step and refreshing removes its binding.

## Start it in the game

Call **DialogueAssetPlayer → PlayConversation()** from a Timeline Signal Receiver, interaction UnityEvent, button, or other event. Use **StopConversation()** to interrupt it.

For a volume trigger, enable **Trigger On Player Enter**, add a Collider with **Is Trigger** checked, and use the `Player` tag on the entering object. Normal Unity physics requirements for trigger callbacks still apply. Do not connect an old node trigger to the generated node as well.

The optional Voice Source and Effects Source must be separate, dedicated AudioSources. Leave both empty to create separate 2D sources automatically. Voice routes to the existing **Dialogue** mixer group; effects route to **Sound Effects**, so both respect your saved settings. Do not attach the old hardcoded `YarnAudioController` just to play these new entries.

The player uses the assigned Dialogue System Controller to disable and restore its configured gameplay components and objects. Use the same controller setup as your normal conversations. This player is for normal dialogue; it does not automatically use the passive thought-dialogue display.

## Generated files and compatibility

- Edit the Character and Conversation assets. Do not edit their generated Yarn files.
- Output is under `Assets/Dialogue/Generated/`, already included by the existing Yarn Project's `**/*.yarn` source pattern.
- Generated node and line IDs remain stable when assets are renamed or moved. Duplicating a conversation creates a different node on generation.
- Commit the assets, generated files, scripts, and their `.meta` files together.
- If deleting a conversation permanently, also delete its corresponding generated Yarn file (use **Select Generated Yarn File** before deleting the asset).
- Existing scenes, handwritten Yarn files, and audio controllers are not automatically migrated. Try one conversation first, then replace its old trigger with this player when ready.
