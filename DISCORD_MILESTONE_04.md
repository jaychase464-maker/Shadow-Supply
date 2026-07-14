# 🏗️ SHADOW SUPPLY DEVELOPMENT UPDATE

## Milestone 4 — Persistent Furniture Placement

Shadow Supply now has its first persistent property-customization system.

Players can enter build mode, preview furniture in the world, rotate it, validate available space, place it, remove it, and preserve the layout through save and load.

### ✅ Added

- Furniture placement mode
- Camera-centered placement preview
- Floor placement
- Grid snapping
- Green valid-placement preview
- Red blocked-placement preview
- Collision detection
- Slope validation
- Furniture rotation
- Mouse-wheel rotation
- Multiple furniture selections
- Persistent furniture IDs
- Removing targeted furniture
- Save and load integration
- Development placement HUD

### 🪑 Development Furniture

- Workbench
- Storage Shelf
- Utility Cabinet

These are temporary development models used to validate the complete placement system before final furniture assets are introduced.

### 🎮 Controls

- `B` — Enter or exit build mode
- `1–3` — Select furniture
- `R` — Rotate clockwise
- `Shift + R` — Rotate counterclockwise
- `Mouse Wheel` — Rotate
- `Left Click` — Place
- `Delete` or `Backspace` — Remove targeted furniture
- `Right Click` — Exit build mode

### 💾 Save-System Upgrade

Save schema has advanced from version `1` to version `2`.

Version `2` stores:

- Furniture definition ID
- Persistent object ID
- World position
- World rotation

Existing version `1` saves remain supported and migrate automatically without losing player, inventory, hotbar, or world-item data.

### 📌 Current Version

`v0.4.0-placement-foundation`

## 🔜 Next Update

### Milestone 5 — Furniture Ownership and Acquisition

Planned features include:

- Furniture inventory items
- Buying furniture
- Delivery integration
- Consuming furniture when placed
- Returning furniture when picked up
- Wall-placement foundations

Shadow Supply properties can now begin becoming functional player-built workspaces. 🖤
