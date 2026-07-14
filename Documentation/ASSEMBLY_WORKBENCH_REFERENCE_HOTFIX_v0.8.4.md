# Shadow Supply v0.8.4 — Assembly Workbench Reference Hotfix

## Symptom

Starting interactive production switched to the top-down workbench camera,
but no recipe parts appeared.

The overlay displayed:

- `Assembly unavailable.`
- `Steps: 0/0`

No Unity Console error was produced.

## Root cause

`InteractiveAssemblyController.Configure(...)` assigned the powered
workbench while the setup script was running.

However, the controller's `workbench` field was a private, non-serialized
runtime field. Unity therefore discarded that reference during scene/domain
reload.

The top-down camera and assembly surface survived because their fields were
serialized, but the controller no longer knew which production workbench
owned it. Without that reference it could not read the active recipe or
spawn any parts.

## Fix

The controller now:

- serializes its powered-workbench reference
- resolves the workbench automatically from its parent hierarchy in `Awake`
- resolves again when the assembly view opens
- resolves again before rebuilding recipe parts
- repairs the workbench's controller reference when needed
- logs a warning only when no parent workbench can actually be found

## Expected behavior

Starting the recipe should now spawn five table interactions:

- Basic Toolkit
- Metal Component 1
- Metal Component 2
- Polymer Housing
- Packaging Material

The toolkit is clicked. The four consumed parts are dragged from the table
into the open package.

The player does not drag icons directly from the hotbar. Starting the recipe
reserves the inventory materials and creates their physical workbench
versions.

## Installation

1. Exit Play Mode.
2. Close Unity.
3. Extract this ZIP into the Unity project root.
4. Replace:

   `Assets/_Project/Scripts/Runtime/Production/InteractiveAssemblyController.cs`

5. Reopen Unity and wait for compilation.
6. Open `Dev_Playground`.
7. Enter Play Mode and start the recipe again.

The interactive-production setup menu does not need to be rerun.

## Acceptance test

- Top-down assembly view opens
- Overlay no longer says `Assembly unavailable`
- Overlay shows `Steps: 0/5`
- Five physical interaction objects appear on the workbench
- Toolkit can be clicked
- Four consumed parts can be dragged
- Invalid drops return to the table
- Valid drops snap into the package
- Lid becomes available at `Steps: 5/5`
- No red Console errors appear

## Version

`v0.8.4-assembly-workbench-reference-hotfix`
