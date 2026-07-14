# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.3.3 — Inventory UI Runtime Rebuild

The inventory was still rendering as raw default UI Toolkit controls after the previous style hotfix.

The styling system has now been rebuilt using a more reliable approach.

### 🐛 Previous Behavior

- Hotbar buttons stretched across the entire screen
- Slots stacked vertically
- Default gray Unity buttons remained visible
- Inventory panels and colors did not render
- The full inventory layout was unusable

### ✅ Rebuilt

- Removed the inventory's dependency on USS for its core presentation
- Added runtime inline styling for the entire interface
- Restored the horizontal eight-slot hotbar
- Restored the six-column inventory grid
- Restored the dark industrial frame and panels
- Restored amber selected-item highlights
- Restored teal status accents
- Restored equipment and item-details panels
- Restored properly sized action buttons
- Item artwork now uses scale-to-fit UI image elements
- Preserved save compatibility and inventory behavior

### 📌 Current Version

`v0.3.3-inventory-inline-style`

This update replaces the failed stylesheet-only approach with a self-contained runtime UI presentation. 🎒
