# Milestone 8D — First Persistent Supplier Relationship

## Goal

Milestone 8D connects Mara Voss's earned referral to the first persistent
supplier relationship.

Elias Mercer is not unlocked by a generic player level. The player must
first earn Mara's referral through her buyer relationship.

## Supplier

### Elias Mercer

- District: Blackwater South
- Role: Print and materials broker
- Access requirement: Mara Voss referral
- Payment type: Clean cash
- Delivery method: Physical crate at the starter-garage delivery pad
- Delivery delay: Approximately 8 seconds
- Full stock refresh: Approximately 120 seconds in the development build

## Relationship dimensions

Elias tracks:

- Rapport
- Payment trust
- Business respect
- Successful purchases
- Lifetime clean-cash spending

The UI presents descriptive relationship states instead of raw values.

## Introduction choices

After Mara's referral, the player chooses one opening approach:

- Lead with Mara's name
- Emphasize reliable payment
- Immediately request higher volume

Each approach changes rapport, trust, and respect differently.

## Relationship-gated catalog

### Packaging Material

- Available immediately after the introduction
- Maximum stock: 8
- Base unit price: $12

### Polymer Housing

- Requires at least one successful purchase
- Requires trust and respect
- Maximum stock: 5
- Base unit price: $32

### Metal Components

- Requires at least two successful purchases
- Requires stronger trust and respect
- Maximum stock: 8
- Base unit price: $40

### Basic Toolkit

- Requires at least four successful purchases
- Requires established trust and respect
- Maximum stock: 1
- Base unit price: $180

## Dynamic pricing

Prices begin above the supplier's base rate.

Reliable purchases improve trust. Higher trust and respect lower the price
multiplier over time. Negative rapport can increase prices.

The current pricing foundation supports future:

- Credit
- Debt
- Emergency supply
- Rare private stock
- Faction pricing
- Market shortages
- District price changes
- Delivery priority

## Physical deliveries

Supplier purchases do not appear instantly in inventory.

The flow is:

1. Select an unlocked stock item.
2. Choose a quantity.
3. Pay clean cash.
4. Wait for the supplier delivery.
5. A physical delivery crate appears at the garage delivery pad.
6. Walk to the crate.
7. Collect the items into inventory.

The existing persistent crate system is reused, so uncollected supplier
deliveries save and load like furniture deliveries.

Only one supplier order can be pending at a time in this milestone.

## Stock and relationship progression

Stock is limited.

Successful physical deliveries:

- Increase successful-purchase history
- Increase trust
- Increase respect
- Occasionally increase rapport
- Unlock additional catalog items
- Improve future pricing

Ordering three or more units in one shipment grants slightly more respect.

## Save schema 9

Schema 9 saves:

- Supplier ID
- Introduction completion and choice
- Rapport
- Trust
- Respect
- Successful purchases
- Lifetime clean-cash spending
- Individual stock quantities
- Restock timer
- Pending stock item
- Pending quantity
- Pending payment
- Remaining delivery time

Existing schema-8 buyer saves migrate automatically.

Physical crates continue using the existing delivery-crate persistence.

## Installation

Prerequisites:

- Confirmed Milestone 8C
- Mara Voss present and working
- Physical garage delivery pad working
- No red Console errors before installation

Steps:

1. Exit Play Mode.
2. Close Unity.
3. Extract the ZIP into the Unity project root.
4. Replace all prompted files.
5. Reopen Unity and wait for compilation.
6. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

7. Run:

   `Shadow Supply → Setup → Apply Milestone 8D First Supplier Relationship`

8. Run:

   `Shadow Supply → Validation → Validate Milestone 8D First Supplier Relationship`

Expected structural result:

   `FIRST SUPPLIER RELATIONSHIP READY`

## Normal test route

1. Complete Mara's buyer progression until her referral unlocks.
2. Find Elias near the starter garage.
3. Confirm his marker changes from `UNKNOWN BROKER` to his name.
4. Speak to him.
5. Choose an introduction response.
6. Order Packaging Material.
7. Confirm clean cash is deducted.
8. Confirm the order enters a pending-delivery state.
9. Wait approximately 8 seconds.
10. Confirm a physical crate appears at the delivery pad.
11. Collect the crate.
12. Confirm the item appears in inventory.
13. Repeat purchases and confirm better stock unlocks.
14. Confirm the displayed price multiplier improves.

## Fast development test route

Enter Play Mode and run:

`Shadow Supply → Testing → Grant First Supplier Referral (Play Mode)`

This temporary override:

- Allows immediate supplier testing
- Does not modify Mara
- Does not persist into the saved scene
- Is cleared when Play Mode ends

## Save/load tests

### Relationship and stock

1. Complete the supplier introduction.
2. Make several purchases.
3. Save.
4. Make additional purchases or wait for restock.
5. Load.
6. Confirm relationship states, purchase history, prices, and stock return.

### Pending delivery

1. Place an order.
2. Save before the delivery arrives.
3. Wait for it to arrive.
4. Load.
5. Confirm the saved remaining delivery time returns.
6. Confirm only one crate is created.

### Delivered crate

1. Allow a crate to arrive but do not collect it.
2. Save.
3. Collect the crate.
4. Load.
5. Confirm the saved crate returns with the correct item and quantity.

## Regression tests

- Mara's buyer relationship still works
- Buyer referrals still save
- Furniture orders still charge clean cash
- Furniture deliveries still spawn
- Existing delivery crates still collect correctly
- Interactive production still works
- Electrical system still works
- Save/load still works
- No red Console errors appear

## Deferred

- Supplier credit and debt
- Multiple simultaneous supplier orders
- Day-based stock refresh
- Random shortages
- Supplier schedules
- Faction-controlled stock
- Delivery interception
- Supplier arrest or disappearance
- Counterfeiting-specific materials
- ShadowOS remote ordering
- Employees handling supplier pickups

## Version

`v0.8.7-first-supplier-relationship`
