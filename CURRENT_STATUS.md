# Shadow Supply — Current Status

Update this file after each meaningful development session.

## Current version

- Current clean rewrite: `v0.3.0-save-foundation`
- Repository: `jaychase464-maker/Shadow-Supply`
- Source-of-truth branch: `main`
- Save schema version: `1`
- Legacy v0.10.x work: historical design reference only
- Experimental v0.11.x island rebuild: not the active project

## Unity version

- Unity Editor: `6000.0.58f2`
- Revision: `92dee566b325`

## Package versions

- Universal Render Pipeline: `17.0.4`
- Input System: `1.14.2`
- AI Navigation: `2.0.9`
- uGUI: `2.0.0`

## Completed milestones

### Milestone 1 — Playable Foundation

- Verified working locally
- First-person movement
- Sprint
- Jump
- Crouch
- Cursor locking
- Interaction framework
- Development playground

### Milestone 2 — Items and Inventory

- Verified working locally
- Stable item definitions
- Item categories, quality, and condition
- Stackable inventory
- Eight-slot hotbar
- World pickups
- Held-item display
- Dropping items
- Development inventory UI
- Dropped-item physics hotfix committed

## Current milestone

### Milestone 3 — Save and Load Foundation

- Versioned save files
- Three save slots
- Player transform persistence
- Inventory persistence by item ID
- Hotbar persistence
- World-pickup persistence
- Dropped-item physics-state persistence
- Save-slot metadata
- Development save HUD

## Current compilation state

- Milestone 1 and Milestone 2 reported working
- No compilation errors were reported before Milestone 3
- Milestone 3 package revision 2 prepared after resolving `CS0104`
- Local compilation and acceptance testing pending

## Current known risks

- Save schema migrations have not yet been needed.
- The save and inventory interfaces are temporary development interfaces.
- Runtime fallback meshes are placeholders.
- Production item prefabs will need correctly sized colliders.
- The repository only reflects committed and pushed local changes.

## Next planned work

1. Validate all three save slots.
2. Verify player, inventory, hotbar, and world-item restoration.
3. Commit and push Milestone 3.
4. Begin persistent furniture placement.
5. Integrate placed objects into save schema version `1` without changing existing fields.
6. Build floor placement before wall placement and electrical objects.

## Session log

### 2026-07-13

- Changes:
  - Verified Milestone 1.
  - Added and verified Milestone 2 inventory behavior.
  - Committed the dropped-item physics hotfix.
  - Prepared Milestone 3 save and load foundation.
  - Corrected ambiguous `Random` reference in `WorldItemPickup.cs`.
- Tests performed:
  - Milestone 1 passed local playtesting.
  - Milestone 2 inventory systems passed local playtesting.
  - Initial Milestone 3 package reported `CS0104` in `WorldItemPickup.cs`.
  - Corrected package testing pending.
