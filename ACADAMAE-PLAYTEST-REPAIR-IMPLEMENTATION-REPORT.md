# Acadamae Playtest Repair Implementation Report

Status: COMPLETE AND LOCALLY DEPLOYED

## 2026-08-20 authoritative correction

Fresh human evidence superseded the historical completion claim below. The ordinary selected Summon Monster I variant can carry a null outer `ParamSpellSlot` while retaining the exact prepared slot on its `ConvertedFrom` root. That made the integrated eligibility path reject `not-prepared`, so the command remained full-round and no completion save occurred. Production now resolves only an available same-spellbook slot whose exact spell object occurs in the converted chain and binds it during authoritative command construction. Two fresh native-command guarded runs passed 15/15; final visible UI confirmation remains human-gated.

Starting source was `c80615e917d1994daad679e8a78af11ae2c7e115`, version 0.0.75. The focused repair targets 0.0.76 / `0.0.76-acadamae-mode-fatigue-icon-repair` without changing any prior blueprint identity or accepted feature-module behavior.

## Per-character mode

The existing Acadamae feat now grants `Use Acadamae Graduate` through restoring
native `AddFacts`. The immediate persistent activatable defaults off and owns a
hidden, no-FX marker. Exact feat plus marker ownership gates both action
presentation and command construction. An accelerated command snapshots its
save obligation; later toggle changes do not rewrite it.

New stable identities:

- `KMG.Feats.AcadamaeGraduateModeMarker` - `b5fc52ec666640318f8921d5fa60ec39`
- `KMG.Feats.UseAcadamaeGraduate` - `a780ab99b76849ed825729808e2bbf29`

The active registry is 252 in all module configurations; the ledger contains
253 entries including one historical reservation.

## Ordinary fatigue

The rejected 0.0.75 call cloned the summoning spell context. Version 0.0.76
uses `AddBuff(Fatigued, caster, null)` to create a root, caster-owned native
context, then calls public native `Buff.MakePermanent()`. Exact installed IL
shows that method clears the nullable end time and refreshes the collection's
event schedule. The canonical blueprint remains `RemoveOnRest`, serialized,
visible, and intercepted by the existing Cord `RuleApplyBuff` boundary.

Guarded runtime proved the buff is permanent, root-context, distinct from the
spell context, survives disposal/collection of that context, and is removed by
actual `RestController.ApplyRest`. With the Cord equipped, the application is
suppressed before a buff is returned, so `MakePermanent()` is not invoked and
exactly one accepted d6 substitution remains.

## Cord art

The Cord now uses the project sprite `KMG_Icon_cord-of-stubborn-resolve`, not
the Belt of Constitution +2 donor sprite. The original 1254x1254 chroma source
was produced with OpenAI image generation and deterministically exported to a
128x128 transparent PNG. Source SHA-256 is
`d7e5dfa7228419df65e3bfa88aafa7b94caa1e5cfadfb1a159686805042655c8`;
production SHA-256 is
`cf3f040eb22691b1e526eb32cc31d1151eafef7113cb0ebe55d0c2637d5d9928`.
No native or third-party pixels were copied.

## Qualification summary

Repository validation, 970/970 deterministic tests, clean exact-reference
Release compilation, project-asset/SoundBank audits, deterministic packaging,
and strict 44-file package validation pass. Standalone mechanics/Cord, all four
module combinations, exact Call of the Wild, Arms & Armor, Toggle Custom
Soundpacks, qualified combined, and two consecutive high-risk combined runs
pass with exact transaction restoration. Two guarded
`KMG_AUTOMATION_WORKING` smoke runs pass.

Final release-code commit `eab12bdbef962398fa7ab9d6fb6b7eace67bea76`
passed two consecutive standalone and two consecutive high-risk combined runs.
The qualified package was deployed through `Deploy-Local.ps1`; backup
`20260810T0141375127164Z` was retained and deployment manifest
`20260810T0141392337217Z` verified version, DLL, icon, and settings preservation.
