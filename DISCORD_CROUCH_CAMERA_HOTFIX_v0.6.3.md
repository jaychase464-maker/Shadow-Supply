# 🛠️ SHADOW SUPPLY DEVELOPMENT HOTFIX

## v0.6.3 — Crouch Recovery and First-Person Camera Fix

The new rigged player body exposed two crouching issues during testing.

### 🐛 Previous Behavior

- The player could crouch but could not return to standing
- The standing-clearance test could detect the player's own collision
- The crouching camera entered the torso
- Looking around while crouched exposed the inside of the character mesh

### ✅ Crouch Recovery Fixed

- Standing clearance now checks only the added upper-body space
- The player's own CharacterController is ignored
- Character child objects are ignored
- Ground-level contact is ignored
- Real ceilings and low obstacles still prevent standing

### ✅ First-Person Body Alignment Fixed

- The visible body now sits slightly behind the camera
- Crouching moves the local body farther away from the camera
- Looking sharply downward adds extra camera clearance
- Transitions remain smooth
- The shadow-only body stays centered for accurate shadows

### 📌 Current Version

`v0.6.3-crouch-camera-hotfix`
