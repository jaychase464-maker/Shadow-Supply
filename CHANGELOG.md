# Changelog

## Unreleased

### Added

#### Milestone 5 — Furniture Ownership and Acquisition

- Player clean-cash and dirty-cash wallet foundation
- Furniture purchase prices
- Physical furniture supplier terminal
- Furniture shop interface
- Physical delivery marker and delivery crates
- Persistent uncollected delivery crates
- Furniture inventory packages
- Owned furniture quantities in build mode
- Placement consumes one furniture package
- Packing furniture returns one package
- Full-inventory protection during packing and delivery collection
- Wall-placement foundation
- CCTV camera wall placeable
- Door keypad wall placeable
- User-supplied imported workbench model
- User-supplied imported pallet-rack model
- User-supplied imported office-desk model
- User-supplied imported CCTV model
- User-supplied imported keypad model
- Furniture inventory icons
- Save schema version `3`

### Changed

- Placement database expanded to six placeable definitions.
- Build selection expanded from keys `1–3` to keys `1–6`.
- Furniture removal now packs the object instead of deleting it for free.
- Save schema migrated from version `2` to version `3`.
- Save versions `1` and `2` remain supported.

### Fixed

- Furniture can no longer be placed without ownership.
- Furniture can no longer disappear when packing fails because inventory is full.
- Purchased furniture is no longer granted directly; it arrives physically.
