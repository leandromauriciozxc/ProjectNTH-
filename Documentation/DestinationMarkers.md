# Destination markers

Place a marker at a destination to guide the player there. It stays visible through walls when that destination is in the camera's view. Looking away or moving it off-screen hides it; there are no screen-edge arrows.

## Place a marker

1. Exit Play mode and drag **Assets/Assets/Prefabs/Destination Marker** into the scene. Alternatively, select an area object and choose **Tools > Project NTH > Create Destination Marker** to create one at that object's position.
2. Move the marker to the destination, usually at floor level. The Scene view gizmo shows where its icon will appear.
3. In **Destination Marker**, set **Area Name**, such as `Pantry`, `Locker Room`, or `Elevator`.
4. Leave **Player Camera** empty to use the camera tagged `MainCamera`. With Cinemachine, this is the real rendering camera, not a virtual camera. Assign a Camera explicitly if needed.
5. Enter Play mode and face the destination. You should see a mint diamond, the area name and optional distance, even through an opaque wall.

**World Offset** raises the icon above the marker's position (default 1.5 metres). It ignores parent scale. **Show Distance** can be turned off. Distance is a straight line from the camera to the destination, or from **Distance Origin** if you assign the player's transform. **Maximum Distance = 0** means unlimited range. **Icon** accepts a replacement UI sprite. Blank Area Name leaves only the icon and optional distance.

The UI is built automatically during Play mode. No existing Canvas, EventSystem, camera changes, wall-material changes or physics colliders are needed. Markers do not intercept clicks or the player's interaction ray. Disabling/removing a marker also removes its generated UI. Multiple markers can be active independently.

## Show the next objective

Use **Marker Visible** to choose which markers are initially active. From an existing Interactable event, Timeline Signal Receiver, or another UnityEvent, drag in the marker and choose:

- **DestinationMarker.Show()** to reveal the next destination.
- **DestinationMarker.Hide()** to hide the completed destination.
- **DestinationMarker.SetVisible(bool)** when the event supplies a checkbox.
- **DestinationMarker.SetAreaName(string)** to change its label.

You can also activate/deactivate the marker GameObject. Reaching a marker does not automatically finish a task or hide it, so walking close on the other side of a wall cannot accidentally complete an objective. Connect Show/Hide to the story event that actually completes the task. Markers are not automatically connected to particular dialogue sequences.

## Add VFX later

Place your effect under the prefab's **VFX (add effects here)** child. The marker's Show/Hide methods control that child too. The effect remains at the world destination, while the HUD icon tracks it on-screen. The HUD icon is visible through walls; world particles/meshes still follow their own material's depth settings.

## Check in the game

- Face a marker from another room: the icon should remain visible through a wall.
- Turn away or move the destination outside the view: the marker should disappear with no edge arrow.
- Move the player/camera and confirm the marker follows its destination without inheriting its scale or rotation.
- Show/hide one marker while another is active; only that marker and its VFX child should change.
- Disable and re-enable the marker, then unload its scene: no duplicate or leftover UI should remain.
- Check your dialogue/cutscene flow; hide the current objective through an event when desired. Transition fades render above the marker by default.
