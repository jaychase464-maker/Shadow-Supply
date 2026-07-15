# Supplier Relationship System Architecture

## `SupplierDefinition`

Defines a persistent supplier:

- Stable supplier ID
- Name
- District
- Biography
- Required buyer referral
- Initial relationship
- Stock catalog
- Restock interval
- Delivery delay

## `SupplierStockDefinition`

Defines one relationship-gated catalog item:

- Stable stock ID
- Inventory item
- Base unit price
- Maximum stock
- Required purchases
- Required rapport
- Required trust
- Required respect

## `SupplierNPC`

Owns runtime supplier state:

- Referral gate
- Introduction choice
- Rapport
- Trust
- Respect
- Dynamic prices
- Runtime stock
- Restocking
- Payment
- Pending delivery
- Relationship progression
- Save restoration

## `SupplierRelationshipHUD`

Displays:

- Relationship descriptions
- Account history
- Price multiplier
- Restock timing
- Referral state
- Introduction choices
- Locked and unlocked stock
- Quantity selection
- Clean-cash totals
- Pending delivery

## Delivery integration

`FurnitureDeliverySystem` retains all previous furniture behavior and adds
`SpawnItemDelivery(...)`.

The method uses:

- The existing delivery point
- The existing delivery-crate prefab
- The existing crate-spacing rules
- The existing persistent delivery save system

This avoids creating a second incompatible delivery framework.

## Relationship-first gate

Elias checks the runtime `BuyerNPC` with stable ID:

`buyer-mara-voss`

Access becomes available only when Mara's persistent `ReferralUnlocked`
state is true.

The development override exists only in Play Mode and is never written to
save data.
