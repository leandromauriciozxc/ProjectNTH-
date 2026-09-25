# The journey back to the locker

These scripts add an endless return journey to `Scene1TestingExit8`. The original scene, player prefab and scripts have not been edited or wired up. Follow the steps below in Unity when you are ready to change your test scene.

The first visit is normal. Entering production triggers the medicine reminder and starts the loop. On the way back, a doorway relocates the player to another part of the same floor. The route repeats until the story calls `EndLoop()`.

This version implements traversal only. It does not judge anomalies, change props, reset doors, add medicine interactions, or implement a symptom system. Existing room and interaction states persist because the scene is not reloaded.

## 1. Add the controller

1. Let Unity import the three scripts in `Assets/Assets/Scripts/Looping`.
2. Create a new empty **scene-root** GameObject named `MazeLoopSystem`. Add **Maze Loop Controller**.
3. Assign the **CharacterController on the active scene player** to **Player**. Use the instance in the Hierarchy, not the prefab asset or a camera. The saved scene references more than one player prefab instance, so select the one you actually play with.
4. Leave **Begin On Play** off. Leave **Fade To Black** on initially.
5. Enabled scripts beneath that player's root are paused during the brief transition and restored afterward. If `Raycast`, a camera controller or a movement script lives elsewhere in your hierarchy, add it to **Additional Scripts To Pause**. Do not add this loop controller or the gates. Scripts already disabled before the transition remain disabled.

The black overlay is created only in Play Mode; no Canvas, image asset or material setup is required. Fades use real time and player control is restored if the loop is ended or its controller is disabled during a transition.

## 2. Start the story beat inside production

1. Create a new empty GameObject named `ProductionReminder` just inside `ProductionArea`, beyond `DoorProduction`.
2. Add **Maze Loop Story Trigger** and assign `MazeLoopSystem` to **Controller**.
3. Centre its green wireframe box around the player's body at that location. Adjust **Size** so the route into production crosses the box. Keep its scale at `(1, 1, 1)` and rotate around Y only. No collider or Rigidbody is needed.
4. Leave **Begin Loop On Entry** on for the initial prototype. The first entry starts the loop. Use **On First Entry** for the reminder text, voice line or other story event when those exist.

If a dialogue or cutscene must finish before the maze begins, turn **Begin Loop On Entry** off. Start that sequence from **On First Entry**, and have its completion event call `MazeLoopController.BeginLoop()` instead.

The volume fires once per component instance. Walking through it again does not restart the session. Spawning inside it counts as the first entry, so start outside it to test the normal journey. `ResetTrigger()` rearms the story trigger for a later deliberate entry.

## 3. Make one repeatable corridor first

Create this arrangement before setting up a larger maze:

```text
Production reminder
        |
        v
Arrival_Hallway ---- walk toward lockers ----> Gate_Locker
        ^                                          |
        |_______________ relocation _______________|
```

1. Create a new empty GameObject named `Gate_Locker` on the approach to a locker doorway, before the player can reach the lockers. Add **Maze Loop Gate** and assign **Controller**.
2. Place its origin at roughly the player's capsule-centre height, in the centre of the doorway. The cyan rectangle should cover the opening. Change **Opening Size** to fit; object scale is ignored.
3. Rotate around Y so its **blue Z arrow points toward the lockers**. The gate fires when the player's body crosses from behind the rectangle to its arrow side. Looking around or standing in the doorway does not trigger it. Walking backward through it in the arrow direction still counts.
4. Create an empty GameObject named `Arrival_Hallway` farther back along the corridor, on the approach side of the gate. It is an arrival marker, so it needs **no script**.
5. Give the arrival marker the same height above the floor as the gate. Point its blue Z arrow in the direction travel should continue after arrival, normally back toward `Gate_Locker`.
6. On the gate, set **Destinations** to one element and assign `Arrival_Hallway`.

The controller maps the player's offset and heading from the gate to the arrival marker. Entering the left side of a doorway puts the player on the corresponding left side at arrival; camera pitch is preserved. Keep the arrival area wide enough for every crossing position, and keep it free of walls, closed doors and furniture. It does not search for a safe landing point. Use level, similarly sized passages for this first setup.

Place the arrival far enough back that the player has corridor to walk again. Putting both markers at the same point will not create a useful loop. The destination can face another direction, including 180 degrees from the source, as long as its arrow points into clear walking space.

Leave the fade enabled while testing. For a less noticeable transition later, arrange matching doorframes, a corridor bend or a closed-door transition to hide both ends, then try **Fade To Black** off. This is a relocation system, not a rendered portal: views through the doorway do not show the destination in advance.

## 4. Cover the alternate approaches and vary the route

The saved `LockerArea` contains **two `DoorLocker` instances**. Put a gate on each accessible approach if both doors can reach the lockers. Otherwise the player can walk around the loop. Gate placement is manual; the scripts do not detect or block alternate routes.

Each gate supports several arrival markers. For example:

| Destination entry | Result on that gate's next crossing |
| --- | --- |
| 0: `Arrival_Hallway` | Return to the familiar hallway |
| 1: `Arrival_PantryApproach` | Arrive on the approach to the pantry |
| 2: `Arrival_ITApproach` | Arrive on the approach to the IT room |

The sequence wraps `0 → 1 → 2 → 0` indefinitely. **Each gate has its own sequence**, advancing only when that gate successfully relocates the player. Starting a new loop session resets all gate sequences. Use one destination first, then add routes gradually and ensure each destination leads back to a gate.

Gates are one-way. For a path that should also loop when retraced, add a separate gate facing the opposite direction and give it its own arrival marker. Avoid overlapping opposite gates; leave walking space between boundaries and arrivals. An arrival does not itself count as another crossing.

Keep the controller, story trigger, gates and destination markers in a separate scene-root group. Do not parent them to the player or room objects you intend to deactivate as future anomalies. You do not need to duplicate `AllWalls`, the room furniture, or the complete floor for these loops.

## 5. End the loop and connect later story work

- `BeginLoop()` activates the routes. Calling it again while active does not reset progress.
- `EndLoop()` stops all relocations and cancels a pending fade. The player stays at the current location, can follow the ordinary layout, and can reach the real locker if the original route allows it. It does not teleport the player back or reset room props.
- **On Passage Completed** fires after relocation while the screen is covered. Its integer starts at 1. Use it later for a separate room-variation script, sound changes or story progression. Avoid changing player-script enabled states in this callback because the transition restores their previous states.
- **On Loop Ended** can restore any future room variations.
- If another system teleports the player, call `NotifyPlayerRepositioned()` afterward so gates do not mistake that movement for walking through the floor.

There is no automatic passage limit, victory condition, or “correct direction” rule yet. Your future anomaly or story controller can decide when to call `EndLoop()`.

## Play Mode checks

1. Before the production reminder, visit the locker normally: gates should do nothing.
2. Enter the green production volume: the loop starts once.
3. Walk back toward the locker: crossing a cyan gate should briefly fade and place you at its arrival marker.
4. Walk, sprint and strafe across each gate. Try looking behind you while crossing. Only body movement through the rectangle should count.
5. Stand still at arrival: there should be no second relocation. Walk the route again to repeat.
6. Add two destinations to one gate and repeat it three times: arrivals should be A, B, A.
7. In Play Mode, use the controller component's context menu **End Loop (Play Mode)**. The locker should become reachable along the ordinary route.
8. End the loop or disable its controller during a fade: the screen should clear and player controls should return.

Verification performed: the three scripts compile against the installed Unity 2022.3.62f3 assemblies, and 14 automated doorway-crossing checks pass, including forward/reverse travel, fast movement, opening bounds, diagonal crossings and standing on the plane. An isolated Unity Play Mode test launch stopped before executing because its licensing client could not connect (exit code 199). Fade, script restoration, route sequencing and story-trigger behavior therefore still require the Play Mode checks above, along with manual marker placement, clear landing space, doorway sightlines and alternate routes in your level.
