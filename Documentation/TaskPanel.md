# Task panel

## Add it to a scene

1. Outside Play mode, choose **Tools > Project NTH > Create Task Panel**, or drag **Assets/Assets/Prefabs/Task Panel** into the Hierarchy.
2. On **Task Panel UI**, change **Task Message** to your first instruction. The prefab's example is **Go to locker**.
3. Enter Play mode and click the Game view to give it keyboard focus.

The upper-left panel opens, holds the message for **Display Duration** (4 seconds by default), then shrinks to a yellow diamond and a **TAB** hint. Pressing Tab opens the current message again. Repeated presses restart the timer. It never takes the player's cursor or intercepts clicks.

Use one Task Panel in each gameplay scene, or one in a shared UI scene. The menu selects an existing panel in the active scene instead of creating duplicates. The supplied prefab builds its Canvas when the game runs; no existing Canvas or EventSystem is required. No scene is changed until you add the prefab/menu object.

## Change the task during gameplay

On an Interactable event or a Timeline Signal Receiver response:

1. Add an event listener with **+**.
2. Drag the scene's **Task Panel** object into the object slot.
3. Choose **TaskPanelUI > SetTask (string)**.
4. Enter the new instruction, such as **Return to the elevator**.

`SetTask` immediately reveals the new message while the component is active and enabled. When disabled, it stores the message; re-enabling follows **Show On Enable**. `Reveal()` repeats the current message, `Collapse()` shrinks it early, and `ClearTask()` removes both the message and its reminder. An empty/whitespace task also hides the panel. To wait for a story event before showing any task, leave **Task Message** empty.

These methods are also callable from C#. This component displays one current instruction; your events decide when a task starts or is completed. It does not infer completion or sequence tasks automatically.

## Use with destination markers

The task panel and Destination Markers can be controlled by the same event. For example, when the locker task starts, call `TaskPanelUI.SetTask("Go to locker")` and the locker's `DestinationMarker.Show()`. On completion, call `ClearTask()` and that marker's `Hide()`. To advance to another task, hide the previous destination marker, set the new task message, and show the next destination marker. The panel does not change marker visibility automatically.

## Adjust the look

- **Display Duration:** how long the full message remains open after each reveal.
- **Animation Duration:** opening/shrinking speed; set to **0** for instant transitions.
- **Show On Enable:** open automatically when enabled; switch off to begin as the small reminder.
- **Screen Offset:** X moves it right; Y moves it down from the upper-left safe area. Values use a 1920 x 1080 reference resolution.
- **Minimum / Maximum Width:** short messages fit the reference panel; longer messages wrap and increase its height within the screen.
- **Font / Font Size:** the prefab uses the project's existing OldCupboard TextMesh Pro font. You can assign another TMP font asset.
- **Background / Border / Text / Icon Color:** the supplied dark, square panel and yellow diamond follow the reference images.
- **Sorting Order:** default **0**, above the destination markers' **-100**. Keep pause menus and transition fades above it.

Timing uses real time, so opening and shrinking work while `Time.timeScale` is zero. Tab uses the project's Input System and does not open the reminder while typing into a focused Unity input field. Disable the Task Panel object when a cutscene or menu should suppress it completely; re-enabling restores the current task.

The Canvas is owned by the same scene as the Task Panel. Disabling/destroying the component or unloading that scene removes its generated UI. Task state is local to this component and is not saved between scene loads.
