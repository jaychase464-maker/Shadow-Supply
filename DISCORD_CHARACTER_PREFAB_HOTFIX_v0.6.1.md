# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.6.1 — Character Preview Prefab Fix

The Milestone 6 setup tool was attempting to reorganize meshes while the imported
FBX was still connected as a Unity model-prefab instance.

Unity blocks child reparenting inside a connected prefab instance, causing
warnings for the body, eyes, teeth, tongue, eyelashes, hair, beard, and briefs.

### ✅ Fixed

- Imported FBX preview is now unpacked before modular reorganization
- Body parts can now move into the correct character groups
- Original imported FBX remains untouched
- Character preview prefab is rebuilt correctly
- Preview station is rebuilt automatically
- Character materials and part IDs remain unchanged

### ℹ️ Expected Validation Result

The character validator will still report:

`RIGGING REQUIRED`

That result is correct because the source FBX does not contain a skeleton or skin
weights.

### 📌 Current Version

`v0.6.1-character-prefab-unpack`
