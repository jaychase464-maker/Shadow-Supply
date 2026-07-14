# Milestone 5 — Furniture Ownership, Ordering, Deliveries, and Wall Placement

## Included

- Clean-cash and dirty-cash wallet foundation
- Furniture purchase prices
- Furniture supplier terminal
- Immediate physical delivery crates
- Persistent uncollected deliveries
- Furniture inventory packages
- Placement consumes one owned package
- Packing furniture returns one package
- Inventory-full protection during packing and collection
- Owned quantities displayed in build mode
- Wall-placement foundation
- CCTV camera wall placement
- Door keypad wall placement
- Save schema version `3`
- Automatic migration from save versions `1` and `2`
- Five user-supplied imported 3D assets:
  - Workbench
  - Metal pallet rack
  - Office desk
  - CCTV camera
  - Door keypad
- Utility cabinet remains a generated development model

## Installation

1. Close Unity.
2. Extract this package into the Unity project root.
3. Reopen Unity and wait for all FBX files, textures, and scripts to import.
4. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

5. Run:

   `Shadow Supply → Setup → Apply Milestone 5 Furniture Ownership`

6. Enter Play Mode.

## Gameplay flow

1. Walk to the furniture supplier terminal.
2. Press `E`.
3. Order furniture using clean cash.
4. Close the shop with `Escape`.
5. Walk to the orange delivery marker.
6. Press `E` on the delivery crate.
7. Press `B` to enter build mode.
8. Select the owned item with keys `1–6`.
9. Place it with left click.
10. Aim at placed furniture and press `Delete` to pack it back into inventory.

## Build controls

- `B`: Enter or exit build mode
- `1`: Workbench
- `2`: Storage Rack
- `3`: Office Desk
- `4`: Utility Cabinet
- `5`: CCTV Camera
- `6`: Door Keypad
- `R`: Rotate clockwise
- `Shift + R`: Rotate counterclockwise
- `Mouse Wheel`: Rotate
- `Left Mouse`: Place and consume one package
- `Delete` or `Backspace`: Pack targeted object into inventory
- `Right Mouse`: Exit build mode

## Acceptance test

### Shop and wallet

- Supplier terminal opens with `E`.
- Starting clean cash is `$1,200` for a new unsaved scene.
- Ordering deducts the correct price.
- Ordering fails without enough clean cash.
- Every successful order produces one delivery crate.

### Delivery

- Delivery crates appear on or beside the orange marker.
- Pressing `E` collects the furniture package.
- A full inventory prevents collection without deleting the crate.
- Saving with an uncollected delivery and loading restores the crate.

### Ownership

- Build mode displays the selected furniture's owned count.
- Furniture with zero ownership cannot be placed.
- Successful placement consumes exactly one package.
- Packing placed furniture returns exactly one package.
- A full inventory prevents packing and leaves the furniture in the world.

### Floor and wall placement

- Workbench, rack, desk, and cabinet require a floor.
- CCTV camera and keypad require a wall.
- Wall items face away from the mounted surface.
- Wall objects rotate in ninety-degree increments.
- Floor objects retain fifteen-degree rotation.
- Collision validation works for both surface types.

### Save migration

- Existing save schema `1` and `2` files load.
- Existing inventory, player, world items, hotbar, and furniture remain intact.
- Schema `3` stores wallet cash and uncollected delivery crates.

## Model note

The imported files were supplied by the project owner in earlier Shadow Supply asset uploads. Their final licensing, attribution, and commercial-use requirements must be verified before public release.
