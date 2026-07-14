# Shadow Supply — Bug Tracker

Use one entry per bug.

## Active bugs

No active clean-rewrite bugs are currently recorded.

## Historical reports

### BUG-001 — Wall outlets face the wrong direction

- Status: Historical legacy-project report
- Area: Electrical placement
- The active clean rewrite has not implemented this system yet.

### BUG-002 — Electrical connectors do not insert correctly

- Status: Historical legacy-project report
- Area: Physical plug system
- The active clean rewrite has not implemented this system yet.

### BUG-003 — Entrance door blocked by electrical panels

- Status: Historical legacy-project report
- Area: Starter property layout
- The active clean rewrite has not implemented this scene yet.

## Resolved or fix committed

### BUG-004 — Dropped inventory items float instead of falling

- Status: Fix committed; regression test should remain in inventory acceptance tests
- Reported version: `v0.2.0-inventory`
- Fixed version: `v0.2.1-inventory-hotfix`
- Commit: `d7d8463a6ed16a61bb36df8d54d03ccc0b91fbb5`
- Affected script:
  - `Assets/_Project/Scripts/Runtime/Inventory/WorldItemPickup.cs`
- Fix:
  - Explicit gravity and dynamic Rigidbody state
  - Cleared constraints
  - Continuous collision detection
  - Solid fallback collider
  - Initial force and randomized torque

### BUG-005 — Ambiguous `Random` reference prevents Milestone 3 compilation

- Status: Fixed in Milestone 3 package revision 2
- Severity: High
- Reported version: `v0.3.0-save-foundation` package revision 1
- Affected script:
  - `Assets/_Project/Scripts/Runtime/Inventory/WorldItemPickup.cs`
- Compiler error:
  - `CS0104: 'Random' is an ambiguous reference between 'UnityEngine.Random' and 'System.Random'`
- Fix:
  - Replaced `Random.rotation` with `UnityEngine.Random.rotation`.
