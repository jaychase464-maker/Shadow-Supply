# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.5.3 — Floor Furniture Calibration

The CCTV camera and door keypad are now correctly sized, but the workbench and
office desk were still much too small.

### ✅ Corrected

- Workbench visible model increased to a usable full-size scale
- Office desk visible model increased to normal furniture scale
- Storage rack scale preserved
- Utility cabinet scale preserved
- CCTV camera scale preserved
- Door keypad scale preserved
- Floor alignment remains active
- Placement colliders retain their intended real-world dimensions

### 🔧 New Asset Calibration Support

The furniture importer now supports a separate visual calibration value for each
3D model.

This allows unrelated FBX assets with unusual source bounds or unit settings to
be corrected individually without breaking models that already look right.

### 📌 Current Version

`v0.5.3-floor-furniture-calibration`
