# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.3.4 — Inventory Clicking Fixed

The redesigned inventory was rendering correctly, but item slots could not be selected with the mouse.

### 🐛 Root Cause

The full inventory root was configured to ignore UI Toolkit pointer picking.

This prevented slot buttons from reliably receiving mouse input.

### ✅ Fixed

- Enabled pointer interaction on the full inventory hierarchy
- Enabled pointer interaction on inventory slots
- Enabled pointer interaction on hotbar slots
- Enabled pointer interaction on action buttons
- Enabled pointer interaction on the inspection screen
- Added direct left-click slot selection
- Prevented slot clicks from propagating into other UI layers
- Brings the inventory to the front when opened
- Focuses the inventory panel when opened
- Preserved inventory behavior and save compatibility

### 🎮 Result

Clicking an occupied inventory slot now:

- Moves the amber selection highlight
- Updates the item-details panel
- Enables the correct Use, Drop, Split, and Inspect actions

### 📌 Current Version

`v0.3.4-inventory-interaction-hotfix`
