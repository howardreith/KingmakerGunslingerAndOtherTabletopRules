# Sprint 86 battered ownership persistence carrier

Host the exact-item/origin-unit ledger on the same player-owned save graph used
by firearm state. Serialize only schema version and primitive identity strings.
Reject unsupported, null, duplicate, padded, empty, or conflicting records;
never serialize runtime item/unit references or infer ownership from equipment.

The provider must reuse the exact main-character resolution contract and create
the UnitPart only for a validated write. Focused source checks, repository
validation, the complete domain suite, clean Release build, and strict package
validation qualify the carrier. Runtime reconstruction is deferred until the
starting-item binder supplies a real exact item and unit pair.
