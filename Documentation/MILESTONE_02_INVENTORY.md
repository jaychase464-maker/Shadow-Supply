# Milestone 2 — Items, Inventory, Hotbar, and Held Items

## Included

- Stable-ID `ItemDefinition` ScriptableObjects
- Item categories
- Item quality tiers
- Item condition values
- Stackable item data
- Twenty-four-slot player inventory
- Eight-slot hotbar
- World item pickups
- Partial pickup support when inventory capacity is limited
- Number-key and mouse-wheel hotbar selection
- Held-item display
- Runtime fallback display models
- Drop-one-item behavior
- Hotbar HUD
- Inventory development interface
- Four sample development items
- One-click playground upgrade tool

## Setup

1. Extract this package into the Unity project root.
2. Reopen Unity and wait for compilation.
3. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

4. Run:

   `Shadow Supply → Setup → Apply Milestone 2 Inventory`

5. Enter Play Mode.

## Controls

- `E`: Pick up the targeted item
- `1–8`: Select a hotbar slot
- `Mouse Wheel`: Cycle hotbar selection
- `TAB` or `I`: Open and close inventory
- `Q`: Drop one item from the selected slot

## Test items

- Metal Components
- Polymer Housing
- Basic Toolkit
- Sealed Package

## Acceptance checklist

- No red Console errors
- Looking at a pickup displays its name and quantity
- Pressing E transfers the pickup into inventory
- Matching items stack up to their maximum stack size
- Overflow uses another empty slot
- Hotbar selection works with number keys
- Mouse wheel cycles the hotbar
- Selected items appear in the player's hand
- TAB or I opens the inventory
- Player movement pauses while inventory is open
- Q removes one item and drops it into the world
- Dropped items can be picked up again

## Architecture notes

Item definitions use a stable serialized ID so later save files can reference item IDs rather than Unity object references. The temporary OnGUI interface is a development interface and will later be replaced by the polished UI Toolkit inventory after the behavior is stable.

## Next milestone

Milestone 3 will implement the first save architecture:

- Versioned save files
- Save slots
- Player transform persistence
- Inventory serialization by item ID
- Item database lookup
- Hotbar selection persistence
- World pickup persistence foundation
