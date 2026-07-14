# Shadow Supply v0.3.1 — Inventory UI Redesign

## Goal

Replace the temporary `OnGUI` inventory with the full industrial UI concept selected for Shadow Supply.

The new interface uses Unity UI Toolkit with a UXML layout, USS stylesheet, and a data-driven runtime controller. Unity's supported runtime UI workflow uses a `UIDocument`, UXML structure, USS styling, and MonoBehaviour logic.

## Included

- Full-screen industrial inventory overlay
- Permanent compact gameplay hotbar
- Twenty-four dynamically generated inventory slots
- Eight dynamically generated hotbar slots
- Selected-slot amber highlighting
- Item detail card
- Item category, quantity, quality, condition, value, and description
- Equipment paper-doll presentation
- Cash, dirty cash, heat, and weight header presentation
- Item icon support through Resources
- Development item icons matching the selected concept
- Clickable inventory slots
- Functional Use, Drop, Split, and Inspect actions
- Large item inspection modal
- Keyboard and mouse-wheel hotbar navigation
- Cursor and first-person controller management
- Existing inventory and save-slot data preserved

## Important status values

Cash, dirty cash, and heat are presentation placeholders until their gameplay systems are implemented.

The current weight display uses total item quantity against a temporary capacity of `60`. A true per-item weight system will replace this later.

## Installation

1. Close Unity.
2. Extract the package into the Unity project root.
3. Allow files to merge and replace.
4. Reopen Unity and wait for compilation.
5. Open `Assets/_Project/Scenes/Development/Dev_Playground.unity`.
6. Run:

   `Shadow Supply → Setup → Apply Inventory UI Redesign`

7. Enter Play Mode.

## Controls

- `TAB` or `I`: Open or close inventory
- Click an item: Select it
- `E`: Use or equip selected item
- `Q`: Drop one selected item
- `R`: Split selected stack
- `1–8`: Select hotbar slot
- Mouse wheel: Cycle hotbar
- `Escape`: Close inspection or inventory

## Use behavior

- Selecting a hotbar item equips that slot.
- Using an inventory item outside the hotbar swaps it into the currently selected hotbar slot.
- Existing item quality and condition remain intact during the swap.

## Split behavior

- A stack must contain at least two items.
- An empty inventory slot must be available.
- Half of the stack is moved into the first empty slot.

## Acceptance test

- The compact hotbar appears during normal gameplay.
- `TAB` opens the full industrial inventory.
- The cursor unlocks and player movement stops while open.
- All twenty-four inventory slots render.
- Item names, quantities, quality, condition, value, and descriptions are correct.
- Clicking different slots updates the detail panel.
- Amber selection and equipped-slot borders update correctly.
- `E` moves or equips the selected item.
- `Q` drops one item with physics.
- `R` splits a valid stack.
- Inspect opens a large item presentation.
- `Escape` closes the inspection first, then the inventory.
- Closing the inventory restores movement and cursor lock.
- Save-system inventory data remains compatible.
- No red Console errors appear.

## Save compatibility

This update does not change save schema version `1`. Inventory slots still serialize using the same item IDs, quantities, quality values, and condition values.
