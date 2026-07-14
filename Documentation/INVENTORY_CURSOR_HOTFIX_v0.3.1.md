# Shadow Supply v0.3.1 — Inventory Cursor Compile Hotfix

## Problem

`InventoryHUD.cs` imports both `UnityEngine` and `UnityEngine.UIElements`.

Both namespaces contain a type named `Cursor`, so the compiler could not determine which cursor type should be used for runtime cursor locking and visibility.

## Fix

The four runtime cursor references now explicitly use:

```csharp
UnityEngine.Cursor.lockState
UnityEngine.Cursor.visible
```

This preserves the existing UI Toolkit inventory behavior and removes the ambiguity without changing save data, inventory data, scene objects, or input controls.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Allow `InventoryHUD.cs` to be replaced.
4. Reopen Unity and wait for compilation.
5. Enter Play Mode and test opening and closing the inventory.

No setup menu or scene regeneration is required.

## Acceptance test

- No `CS0104` Cursor errors appear.
- `TAB` or `I` opens the inventory.
- The cursor unlocks and becomes visible while the inventory is open.
- Closing the inventory hides and locks the cursor.
- Player movement returns after closing the inventory.
