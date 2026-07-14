# Physical Electrical System Architecture

## Runtime classes

### `ElectricalPanel`

Owns the garage service and circuit state:

- main capacity
- main breaker
- circuit definitions
- circuit capacities
- calculated loads
- tripped states

### `ElectricalGridSystem`

Recalculates connected demand and refreshes powered equipment.

The first pass refreshes at a controlled interval because the garage has very
few electrical objects. Later world properties can switch to explicit dirty
registration if profiling requires it.

### `PowerOutlet`

Represents one physical outlet with a stable ID and circuit assignment.

### `OutletSocket`

Represents one individually occupied socket and its exact plug snap point.

### `PlayerPlugController`

Holds one plug in front of the first-person camera and connects or releases it.

### `PowerPlug`

Owns:

- persistent ID
- cable reach
- held state
- connected outlet/socket
- physical Rigidbody state
- saved free position

### `PowerCableVisual`

Draws the machine-to-plug cable using a sagging segmented line.

### `MachinePowerConnection`

Converts physical connection and circuit state into a stable `IsPowered`
result for future crafting and machine systems.

### `CircuitBreakerSwitch`

Provides physical `E` interactions for main and branch breakers.

### `ElectricalLightLoad`

Adds the garage fluorescent fixtures to calculated circuit demand.

## Stable IDs

Current installed IDs:

- panel: `starter-garage-panel`
- workbench machine: `starter-workbench`
- workbench plug: `starter-workbench-plug`
- outlets: `garage-outlet-01` through `garage-outlet-06`
- circuits:
  - `garage-lighting`
  - `garage-work`
  - `garage-office`

Save restoration uses these IDs rather than scene hierarchy paths.

## Future compatibility

The architecture is designed to support:

- property-specific panels
- outlet circuit reassignment
- power strips as downstream outlets
- extension cords as plug-to-socket devices
- movable machines with stable power connections
- generator-backed circuits
- circuit upgrades
- illegal power tapping
- heat generation from excessive usage
