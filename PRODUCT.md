# Project NTH

<!-- impeccable:product-schema 1 -->

## Platform

Unity game, currently developed on Windows with Unity 2022.3.62f3. Release platforms are not specified in the confirmed brief.

## Users and Operating Context

Players explore rooms and hallways, interact with objects, follow tasks and destination markers, and encounter dialogue. Yarn Spinner supplies the dialogue content.

## Capabilities and Constraints

- Normal conversations use the existing dialogue controller and its configured gameplay restrictions.
- Player thoughts are a separate, passive dialogue display at the bottom of the screen. Each thought fades in and out without taking control away from the player.
- Thought text can be authored in Yarn and triggered by an area or interaction. The thought display advances automatically.
- The task reminder occupies the upper left, and Tab reveals its message. Destination markers remain visible through walls when the player faces them.
- Preserve existing game controls and unrelated scene behavior when extending UI.

## Evidence on Hand

These requirements come from the user's approved dialogue and HUD requests. Implementation and setup details are recorded in `Documentation/ThoughtDialogue.md`, `Documentation/TaskPanel.md`, and `Documentation/DestinationMarkers.md`. Broader audience, release, and commercial positioning are unconfirmed.
