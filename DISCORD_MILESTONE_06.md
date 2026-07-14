# 🧍 SHADOW SUPPLY DEVELOPMENT UPDATE

## Milestone 6 — Modular Player Character Foundation

Development has started on Shadow Supply's permanent customizable player
character.

The selected character model has now been imported and prepared as the base for
future clothing, backpacks, equipment, mirrors, CCTV views, and third-person
presentation.

### ✅ Added

- Modular base-body structure
- Hair slot
- Facial-hair slot
- Underwear slot
- Torso-clothing slot
- Leg-clothing slot
- Footwear slot
- Gloves slot
- Headwear slot
- Backpack slot
- Chest-accessory slot
- Hip-accessory slot
- Stable character-part IDs
- Character-part database
- Equipment socket registry
- First-person hidden-layer foundation
- Character source-preview prefab
- In-game preview station
- Rig-readiness validation tool

### 🎨 Character Assets Prepared

- Body
- Eyes
- Teeth
- Tongue
- Eyelashes
- Buzz-cut hair
- Chin-curtain beard
- Briefs
- URP-compatible materials
- Optimized character texture set

### ⚠️ Rigging Status

The selected source model is modular, but the supplied FBX does not include a
skeleton, skin weights, or a valid humanoid avatar.

The model has intentionally not been attached to the moving player yet. Attaching
an unrigged model would create an unanimated character that slides through the
world.

### 🔜 Next Character Step

A rigged T-pose version of the same model will unlock:

- Full-body first-person character
- Idle, walking, sprinting, crouching, and jumping
- Visible body when looking down
- Hidden local head to prevent camera clipping
- Full-body shadows
- Modular skinned clothing
- Backpacks attached to the spine
- Hand, hip, chest, and head equipment
- Saved character appearance

### 📌 Current Version

`v0.6.0-character-asset-foundation`
