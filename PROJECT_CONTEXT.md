# Shadow Supply — Project Context

## Game concept

Shadow Supply is a single-player, first-person, open-world underground business and criminal-logistics simulator.

The player begins with little money, a basic property, a ShadowOS smartphone, a small supplier network, and limited customers. Progression expands into production, storage, deliveries, properties, employees, territory, investigations, security, vehicles, and a regional underground operation.

The intended tone is gritty, grounded, industrial, and semi-realistic.

## Core progression

Runner → Dealer → Supplier → Organizer → Crew Leader → Regional Operator → Underground Kingpin

## Current technical direction

- Engine: Unity
- Render pipeline: Universal Render Pipeline
- Input: Unity Input System
- Perspective: First person
- World: Continuous open-world island/city
- UI direction: Dark industrial interface with amber, red, and teal accents
- Main device interface: ShadowOS smartphone

## Critical preservation rule

The repository is the source of truth.

Do not replace existing systems with condensed or simplified recreations. Before modifying a feature:

1. Find and read the relevant scripts.
2. Identify linked prefabs, scenes, ScriptableObjects, and serialized fields.
3. Check save/load dependencies.
4. Preserve existing behavior unless a change is explicitly requested.
5. Provide the full script whenever a script is modified.

## Known major gameplay systems

- First-person movement
- Interaction framework
- Inventory and item quality
- Hotbar and held items
- Buyer NPCs
- Supplier NPCs and supplier stock
- Daily demand
- Crafting and assembly
- Workstations
- Furniture purchase and placement
- Persistent placed objects
- Property ownership and upgrades
- ShadowOS phone
- Save/load
- Heat and investigation
- Dirty and clean cash
- Laundering
- Deliveries
- Vehicle purchasing and use
- Electrical grid and breaker capacity
- Physical plug, socket, cable, and power-strip system
- Powered machines and lights
- CCTV and keypad security
- Menus, HUD, map, inventory, and pause screens

## Art direction

Preferred:

- Realistic or semi-realistic industrial assets
- Weathered materials
- Worn warehouses, garages, brick industrial buildings, rail yards, docks, farms, and suburbs
- Realistic vehicles and props
- Optimized PBR materials
- LODs for repeated assets

Avoid in close-range final art:

- PSX-style assets
- Strongly stylized low-poly packs
- Assets with incompatible proportions or visual language
- Unoptimized high-poly vegetation and clutter without LODs
