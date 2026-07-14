# Shadow Supply v0.8.3 — Interactive Production Framework

## Design rule

Timer-only production is no longer the intended production model.

Every Shadow Supply recipe must contain a physical/manual interaction
sequence. The recipe data determines which objects appear on the workbench,
while the shared framework handles clicking, dragging, package loading,
closing, persistence, and power interruption.

## Current Sealed Hardware Package interaction

Starting the recipe now:

1. Reserves and consumes the recipe's consumable materials.
2. Opens a dedicated top-down workbench camera.
3. Sprawls the required objects across the table.
4. Requires the player to click the reusable Basic Toolkit.
5. Requires the player to drag both Metal Components into the open box.
6. Requires the player to drag the Polymer Housing into the box.
7. Requires the player to drag Packaging Material into the box.
8. Highlights the package lid only after every required step is complete.
9. Requires the player to click the lid.
10. Animates the lid closing and creates the finished package.
11. Leaves the completed package on the workbench until collected.

## Generic recipe behavior

The interaction sequence is generated automatically from each
`ProductionRecipe`:

- Consumed ingredient units become draggable table objects.
- Reusable requirements become clickable tool objects.
- Each object receives a deterministic step index.
- All steps must be completed before the package can close.
- A recipe cannot bypass the manual assembly phase.

This makes the framework reusable for future recipes rather than
hard-coding the current package.

## Workbench camera

The upgrade creates an orthographic top-down assembly camera above the
workbench.

During assembly:

- normal movement is disabled
- the mouse cursor is unlocked
- the player camera is temporarily replaced by the assembly camera
- the actual item display prefabs appear across the physical table
- the player can exit with `Esc`
- the unfinished work remains on the table
- interacting with the workbench resumes the same assembly

## Power behavior

Manual assembly still depends on the physical electrical system.

When power is lost:

- dragging stops
- a held part returns to its starting location
- completed steps remain complete
- the package lid cannot be closed
- progress is not deleted

Restoring power immediately allows assembly to continue.

## Save schema 7

Schema `7` adds deterministic interactive-step persistence.

The game now saves:

- active recipe ID
- active output quality and condition
- completed interaction-step indices
- pending output item
- pending output quantity
- pending output quality
- pending output condition

Loading an unfinished recipe respawns the workbench objects and restores
every completed step.

Existing schema-6 production saves migrate with an empty completed-step
list, meaning an old in-progress timer recipe restarts at the beginning of
its new manual assembly sequence without losing its reserved ingredients.

## Installation

Prerequisites:

- Milestone 8B first recipe confirmed working
- physical workbench power confirmed working
- no red Console errors before installation

Steps:

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity and wait for compilation.
5. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

6. Run:

   `Shadow Supply → Setup → Apply Milestone 8B Interactive Production Upgrade`

7. Run:

   `Shadow Supply → Validation → Validate Milestone 8B Interactive Production`

Expected structural result:

   `INTERACTIVE PRODUCTION READY`

Do not rerun the old Milestone 8B setup afterward unless the interactive
upgrade is run again.

## Test flow

1. Connect the workbench to a powered outlet.
2. Interact with the workbench.
3. Start the Sealed Hardware Package.
4. Confirm the camera changes to a top-down table view.
5. Confirm five interaction objects appear:
   - one Basic Toolkit
   - two Metal Components
   - one Polymer Housing
   - one Packaging Material
6. Click the toolkit.
7. Drag each loose ingredient into the box.
8. Confirm failed drops return to the table.
9. Confirm the lid remains unavailable until all five steps are complete.
10. Click the highlighted lid.
11. Confirm the lid closes.
12. Collect the finished package.
13. Confirm its quality and condition remain intact.

## Power interruption test

1. Begin the manual assembly.
2. Complete several steps.
3. Turn off the work breaker.
4. Confirm dragging is blocked.
5. Confirm completed parts remain in the package.
6. Restore power.
7. Finish and close the package.

## Save/load test

1. Complete only some assembly steps.
2. Exit the workbench view.
3. Save.
4. Finish or alter the assembly.
5. Load.
6. Confirm the exact saved completed steps return.
7. Confirm unfinished parts return to the table.
8. Confirm the package can still be finished.
9. Save again with finished output waiting.
10. Collect the output and load.
11. Confirm the saved output returns.

## Future interactive step types

The shared system is the foundation for future recipe-specific actions:

- screw or bolt placement
- tool dragging
- hold-to-tighten interactions
- cutting and folding
- taping and sealing
- drilling alignment
- wiring connections
- ordered component placement
- timed precision zones
- contamination and mistake penalties
- machine-specific cameras and surfaces

These will extend the manual framework rather than return to passive
production timers.

## Version

`v0.8.3-interactive-production-framework`
