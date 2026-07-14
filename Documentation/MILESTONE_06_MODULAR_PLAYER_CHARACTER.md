# Milestone 6 — Modular Player Character Asset Foundation

## Purpose

This milestone prepares the selected male character model for Shadow Supply's
future clothing, backpack, equipment, mirror, CCTV, and third-person systems.

The supplied FBX is a strong modular source asset, but it is not yet a gameplay-
ready animated character.

## Source-model inspection

The supplied FBX contains:

- 8 separate mesh objects
- 17 materials
- Body
- Eyes
- Teeth
- Tongue
- Eyelashes
- Buzz-cut hair
- Chin-curtain facial hair
- Briefs

The FBX does not contain:

- Skeleton nodes
- Skin deformers
- Skin weights
- Skinned mesh renderers
- A valid humanoid avatar

Because of that, this package deliberately does not attach the source model to
the moving first-person player. Doing so would produce an unanimated statue that
slides through the world.

## Included foundation

- Imported source FBX
- Curated 2K-or-smaller character textures
- Combined RGBA hair, scalp, beard, and eyelash textures
- URP material generation
- Modular character slots
- Stable character-part IDs
- Character-part database
- Base-body group
- Hair group
- Facial-hair group
- Underwear group
- Equipment socket registry
- Head socket
- Back socket
- Chest socket
- Left-hand socket
- Right-hand socket
- Hips socket
- Left-hip socket
- Right-hip socket
- First-person hidden-layer foundation
- Character rig-status component
- Character source-preview prefab
- In-scene preview station
- Rig validation menu

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Reopen Unity.
4. Wait for the 60 MB FBX and character textures to import.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 6 Character Foundation`

7. Inspect the character at the generated `CharacterPreviewStation`.
8. Run:

   `Shadow Supply → Validation → Validate Player Character Rig`

The validator should report `RIGGING REQUIRED`. That is expected for this source
FBX.

## Modular slots prepared

- Base Body
- Hair
- Facial Hair
- Underwear
- Torso Clothing
- Leg Clothing
- Footwear
- Gloves
- Headwear
- Backpack
- Chest Accessory
- Hip Accessory

## Equipment sockets prepared

- Head
- Back
- Chest
- Left Hand
- Right Hand
- Hips
- Left Hip
- Right Hip

The current sockets are source-preview positions. After rigging, these sockets
will be remapped beneath the corresponding humanoid bones so equipment follows
animation correctly.

## Required next asset step

The exact same FBX must be auto-rigged or manually rigged before gameplay
integration.

Recommended export requirements:

- FBX format
- Embedded or separate textures are acceptable
- T-pose
- One humanoid skeleton
- Skin weights applied to every body component
- Hair, beard, briefs, eyes, teeth, tongue, and eyelashes retained
- No animation required in the first rigged delivery
- Do not merge all modular parts into one mesh unless unavoidable
- Keep original mesh and material names when possible

After the rigged FBX is supplied, the next package will:

- Configure Unity Humanoid
- Generate a valid Avatar
- Attach the visual character below the first-person camera
- Hide the local head and hair layer
- Keep full-body shadows
- Add idle, walk, sprint, crouch, and jump animation parameters
- Move sockets onto humanoid bones
- Prepare separate skinned clothing meshes
- Add backpack attachment behavior
- Add appearance data to save schema

## Git note

The raw FBX is approximately 60 MB. GitHub accepts files below 100 MB, but this
is large enough that Git LFS should be considered before the character asset
library grows substantially.
