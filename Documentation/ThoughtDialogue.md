# Player thoughts

Thoughts use Yarn Spinner and appear at the bottom center of the screen. Each line fades in, remains readable, fades out, and leaves a short gap before the next. Movement, looking, interaction, cursor state, and time scale are never changed by this display. It has no advance key, choices, raycast targets, or EventSystem selection.

## Add it to a scene

1. Let Unity finish importing the scripts.
2. Choose **Tools → Project NTH → Create Thought Dialogue**. This adds the configured **Thought Dialogue** prefab, or selects the existing system in this scene.
3. Its **Yarn Project** already points to `Assets/Dialogue/Yarn Project.yarnproject` and its font uses the existing OldCupboard font.
4. Check **Conversation Runner** points to your normal dialogue's **Dialogue Runner**. The menu finds it through your existing **Dialogue System Controller**; the system also tries this at runtime if unassigned. Assign it yourself if you use a different conversation setup.

No Canvas setup is required. The subtitle Canvas and a separate thought runner are created during play and cleaned up with the system. Nothing plays automatically. Add the system separately to each scene that needs thoughts.

## Write the thoughts

An example is supplied in `Assets/Dialogue/ThoughtExamples.yarn`. Edit it or create a `.yarn` file in the same folder with a unique node name:

```yarn
title: Thought_LockedDoor
---
It's locked.
There must be another way through.
===
```

Each ordinary Yarn dialogue line gets its own fade cycle. Character names are omitted from the display. Conditions, variables, and jumps work as usual. Use short lines; long lines wrap and receive extra reading time. Avoid choice arrows (`->`): the passive presenter warns and skips choices rather than requesting input. Yarn commands still execute normally, so use thought-only nodes without commands that move the player, switch cameras, or otherwise alter gameplay.

## Trigger it

For an existing Unity event (such as a door interaction): add a listener, drag **Thought Dialogue** into the object field, select **ThoughtDialogueSystem → PlayThought(string)**, and enter the node name, for example `Thought_Example`.

For an area trigger: add a **Box Collider** with **Is Trigger** checked and a **Thought Dialogue Trigger** component. Assign **Thought System**, select the **Yarn Node**, and keep **Trigger Once** enabled for a one-time thought. The entering collider must have the **Player** tag; normal Unity trigger physics requirements still apply. You can also call **TriggerThought()** from an interaction event without using a collider.

From an existing Yarn conversation:

```yarn
<<thought "Thought Dialogue" Thought_Example>>
```

The quoted name must match the scene GameObject's name exactly. This command queues the thought and returns immediately. The thought waits for the conversation to end. If you already have a `thought` command in a different integration, resolve that command-name conflict first.

## Timing and behavior

- **Fade In Duration / Fade Out Duration:** default 0.35 seconds each; 0 makes that transition instant.
- **Hold Duration:** default 3 seconds; the minimum fully visible time, excluding fades. This value is adjustable, including down to 0.
- **Seconds Per Word:** default 0.3. Reading time is the greater of Hold Duration and word count × Seconds Per Word. Set to 0 for fixed timing.
- **Gap Between Lines:** default 0.2 seconds with no thought visible.
- **Bottom Offset / Maximum Width / Font Size:** placement and wrapping in 1920 × 1080 reference pixels, respecting the screen's safe area.
- **Background Color:** a subtle dark backing keeps text readable over the scene. Its alpha can be reduced or set to 0.

New nodes queue in order. Duplicate requests for a currently playing or queued node do not add another copy. A repeatable trigger can play it again once it has finished. If a normal conversation starts, the current thought hides immediately and resumes from its fade-in with full reading time afterward. Subsequent thought nodes wait too. This pauses presentation, not arbitrary Yarn commands that are already executing.

**Share Conversation Variables** shares story state with the assigned conversation runner only when they use the same Yarn Project. Otherwise the thoughts keep their own in-memory variables. These settings are intended to be configured before play.

**ClearThoughts()** cancels the current node and discards queued thoughts. Disabling the system does the same and removes its Canvas; re-enabling does not replay cancelled thoughts. A missing node or disabled system does not consume a one-time trigger. Timings use unscaled time, so they continue while time scale is zero unless a normal conversation is active.

## Quick check

Trigger `Thought_Example` while walking and looking around: two lines should fade individually without locking controls. Trigger it while a normal conversation is running: it should wait. Start a conversation partway through a thought: the thought should hide, then reappear with full reading time after the conversation ends.
