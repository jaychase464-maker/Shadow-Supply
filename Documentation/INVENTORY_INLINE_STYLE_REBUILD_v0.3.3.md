# Shadow Supply v0.3.3 — Inventory Inline Style Rebuild

## Why this update exists

The inventory UXML structure was loading correctly, but Unity continued to render default UI Toolkit controls instead of applying the intended industrial stylesheet.

The prior stylesheet hotfix did not solve the problem.

## New approach

The inventory no longer depends on USS for its essential appearance.

`InventoryHUD.cs` now applies the complete layout and visual presentation through runtime inline UI Toolkit styles, including:

- Full-screen inventory frame
- Horizontal compact hotbar
- Six-column inventory grid
- Eight-slot inventory hotbar
- Header statistics
- Equipment panel
- Item details panel
- Action buttons
- Footer controls
- Inspection modal
- Amber selection states
- Teal status accents
- Item images scaled with UI Toolkit `Image` elements

The USS file may still load, but it is no longer required for the interface to be usable or correctly arranged.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace `Assets/_Project/Scripts/Runtime/UI/InventoryHUD.cs`.
4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Press Play.

Do not run any setup menu again.

## Acceptance test

### Gameplay

- Eight hotbar slots appear horizontally at the bottom center.
- Slots are dark and compact.
- The equipped slot has an amber border.

### Full inventory

- `TAB` or `I` opens the inventory.
- The interface fills the screen.
- The inventory grid uses six columns.
- The right-side equipment and details panels are visible.
- Item icons fit inside their slots.
- Buttons no longer stretch across the screen.
- Closing the inventory restores player control and cursor lock.

## Version

`v0.3.3-inventory-inline-style`
