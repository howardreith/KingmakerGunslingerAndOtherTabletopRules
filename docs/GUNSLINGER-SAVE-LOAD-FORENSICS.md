# Gunslinger Save/Load Forensics

## Evidence safety

- The user's affected save is immutable original evidence.
- Inspect only the exact Kingmaker save directory through existing repository tooling.
- Identify by catalog metadata and timestamps; never guess among ambiguous candidates.
- Record filename, display name, absolute path, size, last-write time, and SHA-256.
- Make a byte-identical ignored transaction-owned copy before archive inspection.
- Never resave, rename in place, delete, migrate, repair destructively, or permit Cloud replacement of the original.
- Never use or overwrite `KMG_AUTOMATION_BASELINE`.

## Evidence ledger

| Field | Result |
|---|---|
| Intake source commit | `e2e3d9ec941549a889a1e03a590e24241b745b7f` |
| Candidate version | `0.0.88` |
| Candidate package SHA-256 | `FAFBAE86F4D890A958435C2D3D87ED6BFABC5504988E709B0960A90BF161F8CA` |
| Candidate DLL SHA-256 | `E54E35145EABD51461E9277C1B1CCD8CF7EEA29BA48CFB156D40ADDC9FA4E1EB` |
| Firearm AssetBundle SHA-256 | `1AA75FA1230ABFB60CD5148CA90B99D604DBECE7D80D98D85CB7D7C0A885A8FF` |
| SoundBank SHA-256 | `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18` |
| Original affected save | pending exact unambiguous identification |
| Original SHA-256 | pending |
| First material exception | pending log correlation |
| Failure boundary | pending descriptor/area/unit/fact/item/scene classification |
| Focused Aim causation | unproven correlation only |

## Reproduction matrix

Guarded disposable cases are: no Mysterious Stranger; Mysterious Stranger
without Focused Aim; armed before shot; immediately after committed Focused Aim;
after kill recovery; after marker expiry; zero Grit; another armed deed; Tenebrous
Depths versus simple base area; two independent users; weapon switch; interrupted
or cancelled shot. Every written fixture must receive a fresh-process load. Two
complete fresh-process PASS sequences are required before release qualification.

## Root-cause audit

Audit Focused Aim marker facts/actions, custom UnitPart/fact components,
`ArmMysteriousStrangerDeed`, resource and attack transaction cleanup, execution
context/duration ownership, static registries, save/load reconciliation hooks,
and all `0.0.88` identity/registration changes. Static dictionaries are not save
data by assumption; follow the first exact exception and serialized record.

## Recovery contract

Preserve accepted damage, Grit spend, and kill recovery. Clear only exact stale
project-owned transient state at an authoritative boundary. Maintain two-unit
isolation and unrelated content. Prefer an idempotent compatibility adapter for
an already serialized project representation. Emit precise diagnostics rather
than swallowing reconstruction exceptions. The original remains byte-identical.
# P0 compatibility repair checkpoint - 2026-08-20

- Affected original: `Quick_6.zks`, display `Quicksave1`, game `Akasa`, game ID `a62c36f7-e9ab-4a97-85e4-f67fc0d6ad01`, area `957af755145b0494587c511e18f1d7c6` (`Area_Dwarf_10`), size 489715, SHA-256 `B4D6D093EABAB2080E8AE4D8A501B56449E0FC8D7850C0527495BA853032655D`.
- The first material failure was reproduced twice: `Buff.SpawnParticleEffect_Patch1` dereferenced a null project buff FX link during `UnitEntityData.SpawnBuffsFxs` and `AttachToViewOnLoad`.
- The save contains two references to Focused Aim marker `ac72998da0a146cf9ca0cff3ea161303`. Its 0.0.88 blueprint factory omitted `FxOnStart`, `FxOnRemove`, and `ResourceAssetIds`, unlike the repository's save-safe buff factories.
- The stable Focused Aim identity now initializes both FX links to empty `PrefabLink` instances and resources to an empty array. No saved fact is deleted and accepted spend, damage, and kill-recovery mechanics are unchanged.
- Focused affected-copy runs `20260820T1245409883715Z-p0-affected-focused-aim-save-load` and `20260820T1250040030957Z-p0-affected-focused-aim-save-load` crossed the former exception boundary and entered Tenebrous scene construction. Neither emitted the native after-load callback; the latter was allowed 600 seconds and was force-terminated by its owner harness.
- Same-area control `Quick_5.zks` has no Focused Aim marker, size 644034, SHA-256 `980821EB2450346F2439EAF912DA00CF52822565C7658D7B9592DD485CF6C921`. Transaction-owned run `20260820T1307384811579Z-p0-affected-focused-aim-save-load` stalled at the same scene/shader boundary. This proves the post-repair completion stall is not specific to serialized Focused Aim state.
- Normal `KMG_AUTOMATION_WORKING` run `20260820T0920432805446Z-working-save-smoke` passed in approximately two minutes on the same machine.
- Every run staged only a disposable copy, removed it in `finally`, and reverified the original affected and control hashes. The original affected save remains byte-identical.
- Source/package qualification: repository validation PASS; dependency-free suite `1161/1161`; clean Release/package/output/SoundBank/strict-package gates PASS. Candidate package SHA-256 `446E7974ECEA9AD9A5201D76D463CECAB33D871FB6017C2D3CAD8E40E59D1E36`; DLL SHA-256 `96BF5C5511DF72E010CFE001350834279335FBDA50D2458C284BA81A339D776F`.
- Disposition: root cause and bounded compatibility repair are source-qualified. Full affected-save recovery and two consecutive fresh loads remain release-blocking because the guarded Tenebrous scene never reaches the authoritative callback, including for the no-marker control.
