# Counterfeit Press Architecture

## `CounterfeitRecipeDefinition`

Data-driven definition for a fictional counterfeiting recipe.

Current fields:

- Stable recipe ID
- Display name
- Description
- Blank note stock
- Pigment capsule
- Security film
- Packaging material
- Reusable toolkit
- Output
- Output quantity
- Power requirement
- Print-control hold duration
- Reputation reward

## `CounterfeitPressStation`

Owns persistent production state:

- Materials
- Power
- Active process
- Ordered step
- Mistakes
- Print progress
- Reserved quality
- Pending output
- Reputation reward
- Output collection

It implements `IInteractable` and replaces the disabled development
production component on the starter workbench.

## `CounterfeitPressInteractionController`

Owns the physical top-down interaction view:

- Part spawning
- Dragging
- Target-zone validation
- Ordered-step enforcement
- PRINT hold interaction
- Toolkit interaction
- SEAL interaction
- Power interruption
- Save-state visual reconstruction

## `CounterfeitPressPart`

Marks physical process objects and stores:

- Part type
- Optional inventory definition
- Drag capability
- Collider
- Start pose
- Completed pose
- Highlight state

## `CounterfeitPressActionTarget`

Identifies clickable station controls:

- PRINT
- SEAL

## `CounterfeitPressHUD`

Handles:

- Recipe requirements
- Material counts
- Counterfeiting reputation
- Process instructions
- Step progress
- Mistake count
- PRINT progress
- Output collection
- Player control locking

## `IndustryReputationSystem`

Generic stable-ID reputation storage intended for future industries:

- Counterfeiting
- Forged documents
- Illegal technology
- Smuggling
- Other later branches

Milestone 9A only activates the counterfeiting industry.

## Save integration

`SaveManager` schema 10 captures and restores:

- Every counterfeit press
- Every industry reputation entry

Existing production, buyer, supplier, delivery, electrical, property,
inventory, and character save data remain in the same save file.
