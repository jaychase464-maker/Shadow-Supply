# Milestone 3 Package Revision 2 — Compile Fix

## Problem

The first Milestone 3 package added `using System;` to `WorldItemPickup.cs` for persistent GUID generation. The existing `Random.rotation` call then became ambiguous because both `System.Random` and `UnityEngine.Random` were in scope.

Unity reported:

```text
Assets\_Project\Scripts\Runtime\Inventory\WorldItemPickup.cs(136,17): error CS0104: 'Random' is an ambiguous reference between 'UnityEngine.Random' and 'System.Random'
```

## Fix

The complete script now explicitly calls:

```csharp
UnityEngine.Random.rotation
```

The other random physics calls were already fully qualified. No save-schema or serialized-data changes were made.

## Installation

1. Close Unity.
2. Extract `ShadowSupply_Milestone3_SaveLoad_v2.zip` into the Unity project root.
3. Allow all files to merge and replace the revision 1 files.
4. Reopen Unity and wait for compilation.
5. Run `Shadow Supply → Setup → Apply Milestone 3 Save System`.
6. Continue the Milestone 3 acceptance test.
