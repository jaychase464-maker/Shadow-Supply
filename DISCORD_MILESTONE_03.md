# 💾 SHADOW SUPPLY DEVELOPMENT UPDATE

## Milestone 3 — Save and Load Foundation

Shadow Supply now has its first persistent save system.

Players can use multiple save slots and restore their character, inventory, hotbar, and physical world items after changing the game state.

### ✅ Added

- Versioned save-file architecture
- Three independent save slots
- Player-position persistence
- Player view-direction persistence
- Exact inventory-slot persistence
- Item quality and condition persistence
- Stable item-ID database
- Hotbar-selection persistence
- Physical world-item persistence
- Dropped-item position and rotation persistence
- Dropped-item velocity and spin persistence
- Save timestamps and slot metadata
- Atomic save writing
- Automatic backup files
- Scene-aware loading foundation
- Development save HUD

### 🎮 New Controls

- `F1` — Select save slot 1
- `F2` — Select save slot 2
- `F3` — Select save slot 3
- `F5` — Save
- `F9` — Load

### ⚙️ Technical Progress

Inventory items are now restored using their permanent item IDs rather than temporary Unity object references.

World pickups are also assigned persistent IDs, giving future systems a foundation for:

- Furniture placement
- Storage containers
- Workstations
- Electrical connections
- Property upgrades
- Vehicles
- Buyer and supplier progression
- Complete world-state restoration

Save files currently use schema version `1`. Future save changes will require explicit migrations so player progress is not silently broken.

### 📌 Current Version

`v0.3.0-save-foundation`

## 🔜 Next Update

### Milestone 4 — Furniture Placement Foundation

Planned features include:

- Placeable-object definitions
- Placement preview
- Object rotation
- Collision validation
- Floor placement
- Persistent placed-object IDs
- Save and load integration

Shadow Supply can now preserve the first complete player and world gameplay state. 🖤
