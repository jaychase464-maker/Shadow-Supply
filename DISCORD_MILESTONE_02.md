# 📦 SHADOW SUPPLY DEVELOPMENT UPDATE

## Milestone 2 — Items and Inventory

The first real gameplay system has been added to the new Shadow Supply project.

Players can now collect physical items from the world, store them in a stackable inventory, select items through a hotbar, hold them in first person, and drop them back into the environment.

### ✅ Added

- Scriptable item definitions with stable IDs
- Item categories
- Item quality tiers
- Item condition values
- Twenty-four-slot inventory
- Eight-slot hotbar
- Stackable item quantities
- Physical world pickups
- Partial pickups when inventory space is limited
- Number-key hotbar selection
- Mouse-wheel hotbar cycling
- First-person held-item visuals
- Dropping selected items
- Picking dropped items back up
- Inventory interface foundation
- Hotbar interface
- Four development test items

### 🧰 Test Items

- Metal Components
- Polymer Housing
- Basic Toolkit
- Sealed Package

### 🎮 New Controls

- `E` — Pick up an item
- `1–8` — Select a hotbar slot
- `Mouse Wheel` — Cycle the hotbar
- `TAB` or `I` — Open or close inventory
- `Q` — Drop one selected item

### ⚙️ Technical Progress

Items now receive stable IDs instead of relying only on Unity asset references. This prepares the inventory for reliable save files, persistent world objects, crafting recipes, suppliers, buyers, storage, and deliveries.

### 📌 Current Version

`v0.2.0-inventory`

## 🔜 Next Update

### Milestone 3 — Save and Load Foundation

Planned features include:

- Versioned save files
- Multiple save slots
- Player position persistence
- Inventory persistence
- Item lookup by stable ID
- Hotbar selection persistence
- World item persistence foundation

Shadow Supply now has its first complete collect, store, hold, and drop gameplay loop. 🖤
