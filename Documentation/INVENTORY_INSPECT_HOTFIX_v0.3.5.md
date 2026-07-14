# Shadow Supply v0.3.5 — Inventory Inspect Modal Hotfix

## Problem

The Inspect button received input, but the inspection window did not become visible.

## Root cause

The full inventory screen is deliberately moved to the front of the UI hierarchy
when the inventory opens so its slots and buttons receive pointer input.

The inspection modal is a sibling of that screen. Opening the modal changed its
display state but did not move it above the inventory screen, so it rendered behind
the inventory and appeared to do nothing.

## Fixed

- Moves the inspection modal to the front every time Inspect is pressed
- Forces the modal to repaint immediately
- Keeps the inspection card above the modal backdrop
- Focuses the close button when the modal opens
- Restores the full inventory screen to the front when the modal closes
- Preserves all inventory, item, input, and save data

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:
   `Assets/_Project/Scripts/Runtime/UI/InventoryHUD.cs`
4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Press Play.

Do not run the setup menu again.

## Acceptance test

1. Pick up an item.
2. Open the inventory.
3. Select the occupied slot.
4. Press Inspect.
5. Confirm the inspection card appears over a darkened background.
6. Confirm the item name, category, description, quantity, quality, condition,
   value, and large item image appear.
7. Close it with the X button, Escape, or by clicking the backdrop.
8. Confirm the normal inventory remains usable.

## Version

`v0.3.5-inventory-inspect-hotfix`
