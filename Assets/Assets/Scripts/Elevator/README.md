# Stationary elevator with a world-space indicator

Unity 2022.3; uses the project's TextMeshPro, Interactable and E-key raycast.
Nothing automatically modifies a scene when these scripts import.

## First setup

1. Open Unity and `Scene1TestingExit8`. Exit Play Mode.
2. Select the existing **Elevator** object in the Hierarchy. Its door panels must be in their closed Edit Mode positions.
3. Choose **Tools > Project NTH > Set Up Selected Elevator**.
4. The new **Elevator Controller** takes over the existing doors. The old **ElevatorDoor** component stays attached but disabled; do not enable both. Setup copies its opening distance and door duration.
5. Under **Elevator > Elevator Controls**, check **Cabin Indicator**, **Hall Indicator**, **Cabin Buttons** and **Hall Call Buttons** (older setups use **Hall Open Button**). Placement is based on the saved elevator geometry; inspect both sides and adjust their transforms if needed. The indicators are World Space Canvases with TMP text. Keep them slightly in front of the wall to avoid flickering.
6. Save the scene when happy. The setup operation supports Undo and refuses to install twice.

## Try the example

With **Open On Start** enabled, the cabin starts closed at **Starting Floor**, waits **Initial Opening Delay**, and opens. Enter, look at another floor button, and press **E**. Doors close over 1.5 seconds, the indicator shows the direction during a 2-second-per-floor simulated ride, then shows the destination name. After 0.6 seconds the doors open and stay open unless another hallway call is queued. Repeated floor-button presses during a ride are ignored.

The cabin and player do not physically move. Player movement and looking are not disabled. This controller does not include a doorway obstruction sensor. Selecting the current floor simply opens its doors. A legacy hallway OPEN button still opens doors; change its Action to a hallway call as described below.

## Floor names and the saved scene's door-symbol entries

The indicator uses **Elevator Controller > Floors > Label** in both travel directions. Floor buttons with **Automatic Label** enabled use those same names. Door controls and hallway arrows must not be extra entries in the Floors list.

The inspected scene has `G`, `>|<`, `<|>`, then `1` through `15`. In Edit Mode, select the Elevator and click **Remove Door Entries and Remap Buttons** in its controller Inspector. This removes only those two symbol entries and preserves each real floor button's destination, arrival events and Starting Floor. It supports Undo and does not save the scene automatically. The repair refuses to remove entries with arrival events or real floor/call buttons pointing to them; resolve those assignments first. The corrected list is `G`, `1`, ... `15`.

## Hallway UP and DOWN calls

For your existing panel, select the hallway button and set **Action = Call Up**. Duplicate it for **Action = Call Down** and position the duplicate beside/below it. On BOTH, set **Hallway Floor** to the floor where the player stands, and keep the same Elevator reference. Keep **On Interact > ElevatorFloorButton.Press()** pointed at each button's own component. Enable Automatic Label for UP/DOWN text, or disable it to use your own arrow text.

UP/DOWN calls bring the elevator to that hallway floor; they do not move the player one floor. The player chooses a destination after entering. Calls received during a trip are remembered; the buttons stay highlighted while pending. Further callers are served after **Hallway Boarding Time** has elapsed with doors open. Up is unavailable on the highest floor; Down is unavailable on the lowest. This is a single-cabin, first-requested-floor queue, not a multi-elevator dispatch system.

## Starting floor and initial door delay

- Choose the named **Starting Floor** in the controller Inspector before Play Mode. The indicator immediately shows this floor; it does not count upward from the first entry.
- Enable **Open On Start**, then set **Initial Opening Delay** in seconds. Doors remain closed during this delay, then animate open over **Door Duration**. Set the delay to 0 to start opening immediately.
- Leave Open On Start disabled to begin with closed doors until requested. Startup settings are applied when the elevator first activates, including when it is initially inactive.
- Enable **Apply Starting Floor On Start** if that floor's On Arrival event should initialize its landing area. Otherwise arrange the initial landing yourself. A normal disable/re-enable returns to the last reached floor; it does not replay the initial delay once startup has completed.

## Front-only interaction

**Front Face Only** is enabled on elevator buttons. The E-key raycast rejects hits from behind, so neither the hand prompt nor the action activates there. The default **Front Direction = (0, 0, -1)** matches the existing World Space Canvas text. If a custom button faces the opposite way, change it to `(0, 0, 1)`. This check respects the button's rotation. Other kinds of Interactable keep their existing behavior.

## Add OPEN and CLOSE to an existing panel

Keep your existing floor buttons and panel layout. **Do not rerun setup** to update an existing elevator.

1. Select the bottom OPEN button. In **Elevator Floor Button**, set **Action = Open Doors**.
2. Select the bottom CLOSE button. Set **Action = Close Doors**.
3. Both buttons need their **Elevator** reference assigned to the cabin's controller, plus **Label** and **Background** references. If you duplicated a working floor button, those should already be assigned; check each button's **Interactable > On Interact ()** targets its own **ElevatorFloorButton.Press()**.
4. **Floor Index is ignored** for both door actions. Other buttons keep **Action = Select Floor** and their existing floor indices.
5. Save and test with E. With **Automatic Label** enabled, labels become OPEN/CLOSE in Play Mode. To use your own text, follow the label instructions below.

OPEN opens the doors at the current floor. It can reverse a closing door smoothly; if a floor selection was closing the doors for departure, OPEN cancels that pending trip. Select the destination again when ready. CLOSE closes the doors at the current floor without moving the elevator or firing an arrival event. It can also reverse opening doors. Repeated requests in the same direction do not restart the animation. Both controls are ignored during travel and the brief arrival pause, so the doors cannot open between floors.

Existing hallway OPEN buttons automatically retain their behavior when the old Open Doors Only setting migrates to the new Action field. Fresh elevator setups include OPEN and CLOSE beneath the example floor buttons.

## Change a button's label

1. Exit Play Mode and select the button.
2. In **Elevator Floor Button**, uncheck **Automatic Label**.
3. Select the text object assigned to **Label** (normally the button's **Label** child), and edit its **TextMeshPro > Text Input**.
4. Save the scene. The script will leave that text untouched during Play Mode, including when doors move or the elevator changes floors.

This works for both door and floor buttons. **Action** and **Floor Index** still control what the button does; the label only changes its appearance. Leave **Automatic Label** enabled if you want the script to use the floor name or OPEN/CLOSE. The separate E-key interaction prompt still describes the action.

If E does not work, check the player's **Raycast > M Layermask** includes **Interactable**, and that the controls are within the raycast distance (normally 3 metres). The new buttons have solid BoxColliders on the existing Interactable layer; a wall collider in front of them will block interaction. No mouse cursor, GraphicRaycaster or new EventSystem is required.

## Connect actual destinations

The two initial floors are an editable demonstration. They do not yet change the area outside the cabin or load scenes.

On **Elevator Controller > Floors**, list floors lowest to highest and edit each **Label** (for example `G`, `2`, `3`). Button **Floor Index** uses array positions: 0 is the first floor, 1 the second, and so on. Duplicate a cabin button and change its Floor Index for each added floor. Set **Starting Floor** to the initial floor's index. The display advances through every intervening entry; include intermediate floors if you want them shown.

For the planned illusion of travel, place alternate landing areas at the same doorway. Under each floor's **On Arrival ()** event:

- Add the other landing roots with **GameObject > SetActive(bool)** unchecked.
- Add this floor's landing root with **GameObject > SetActive(bool)** checked.

These events run after reaching the destination, while the doors are completely closed. They do not fire for intermediate floors or same-floor requests. Startup invokes its event only when Apply Starting Floor On Start is enabled. Otherwise set the initial landing's active state yourself in Edit Mode. Give each destination its own complementary events so return trips work.

Keep the elevator, player, controls and shared lighting outside those landing roots. Do not disable the controller during an arrival event, or enable the old ElevatorDoor script. Scene loading/teleporting would need integration with your existing transition system; it is not included in this example.

## Sound and appearance

Assign **Door Clip**, **Travel Loop** and **Arrival Chime** on the controller if you have recordings. The setup provides separate 3D AudioSources for effects and travel. Unassigned clips are silent. The number and direction use the existing font and a muted green display. If that font lacks arrow glyphs, direction reads UP/DOWN. Display text refreshes on elevator state changes rather than every frame.

## Check in Play Mode

- Ride up and down; verify numbers, door closure before travel and opening after arrival.
- Choose a non-first Starting Floor and a 3-second Initial Opening Delay; verify the initial name and closed-door delay before animation begins.
- Look at the back of a cabin/hallway button: it should not show an E prompt or activate.
- Call from a different hallway floor while the elevator is busy; verify that it finishes its current trip, waits for boarding, and comes to the caller's floor.
- Press a floor repeatedly while travelling; only the original trip should complete.
- Select the current floor; there should be no travel or landing event.
- Press CLOSE then OPEN while stationary: doors move without changing floors or invoking landing events.
- Reopen halfway through a departing door closure: the doors should reverse without jumping and the pending trip should cancel. Choose the destination again to depart.
- Press both door controls during travel: the doors should stay closed and the trip should finish normally.
- Check both indicators from the cabin and hallway for orientation and clipping.
- After wiring landing events, confirm the correct landing and its colliders are active in both directions, and the player stays in the cabin.
- Confirm optional sounds stop on arrival, and after disabling the elevator during a ride. Re-enabling it resets it to its last reached destination and the configured Start Open state.

The scripts can be compiled with Unity closed. Scene placement, event wiring and runtime behavior still require verification in Unity.
