# Shadow Supply — Current Status

Update this file after each meaningful development session.

## Current version

- Current clean rewrite: `v0.2.1-inventory-hotfix`
- Repository: `jaychase464-maker/Shadow-Supply`
- Source-of-truth branch: `main`
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

## Current milestone

### Milestone 2 — Items and Inventory

- Inventory behavior verified locally
- Stable item definitions
- Item categories, quality, and condition
- Stackable inventory
- Eight-slot hotbar
- World pickups
- Held-item display
- Dropping items
- Development inventory UI
- Dropped-item physics hotfix awaiting verification

## Current compilation state

- Milestone 2 reported working locally
- No compilation error was reported
- `v0.2.1` dropped-item physics hotfix pending local test

## Current known risks

- Save data has not been implemented yet.
- The inventory UI is a temporary development interface.
- Runtime fallback meshes are placeholders until final item models are assigned.
- Display prefabs will need correctly sized production colliders later.
- The repository only reflects committed and pushed local changes.

## Next planned work

1. Verify dropped items fall, collide, and tumble correctly.
2. Commit and push the hotfix.
3. Mark BUG-004 resolved with the hotfix commit hash.
4. Implement a versioned save architecture.
5. Serialize inventory using stable item IDs.
6. Add save-slot metadata and player transform persistence.
7. Build persistent world-item foundations before placement and electrical systems.

## Session log

### 2026-07-13

- Changes:
  - Verified Milestone 1 locally.
  - Added Milestone 2 inventory implementation.
  - Prepared dropped-item physics hotfix.
- Tests performed:
  - Milestone 1 passed local playtesting.
  - Milestone 2 inventory systems passed local playtesting.
  - Dropped items were reported floating instead of falling.
  - Physics hotfix testing pending.
