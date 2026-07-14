# 🛠️ SHADOW SUPPLY HOTFIX

## v0.2.1 — Dropped Item Physics

A physics issue discovered during Milestone 2 testing has been addressed.

### 🐛 Issue

Items dropped from the hotbar spawned in front of the player but remained suspended in the air instead of falling naturally.

### ✅ Fixed

- Dropped items now use gravity correctly
- Items are forced into a dynamic physics state
- Rigidbody constraints are cleared during the drop
- Collision detection has been improved
- Dropped objects now receive randomized rotation
- Items tumble while falling
- Missing solid colliders receive a fallback collider
- Physics objects are immediately awakened after spawning

### 🎮 Result

Pressing `Q` now throws the selected item into the world. It should fall, rotate, collide with the environment, settle on the ground, and remain available to pick back up.

### 📌 Current Version

`v0.2.1-inventory-hotfix`

## 🔜 Next

Once this hotfix passes testing, development will continue with:

### Milestone 3 — Save and Load Foundation

- Versioned save files
- Multiple save slots
- Player position persistence
- Inventory persistence
- Stable item-ID lookup
- Hotbar selection persistence
- Persistent world-item foundation
