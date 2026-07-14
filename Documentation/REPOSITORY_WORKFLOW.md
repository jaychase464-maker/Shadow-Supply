# Repository Workflow

## Branches

Recommended:

- `main` — verified working version
- `develop` — integration work
- `feature/<name>` — isolated features
- `fix/<bug-name>` — bug fixes
- `world/island-rebuild` — island merge work

## Before changing a system

1. Pull the latest repository state.
2. Create a branch.
3. Open the project and confirm it compiles before edits.
4. Reproduce the issue.
5. Inspect related scripts, prefabs, scenes, and save data.
6. Make the smallest complete change.
7. Test.
8. Update documentation.
9. Commit with a descriptive message.

## Suggested commit messages

```text
fix(electrical): correct outlet wall orientation
fix(electrical): align machine connector snap point
fix(scene): clear starter garage entrance
feat(world): add rail-yard district shell
docs(system-map): document save restoration dependencies
```

## Never commit generated folders

- Library
- Temp
- Logs
- obj
- Build
- Builds
- UserSettings
- .vs
