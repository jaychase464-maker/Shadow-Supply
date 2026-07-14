# Milestone 8A — Physical Garage Power

## Goal

Milestone 8A begins Shadow Supply's physical electrical system.

The player must physically grab the starter workbench plug, carry it across
the garage, and connect it to an individual wall-outlet socket.

Power is no longer granted through a menu or a generic proximity check.

## Included systems

### Physical plug interaction

- Workbench plug exists as a physical world object
- `E` grabs an unplugged plug
- `E` unplugs a connected plug and places it in the player's hand
- `E` connects the held plug to a highlighted outlet socket
- `R` releases a held plug
- Released plug uses gravity and collision
- Plug movement is limited by its cable length
- Cable remains visible while held, released, and connected

### Controlled cable visualization

The cable uses a segmented `LineRenderer` rather than unstable rope physics.

It supports:

- a permanent machine-side cable anchor
- live plug endpoint tracking
- visual sag based on remaining slack
- maximum length enforcement
- a taut appearance near maximum reach
- saved connected and disconnected states

Cable collision is intentionally deferred. The visual cable may pass through
furniture during this foundation milestone.

### Individual outlet sockets

Every installed garage outlet now has:

- a stable outlet ID
- a circuit assignment
- an upper socket
- a lower socket
- individual occupancy
- exact plug snap points
- held-plug compatibility highlighting
- powered and unpowered state

The workbench can be connected to either socket when the cable can reach it.

### Breaker panel

The installed garage panel now controls:

- Main breaker
- Work-outlet breaker
- Lighting breaker
- Office-outlet circuit state
- Main service capacity
- Individual circuit capacity
- Current calculated load
- Tripped breaker state

A tripped breaker remains off until the player interacts with it to reset it.

### Powered workbench foundation

The starter workbench now has:

- stable machine ID
- 850-watt demand
- machine-side cable anchor
- physical plug
- power cable
- red/green power indicator
- powered-state event foundation

The workbench is considered powered only when:

1. Its plug is connected.
2. The selected outlet circuit is on.
3. The circuit is not tripped.
4. Main power is on.
5. The main breaker is not tripped.

Crafting is not added yet. Milestone 8B will use this powered state.

### Lighting circuit

The existing garage light switch now respects electrical power.

The lights require:

- the wall switch to be on
- the lighting breaker to be on
- the main breaker to be on

The four fluorescent fixtures draw a combined 240 watts.

### Electrical status HUD

A temporary development HUD displays:

- main power state
- circuit state
- circuit load and capacity
- workbench powered state
- held-plug controls

This will later be replaced by finalized in-world feedback and UI.

## Save schema 5

Save schema `5` adds:

- main-breaker state
- main tripped state
- circuit on/off states
- circuit tripped states
- garage light-switch state
- plug persistent IDs
- connected outlet IDs
- connected socket indexes
- disconnected plug positions
- disconnected plug rotations

Existing schema-4 saves migrate automatically.

## Installation

Prerequisite:

- Confirmed starter garage through `v0.7.7`
- Existing save/load system
- No red Console errors before installation

Steps:

1. Close Unity.
2. Extract the ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity and wait for compilation.
5. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

6. Run:

   `Shadow Supply → Setup → Apply Milestone 8A Physical Garage Power`

7. Run:

   `Shadow Supply → Validation → Validate Milestone 8A Physical Garage Power`

Expected structural result:

   `PHYSICAL GARAGE POWER READY`

Do not rerun the Milestone 7 garage setup afterward without rerunning
Milestone 8A, because rebuilding the garage also removes its electrical
runtime configuration.

## Controls

- `E` on unplugged plug — grab plug
- `E` on connected plug — unplug and hold
- `E` on compatible socket while holding — connect plug
- `R` while holding — release plug
- `E` on breaker switch — toggle or reset breaker
- `E` on garage light switch — toggle requested lighting state

## Acceptance test

### Setup

- Milestone 8A setup completes without exceptions
- `Milestone8_PhysicalElectricalSystem` appears under the garage
- Player receives `PlayerPlugController`
- Main panel receives `ElectricalPanel`
- Six outlets receive `PowerOutlet`
- Every outlet contains two socket objects
- Workbench receives `MachinePowerConnection`

### Plug interaction

- Plug begins disconnected near the workbench
- Looking at the plug shows a grab prompt
- `E` moves the plug in front of the camera
- Cable follows from the workbench
- Cable sags while slack
- Plug stops extending at maximum cable length
- Compatible outlet sockets highlight
- `E` snaps the plug into the selected socket
- Plug occupies only that socket
- Second socket remains available
- `E` on connected plug unplugs it
- `R` releases the held plug

### Workbench power

- Workbench indicator is red while disconnected
- Connecting to a powered work outlet turns it green
- Workbench HUD state changes to `POWERED`
- Turning off the work breaker removes power
- Resetting the breaker restores power
- Turning off main power removes all workbench power

### Lighting

- Garage lights work when switch and lighting breaker are on
- Turning off lighting breaker turns fixtures off
- Wall switch state remains remembered while circuit is off
- Restoring the circuit returns the lights when the switch was left on
- HUD shows approximately 240 watts on the lighting circuit

### Save/load

Test both connected and disconnected plug states.

- Connect workbench plug to a specific outlet socket
- Change at least one breaker state
- Change garage light-switch state
- Save
- Move or disconnect the plug
- Change breaker states
- Load
- Plug returns to the saved outlet and socket
- Socket occupancy restores
- Breakers restore
- Light switch restores
- Workbench powered state restores
- Existing schema-4 saves still load

### Regression

- Player movement still works
- Character body still works
- Inventory and hotbar still work
- Furniture shop and deliveries still work
- Furniture placement still works
- Garage doors and keypads remain intact
- No red Console errors appear

## Deferred

- Cable collision with furniture and walls
- Extension cords
- Four-socket power strips
- Daisy chaining
- Cable reels
- Generators
- Sparks and damaged cords
- Multiple production machines
- Final breaker-panel animation
- Production recipe UI
- Workbench crafting

## Version

`v0.8.0-physical-garage-power`
