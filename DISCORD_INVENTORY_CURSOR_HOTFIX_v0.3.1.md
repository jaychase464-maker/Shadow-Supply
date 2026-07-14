# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.3.1 — Inventory Cursor Compile Fix

A compile issue found during installation of the redesigned inventory interface has been corrected.

### 🐛 Issue

Unity reported an ambiguous `Cursor` reference because both the Unity runtime and UI Toolkit contain a type named `Cursor`.

### ✅ Fixed

- Runtime cursor references now explicitly use `UnityEngine.Cursor`
- Inventory cursor unlocking remains functional
- Inventory cursor visibility remains functional
- Closing the inventory restores cursor lock
- No save data or inventory behavior was changed
- No scene regeneration is required

### 📌 Current Version

`v0.3.1-inventory-ui` — Cursor Hotfix

Testing of the full industrial inventory interface can now continue. 🎒
