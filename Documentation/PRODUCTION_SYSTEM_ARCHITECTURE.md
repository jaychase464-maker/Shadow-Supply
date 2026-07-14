# Production System Architecture

## Runtime classes

### `ProductionIngredient`

Defines one recipe requirement:

- item
- quantity
- consumed or reusable

### `ProductionRecipe`

ScriptableObject containing:

- stable recipe ID
- display name
- description
- duration
- power requirement
- ingredient list
- output item
- output quantity

### `ProductionRecipeDatabase`

Stable recipe lookup for save restoration and future workstations.

### `PoweredWorkbenchProduction`

Owns the starter workbench process:

- validates requirements
- consumes materials
- captures ingredient quality
- runs or pauses the timer
- creates pending output
- collects output into inventory
- exposes save state
- restores save state

### `ProductionWorkbenchHUD`

Temporary OnGUI production interface.

The gameplay state is owned by `PoweredWorkbenchProduction`, not the UI.
This keeps production running when the interface is closed.

## Stable IDs

- Workbench: `starter-workbench`
- Recipe: `recipe-sealed-hardware-package`
- Packaging Material: `production-packaging-material`
- Finished Product: `product-sealed-hardware-package`

## State model

A workbench can be in exactly one of three meaningful states:

1. Idle
2. Producing
3. Output ready

Starting another recipe is blocked until pending output is collected.

## Future expansion

The same architecture can support:

- drill press recipes
- packaging stations
- repair benches
- recipe unlocks
- station upgrades
- multiple queued outputs
- batch production
- NPC workers
- recipe-specific machine demands
- player skill and failure chances
