# Settings panel

SceneOutdoor's existing **Settings** button opens the new panel. Back or Escape returns to the menu, except while confirming a display change, when Escape first reverts that change. Audio, quality, frame rate and controls apply immediately; preferences are saved when the panel closes and when the game exits. They carry over to IndoorScene and future sessions.

## Included controls

| Page | Options |
| --- | --- |
| Audio | Master, Music, Sound effects, Dialogue volume |
| Display | The project's quality profiles, Fullscreen/Windowed, resolution, 30/60/120/Unlimited FPS |
| Controls | Mouse sensitivity, Invert Y |

Screen mode and resolution are staged until **Apply display**. The player has 15 seconds to select **Keep changes**; **Revert**, Escape, closing the panel, or timeout restores the previous display. The timer works when time scale is zero. Quality and FPS do not require that confirmation. Fullscreen uses borderless fullscreen.

The default frame rate is still 60. A limit is a maximum target, not a guarantee that the game can reach that FPS. VSync stays disabled so it cannot override the selected cap. Scene loading's temporary performance boost still restores the selected frame rate afterward.

Mouse settings use the existing sensitivity asset's limits and initial values. Saving player preferences does not modify that asset.

## Connecting sounds

Open **Assets/Assets/Resources/Game Audio.mixer**. There are three groups under Master:

| Sound | Audio Source → Output |
| --- | --- |
| Background music | Game Audio → Music |
| Footsteps, cars, doors, elevator sounds, ambient wind/noise | Game Audio → Sound Effects |
| Spoken dialogue or thought voice recordings | Game Audio → Dialogue |

Select an object with an **Audio Source**, then assign its **Output** to the appropriate group. Keep the source's own Volume for balancing individual sounds. The settings sliders change the mixer group's volume, preserving each source's volume, distance falloff and spatial blend. Master reduces all three groups together.

For existing scenes, **GameSettings** routes sources with an empty Output to **Sound Effects** when the scene loads, including inactive objects. Explicit Output assignments are preserved. This fallback avoids having to manually reconnect every existing footstep, car, and door sound. It does not guess which clips are music.

For a prefab spawned during gameplay, assign its Audio Source Output on the prefab. Alternatively add **Project NTH → Audio → Settings Audio Route** to the Audio Source object and choose its category. That component routes the source whenever it is enabled. Newly created AudioSources and `AudioSource.PlayClipAtPoint` sounds are not covered by a one-time scene scan; route them explicitly with `GameSettings.Route(source, category)` before playing them.

The scene-load fallback leaves sources assigned to another mixer alone. Those sounds are outside this panel's control unless that mixer routes into Game Audio. An explicit **Settings Audio Route** component or the Yarn routing described below can replace a source's Output assignment.

### Yarn voices and effects

Existing `YarnAudioController` commands keep their current clip assignments and stop/play behavior. Before playing each clip, the controller selects the group:

- `Jan...`, `MainChar...`, `Guard...`, and `whisper` use **Dialogue**.
- `Step1`, `Step2`, `Step3`, and `door_slam` use **Sound Effects**.

No Yarn dialogue text or command syntax needs changing. New sounds added to this controller should also be classified when added to `PlaySound`.

The passive thought panel only displays text. If a thought has a recorded voice, its Audio Source should use Dialogue too. Text visibility and timing do not change when voice volume is muted.

## Visual effects

The Graphics quality control selects the project's existing Unity quality profiles. It uses whatever URP settings those profiles already contain. It does not automatically control individual fog, bloom, camera shake, or plugin effects. Those need a specific connection to their controlling components; this version does not add separate toggles for them.

## Reusing and editing the panel

- Prefab: **Assets/Assets/Prefabs/Settings Panel.prefab**.
- Add one to another scene using **Tools → Project NTH → Create Settings Panel**.
- Connect a menu Button's **On Click** to **SettingsPanelUI.Open**.
- Assign that menu's separate button root to **Menu To Hide**. Do not parent the settings panel underneath the menu root that it hides.
- For a pause menu, assign movement, look, and interaction components to **Disable While Open**. This panel restores their previous enabled states. It does not itself pause time or install a gameplay Escape shortcut.
- The panel starts invisible and does not block clicks while closed. To inspect its layout in Edit Mode, temporarily set the root Canvas Group Alpha to 1; put it back to 0 before saving. Page objects can be selected/activated individually for layout editing.
- An EventSystem is reused if one exists; a standalone panel creates a fallback compatible with the project's Input System.

`GameSettings` starts automatically before the first scene and persists between scenes. Do not add a separate copy to every scene. The mixer stays in Resources so it is included and available even before the settings UI opens.

Preferences use the `ProjectNTH.Settings.` prefix in PlayerPrefs. Display preferences are committed only after Keep changes. Other preferences are flushed on panel close, application pause and quit.

## Validation

The changed code passed editor and player compilation against the project's Unity assemblies. An isolated Unity 2022.3 project passed 176 assertions covering mixer values, Yarn category routing, preservation of local source volumes, existing explicit routing, inactive sources, scene loading/unloading, preference reload, the FPS policy, menu restoration, and display confirmation timeout at time scale zero. Layout renders cover Audio at 1920×1080; Audio, Display and Controls at 1061×591 and 1024×768; and display confirmation at 1061×591. These renders use an isolated dark-background fixture rather than a full gameplay scene.

The exact serialized Outdoor Settings button and panel connection also passed a separate isolated Unity check: the button opened Settings and hid the menu, then closing Settings restored the menu.

The isolated check does not run the full Outdoor/Indoor scenes or verify a physical monitor's resolution switch. Check resolution/fullscreen behavior in a Windows build; the Unity Game view is not a standalone game window.
