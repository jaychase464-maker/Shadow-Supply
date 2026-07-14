# ChatGPT Start Here

Use this repository as the source of truth for Shadow Supply.

## Required reading order

Before making changes:

1. Read `Documentation/SHADOW_SUPPLY_GAME_BIBLE.md`.
2. Read `PROJECT_CONTEXT.md`.
3. Read `CURRENT_STATUS.md`.
4. Read `SYSTEM_MAP.md`.
5. Read `BUGS.md`.
6. Search for and inspect every script, prefab, scene, ScriptableObject, and save structure relevant to the requested change.

## Authority order

1. `Documentation/SHADOW_SUPPLY_GAME_BIBLE.md` defines the approved release vision.
2. The current repository defines the actual implemented state.
3. `CURRENT_STATUS.md` defines the current confirmed milestone state.
4. Milestone documents describe individual implementations.

If implementation and the Bible conflict, do not silently choose one. Identify the conflict and either correct the implementation or propose a Bible amendment.

## Rules

- Preserve existing functionality.
- Do not create a simplified replacement for an existing system.
- Do not assume an old chat summary is more current than the repository.
- Inspect the actual current files.
- When editing a script, provide the complete script.
- Call out serialized-field, prefab, scene, ScriptableObject, or save-data migration requirements.
- Do not change persistent IDs without a migration plan.
- Treat package code under `Library/PackageCache` as generated dependency code, not project code.
- Prefer package-version fixes over manually editing cached package files.
- Document fixes in `CHANGELOG.md` and `BUGS.md`.
- Every future production recipe must use manual interaction steps.
- Major opportunities must be relationship-, faction-, or district-driven rather than unlocked only through a generic level.
- Core design changes must update:
  - `Documentation/SHADOW_SUPPLY_GAME_BIBLE.md`
  - `Documentation/GAME_BIBLE_CHANGELOG.md`
