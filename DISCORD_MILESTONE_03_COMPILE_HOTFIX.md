# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## Milestone 3 — Package Revision 2

A compile issue found during the first Milestone 3 installation has been corrected.

### 🐛 Issue

Unity reported an ambiguous `Random` reference inside the world-item persistence script because both Unity and .NET provide a class named `Random`.

### ✅ Fixed

- Corrected the conflicting random-rotation call
- Preserved dropped-item tumbling physics
- Preserved persistent world-item IDs
- Preserved save schema version `1`
- No existing inventory or save data fields were changed

### 📌 Current Version

`v0.3.0-save-foundation` — Package Revision 2

Milestone 3 testing can now continue with player, inventory, hotbar, and world-item persistence. 💾
