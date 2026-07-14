# Shadow Supply — System Map

Update paths and class names after the repository is populated. Do not guess paths; inspect the project and replace placeholders with exact locations.

## Electrical and physical plug system

Known scripts from the legacy project:

- `PowerOutlet.cs`
- `PowerStripController.cs`
- `ElectricalGridSystem.cs`
- `ElectricalPanel.cs`
- `MachinePowerConnection.cs`
- `PowerCableVisual.cs`

Responsibilities:

- Persistent outlet IDs
- Socket assignments
- Plug and machine-connector alignment
- Cable visuals
- Power-strip sockets
- Power-strip switch state
- Downstream power delivery
- Breaker load
- Circuit capacity
- Overload behavior
- Save/load restoration
- Powered machine and light state

Preserve:

- Existing persistent IDs
- Existing save data fields
- Existing socket ordering
- Existing prefab references
- Existing cable and connector orientation rules

## Player and interaction

Document exact scripts for:

- First-person controller
- Camera look
- Player interaction raycast
- Interaction prompt UI
- Cursor lock/unlock
- Held-item system
- Item pickup and placement

## Inventory

Document exact scripts for:

- Inventory storage
- Item definitions
- Item condition
- Item quality
- Hotbar
- Stack rules
- Visible held items
- Shelf or world inventory display

## Crafting and production

Document exact scripts for:

- Crafting recipes
- Workbench
- Drill press
- Assembly
- Repair
- Packaging
- Printer input/output
- Tool requirements
- Station upgrades
- Skill and quality calculations

## Buyers

Document exact scripts for:

- Buyer NPC interaction
- Buyer demand
- Daily demand generation
- Buyer progression
- Pricing
- Reputation changes
- Deal completion

## Suppliers and deliveries

Document exact scripts for:

- Supplier NPCs
- Supplier stock
- Shop UI
- Supplier cart
- Combined deliveries
- Delivery spawning
- Purchased object delivery
- Item disappearance after purchase

## ShadowOS

Document exact scripts for:

- Phone open/close
- Phone UI
- Contacts
- Supplier marketplace
- Property listings
- Messages
- Missions
- Map
- Security camera viewing
- Remote access controls

## Furniture placement

Document exact scripts for:

- Purchase catalog
- Placement preview
- Rotation
- Surface and wall snapping
- Collision validation
- Placement bounds
- Persistent placed-object IDs
- Furniture save/load
- Property ownership checks

## Save and load

Document exact scripts for:

- Save manager
- Save slot metadata
- Player state
- Inventory state
- Property state
- Placed objects
- Electrical connections
- Buyer progression
- Supplier progression
- Time and day
- Cash and heat
- Vehicles
- World-state restoration

## Properties and world

Document exact scripts/scenes for:

- Starter garage
- Warehouses
- Safehouses
- Property purchase
- Renovation upgrades
- District triggers
- World streaming
- Traffic
- Rail system
- Day/night
- Weather or fog

## UI

Document exact paths for:

- Main menu
- Pause menu
- HUD
- Inventory
- Shop
- Buyer UI
- Map
- Save/load
- Settings
- Property UI
- Interaction prompt

## Vehicles

Document exact scripts for:

- Vehicle purchasing
- Vehicle ownership
- Enter/exit
- Driving
- Deliveries
- Traffic AI
- Save/load

## Security

Document exact scripts for:

- CCTV cameras
- Keypad locks
- Door access
- Security upgrades
- Remote monitoring
- Power dependency
