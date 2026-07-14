# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.6.4 — Right Ankle Alignment

The new rigged player character had one remaining deformation issue: the right
ankle and foot could point sideways.

### 🐛 Previous Behavior

- Right ankle appeared twisted
- Right foot pointed in a different direction from the left foot
- The issue remained during idle and procedural movement
- The character shadow showed the same twisted-foot pose

### ✅ Fixed

- Added automatic humanoid foot-direction calibration
- Uses the left foot as the alignment reference
- Corrects only the right ankle's sideways rotation
- Ignores small natural differences between the feet
- Runs after procedural locomotion
- Does not accumulate rotation over time
- Applies to both the visible body and full shadow body

### 📌 Current Version

`v0.6.4-right-ankle-alignment`
