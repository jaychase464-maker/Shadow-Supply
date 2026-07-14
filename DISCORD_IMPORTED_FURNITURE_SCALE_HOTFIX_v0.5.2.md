# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.5.2 — Imported Furniture Scale Fix

The first test of the imported furniture models exposed extreme scale differences.

### 🐛 Issue

- Workbench and office desk could appear almost microscopic
- CCTV camera could appear enormous
- Placement previews did not match realistic object sizes

### 🔍 Root Cause

The prefab builder replaced the scale already supplied by Unity's FBX importer.

Different models use different unit conversions, so discarding the imported scale
caused some assets to shrink and others to grow dramatically.

### ✅ Fixed

- Preserves the original FBX importer scale
- Applies a uniform correction multiplier
- Prevents model stretching
- Fits models inside their intended real-world dimensions
- Grounds floor furniture correctly
- Aligns wall objects to their mounting surface
- Rebuilds the generated furniture prefabs with corrected scale

### 📌 Current Version

`v0.5.2-imported-furniture-scale-hotfix`
