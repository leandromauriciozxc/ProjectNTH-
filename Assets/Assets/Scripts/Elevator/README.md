# Stationary elevator with a world-space indicator

Unity 2022.3; uses the project's TextMeshPro, Interactable and E-key raycast.
Nothing automatically modifies a scene when these scripts import.

## First setup

1. Open Unity and `Scene1TestingExit8`. Exit Play Mode.
2. Select the existing **Elevator** object in the Hierarchy. Its door panels must be in their closed Edit Mode positions.
3. Choose **Tools > Project NTH > Set Up Selected Elevator**.
4. The new **Elevator Controller** takes over the existing doors. The old **ElevatorDoor** component stays attached but disabled; do not enable both. Setup copies its opening distance and door duration.
5. Under **Elevator > Elevator Controls**, check **Cabin Indicator**, **Hall Indicator**, **Cabin Buttons** and **Hall Open Button**. Placement is based on the saved elevator geometry; inspect both sides and adjust their transforms if needed. The indicators are World Space Canvases with TMP text. Keep them slightly in front of the wall to avoid flickering.
6. Save the scene when happy. The setup operation supports Undo and refuses to install twice.

## Try the example

The cabin starts open on floor **1**. Enter, look at the new **2** button near the right door jamb, and press **E**. Doors close over 1.5 seconds, the indicator shows the direction during a 2-second simulated ride, then shows **2**. After 0.6 seconds the doors open and stay open. Selecting **1** returns. Repeated presses during a ride are ignored.

The cabin and player do not physically move. Player movement and looking are not disabled. This initial controller does not include a doorway obstruction sensor or automatic door closing: select a floor from inside the cabin to depart. Selecting the current floor simply opens its doors. The hallway OPEN button also opens idle doors; it is not a multi-floor external call dispatcher.

If E does not work, check the player's **Raycast > M Layermask** includes **Interactable**, and that the controls are within the raycast distance (normally 3 metres). The new buttons have solid BoxColliders on the existing Interactable layer; a wall collider in front of them will block interaction. No mouse cursor, GraphicRaycaster or new EventSystem is required.

## Connect actual destinations

The two initial floors are an editable demonstration. They do not yet change the area outside the cabin or load scenes.

On **Elevator Controller > Floors**, list floors lowest to highest and edit each **Label** (for example `G`, `2`, `3`). Button **Floor Index** uses array positions: 0 is the first floor, 1 the second, and so on. Duplicate a cabin button and change its Floor Index for each added floor. Set **Starting Floor** to the initial floor's index. The display advances through every intervening entry; include intermediate floors if you want them shown.

For the planned illusion of travel, place alternate landing areas at the same doorway. Under each floor's **On Arrival ()** event:

- Add the other landing roots with **GameObject > SetActive(bool)** unchecked.
- Add this floor's landing root with **GameObject > SetActive(bool)** checked.

These events run only after reaching the destination, while the doors are completely closed. They do not fire for intermediate floors, same-floor requests or startup. Set the initial landing's active state yourself in Edit Mode. Give each destination its own complementary events so return trips work.

Keep the elevator, player, controls and shared lighting outside those landing roots. Do not disable the controller during an arrival event, or enable the old ElevatorDoor script. Scene loading/teleporting would need integration with your existing transition system; it is not included in this example.

## Sound and appearance

Assign **Door Clip**, **Travel Loop** and **Arrival Chime** on the controller if you have recordings. The setup provides separate 3D AudioSources for effects and travel. Unassigned clips are silent. The number and direction use the existing font and a muted green display. If that font lacks arrow glyphs, direction reads UP/DOWN. Display text refreshes on elevator state changes rather than every frame.

## Check in Play Mode

- Ride up and down; verify numbers, door closure before travel and opening after arrival.
- Press a floor repeatedly while travelling; only the original trip should complete.
- Select the current floor; there should be no travel or landing event.
- Check both indicators from the cabin and hallway for orientation and clipping.
- After wiring landing events, confirm the correct landing and its colliders are active in both directions, and the player stays in the cabin.
- Confirm optional sounds stop on arrival, and after disabling the elevator during a ride. Re-enabling it resets it to its last reached destination and the configured Start Open state.

The scripts can be compiled with Unity closed. Scene placement, event wiring and runtime behavior still require verification in Unity.
