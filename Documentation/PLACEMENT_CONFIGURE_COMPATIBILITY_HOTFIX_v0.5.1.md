# Shadow Supply v0.5.1 — Placement Configure Compatibility Hotfix

## Problem

Milestone 5 expanded `PlacementController.Configure` so it could receive the
player inventory reference.

The older Milestone 4 setup script still calls the original two-argument version:

```csharp
controller.Configure(playerCamera, database);
```

This caused Unity to report `CS7036`.

## Fix

`PlacementController` now supports both signatures:

```csharp
Configure(Camera, PlaceableDatabase)
Configure(Camera, PlaceableDatabase, PlayerInventory)
```

The two-argument compatibility overload automatically finds `PlayerInventory`
on the player object and forwards to the Milestone 5 configuration method.

No save data, item IDs, furniture IDs, models, prefabs, or scene data were changed.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Runtime/Placement/PlacementController.cs`

4. Reopen Unity and wait for compilation.
5. Continue with:

   `Shadow Supply → Setup → Apply Milestone 5 Furniture Ownership`

Do not run the Milestone 4 setup again.

## Version

`v0.5.1-placement-configure-compatibility`
