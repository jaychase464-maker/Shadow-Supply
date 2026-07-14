# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.3.2 — Inventory Style Rendering Fix

The first in-engine test of the redesigned inventory exposed a major UI rendering problem.

### 🐛 Issue

The inventory structure loaded, but the industrial stylesheet failed to apply.

This caused:

- Default gray UI Toolkit buttons
- Hotbar slots stretched vertically
- Inventory slots displayed as one large column
- Tiny unstyled text
- Missing panels, colors, spacing, and highlights

### ✅ Fixed

- Corrected an invalid grouped USS selector
- Attached the stylesheet directly to the runtime inventory template
- Forced the UI document and template to fill the screen correctly
- Restored the intended horizontal hotbar
- Restored the six-column inventory grid
- Restored dark industrial panels
- Restored amber and teal accents
- Preserved inventory behavior and save compatibility
- Preserved the previous cursor compile fix

### 📌 Current Version

`v0.3.2-inventory-style-hotfix`

The redesigned inventory can now continue full in-engine testing. 🎒
