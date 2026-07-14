# ChatGPT Start Here

Use this repository as the source of truth for Shadow Supply.

Before making changes:

1. Read `PROJECT_CONTEXT.md`.
2. Read `CURRENT_STATUS.md`.
3. Read `SYSTEM_MAP.md`.
4. Read `BUGS.md`.
5. Search for and inspect every script, prefab, scene, and save structure relevant to the requested change.

Rules:

- Preserve existing functionality.
- Do not create a simplified replacement for an existing system.
- Do not assume an old chat summary is more current than the repository.
- Inspect the actual current files.
- When editing a script, provide the complete script.
- Call out serialized-field, prefab, scene, or save-data migration requirements.
- Do not change persistent IDs without a migration plan.
- Treat package code under `Library/PackageCache` as generated dependency code, not project code.
- Prefer package-version fixes over manually editing cached package files.
- Document fixes in `CHANGELOG.md` and `BUGS.md`.
