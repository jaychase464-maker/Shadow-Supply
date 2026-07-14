# Shadow Supply — Current Status

Update this file after each meaningful development session.

## Current version

- Current clean rewrite: `v0.3.1-inventory-ui`
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
- Local acceptance testing pending

## Current UI pass

### v0.3.1 — Inventory UI Redesign

- UI Toolkit runtime inventory
- Industrial UXML and USS presentation
- Twenty-four inventory slots
- Eight-slot in-menu hotbar
- Compact gameplay hotbar
- Item detail card
- Equipment display
- Use, Drop, Split, and Inspect actions
- Item icon resource support
- Save schema unchanged

## Current compilation state

- Milestone 1 and Milestone 2 reported working
- Milestone 3 package revision 2 resolved the reported `CS0104`
- Inventory UI redesign awaiting local compilation and playtesting

## Current known risks

- Cash, dirty cash, and heat are temporary presentation values.
- Weight currently uses item quantity rather than true item mass.
- Save schema migrations have not yet been needed.
- Runtime fallback meshes remain placeholders.
- Production item prefabs will need correctly sized colliders.
- The repository only reflects committed and pushed local changes.

## Next planned work

1. Validate Milestone 3 save and load.
2. Validate the redesigned inventory interface.
3. Commit and push the combined update.
4. Begin persistent furniture placement.
5. Integrate placed objects into save schema version `1` without changing existing fields.
6. Build floor placement before wall placement and electrical objects.

## Session log

### 2026-07-14

- Changes:
  - Added production-style inventory UI.
  - Added compact gameplay hotbar.
  - Added item detail and inspection panels.
  - Added stack splitting and inventory-to-hotbar swapping.
  - Preserved save schema version `1`.
- Tests performed:
  - Local Unity compilation pending.
  - Inventory UI acceptance testing pending.
