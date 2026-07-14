# Milestone 7A — Modular Starter Garage

## Purpose

Milestone 7A replaces the open development test room with Shadow Supply's first
real property: a rough but usable starter garage built from modular pieces and
the user's supplied environment assets.

This is the visual, scale, collision, and layout test gate before the full
property, electrical-load, and persistence pass.

## Locked progression rule

### Starter garage

The starter garage includes a basic setup from the beginning:

- Workbench
- Storage rack
- Office desk
- Utility cabinet
- Computer monitor
- Breaker-panel housing
- Electricity meter
- Wall outlets
- Light switch
- Ceiling lights
- CCTV camera
- Door keypad
- Ventilation
- Exterior utility and delivery dressing

These objects are marked as included property assets. They are not treated as
free furniture packages and cannot currently be packed or sold.

### Purchased hideouts

The first purchased hideout and later properties will begin as empty shells.
The player must buy, deliver, and place their own furniture and equipment.

## Garage dimensions

- Exterior footprint: approximately `12 m × 8 m`
- Interior height: approximately `4.2 m`
- Four-meter modular structural grid
- Main overhead-door opening: approximately `4 m`
- Exterior delivery pad in front of the garage
- Asphalt property lot around the building

## Included construction

- Modular brick wall pieces
- Generated structural infill around openings
- Cracked-concrete interior slab
- Asphalt exterior lot
- Concrete delivery pad
- Temporary modular flat roof
- Beamed-brick ceiling surface
- Rusty structural columns and opening header
- Temporary sectional overhead door
- Separate entry door and frame
- Exposed ventilation pieces
- Interior fluorescent fixtures
- Exterior street light and utility props
- Curbs, pallets, boxes, and bins

## Basic interactions included

- `E` opens and closes the temporary overhead garage door
- `E` opens and closes the regular entry door
- `E` toggles the installed garage lights

These interactions are intentionally simple for the first property test.

## Deferred to the next garage-system pass

- Door-state save persistence
- Breaker capacity and overload
- Physical power cables
- Plugging machines into outlets
- Saved light-switch state
- Starter-property ownership record
- Moving or replacing included furniture
- Hideout purchase flow
- Final purchased roof model
- Final purchased overhead-door model

## Installation

Prerequisites:

- Milestones 1–6 installed
- Milestone 5 furniture prefabs generated
- Rigged player-character integration installed
- `Dev_Playground` opens without red Console errors

Steps:

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace files when prompted.
4. Reopen Unity.
5. Wait for the imported models and textures to finish processing.
6. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

7. Run:

   `Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

The setup removes the old `Environment` root and replaces it with
`StarterGarage_Property`.

It also moves the existing furniture delivery point and supplier terminal into
the new property layout.

## Expected hierarchy

```text
StarterGarage_Property
├── Structure
│   ├── Modular Walls
│   ├── Roof and Beams
│   ├── Temporary Overhead Garage Door
│   ├── Entry Door Assembly
│   ├── Installed Lighting
│   └── Installed Utilities
├── Exterior Asphalt Lot
├── Delivery Concrete Pad
├── Included Starter Furniture
├── Exterior Dressing
├── Player Spawn
├── Delivery Drop
└── Supplier Terminal Point
```

## Acceptance test

### Structure

- Garage appears approximately 12 m × 8 m
- Brick modules align without major gaps
- Floor is level
- Roof covers the full interior
- Player cannot walk through walls
- Player can pass through both intended openings
- Player does not become trapped by the roof or beams

### Doors

- Overhead door begins open
- Looking at the overhead door displays an interaction prompt
- `E` closes the overhead door
- `E` opens it again
- Door collision moves with the door
- Entry door rotates open and closed
- Entry frame does not block the doorway

### Interior

- Workbench is located along the left work wall
- Storage rack is against the rear wall
- Desk and monitor form an office corner
- Utility cabinet does not block the entry path
- Breaker panel, outlets, switch, CCTV, and keypad are visible
- Furniture scale matches the player

### Lighting

- Four fluorescent fixtures are visible
- Garage is readable at night or in shadow
- The wall switch toggles all four lights
- Lights do not create severe flickering or duplicate illumination

### Exterior

- Delivery pad is accessible
- Furniture deliveries appear at the exterior drop point
- Supplier terminal remains usable
- Asphalt lot surrounds the building
- Street light, bins, pallets, boxes, power box, and curbs appear
- Exterior props do not block the main walking path

### Regression

- Player movement and crouching still work
- Rigged first-person body still works
- Interaction still works
- Inventory and hotbar still work
- Furniture purchasing and deliveries still work
- Build mode still works
- Existing saves can still be loaded
- No red Console errors appear

## Current version

`v0.7.0-modular-starter-garage`
