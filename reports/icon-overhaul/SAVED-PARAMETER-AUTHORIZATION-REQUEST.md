# Saved firearm parameter verification — authorization request (rev. 2)

Status: **SAVED_PARAMETER_ROUND_TRIP = NOT RUN — AUTHORIZATION/INPUT REQUIRED.**

Revision 2 (2026-09-18). Revision 1 described the feat set inconsistently
(three Weapon Focus choices and three Rapid Reload children — six feats — while
repeatedly saying five) and proposed a fresh level-1 Gunslinger without a legal
progression route for that set. This revision states the exact parametrized-vs-
static distinction and bases the proposed character on the legal level-up route
established by the Task B fixture (`disposable-firearm-higher-feat-roots`).
Nothing below may run before an explicit owner decision; this document is a
request, not consent.

## What parameter persistence is and is not already proven

- Catalog identity tests, in-memory controller selections and the
  same-artifact `working-save-smoke` do **not** prove a fresh-process disk
  round trip of firearm `FeatureParam` values.
- The permitted `KMG_AUTOMATION_WORKING` archive contains no supported firearm
  parameter GUIDs (Codex read-only inspection, 2026-09-13/14).
- An owner-approved dedicated parameter-bearing save (owner-created or
  owner-authorized creation) is an acceptable alternative to automated
  creation; either route still needs a separate decision recorded below.

## Proposed fixture (one dedicated test save, one character)

| Item | Value |
|---|---|
| Save name | `KMG_AUTOMATION_FIREARM_PARAMS` — to be confirmed unused at execution time (name-existence check immediately before any write; a pre-existing same-name save is never overwritten and stops the request) |
| Character | One disposable mercenary built through the same legal level-up route the Task B fixture qualifies: real class levels, real firearm proficiency, real prerequisite chains; **no** inserted target facts and **no** bypassed eligibility checks |
| Committed facts (5 total, internally consistent) | • **3 parametrized native-root facts**: native Weapon Focus root committed once per firearm parameter — Pistol, Musket, Blunderbuss (each committed fact carries `FeatureParam` → the corresponding `KMG_WeaponFocus_*` feature blueprint).<br>• **1 higher parametrized root fact**: native Greater Weapon Focus with the Pistol parameter (parameter → the `KMG_GreaterWeaponFocus_Pistol` feature), proving a higher root's parameter persists, not only Weapon Focus.<br>• **1 static owned fact**: Rapid Reload (Pistol) — a plain static child feature with **no** parameter, included to distinguish static-fact persistence from parametrized-parameter persistence. |
| Not included | Weapon Specialization / Greater Weapon Specialization / Improved Critical parameters and the other two Rapid Reload children: their persistence paths are equivalent to the ones above (same root mechanics, same serialization path); including them would grow the fixture without a distinct claim. Sharing this coverage rationale is deliberate and final. |

Parametrized roots and static children are different claims and are reported
separately: a PASS for static facts alone would not be reported as parameter
persistence.

## Exact identities to verify before and after the round trip

The five committed facts' root GUIDs and parameter blueprints (resolved from
the live blueprint graph at execution time, not pasted from documentation):
native Weapon Focus root, native Greater Weapon Focus root, and the
`KMG_WeaponFocus_{Pistol,Musket,Blunderbuss}` /
`KMG_GreaterWeaponFocus_Pistol` parameter blueprints, plus the static
`KMG_RapidReload_Pistol` child. The fixture asserts the exact root/parameter
pairs on the in-memory unit before saving and on the loaded unit after the
fresh-process reload.

## Operations, limits and protections

| Aspect | Commitment |
|---|---|
| Exact save path/format | The actual native save location, file set and sidecars are verified and recorded **read-only** before any write phase; this request does not assume a single file or an existing scenario's ability to save |
| Write operations | Exactly one creation of the dedicated save through the ordinary native save path from a narrowly allowlisted guarded scenario (Steam App ID 640820, `-kmgRuntimeTestRequest` only); no campaign access, no other save is selected, loaded, written, renamed or deleted |
| Protected inputs | `KMG_AUTOMATION_BASELINE` — never selected/loaded/written/renamed/deleted; `KMG_AUTOMATION_WORKING` — not overwritten (read-only use only, if used at all); all campaign saves untouched (names may be inventoried; payloads are not read or hashed) |
| Fresh-process reload | One guarded launch loading exactly `KMG_AUTOMATION_FIREARM_PARAMS`; verify the five facts, their root/parameter identities, native P/M/B monogram presentation on the loaded sheet/Total path, and absence of unintended facts |
| Before/after verification | Inventory + SHA-256 of the protected saves captured before the write and re-verified after every phase; both must be byte-identical |
| Cleanup/retention | Deletion of the dedicated save only through the ordinary native delete path and only if the owner's decision explicitly includes deletion; owner creation of the file is not permission to delete it. If deletion is not authorized, the save is retained and reported. Runtime JSON evidence retained under `runtime-evidence/<unique-run>/` |
| Claim scope | Same-version persistence only. Historical migration of older saves is a distinct claim requiring its own fixture and is not made here |

## Explicit decision requested

Authorize exactly one of (a) automated creation of the single dedicated save
as above, including or excluding the deletion step — state which; or (b) an
owner-created dedicated parameter-bearing save (exact name and the five facts
listed above) with an authorized read-only fresh-process load. Any other
expansion (more saves, campaign access, baseline/working-save writes) requires
a new decision. Until a decision is recorded here by the owner, the status at
the top remains in force.
