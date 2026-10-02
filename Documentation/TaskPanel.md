# Task panel

## Add it to a scene

1. Outside Play mode, choose **Tools > Project NTH > Create Task Panel**, or drag **Assets/Assets/Prefabs/Task Panel** into the Hierarchy.
2. On **Task Panel UI**, change **Task Message** to your first instruction. The prefab's example is **Go to locker**.
3. Enter Play mode and click the Game view to give it keyboard focus.

The first task slides in from beyond the left edge while its message expands. Once fully visible, it holds for **Display Duration** (4 seconds by default), then shrinks to a yellow diamond and a **TAB** hint. Pressing Tab expands the current message again. Repeated presses restart the timer. It never takes the player's cursor or intercepts clicks.

Use one Task Panel in each gameplay scene, or one in a shared UI scene. The menu selects an existing panel in the active scene instead of creating duplicates. The supplied prefab builds its Canvas when the game runs; no existing Canvas or EventSystem is required. No scene is changed until you add the prefab/menu object.

## Complete a task and show the next one

On the event that actually completes the task (for example, a Trigger Handler's **On Triggered**, a successful interaction, or a Timeline Signal Receiver response):

1. Add an event listener with **+**.
2. Drag the scene's **Task Panel** object into the object slot.
3. Choose **TaskPanelUI > CompleteAndShowNext (string)**.
4. Enter the next instruction, such as **Open your locker**.

This runs the whole sequence: expand the current task if needed, draw a cross-out from left to right across its text, pause, retract the panel off the left edge, then animate in the next instruction. Wrapped messages are crossed out one line at a time, in reading order. The next task gets its full display time after it has entered.

For the final task, choose **TaskPanelUI > CompleteTask()** instead. It plays the same completion animation and then leaves the panel hidden, including the TAB hint.

Tab and `Reveal`/`Collapse` cannot interrupt completion. Repeated completion calls are ignored while that animation is running. Use **Trigger Once** on arrival triggers, or a one-time success event on interactions, so an old event cannot complete a later task after the animation finishes. This UI does not track task IDs or enforce story order.

## Start or replace a task without marking it complete

On an Interactable event or a Timeline Signal Receiver response:

1. Add an event listener with **+**.
2. Drag the scene's **Task Panel** object into the object slot.
3. Choose **TaskPanelUI > SetTask (string)**.
4. Enter the new instruction, such as **Return to the elevator**.

`SetTask` starts the new instruction's entrance animation while the component is active and enabled. It does not cross out the old task: use `CompleteAndShowNext` for a successful completion. Calling `SetTask` during completion queues its message until the old panel has left; another `SetTask` replaces that queued message. This also supports an event that calls `CompleteTask()` followed by `SetTask(string)`.

When disabled, `SetTask` stores the message; re-enabling follows **Show On Enable**. `Reveal()` repeats the current message and `Collapse()` shrinks it early. `ClearTask()` immediately removes the message and reminder, cancels any animation, and discards the queued next task. An empty/whitespace `SetTask` hides the panel when no completion is running, or queues no next task during completion. To wait for a story event before showing any task, leave **Task Message** empty.

These methods are also callable from C#. This component displays one current instruction; your events decide when a task starts or is completed. It does not detect gameplay success, choose the next instruction, or enforce objective order.

## Use with destination markers

The task panel and Destination Markers can be controlled by the same event. For example, when the locker task starts, call `TaskPanelUI.SetTask("Go to locker")` and the locker's `DestinationMarker.Show()`. On completion, hide that marker and call `CompleteTask()` or `CompleteAndShowNext(string)`. Show the next destination marker with a separate event listener if needed. Marker events execute when invoked; the panel does not automatically delay them until its animation finishes or change marker visibility itself.

## Adjust the look

- **Display Duration:** how long the full message remains open after its entrance or a Tab reveal.
- **Animation Duration:** message opening/shrinking time, including Tab; default **0.3 seconds**.
- **Entrance Duration:** first/new task slide-in time; default **0.4 seconds**.
- **Crossout Duration:** time to draw across all text lines; default **0.45 seconds**.
- **Completion Pause:** how long the fully crossed-out text stays visible; default **0.35 seconds**.
- **Exit Duration:** time to retract the completed task off-screen; default **0.3 seconds**.
- Set the animation durations and completion pause to **0** for instant state changes without motion.
- **Show On Enable:** slide in with the message open when enabled; switch off to slide in as the small reminder.
- **Screen Offset:** X moves it right; Y moves it down from the upper-left safe area. Values use a 1920 x 1080 reference resolution.
- **Minimum / Maximum Width:** short messages fit the reference panel; longer messages wrap and increase its height within the screen.
- **Font / Font Size:** the prefab uses the project's existing OldCupboard TextMesh Pro font. You can assign another TMP font asset.
- **Background / Border / Text / Icon Color:** the supplied dark, square panel and yellow diamond follow the reference images.
- **Sorting Order:** default **0**, above the destination markers' **-100**. Keep pause menus and transition fades above it.

Timing uses real time, so entrance, completion, opening and shrinking work while `Time.timeScale` is zero. Tab uses the project's Input System and does not open the reminder while typing into a focused Unity input field. Disable the Task Panel object when a cutscene or menu should suppress it completely; re-enabling animates in the current task. If disabled during completion, the completed task is discarded and the queued next task is stored for re-enabling. With no next task, it stays empty.

The Canvas is owned by the same scene as the Task Panel. Disabling/destroying the component or unloading that scene removes its generated UI. Task state is local to this component and is not saved between scene loads.
