# Shadow Supply v0.4.1 — Placeable Database Compile Hotfix

Fixes `CS0177` in `PlaceableDatabase.TryGetDefinition`.

The method now initializes the `out` parameter before checking for an empty ID, then performs the dictionary lookup only for valid IDs.

No save fields, prefabs, scenes, or placeable IDs were changed.
