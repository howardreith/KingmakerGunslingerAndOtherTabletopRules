# Saved firearm parameter verification — authorization request (rev. 3)

Status: **SAVED_PARAMETER_ROUND_TRIP = NOT RUN — AUTHORIZATION/INPUT REQUIRED.**

Revision 3 (2026-09-18). Revision 2 still stated that Greater Weapon Focus
(Pistol) would carry a `KMG_GreaterWeaponFocus_Pistol` parameter. The
higher-feat qualification established the opposite: all five integrated native
roots (Weapon Focus and the four higher roots) commit the **same registered
Weapon Focus choice of the weapon kind** as their `FeatureParam`
(`FIREARM-HIGHER-FEAT-ROOTS-QUALIFICATION.json`, selection logs; observed rows
now emit the actual committed parameter explicitly). This revision makes the
fact table consistent with the inspected contract. Nothing below may run
before an explicit owner decision; this document is a request, not consent.

## Exact expected identities (resolved from the reviewed catalog and the
qualified native selection results — not inferred from a post-load result)

| # | Committed fact (root) | Root GUID | FeatureParam (the actual saved parameter) | Kind of fact |
|---|---|---|---|---|
| 1 | Weapon Focus (Pistol) | `1e1f627d26ad36f43bbd26cc2bf8ac7e` (native parametrized root) | shared Weapon Focus **Pistol** parameter choice (`KMG_WeaponFocus_Pistol` feature blueprint) | parametrized native-root fact |
| 2 | Weapon Focus (Musket) | same root | shared Weapon Focus **Musket** parameter choice | parametrized native-root fact |
| 3 | Weapon Focus (Blunderbuss) | same root | shared Weapon Focus **Blunderbuss** parameter choice | parametrized native-root fact |
| 4 | Greater Weapon Focus (Pistol) | `09c9e82965fb4334b984a1e9df3bd088` | **the same shared Weapon Focus Pistol parameter — NOT the `KMG_GreaterWeaponFocus_Pistol` wrapper** (that wrapper is a menu/identity feature, never the committed parameter) | parametrized native-root fact |
| 5 | Rapid Reload (Pistol) | owned selection parent | the static owned child fact `KMG_RapidReload_Pistol` itself — **no parameter is involved or invented** | static owned fact |

Five facts, internally consistent. Rows 1–4 exercise the parametrized-root
serialization path (four entries that share one serialization route but are
four distinct gameplay feats — sharing the route does not make their mechanics
identical); row 5 exercises the static-fact path and distinguishes it. This is
a representative same-version persistence proposal: it does not establish disk
persistence for the other untested root/weapon combinations (equivalent
serialization paths), nor any historical migration claim.

## Proposed character and legal progression

One disposable character built by the exact progression the qualified Task B
scenario performs: real class levels via real `LevelUpController` visits with
native prerequisite enforcement — Weapon Focus per kind first (BAB 1 +
firearm proficiency), then Greater Weapon Focus (Pistol) at fighter level 8+
(fighter-8 class prerequisite + Weapon Focus parameter prerequisite), and
Rapid Reload (Pistol) from the published owned selection. No target fact is
inserted directly and no eligibility check is bypassed; the character ends as
a real, committed level-up product, not a detached test object.

## Native save-persisted container (to be verified read-only before any write)

The unit must be present in a native save-persisted container (e.g. a
completed mercenary/party member) rather than a detached fixture. The exact
native save format, file set and location for this installation will be
inspected **read-only** and recorded before any write is requested; this
document does not assume a single file, a sidecar layout, or an existing
scenario's ability to save. The expected identities above are fixed before
the load and compared against the loaded result; expected values are never
derived from what the load returns.

## Operations, limits and protections (unchanged in substance)

- One demonstrably unused dedicated test-save name (existence-checked
  immediately before any write; a pre-existing same-name save stops the
  request and is never overwritten).
- Exactly one creation through the ordinary native save path from a narrowly
  allowlisted guarded scenario (Steam App ID 640820, `-kmgRuntimeTestRequest`
  only); one fresh-process guarded load of exactly that save; no campaign
  access; no other save selected/loaded/written/renamed/deleted.
- Protected: `KMG_AUTOMATION_BASELINE` (never selected/loaded/written/
  renamed/deleted), `KMG_AUTOMATION_WORKING` (not overwritten; read-only use
  only if used at all), all campaign saves (names may be inventoried;
  payloads never read, copied or hashed).
- Before/after: inventory + SHA-256 of the protected saves captured before
  the write and re-verified after every phase; both byte-identical.
- Retention: the dedicated save is retained unless deletion is separately
  authorized; owner creation of a file is not permission to delete it.
- Claim scope: same-version persistence only.

## Explicit decision requested

Authorize exactly one of (a) automated creation of the single dedicated save
as above, including or excluding the deletion step — state which; or (b) an
owner-created dedicated save containing exactly the five facts above (exact
name to be agreed) with an authorized read-only fresh-process load. Any other
expansion requires a new decision. Until a decision is recorded here by the
owner, the status at the top remains in force.
