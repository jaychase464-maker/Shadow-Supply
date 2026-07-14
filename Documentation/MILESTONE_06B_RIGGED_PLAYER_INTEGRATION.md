# Milestone 6B — Rigged Player Integration

## Rig inspection

The supplied `PlayerCharacter_Rigged.fbx` was inspected before packaging.

Confirmed in the FBX:

- Kaydara binary FBX version 7700
- Approximately 58.8 MiB
- Mixamo humanoid skeleton
- 66 named `mixamorig` bones
- Spine, neck, head, arms, hands, fingers, legs, feet, and toes
- Skin/deformer data
- Approximately 70 skin clusters
- Modular body, hair, beard, underwear, eyes, teeth, tongue, and eyelash objects
- One embedded animation stack from the rigging export

The embedded animation stack is intentionally not imported. This milestone uses
the Humanoid Avatar and a lightweight procedural locomotion placeholder.

## Added

- Rigged FBX import
- Automatic Unity Humanoid importer configuration
- Valid Avatar requirement
- Rigged gameplay-character prefab generation
- Full-body first-person visual
- Local head-bone hiding
- Local eye, hair, beard, teeth, tongue, and eyelash hiding
- Complete shadows-only body duplicate
- Full character shadow, including the head
- Procedural idle stance
- Procedural walking
- Procedural sprinting
- Procedural crouching
- Procedural jumping/air pose
- Camera-pitch torso follow
- Bone-bound equipment sockets
- Modular character-part markers
- Backpack-ready back socket
- Appearance save-data foundation
- Save-schema migration from version 3 to version 4
- Rigged-player validation menu

## Important animation note

The procedural movement in this milestone is a functional placeholder. It proves
that the rig deforms, that the first-person body follows the controller, and that
the shadow body remains synchronized.

Production-quality animation clips should replace the procedural locomotion in a
later animation pass.

## Installation

Prerequisite:

- Milestone 6 character foundation and the v0.6.1 prefab-unpack hotfix must
  already be installed.
- The character preview should already display with correct materials.

Steps:

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity.
5. Allow `PlayerCharacter_Rigged.fbx` to import.
6. Open `Dev_Playground`.
7. Run:

   `Shadow Supply → Setup → Apply Milestone 6B Rigged Player Integration`

8. Run:

   `Shadow Supply → Validation → Validate Rigged Player Integration`

Expected validation:

```text
RIGGED PLAYER READY
```

## Scene hierarchy

The setup creates:

```text
Player
└── PlayerCharacterVisuals
    ├── LocalBody
    └── ShadowBody
```

### LocalBody

- Visible to the player's camera
- Head bone reduced locally to prevent camera clipping
- Face, hair, beard, eye, teeth, tongue, and eyelash renderers hidden
- Does not cast shadows, preventing duplicate shadows

### ShadowBody

- Not directly visible
- All renderers use `Shadows Only`
- Preserves the complete character silhouette and head shadow
- Uses the same procedural motion as the local body

Neither body contains active colliders. Player collision continues to use the
existing `CharacterController`.

## Controls affected

No control bindings were changed.

The first-person controller now exposes read-only locomotion values used by the
character system:

- Movement input
- Horizontal speed
- Normalized speed
- Sprint state
- Crouch state
- Grounded state
- Vertical velocity
- Camera pitch

## Save schema 4

Character appearance data now includes stable IDs for:

- Base body
- Hair
- Facial hair
- Underwear
- Torso clothing
- Leg clothing
- Footwear
- Gloves
- Headwear
- Backpack
- Chest accessory
- Hip accessory

Older schema-3 saves automatically receive the default current appearance.

## Acceptance test

### Rig and materials

- Validator reports `RIGGED PLAYER READY`
- No pink materials
- Body parts remain attached
- Limbs bend rather than moving as rigid pieces
- Hair, beard, and briefs follow the skeleton

### First-person body

- Looking down shows the torso and legs
- No face, eyeball, hair, beard, teeth, or tongue clipping appears
- Camera remains independent from the head animation
- Character does not block player movement
- Character does not create extra collision

### Locomotion

- Arms rest closer to the sides instead of remaining in a T-pose
- Arms and legs move while walking
- Sprinting increases movement cadence
- Crouching bends and lowers the body
- Jumping produces an airborne pose
- Body remains aligned with player yaw

### Shadows

- Ground and wall shadows include the full body and head
- No second duplicate shadow appears
- Shadow body remains aligned with the visible body

### Existing systems

- Interaction still works
- Inventory still works
- Furniture placement still works
- Save and load still work
- Existing schema-3 saves load successfully
- New saves write schema 4
- No red Console errors appear

## Current version

`v0.6.2-rigged-player-integration`
