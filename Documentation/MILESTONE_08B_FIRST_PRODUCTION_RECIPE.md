# Milestone 8B — First Production Recipe

## Goal

Milestone 8B turns the physically powered starter workbench into the first
functional production station in Shadow Supply.

The player can now:

1. Collect the required materials.
2. Physically power the workbench.
3. Open the workbench interface.
4. Begin assembly.
5. Lose power without losing progress.
6. Finish a physical package on the workbench.
7. Collect the package into inventory.
8. Save and restore production progress.

## First recipe

### Sealed Hardware Package

Requirements:

- `2× Metal Components` — consumed
- `1× Polymer Housing` — consumed
- `1× Packaging Material` — consumed
- `1× Basic Toolkit` — required but not consumed
- powered starter workbench
- approximately `8 seconds`

Output:

- `1× Sealed Hardware Package`

The finished package is a fictional game item and contains no real-world
assembly instructions.

## Powered production behavior

Production only advances while the workbench reports `IsPowered`.

If the player:

- unplugs the machine
- turns off the work circuit
- trips the work circuit
- turns off the main breaker

production pauses immediately.

Restoring power resumes from the same remaining time.

## Production quality

Output quality is calculated from consumed ingredient stacks.

The first implementation uses:

- ingredient quality
- ingredient condition
- quantity-weighted averaging

The Basic Toolkit is a requirement but is not included in the quality
average because it is not consumed.

Future systems can extend this calculation with:

- player production skill
- workbench tier
- machine condition
- timing mistakes
- recipe familiarity
- environmental cleanliness

## Finished-output handling

Completed output does not silently disappear into inventory.

Instead:

- the timer finishes
- a sealed package appears on the workbench output tray
- the workbench reports `OUTPUT READY`
- the player presses `COLLECT PACKAGE`
- inventory space is checked
- the package is added with its calculated quality and condition

If inventory is full, the output remains safely on the workbench.

## Production interface

The temporary Shadow Supply production interface displays:

- current workbench power
- recipe description
- required ingredients
- owned quantities
- consumed versus reusable requirements
- output
- assembly duration
- production progress
- paused/no-power state
- finished output quality
- collection controls

Player movement, interaction, hotbar input, inventory UI, and plug input are
temporarily disabled while this interface is open.

Production itself continues while the interface is closed.

## New items

### Packaging Material

- Category: Material
- Stack size: 20
- Base value: $8
- Stable ID: `production-packaging-material`

### Sealed Hardware Package

- Category: Product
- Stack size: 10
- Base value: $180
- Stable ID: `product-sealed-hardware-package`

Both items include:

- generated display prefabs
- held-item setup
- inventory icons
- stable save IDs

## Save schema 6

Save schema `6` adds production-workbench persistence.

Saved data includes:

- stable workbench ID
- active recipe ID
- remaining production time
- pending-output state
- pending item ID
- output quantity
- output quality
- output condition

Existing schema-5 electrical saves migrate automatically.

## Development test supplies

The setup ensures the player has at least:

- 4 Metal Components
- 2 Polymer Housings
- 1 Basic Toolkit
- 3 Packaging Materials

These quantities are only ensured for the current development test loop.
Supplier purchasing and the first full economy loop follow in later passes.

## Installation

Prerequisites:

- Confirmed Milestone 8A through `v0.8.1`
- Workbench plug and power system working
- Starter garage working
- No red Console errors before installation

Steps:

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity and wait for compilation.
5. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

6. Run:

   `Shadow Supply → Setup → Apply Milestone 8B First Production Recipe`

7. Run:

   `Shadow Supply → Validation → Validate Milestone 8B First Production Recipe`

Expected structural validation:

   `FIRST PRODUCTION RECIPE READY`

## Test flow

1. Ensure the workbench plug is connected to a powered work outlet.
2. Look at the workbench and press `E`.
3. Confirm all four requirements are listed.
4. Confirm the toolkit says `required tool`.
5. Press `START ASSEMBLY`.
6. Confirm consumed items leave inventory.
7. Turn off the work breaker during production.
8. Confirm the timer pauses.
9. Restore power.
10. Confirm the timer resumes.
11. Wait for completion.
12. Confirm a package appears on the output tray.
13. Press `COLLECT PACKAGE`.
14. Confirm the finished item appears in inventory.
15. Inspect its quality and condition.

## Save/load test

### In-progress process

1. Start production.
2. Wait several seconds.
3. Save.
4. Allow the process to finish or leave the area.
5. Load.
6. Confirm the saved remaining time returns.
7. Confirm power loss still pauses the restored process.

### Finished output

1. Finish a package but do not collect it.
2. Save.
3. Collect it.
4. Load.
5. Confirm the saved package returns to the output tray.
6. Confirm quality and condition are preserved.

## Regression test

- Physical plug still works
- Cable floor following still works
- Breakers still work
- Garage lighting still works
- Inventory and hotbar still work
- Furniture shop and deliveries still work
- Save/load still works
- Garage doors still work
- No red Console errors appear

## Deferred

- Buyer order
- Delivery location
- Dirty-cash payment
- Reputation reward
- Heat reward
- Multiple production recipes
- Production skill
- Workbench upgrades
- Failure or mistake interactions
- Physical ingredient placement
- Animated tools and assembly motions
- Final UI Toolkit production interface

## Version

`v0.8.2-first-production-recipe`
