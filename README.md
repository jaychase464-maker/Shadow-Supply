# Shadow Supply

Shadow Supply is a first-person, open-world underground business and logistics simulator built in Unity.

This repository is the source of truth for the current game project. Existing systems must be inspected and preserved before changes are made. Do not replace working systems with simplified recreations.

## Start here

Read these files before modifying the project:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `BUGS.md`
5. `CHANGELOG.md`

## Unity project folders

Commit:

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- `Tools/`
- `Documentation/`
- Repository documentation files

Do not commit:

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`
- `Build/`
- `Builds/`
- `UserSettings/`
- `.vs/`

## Important development rules

- Preserve all existing functionality.
- Read the relevant scripts before editing.
- Return complete scripts when scripts are changed.
- Do not rewrite major systems unless the current implementation has been reviewed.
- Keep save compatibility in mind when changing serialized data or persistent IDs.
- Do not delete prefabs, scenes, materials, models, or scripts merely because they appear unused.
- Test changes against the current main gameplay scene.
