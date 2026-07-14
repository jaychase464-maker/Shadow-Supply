# Shadow Supply — Current Status

Update this file after each meaningful development session.

## Current version

- Current clean rewrite: `v0.2.0-inventory`
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

- Stable item definitions
- Item categories, quality, and condition
- Stackable inventory
- Eight-slot hotbar
- World pickups
- Held-item display
- Dropping items
- Development inventory UI

## Current compilation state

- Awaiting local compilation and Milestone 2 acceptance testing

## Current known risks

- Save data has not been implemented yet.
- The inventory UI is a temporary development interface.
- Runtime fallback meshes are placeholders until final item models are assigned.
- The repository only reflects committed and pushed local changes.

## Next planned work

1. Validate Milestone 2 locally.
2. Commit and push generated assets, scene changes, and `.meta` files.
3. Implement a versioned save architecture.
4. Serialize inventory using stable item IDs.
5. Add save-slot metadata and player transform persistence.
6. Build persistent world-item foundations before placement and electrical systems.

## Session log

### 2026-07-13

- Changes:
  - Verified Milestone 1 locally.
  - Added Milestone 2 inventory implementation.
- Tests performed:
  - Milestone 1 passed local playtesting.
  - Milestone 2 testing pending.
