# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.5.5 — Furniture Collision Rebuild

The furniture models now look correct, but their physical collision was still
using one solid rectangular box.

### 🐛 Previous Behavior

- Players could not get close to desks and workbenches
- Empty space underneath furniture behaved like a solid wall
- Visually enlarged models extended outside their colliders
- Players could clip through portions of the furniture sides

### ✅ Fixed

#### Workbench

- Added tabletop collision
- Added four separate leg colliders
- Open space beneath the table is now accessible

#### Office Desk

- Added desktop collision
- Added left drawer-pedestal collision
- Added right cabinet-pedestal collision
- Center knee space is now open

#### Placement

- Placement validation now uses the combined physical collider bounds
- Visible furniture and gameplay collision now match
- Wall-mounted objects remain unchanged
- Storage rack and cabinet retain full-volume collision

### 📌 Current Version

`v0.5.5-furniture-collision-hotfix`
