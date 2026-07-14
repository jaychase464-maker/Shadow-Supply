# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.5.1 — Placement Setup Compatibility Fix

A compile error found while installing Milestone 5 has been corrected.

### 🐛 Issue

Milestone 5 added inventory ownership to the placement controller, but the older
Milestone 4 setup script still called the original configuration method.

This caused Unity to report `CS7036`.

### ✅ Fixed

- Restored compatibility with the Milestone 4 setup script
- Added a two-argument placement configuration overload
- Player inventory is now resolved automatically when needed
- Milestone 5 can still pass the inventory reference directly
- No save data was changed
- No furniture data was changed
- No model or scene data was changed

### 📌 Current Version

`v0.5.1-placement-configure-compatibility`

Milestone 5 installation and testing can now continue.
