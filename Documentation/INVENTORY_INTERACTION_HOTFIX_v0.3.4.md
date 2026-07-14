# Shadow Supply v0.3.4 — Inventory Interaction Hotfix

## Problem

The redesigned inventory rendered correctly, but inventory slots could not be clicked.

## Root cause

The UXML root element was explicitly configured with:

```xml
picking-mode="Ignore"
```

That prevented the inventory hierarchy from reliably participating in UI Toolkit pointer picking.

## Fixed

- Removed `picking-mode="Ignore"` from the full inventory root
- Explicitly enabled pointer picking on:
  - UI document root
  - Inventory template
  - Full inventory screen
  - Inventory frame
  - Inventory grid
  - Inventory hotbar
  - Slot buttons
  - Action buttons
  - Inspection controls
- Kept non-interactive decorative elements ignored
- Replaced slot `Button.clicked` selection with a direct left-button `PointerDownEvent`
- Stops the slot pointer event after selection to prevent unintended propagation
- Brings the full inventory screen to the front when opened
- Focuses the UI document when the inventory opens

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:
   - `InventoryHUD.cs`
   - `InventoryScreen.uxml`
4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Press Play.

Do not run the setup menu again.

## Acceptance test

1. Pick up at least two different items.
2. Open the inventory with `TAB` or `I`.
3. Click an occupied slot.
4. Confirm:
   - The amber selection border moves to the clicked slot.
   - The details panel updates to the clicked item.
   - Use, Drop, Split, and Inspect reflect the selected item.
5. Click an inventory hotbar slot.
6. Confirm the equipped hotbar slot changes.
7. Close the inventory and verify player control returns.

## Version

`v0.3.4-inventory-interaction-hotfix`
