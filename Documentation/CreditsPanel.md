# Credits panel

The panel follows the supplied reference: an OldCupboard title, fixed Model name and Creator headings, a translucent charcoal panel, and a red Back button. Both columns scroll together. Long names wrap, and each row grows to keep its model and creator aligned.

## Add it to a scene

1. Choose **Tools > Project NTH > Create Credits Panel**, or drag `Assets/Assets/Prefabs/Credits Panel.prefab` into the scene as a root object. It supplies its own overlay Canvas.
2. On its **Credits Panel UI** component, enable **Open On Start** to preview it in Play Mode or use it in a standalone credits scene. Leave it off when opening the panel from a menu.
3. On your menu's Credits button, add an **On Click** event, drag in the Credits Panel object, and select **CreditsPanelUI > Open()**.
4. Optionally assign the existing menu panel to **Menu To Hide**. Back hides the credits and restores that panel's previous active state. Keep the Credits Panel outside that menu's hierarchy, so hiding the menu does not disable the credits too.

The existing MainMenuPanel in SceneOutdoor is an empty scaffold. No Credits button or scene wiring is assumed or added automatically.

## Controls

- Mouse wheel, dragging the list, or dragging the slim scrollbar scrolls the credits.
- Up/Down scroll; Page Up/Page Down move a page; Home/End move to the beginning/end.
- Back or Escape closes the panel. Back is selected on opening, so Enter also activates it.
- Reopening starts at the top. Scrolling also works while the game is already paused.

The scene's existing EventSystem is reused. A fallback using the project's Input System is created only when none is active, and disabled on closing.

## Content and appearance

Select `Assets/Assets/UI/Sketchfab Credits.asset` to edit, reorder, remove, or add entries. It contains the 38 extracted model/creator pairs and their source links. Source links are stored as reference information; clicking a row does not open a website. The source list and its scope are documented in `Documentation/SketchfabCredits.md`.

The prefab's UI hierarchy is editable: **Safe Area / Frame / Panel** contains the background, title, fixed headings, and Scroll View. **Frame / Back** holds the button. The inactive **Row Template** under the root controls the body font, size, and color for both columns. Rows are generated from the credits asset in Play Mode; edit the asset and template instead of individual generated rows. The component controls column proportions, spacing, and minimum row height. Call `RefreshCredits()` if you change the list while running.

## Connecting it during gameplay

The panel unlocks and shows the cursor while open and restores its previous state on closing or disabling. It does not change Time.timeScale. For an in-game menu, put the movement, camera-look, and interaction components you want suspended in **Disable While Open**. Their previous enabled states are restored, including components that were already disabled. For a main-menu scene this list can remain empty.

**On Opened** and **On Closed** are optional UnityEvents for existing menu sounds or navigation. Do not disable the Credits Panel itself while opening it. To hide the panel, call `Close()`; keep its root active so the menu button can call `Open()` again.
