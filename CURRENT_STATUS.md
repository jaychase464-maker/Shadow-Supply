# Shadow Supply — Current Status

## Current version

- Current clean rewrite: `v0.5.0-furniture-ownership`
- Repository: `jaychase464-maker/Shadow-Supply`
- Source-of-truth branch: `main`
- Save schema version: `3`

## Unity version

- Unity Editor: `6000.0.58f2`
- Revision: `92dee566b325`

## Package versions

- Universal Render Pipeline: `17.0.4`
- Input System: `1.14.2`
- AI Navigation: `2.0.9`
- uGUI: `2.0.0`

## Completed milestones

- Milestone 1 — Playable Foundation
- Milestone 2 — Items and Inventory
- Milestone 3 — Save and Load Foundation
- Inventory UI production pass
- Milestone 4 — Persistent Furniture Placement

## Current milestone

### Milestone 5 — Furniture Ownership and Acquisition

- Clean-cash wallet
- Furniture prices
- Supplier shop interface
- Physical furniture delivery crates
- Persistent uncollected deliveries
- Furniture inventory packages
- Placement consumes ownership
- Packing returns ownership
- Wall-placement foundation
- CCTV and keypad wall placeables
- Imported workbench, rack, desk, camera, and keypad models
- Save schema version `3`

## Current compilation state

- Awaiting local Unity import, compilation, setup, and acceptance testing

## Current known risks

- Imported model orientation and scale require local visual confirmation.
- The utility cabinet still uses generated placeholder geometry.
- Shop and build HUDs are development interfaces.
- Deliveries currently arrive immediately rather than using travel time.
- Furniture ordering is not yet connected to ShadowOS.
- Final source-asset licenses must be verified before release.

## Next planned work

1. Validate model scale, orientation, materials, and colliders.
2. Validate ownership consumption and returns.
3. Validate wallet and delivery persistence.
4. Push Milestone 5 after acceptance testing.
5. Build the starter garage vertical slice.
6. Begin physical electrical placement using the wall-placement foundation.
