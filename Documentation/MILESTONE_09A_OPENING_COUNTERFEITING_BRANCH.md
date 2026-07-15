# Milestone 9A — Opening Counterfeiting Production Branch

## Purpose

Milestone 9A replaces the temporary Sealed Hardware Package development loop
with Shadow Supply's intended opening career: low-level counterfeiting.

The new production process is intentionally fictionalized. It supports the
game fantasy without documenting real-world counterfeiting methods.

## First real product

### Replica Note Bundle

A sealed bundle of fictional replica notes produced at the starter garage.

Required materials:

- Blank Note Stock
- Pigment Capsule
- Security Film
- Packaging Material
- Basic Toolkit, reusable

Output:

- 1 Replica Note Bundle
- Base value: $260
- Quality and condition depend on materials, reputation, and mistakes

## Powered imprint press

The existing starter workbench becomes a powered imprint press.

The old Sealed Hardware Package production component remains in the project
for save compatibility but is disabled in the development scene.

The new station continues using the same physical workbench electrical
connection and therefore still requires:

- The plug connected to an outlet
- The correct branch circuit powered
- The main electrical panel powered
- Available circuit capacity

## Ordered interactive process

Production now has nine ordered manual steps:

1. Drag Blank Note Stock into the paper tray.
2. Drag the Pigment Capsule into the pigment bay.
3. Align Security Film on the imprint bed.
4. Hold the illuminated PRINT control until the cycle completes.
5. Drag the printed sheet onto the cutting mat.
6. Click the Basic Toolkit to trim the sheet.
7. Drag the cut note stack into the packaging zone.
8. Drag Packaging Material over the note stack.
9. Click SEAL to create the finished bundle.

The expected object and target zone highlight during each step.

Wrong materials, out-of-order actions, early sealing, and incorrect drops
count as production mistakes.

## Mistakes and quality

Each mistake reduces the final condition and quality score.

Output quality also considers:

- Input material quality
- Input material condition
- Counterfeiting industry reputation
- Number of production mistakes

The current quality tiers remain:

- Poor
- Standard
- Good
- Excellent
- Masterwork

## Counterfeiting industry reputation

Milestone 9A introduces a generic industry-reputation foundation.

The opening industry is:

`industry-counterfeiting`

Display states:

- Unknown Printer
- Street Counterfeiter
- Reliable Printer
- Skilled Counterfeiter
- Master Printer

Completing a run grants counterfeiting reputation. Mistake-heavy runs grant
less reputation.

Higher reputation provides a modest production-quality bonus. It does not
replace material quality or manual execution.

## Elias Mercer catalog conversion

Elias's development hardware stock is replaced with the opening
counterfeiting supply chain:

- Blank Note Stock
- Pigment Capsule
- Security Film
- Packaging Material
- Basic Toolkit

Existing supplier relationship progress is preserved.

Newly introduced stock entries are initialized at full stock when loading an
older supplier save instead of incorrectly restoring at zero.

## Mara Voss order conversion

Mara's three persistent orders now request Replica Note Bundles.

### First Replica Run

- 1 Replica Note Bundle
- Standard quality or better
- $320 dirty cash base payment
- First successful order unlocks Elias's referral

### Repeat Replica Run

- 2 Replica Note Bundles
- Standard quality or better
- $720 dirty cash base payment

### District Sample Run

- 3 Replica Note Bundles
- Good quality or better
- $1,250 dirty cash base payment

Existing buyer relationship and order IDs are retained for save
compatibility.

## Opening progression loop

The intended current loop is:

1. Use the starter material batch.
2. Produce the first Replica Note Bundle.
3. Complete Mara's First Replica Run.
4. Earn Mara's referral.
5. Meet Elias.
6. Purchase additional counterfeiting materials with clean cash.
7. Collect the physical supplier crate.
8. Produce additional bundles.
9. Build buyer trust, supplier trust, and counterfeiting reputation.

This is the first complete relationship-driven opening career loop.

## Save schema 10

Schema 10 saves:

### Counterfeit press

- Stable station ID
- Whether a run is active
- Current ordered step
- Mistake count
- Partial print-control progress
- Reserved input quality score
- Reserved input condition
- Pending output item
- Pending output quantity
- Pending output quality
- Pending output condition

### Industry reputation

- Stable industry ID
- Reputation amount

Existing schema-9 supplier and buyer saves migrate automatically.

## Installation

Prerequisites:

- Confirmed Milestone 8D
- No red Console errors
- No supplier delivery currently pending during installation
- No temporary hardware recipe actively in progress

Steps:

1. Exit Play Mode.
2. Close Unity.
3. Extract the ZIP into the Unity project root.
4. Replace all prompted files.
5. Reopen Unity and wait for compilation.
6. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

7. Run:

   `Shadow Supply → Setup → Apply Milestone 9A Opening Counterfeiting Branch`

8. Run:

   `Shadow Supply → Validation → Validate Milestone 9A Opening Counterfeiting Branch`

Expected structural result:

   `OPENING COUNTERFEITING BRANCH READY`

## Main test

1. Connect and power the starter workbench.
2. Interact with the new imprint press.
3. Confirm one starter batch is available.
4. Start Replica Note Bundle production.
5. Complete all nine steps in order.
6. Intentionally make one incorrect drop.
7. Confirm the mistake counter increases.
8. Finish and collect the output.
9. Confirm quality and condition are present.
10. Confirm counterfeiting reputation increases.
11. Give the bundle to Mara.
12. Confirm Elias's referral unlocks after the first successful order.
13. Buy replacement materials from Elias.
14. Collect the physical delivery crate.
15. Produce another bundle.

## Fast material test

Enter Play Mode and run:

`Shadow Supply → Testing → Grant Counterfeiting Starter Materials (Play Mode)`

This grants enough fictional materials for three production tests.

## Save/load tests

### Partial process

1. Complete several production steps.
2. Exit the press view.
3. Save.
4. Finish or alter the run.
5. Load.
6. Confirm the exact saved step and mistake count return.
7. Confirm the correct physical parts and target state return.

### Print control

1. Partially hold PRINT.
2. Save before completion.
3. Load.
4. Confirm partial print progress returns.

### Pending output

1. Finish a bundle without collecting it.
2. Save.
3. Collect it.
4. Load.
5. Confirm the saved bundle returns.

### Reputation

1. Complete several runs.
2. Save.
3. Complete more runs.
4. Load.
5. Confirm the saved reputation amount and title return.

## Regression tests

- Mara's relationship persists
- Elias's relationship persists
- Supplier prices persist
- Supplier stock persists
- Physical deliveries persist
- Garage electrical system still works
- Workbench interaction remains easy to target
- Furniture deliveries still work
- Inventory quality still works
- Save/load still works
- No red Console errors appear

## Deferred

- Animated printer mechanisms
- Audio for feed rollers and cutting
- Defect inspection minigame
- Multiple counterfeit note designs
- Recipe familiarity
- Station maintenance
- Printer upgrades
- District-specific counterfeit demand
- Heat and evidence generation
- Counterfeiting skill perks
- Employee-operated printing
- ShadowOS production tracking

## Version

`v0.9.0-opening-counterfeiting-branch`
