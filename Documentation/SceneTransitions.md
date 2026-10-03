# Cutscene scene transitions

A Timeline Signal chooses the exact moment the fade begins. The screen fades to black, loads the destination asynchronously, stays black through scene activation and initialization, then fades back in. The temporary overlay survives unloading the old scene and removes itself afterward. No loading screen scene or Canvas setup is needed.

## Connect the building-entry cutscene

1. In the outdoor scene, choose **Tools > Project NTH > Create Scene Transition**. This adds the supplied prefab with **Scene Transition Loader** and an already configured **Signal Receiver**.
2. Set **Destination Scene** to **IndoorScene**. This is the prefab's default, and IndoorScene has been added to Build Settings.
3. Select the GameObject whose **Playable Director** plays your building-entry cutscene, then open **Window > Sequencing > Timeline**.
4. Add a **Signal Track** and drag the **Scene Transition** object into the track's binding slot.
5. On that track, add a **Signal Emitter** at the time you want the screen to **start** fading. Assign the existing **Fade And Load Scene** signal from `Assets/Assets/Timelines/Signals`. Enable **Emit Once**. Leave **Retroactive** off unless skipping past this point should also change scenes.
6. Save the scene and Timeline. Your existing trigger can continue calling the director's **Play()**.

The receiver reaction is already connected to `SceneTransitionLoader.LoadNextScene()`. Place the emitter inside the Timeline's playable duration. To hide a camera cut, start the fade roughly **0.7 seconds before** that cut, or adjust Fade Out Duration. Place it before the end rather than relying on a marker beyond the final clip.

The signal position is intentionally yours to choose; the existing cutscene and scene objects have not been modified automatically. The new destination is appended to Build Settings without changing the existing first scene or scene indices.

## Settings

- **Fade Out Duration:** seconds to reach black (default 0.7).
- **Fade In Duration:** seconds to reveal the new scene (default 0.7).
- **Minimum Black Duration:** minimum time fully black, including loading (default 0.2).
- **Scene Settle Duration:** additional delay after activation and Start, before revealing the scene (default 0.15).
- **Prioritize Loading While Black:** enabled by default. Temporarily increases Unity's background loading priority and removes the frame-rate/VSync limit only during the fully black loading stage. Restores the previous settings before the fade-in, on a loading error, and if the overlay is destroyed. Gameplay and fades keep the usual frame-rate settings.
- **Log Transition Timings:** optional Console/Player log message showing load + activation time, time fully black, and total time including fades.
- **Disable During Transition:** optional movement, camera-look or interaction components to suspend. Components that survive scene unloading have their previous enabled states restored.

Fades use unscaled time, so they still finish when Time.timeScale is zero. The transition does not change time scale, cursor state or audio volume. The destination scene supplies its own player, camera and spawn position. Player state is not transferred between scenes by this component.

Duplicate signals are ignored while loading, and each loader accepts one successful request. A missing or disabled destination reports an error before fading, keeping the current scene visible. Assigning a different destination in the Inspector shows a button to add it to Build Settings when needed.

Asynchronous loading reduces blocking work, but Unity may still briefly stall during activation or scene initialization. The screen stays black across that stage. Content that your own code downloads or initializes later may need a longer settle delay or a separate readiness condition.

For non-Timeline use, any Button or UnityEvent can call **LoadNextScene()** at the desired transition moment.

## Checking a long black screen

Existing prefab instances inherit **Prioritize Loading While Black**, so the existing Timeline Signal needs no rewiring. Test a newly built game: Unity's background loading priority only affects built Players, not the Editor. This gives asset integration more time per frame while the screen is hidden; the project's 60 FPS target is restored before revealing the destination. It cannot remove the cost of reading assets or running destination initialization code.

Enable **Log Transition Timings** on the Scene Transition object to measure a slow transition. Compare the same build, route and hardware with the loading boost on and off, allowing for disk caching between runs. A large **load + activation** value points to asset loading, old-scene unloading or destination startup work. **Fully black** also includes the initial black frame, the initialization frame, and the configured settle/minimum-black delays; these measurements overlap and should not be added together.

The default 0.2-second minimum black duration overlaps loading; it is not an extra wait after loading. Shortening fade durations will not solve a several-second load. If loading still dominates, profile the built Player's asset integration and Awake/Start work, or consider preloading during the cutscene as a separate change (which requires additional memory while both scenes' assets are resident).

Unity reference: [Application.backgroundLoadingPriority](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-backgroundLoadingPriority.html).
