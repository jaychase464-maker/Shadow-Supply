# Shadow Supply — Project Context

## Canonical design source

The approved release vision is defined in:

`Documentation/SHADOW_SUPPLY_GAME_BIBLE.md`

That document is the highest-level design authority. Core design changes require a recorded Bible amendment.

## Game concept

Shadow Supply is a single-player, first-person, open-world underground business and criminal-logistics simulator.

The player begins as a low-level counterfeiter operating from a rundown starter garage. Progression expands through physical production, recurring NPC relationships, district reputation, faction decisions, suppliers, employees, properties, logistics, investigations, security, vehicles, and a regional underground operation.

The game is designed for long persistent saves. Completing major story arcs does not end the simulation.

## Core progression

Organizational scale:

Runner → Dealer → Supplier → Organizer → Crew Leader → Regional Operator → Underground Kingpin

Starting industry:

Low-Level Counterfeiting → Advanced Counterfeiting / Documents / False Identification → Illegal Technology → Specialized underground industries

Progression is not controlled by one universal level. Access depends on combinations of:

- NPC rapport
- NPC trust
- NPC respect
- Employee loyalty
- District respect
- Industry reputation
- Faction standing
- Skill
- Property
- Equipment
- Money
- Introductions
- Story choices

## Current technical direction

- Engine: Unity
- Render pipeline: Universal Render Pipeline
- Input: Unity Input System
- Perspective: First person
- World: Continuous open-world island metropolis and satellite islands
- UI direction: Dark industrial interface with amber, red, and teal accents
- Main device interface: ShadowOS smartphone
- Production direction: Every recipe uses manual physical interactions
- Progression direction: Major access comes through people and networks
- Replayability direction: New worlds vary faction, NPC, market, and event state

## Critical preservation rule

The repository is the implementation source of truth.

Do not replace existing systems with condensed or simplified recreations. Before modifying a feature:

1. Read the Game Bible.
2. Find and read the relevant scripts.
3. Identify linked prefabs, scenes, ScriptableObjects, and serialized fields.
4. Check save/load dependencies.
5. Preserve existing behavior unless a change is explicitly requested.
6. Provide the full script whenever a script is modified.
7. Record any core-direction amendment in the Bible changelog.

## Known major gameplay systems

- First-person movement
- Visible modular player character
- Interaction framework
- Inventory and item quality
- Hotbar and held items
- Interactive manual production
- Workstations
- Physical electrical grid
- Plug, socket, cable, breaker, and circuit system
- Furniture purchase and placement
- Persistent placed objects
- Property ownership and upgrades
- Buyer relationships and orders
- Supplier relationships and stock
- Employee recruitment and management
- District reputation
- Faction relationships
- Industry reputation
- NPC memory
- ShadowOS phone
- Save/load and migrations
- Heat, evidence, and investigation
- Dirty and clean cash
- Laundering
- Deliveries
- Vehicles
- CCTV and keypad security
- Dynamic demand and market state
- Menus, HUD, map, inventory, and pause screens

## Art direction

Preferred:

- Realistic or semi-realistic industrial assets
- Weathered materials
- Worn warehouses, garages, brick industrial buildings, rail yards, docks, farms, suburbs, downtown, and wealthy neighborhoods
- Realistic vehicles and props
- Optimized PBR materials
- LODs for repeated assets

Avoid in close-range final art:

- PSX-style assets
- Strongly stylized low-poly packs
- Assets with incompatible proportions or visual language
- Unoptimized high-poly vegetation and clutter without LODs
