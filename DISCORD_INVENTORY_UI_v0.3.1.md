# 🎒 SHADOW SUPPLY DEVELOPMENT UPDATE

## v0.3.1 — Full Inventory UI Redesign

The temporary development inventory has been replaced with the full industrial interface designed for Shadow Supply.

The new screen is now a real, interactive in-game UI rather than a simple debug menu.

### ✅ Added

- Full-screen industrial inventory interface
- Dark workshop-style presentation
- Amber selected-item highlights
- Teal status accents
- Twenty-four inventory slots
- Eight-slot hotbar inside the inventory
- Compact hotbar during normal gameplay
- Item artwork for development items
- Item details panel
- Equipment paper-doll display
- Cash display
- Dirty-cash display
- Heat display
- Weight display
- Quality and condition information
- Item values and descriptions
- Large item inspection screen
- Functional Use, Drop, Split, and Inspect buttons
- Clickable inventory slots
- Keyboard and mouse-wheel hotbar controls

### 🎮 Controls

- `TAB` or `I` — Open or close inventory
- `Click` — Select an item
- `E` — Use or equip
- `Q` — Drop one
- `R` — Split stack
- `1–8` — Select hotbar slot
- `Mouse Wheel` — Cycle hotbar
- `Escape` — Close inspection or inventory

### ⚙️ Gameplay Behavior

Items outside the hotbar can now be moved into the selected hotbar slot using the Use action.

Stacks can be divided into empty inventory slots, and items can be inspected in a larger dedicated view.

The compact hotbar remains visible while walking around, while the full inventory stops movement and unlocks the cursor.

### 💾 Save Compatibility

The inventory redesign does not change save schema version `1`.

Existing item IDs, quantities, quality, condition, hotbar state, and save data remain compatible.

### 📌 Current Version

`v0.3.1-inventory-ui`

## 🔜 Coming Next

After the save system and redesigned inventory pass final testing, development will move into persistent furniture placement:

- Placeable-object definitions
- Placement previews
- Object rotation
- Collision validation
- Floor placement
- Persistent object IDs
- Save and load integration

Shadow Supply now has its first production-style gameplay interface. 🖤
