# Milestone 8C — First Persistent Buyer Relationship and Order

## Purpose

This milestone introduces the first relationship-first progression loop
required by the Shadow Supply Game Bible.

The buyer is not a disposable delivery target. Mara Voss is a persistent
NPC with rapport, trust, respect, order history, memory, escalating work,
and referral requirements.

## Current buyer

**Mara Voss**

- District: Blackwater South
- Role: Early recurring buyer
- Personality: Cautious, professional, values reliability
- Unlock route: First impression followed by proven deliveries

## Relationship dimensions

- Rapport: personal comfort with the player
- Trust: confidence that the player keeps agreements
- Respect: belief that the player is capable

The UI shows descriptive relationship states rather than raw numbers.

## First impression

The first conversation provides three approaches:

- Professional
- Curious about the neighborhood
- Confident

Each choice changes rapport, trust, and respect differently.

## Orders

### Trial Package

- 1 Sealed Hardware Package
- Standard quality or better
- 10-minute deadline
- $240 dirty cash base reward

### Repeat Run

- Unlocks after one successful order
- 2 Sealed Hardware Packages
- Standard quality or better
- 12-minute deadline
- $540 dirty cash base reward

### Network Test

- Unlocks after two successful orders and sufficient trust/respect
- 3 Sealed Hardware Packages
- Standard quality or better
- 15-minute deadline
- $900 dirty cash base reward

## Physical handoff

The player must:

1. Accept the order.
2. Produce the package at the powered workbench.
3. Collect the package.
4. Select it in the hotbar.
5. Return to Mara.
6. Speak to her.
7. Press `HAND OVER HELD PACKAGE`.

Products are transferred one at a time. Quality and condition are recorded.

## Success and failure

Success:

- Pays dirty cash
- Increases rapport
- Increases trust
- Increases respect
- Advances order history
- May unlock a referral

Failure or cancellation:

- Reduces relationship values
- Records a failed order
- Applies a longer cooldown
- Does not return items already handed over

Declining is allowed and has a smaller trust penalty.

## Referral

After three successful orders and sufficient trust/respect, Mara unlocks a
relationship flag for a future Print-Supply Broker introduction.

The actual supplier NPC is deferred. This milestone proves that future
access can be earned through a person rather than a generic player level.

## Save schema 8

The game now saves:

- Buyer ID
- First-impression completion and choice
- Rapport
- Trust
- Respect
- Successful orders
- Failed orders
- Declined orders
- Referral state
- Active order
- Delivered quantity
- Delivered quality total
- Delivered condition total
- Remaining deadline
- Cooldown
- Last reward

Existing schema-7 saves migrate automatically.

## Installation

1. Close Unity.
2. Extract the ZIP into the project root.
3. Replace all prompted files.
4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 8C First Buyer Relationship`

7. Run:

   `Shadow Supply → Validation → Validate Milestone 8C First Buyer Relationship`

Expected structural result:

   `FIRST BUYER RELATIONSHIP READY`

## Test flow

1. Find Mara outside the starter garage.
2. Speak to her.
3. Select one first-impression response.
4. Accept `Trial Package`.
5. Produce a Sealed Hardware Package.
6. Collect and select it in the hotbar.
7. Speak to Mara.
8. Hand over the held package.
9. Confirm dirty cash increases.
10. Confirm relationship descriptions improve.
11. Wait for the cooldown.
12. Confirm `Repeat Run` becomes available.

## Save/load tests

- Save before the introduction and verify it remains incomplete.
- Complete the introduction, save, choose a different path, then load.
- Accept an order, wait, save, and verify the exact deadline returns.
- Deliver one item of a multi-item order, save, and verify progress returns.
- Complete an order, save, and verify order history and relationship state.
- Unlock the referral, save, and verify it remains unlocked.

## Version

`v0.8.6-first-buyer-relationship`
