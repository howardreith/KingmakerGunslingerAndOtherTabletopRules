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
