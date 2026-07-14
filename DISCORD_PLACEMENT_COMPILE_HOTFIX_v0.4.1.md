# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.4.1 — Placement Database Compile Fix

A compile error found during installation of Milestone 4 has been corrected.

### 🐛 Issue

The placeable database did not always assign its output value, causing Unity to report `CS0177`.

### ✅ Fixed

- Placeable lookups now initialize their output safely
- Empty placeable IDs are rejected explicitly
- Valid placeable database lookups continue normally
- No save data, scene data, or furniture definitions were changed

### 📌 Current Version

`v0.4.1-placement-compile-hotfix`
