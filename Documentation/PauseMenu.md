# Pause menu

The existing Canvas prefab now uses `UiOtherController` to open the authored PauseMenu with **Escape**. Both Outdoor and Indoor use this Canvas prefab.

## Player controls

- **Escape during exploration:** show PauseMenu, stop game time, pause game audio, unlock the cursor, and suspend movement, mouse look, interaction, and camera motion.
- **Resume or Escape:** close PauseMenu and restore the previous time scale, cursor, audio pause state, and each component's previous enabled state.
- **Settings:** hide PauseMenu and open Settings while the game stays paused. Settings' Back button or Escape returns to PauseMenu. Another Escape resumes gameplay.
- **Display confirmation:** Escape first reverts an unconfirmed display change. Further presses close Settings, then resume.
- **Quit:** exit the built game. Unity's `Application.Quit` does not stop Editor Play mode.

Pausing is currently allowed only during exploration. It is blocked at the Outdoor main menu, during an active normal Yarn conversation, while any active Timeline director is playing, during a scene transition, and when another system already set time scale to zero. Passive thoughts can be paused. An enabled PlayerMovement in the controller's scene is required.

Task panel animations and display timers freeze during pause. Thoughts temporarily hide and preserve their remaining reading time. Queued thoughts wait until gameplay resumes. Settings animations and display-confirmation timers continue to work while paused.

## Scene setup

Keep `UiOtherController` on the **active Canvas**, outside the PauseMenu object that gets hidden. Do not disable the whole Canvas to close PauseMenu.

The current connections are already assigned:

| Field or button | Connection |
| --- | --- |
| Pause Menu | Canvas child PauseMenu |
| First Selected | Resume button |
| Resume On Click | UiOtherController.Resume |
| Settings On Click | UiOtherController.OpenSettings |
| Quit On Click | UiOtherController.Quit |
| Settings Panel Prefab | Assets/Assets/Prefabs/Settings Panel.prefab |
| Outdoor Main Menu | The separate main-menu button group |
| Outdoor Settings Panel | The existing scene Settings panel |

Indoor creates a Settings panel from the assigned prefab the first time it is needed. Later openings reuse it. Outdoor reuses its existing panel. Closing Settings restores whichever menu opened it.

The pause menu is hidden at startup and displays above the gameplay HUD, below Settings and scene fades. Outdoor had two Settings buttons at the same position; the original prefab button is disabled in that scene and the scene-added button is connected to OpenSettings. The shared prefab retains its working Settings button for Indoor.

For another scene, reuse the Canvas prefab. Assign **Main Menu** only if that scene has a separate main-menu button group. Assign **Settings Panel** if it already contains a Settings panel; otherwise leave the prefab fallback assigned.

## Adding future gameplay systems

`Time.timeScale = 0` does not stop code that reads input directly or uses unscaled time. The controller therefore suspends the project's current player components explicitly. Add other components that should stop to **Disable While Paused**, or have them check `UiOtherController.IsGamePaused`. Keep menu components out of that list.

Use `UiOtherController.GameplayUnscaledTime` for future real-time gameplay message timers that should freeze during this pause. Normal scaled movement, physics, and timers already follow the zero time scale. Menu timers should continue using ordinary unscaled time.

Game audio pauses through `AudioListener.pause`. If future menu click sounds should remain audible, set their dedicated AudioSource's `ignoreListenerPause` to true in its script. Keep ordinary game sources at the default false.

If you add an always-running background Timeline, it will currently block pausing too. Deliberately relaxing that rule requires deciding how that Timeline, its audio, and any commands should resume together.

## Quick play check

1. Start Outdoor and press Play. During exploration, press Escape; movement and mouse look should stop and the cursor should appear.
2. Open Settings, change a volume, then press Escape. PauseMenu should return and the game should remain frozen.
3. Press Resume or Escape; gameplay should continue from the same state. Repeat in Indoor once its opening cutscene finishes.
4. Check a task message and a passive thought: pausing should preserve their remaining display time.

## Validation

The changed scripts passed editor and player compilation against the project's Unity assemblies; the player check excluded UnityEditor references. An isolated Unity 2022.3.62f3 run passed 35 assertions, using the actual Canvas/Settings prefabs, player input and movement scripts, task panel, and Yarn thought/conversation runners. Checks covered Escape, authored button callbacks, Settings Back, cursor and audio restoration, previously disabled components, main-menu/conversation/Timeline guards, task/thought timing, repeated use, and unloading the paused scene. The isolated footstep handler was a counting stub.

These checks do not replace a full Outdoor/Indoor Windows build play-through with scene-specific triggers and third-party components. Use the quick play check above in both scenes.
