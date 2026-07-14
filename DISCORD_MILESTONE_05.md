# 📦 SHADOW SUPPLY DEVELOPMENT UPDATE

## Milestone 5 — Furniture Ownership and Acquisition

Furniture placement is now connected to the actual game economy and inventory.

Players can purchase furniture, receive it through physical deliveries, collect packaged items, place only what they own, and pack furniture back into inventory.

### ✅ Added

- Clean-cash wallet foundation
- Furniture purchase prices
- Furniture supplier terminal
- Interactive furniture shop
- Physical delivery marker
- Physical furniture delivery crates
- Persistent uncollected deliveries
- Furniture inventory packages
- Owned furniture quantities in build mode
- Placement consumes one owned package
- Packing returns one package
- Inventory-full protection
- Wall-placement foundation
- Save schema version `3`

### 🪑 Furniture Catalog

- Workbench
- Storage Rack
- Office Desk
- Utility Cabinet
- CCTV Camera
- Door Keypad

### 🎨 Imported 3D Models

This milestone begins using the actual models supplied for Shadow Supply:

- Workbench
- Metal pallet rack
- Office desk
- CCTV camera
- Door keypad

The utility cabinet remains a temporary development model pending a dedicated optimization pass.

### 🎮 Gameplay Loop

1. Use the furniture supplier terminal.
2. Pay with clean cash.
3. Collect the physical delivery crate.
4. Enter build mode.
5. Place the owned furniture.
6. Pack it later to return it to inventory.

### 🎮 Controls

- `E` — Use supplier terminal or collect a delivery
- `B` — Enter or exit build mode
- `1–6` — Select furniture
- `R` — Rotate
- `Shift + R` — Rotate backward
- `Mouse Wheel` — Rotate
- `Left Click` — Place
- `Delete` or `Backspace` — Pack targeted furniture
- `Right Click` — Exit build mode

### 🧱 Wall Placement

The placement system now supports wall-mounted objects.

The first wall objects are:

- CCTV Camera
- Door Keypad

This creates the foundation for future outlets, breaker panels, lights, monitors, security devices, and electrical equipment.

### 💾 Save-System Upgrade

Save schema version `3` now stores:

- Clean cash
- Dirty cash
- Uncollected furniture deliveries
- Delivery item IDs
- Delivery positions and rotations

Older version `1` and `2` saves remain supported.

### 📌 Current Version

`v0.5.0-furniture-ownership`

## 🔜 Next Milestone

### Starter Garage Vertical Slice

The next major build will begin turning the development playground into the first complete Shadow Supply property, including:

- Starter garage layout
- Real work areas
- Storage
- Furniture supplier access
- Wall-mounted utilities
- First electrical components
- First production-workflow foundation

Shadow Supply now requires players to actually acquire and manage the equipment used to build their operation. 🖤
