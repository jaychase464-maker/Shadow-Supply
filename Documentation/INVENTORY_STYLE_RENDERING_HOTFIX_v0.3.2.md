# Shadow Supply v0.3.2 — Inventory Style Rendering Hotfix

## Problem

The redesigned inventory structure loaded, but its USS styling did not apply. Unity therefore displayed the raw default UI Toolkit controls:

- Hotbar slots stretched vertically across the screen
- Inventory elements stacked in a single column
- Default light-gray buttons
- Tiny unstyled labels
- Missing industrial panels and selected-slot presentation

## Root cause

The stylesheet contained a comma-separated selector group:

```css
.equipment-left-slots,
.equipment-right-slots
```

Unity UI Toolkit USS does not reliably accept that CSS selector-list syntax in this runtime stylesheet. The failed stylesheet import left the UXML structure visible without its intended layout or visual rules.

The runtime script also attached the stylesheet only to the `UIDocument` root. The corrected implementation attaches the stylesheet directly to the instantiated inventory template.

## Fixed

- Replaced the grouped selector with two valid USS rules
- Instantiates the UXML as a `TemplateContainer`
- Attaches the stylesheet directly to the template container
- Forces the runtime UI root and template to fill the screen
- Preserves the previous `UnityEngine.Cursor` compile fix
- Preserves save schema version 1
- Does not change inventory data, item IDs, slots, or controls

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Allow these files to be replaced:
   - `InventoryScreen.uss`
   - `InventoryHUD.cs`
4. Reopen Unity and wait for the asset database and scripts to finish importing.
5. Open `Dev_Playground`.
6. Press Play.

Do not run the setup menu again.

## Acceptance test

### Gameplay hotbar

- The compact hotbar is centered near the bottom of the screen.
- Eight slots appear horizontally.
- Slots are dark instead of default gray.
- The equipped slot uses an amber highlight.

### Full inventory

- `TAB` or `I` opens a full-screen dark industrial interface.
- The inventory grid displays six columns instead of one vertical column.
- The right-side equipment and details panels appear.
- Header statistics display across the top.
- Footer control hints display across the bottom.
- Item artwork, names, quantities, and selection states render correctly.
- Closing the inventory restores movement and cursor lock.

## Version

`v0.3.2-inventory-style-hotfix`
