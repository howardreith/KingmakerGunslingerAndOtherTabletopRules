# Shield Other Implementation Journal

## 2026-08-09 - mission start

- Created `codex/shield-other-spell` from exact frozen SHA
  `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57` after fetching origin.
- Verified the remote Acadamae branch points exactly to the frozen SHA.
- Working tree was clean; no Shield Other branch existed locally or remotely.
- Began installed-content and exact 2.1.7b damage-engine inventory.
- Static CotW scan found no Shield Other candidate.
- Proved the finalized-damage/pre-HP seam documented in
  `planning/SHIELD-OTHER-INVENTORY.md`.

Next: complete native donor/list inventory and implement the schema-2 module
slice with focused tests.

## 2026-08-09 - feature module schema 2 source-qualified

- Added independent default-enabled `shield-other` active/pending setting and
  publication gate; UI continues to label active versus next-restart state.
- Schema 1 migrates atomically to schema 2 with Shield Other enabled. Missing
  fields default enabled and malformed settings retain the established
  quarantine/fail-open policy.
- Expanded deterministic coverage from four to all eight module combinations.
- Complete domain/reflection suite PASS 970/970. The first sandboxed run failed
  only the existing audio temp-file `File.Replace` ACL boundary; the identical
  permitted run passed.
- Full `Build-Local.ps1` gate PASS: repository validation, clean exact-reference
  Release build, build/icon/SoundBank audits, deterministic packaging, and
  strict package validation. Candidate local-runtime package SHA-256
  `0133b6dc193a67252fb5b0a1ff2943446447fc3d6f24408932c7c3013f1eef5a`;
  DLL SHA-256
  `13d0cc8669f785de235b1cbff3591d780d78a122194978b2ade0d3524da4cc28`.

Next: complete native donor/list inventory, then add pure Shield Other damage
and link policies with focused tests.

## 2026-08-10 - pure damage and link policy source-qualified

- Added dedicated pure policies under `Spells/ShieldOther`.
- Finalized HP damage uses `subject=floor(D/2)` and
  `caster=D-subject`; 0/1/2/3 and 1,000,001 conserve exactly and assign every
  odd remainder to the caster.
- Invalid links and guarded transferred events preserve the original target
  damage and transfer zero, preventing reciprocal/nested recursion.
- Link validity fails closed for missing subject/caster, dead caster,
  different area, and distance beyond native close-range scaling
  `25 + 5 * floor(casterLevel/2)` feet; the exact boundary remains valid.
- Complete deterministic suite PASS 974/974. Full exact-reference
  `Build-Local.ps1` qualification PASS; package SHA-256
  `b8168b7365a96bd01e976d84484fce4855df6a4ca9f8ef8cd52480985de05172`;
  DLL SHA-256
  `1d792ccccb013bda15556d592c2a3fbc12a691c5fc9dc2a3f0b33bd0cfe777d9`.

Next: publish this policy checkpoint, then add and run the guarded native
donor/spell-list/final-live duplicate observer before blueprint construction.

## 2026-08-10 - guarded inventory observer source-qualified

- Added save-free `observe-shield-other-inventory`, allowlisted consistently in
  C# and PowerShell, with a dedicated observer outside the runtime orchestrator.
- The observer scans the final live blueprint library for foreign Shield Other
  candidates, records GUID/name/display/type/assembly, inventories close-range
  allied spell donors and relevant buff components, lists candidate domain
  lists, and resolves Cleric/Paladin/Inquisitor/Oracle/Warpriest/Psychic class,
  spellbook, and live spell-list identities.
- Isolated runtime preflight PASS 86/86. A preceding combined command hit only
  the preflight artifact-fingerprint race while prior build outputs settled;
  the unchanged isolated rerun passed.
- Complete deterministic suite PASS 974/974 and exact-reference Release/package
  qualification PASS. Candidate package SHA-256
  `8861e924071ca2c7c9d42b5d259ba8d7ebcfd5f4dcb6e3e5838a84366a6cffdb`;
  DLL SHA-256
  `de5724f39ac198b441f628178885bd21435c28a1f6f791a5aafc83d26116d192`.

Next: commit/publish this observer, rebuild from the clean SHA, then run it
through the guarded Steam App ID 640820 launcher and curate its exact evidence.

## 2026-08-10 - final live duplicate/donor/list inventory PASS

- Rebuilt clean published `ef8b80f`; package SHA-256
  `0bc0f4f5eb67041886aada03bc55810e340181123fe58e9acbd5fc68b9903f09`,
  DLL SHA-256
  `58dc8a211cde4c0d4fb0467ceb6b44aacad24b07041b750cc6f84803af5be68d`.
- Guarded final-live inventory PASS:
  `20260810T0408000666226Z-observe-shield-other-inventory`.
- Zero duplicate candidates among 104,644 loaded blueprints. Required five
  base lists and unambiguous CotW Oracle/Warpriest/Psychic live chains are now
  recorded in `planning/SHIELD-OTHER-INVENTORY.md`.
- No save name, save load, input, or save mutation was used.

Next: assign the two necessary stable identities, construct presentation-only
donor clones with exact mechanics, and implement transactional level-2 base
publication plus pure publication tests.

## 2026-08-10 - stable ability and target-buff identities

- Assigned append-only symbols/GUIDs: ability
  `6a8c4c1d2fbe4d6a9a724988c1348401` and target buff
  `7bd92e3c44ad42e7b523ee8ed7afc602`.
- Registration is unconditional across module settings; active registry count
  is 254 and ledger count is 255 including the historical reservation.
- The factory clones native Shield of Faith presentation, then replaces its
  mechanics with an explicit Abjuration spell, harmless ally-only close-range
  targeting, caster-level hours, Extend-enabled native duration, and exact +1
  deflection AC/+1 resistance all-saves components. Material focus is documented
  and abstracted; no inventory component is created.
- Complete validation, 975 deterministic tests, exact-reference Release build,
  strict package validation, and local-runtime packaging PASS.

Next: commit/publish this identity checkpoint, then implement pure-tested
transactional level-2 base-list publication and rollback.

## 2026-08-10 - transactional base spell-list publication

- Added a dedicated five-list level-2 transaction for Cleric, Paladin,
  Inquisitor, Community domain, and Protection domain.
- Publication preserves native/foreign order, singularizes Shield Other by
  reference and GUID, recognizes aliased physical level lists, clears the exact
  `m_SpellsFiltered` cache, and is idempotent.
- Rollback retains original list references and refuses if a later foreign
  replacement makes restoration unsafe. Partial publication rolls back as one
  unit. Publication failure is isolated so registered identities and the
  Gunslinger/Acadamae modules remain available.
- Complete validation, 977 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: publish this checkpoint, then implement first-idle optional CotW discovery
and idempotent Oracle/Warpriest/Psychic reconciliation.

## 2026-08-10 - first-idle optional CotW reconciliation

- Added a dedicated first-idle final-live scan after all LoadDictionary postfixes.
- CotW absence is normal. Present Oracle, Warpriest, and Psychic classes must be
  unambiguous by internal/display identity, class-owned spellbook/list structure,
  maximum spell level, spontaneous/prepared model, divine/arcane flag, and
  casting attribute; known GUIDs are supporting signals rather than sole signals.
- Optional level-2 publication is transactional, preserves foreign entries,
  clears native caches, and immediately performs a second idempotent pass.
- A final-live foreign Shield Other candidate prevents optional publication and
  rolls back the retained base transaction; unsafe rollback fails closed.
- Complete validation, 978 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: commit/publish this checkpoint, add a guarded live blueprint/publication
observer, and verify the exact CotW casting-model and all eight list memberships.

## 2026-08-10 - publication observer strengthened

- Extended the existing guarded, save-free inventory observer to assert exactly
  one level-2 Shield Other membership in all five required lists and every live
  optional CotW list.
- Diagnostics now record Oracle/Warpriest/Psychic spontaneous, arcane, and
  casting-attribute fields for exact live validation of the structural resolver.
- Complete validation, 978 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: publish the observer checkpoint, rebuild its clean SHA, then run the
guarded inventory scenario and curate exact membership/model evidence.
