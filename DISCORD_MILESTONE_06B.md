# 🧍 SHADOW SUPPLY DEVELOPMENT UPDATE

## Milestone 6B — Rigged Player Integration

The permanent Shadow Supply player character now has a confirmed humanoid rig
and has been connected to the first-person controller.

### ✅ Rig Confirmed

The new FBX contains:

- A complete Mixamo humanoid skeleton
- 66 named humanoid bones
- Full finger chains
- Spine, neck, and head bones
- Arms, legs, feet, and toe bones
- Skin weights and deformation clusters
- Modular body, hair, beard, and underwear pieces

### 🎮 First-Person Body

- Full body is visible when looking down
- The camera remains independent from the animated head
- The local head is hidden to prevent face clipping
- Eyes, hair, beard, teeth, tongue, and eyelashes are hidden locally
- Player collision still uses the original CharacterController
- Character meshes do not create extra collision

### 🌑 Full Character Shadows

A synchronized shadows-only character preserves:

- Head shadow
- Hair and beard shadow
- Torso shadow
- Arm and hand shadows
- Leg and foot shadows

The visible first-person body does not create a duplicate shadow.

### 🚶 Locomotion Foundation

A procedural placeholder currently supports:

- Idle stance
- Walking
- Sprinting
- Crouching
- Jumping
- Camera-pitch torso follow

Production animation clips will replace this placeholder during the dedicated
animation pass.

### 🎒 Customization Foundation

Bone-bound sockets now support future:

- Backpacks
- Chest equipment
- Headwear
- Handheld equipment
- Hip and belt accessories

### 💾 Save System

Save schema `4` now preserves stable appearance IDs for:

- Body
- Hair
- Facial hair
- Underwear
- Shirts and jackets
- Pants
- Shoes
- Gloves
- Headwear
- Backpacks
- Chest accessories
- Hip accessories

Older saves automatically migrate to the default current appearance.

### 📌 Current Version

`v0.6.2-rigged-player-integration`
