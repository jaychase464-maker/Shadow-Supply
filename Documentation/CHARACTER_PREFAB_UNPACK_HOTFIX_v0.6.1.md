# Shadow Supply v0.6.1 — Character Preview Prefab Hotfix

## Problem

Milestone 6 instantiated the imported FBX as a connected Unity model-prefab
instance and then attempted to move its child meshes into the modular character
groups.

Unity does not permit reparenting individual children while they still reside
inside a connected prefab instance. This produced repeated messages such as:

```text
Setting the parent of a transform which resides in a Prefab instance is not possible
```

Affected objects included:

- Body
- Eye
- Teeth
- Tongue
- Eyelashes
- Hair
- Beard
- Briefs

## Fixed

The setup tool now:

1. Instantiates the source FBX
2. Detects whether it is connected to a prefab instance
3. Finds the outermost prefab-instance root
4. Completely unpacks the temporary preview instance
5. Reparents the loose mesh objects into their modular groups
6. Saves the completed character preview as a new Shadow Supply prefab

The original imported FBX asset is not modified.

## Rig-validation message

The following message is expected and remains correct:

```text
RIGGING REQUIRED
Skinned renderers: 0
Valid humanoid avatar: False
```

That is a validation result, not a setup failure. The supplied source FBX is
modular but unrigged.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone6PlayerCharacterSetup.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 6 Character Foundation`

The setup will rebuild and overwrite the incomplete preview prefab and preview
station. No manual deletion is required.

## Acceptance test

- No prefab-parenting warnings appear
- Body, eyes, teeth, tongue, eyelashes, hair, beard, and briefs are moved into
  their intended modular groups
- `PREFAB_PlayerCharacter_SourcePreview` is rebuilt
- `CharacterPreviewStation` contains the complete character
- Materials remain assigned
- Rig validation still reports `RIGGING REQUIRED`
- No red Console errors appear

## Version

`v0.6.1-character-prefab-unpack`
