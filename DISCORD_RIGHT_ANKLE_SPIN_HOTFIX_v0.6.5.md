# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.6.5 — Right Ankle Spin Fix

The first ankle-alignment patch overcorrected the right foot and caused the ankle
to rotate continuously.

### 🐛 Previous Behavior

- The same correction rotation was applied every frame
- Rotation accumulated instead of settling
- The right ankle spun like a propeller
- The shadow body inherited the same spinning foot

### ✅ Fixed

- Removed the accumulating stored rotation
- Correction now measures the current foot directions every frame
- Only the remaining alignment difference is applied
- Correction stops once the feet are aligned
- Maximum correction is safely clamped
- Local and shadow bodies remain synchronized

### 📌 Current Version

`v0.6.5-right-ankle-spin-hotfix`
