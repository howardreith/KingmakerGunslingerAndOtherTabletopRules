# Sprint 34 — RELEASED v0.0.172; stop before Sprint 35

[Stable release](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.172) · [Product PR #84](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/84) · [Public qualification manifest](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/download/v0.0.172/release-manifest.json)

Sprint 34 is complete. Bearded Devil (Barbazu) and Kyton (Chain Devil) are
released with ten untemplated Summon Monster V–IX routes. Sprint 33 remains
closed; no Sprint 35, DATA, Mounted Combat or Buff Planner work was included.

- Tested/promoted product source: `40ec5f5d758a400d6dd09d2d6f0e78bde9ef242d`
- Product merge: `b7317f37a3f61e9f5b87c86cc1a9ec4e31071146`
- Annotated tag object/target: `af9ee15b191f35fd30ec76c9231a01cfed57c603` → product merge
- Reviewed base preserved in merge ancestry: `4b7e3d914883ac695a4146c03de790b2024ea38c`
- Public ZIP SHA-256: `ef82d2028c636a00eaebf2536fe6123921d1551716ea4a6cd9d6ffa75ac47d86`
- DLL SHA-256: `435695f7e56ad814ab1ab130737745e79ec4dc64d67fbdc6ed6f3d5d511a7e74`
- DLL MVID: `3e67ea42-cacf-4e26-96d8-dc80781dcd0d`
- Source fingerprint: `3e0407b1c53ecb5ec1d5e782a2f5c1e43891d2368e2c4142ae2d622213810bb8`

The release was promoted from the exact qualified artifact; publication did not
rebuild it. The actual public ZIP, manifest and checksum were downloaded from
GitHub and independently verified. The release is stable, non-draft,
non-prerelease and Latest. Strict standalone package validation passed on the
downloaded ZIP; embedded Info/DLL/MVID and both icons plus both original
mesh/albedo pairs are exact.

## Delivered rules and scope

| Creature | Delivered behavior |
|---|---|
| Bearded Devil | Alternative manufactured glaive and two-claw modes; weapon-specific reach; lawful single/full/charge/AoO execution; close natural fallback after disarm; conditional same-owner/same-target two-claw Beard damage and Devil Chills; damaging-glaive Infernal Wound with two bleed, native spell-healing CL 16 checks, independent sources and summon-safe cleanup. |
| Kyton | Four native chain attacks with 10-foot ordinary reach; typed chain armor, cold immunity and bypass-aware regeneration; source-owned Dancing Chains; targeted Unnerving Gaze with native fear/mind-affecting and sight handling; no copied devil fire/poison immunity. |

Both creatures ship distinct original rigged bodies, real painted medallion
icons, independent nontransferable/no-loot equipment, native movement/attack
contacts, private-resource teardown and module-OFF-safe saved identities.

The public routes for each creature are SM V one, SM VI 1d3 and SM VII–IX
1d4+1. No Nature's Ally, celestial/fiendish template children, extra alignment
variants or nested summons were added. Final source-derived totals are:

| Surface | Final count |
|---|---:|
| Unique project creatures | 126 |
| Monster entries / placements | 104 / 582 |
| Nature's Ally entries / placements | 105 / 588 |
| Generated placements | 1,170 |
| Retained wrappers | 29 |
| Visible choices | 1,199 |

Released keys, GUIDs, wrappers and existing identity bindings remain stable;
the two creature identities and their necessary dependencies were append-only.

## Explicit engine adaptations

- Dancing Chains is a standard action controlling only that Kyton's four
  existing, nontransferable chains. Activation makes no attacks. Later native
  single/full attack commands use the same chains with normal action costs and
  up to 15 feet of additional weapon reach. It does not simulate environmental
  chains, create autonomous units, grant a second full attack or touch player
  inventory. Control ends at its source/lifetime boundary.
- Unnerving Gaze is a targeted standard action at 30 feet, Will DC 15 with live
  Charisma scaling, shaken for 1d3 rounds. Players may target eligible allies;
  AI selects enemies. This replaces the passive tabletop gaze/avert-eyes cycle.
- Kingmaker has no Heal skill. By owner direction, no substitute skill action
  was added for printed Heal DC 17 treatment. Infernal Wound retains its real
  spell-healing caster-level DC 16 check.

The complete print/engine/adaptation contract remains in
[SPRINT34-RULES.md](SPRINT34-RULES.md). Unsupported named skill references and
passive/world senses were not replaced with broad bonuses or a new subsystem.

## Qualification and evidence attribution

The complete source/build/package boundary passed **2,767/2,767** tests in
437.6 seconds. Supported exact-reference reproducibility produced the same ZIP
and DLL hashes while preserving the primary artifact and receipt.

| Final coverage | Verdict | Exact execution |
|---|---|---|
| Distinct bodies, icons, movement and weapon/chain contacts | PASS 32/32 | `20261030T1357434810894Z-767e19c9d2d44aa3bdd4e57c2f6b8468` |
| All ten public casts, quantities, slots, duration and cleanup | PASS 15/15 | `20261030T1406236907894Z-61e2d6055199420081aba9fec32cc521` |
| Highest-risk healing/chain/bleed integration | PASS 30/30 | `20261030T1413179448795Z-e905906e3b4e4c7d9c224acbdacbe25b` |
| Mixed ON persistence: prepare → cleanup/save → fresh absence | PASS, three fresh processes | `20261030T0710120427378Z-bcc997a726e54596b4c5708a95d3f0ce`; `20261030T0717589688397Z-b7b21cbfd6b04a10a78c607acd330181`; `20261030T0725329559964Z-25dedf29a7db4979be09299119ebf921` |
| Mixed OFF persistence: prepare → module-OFF cleanup/save → fresh ON absence | PASS, three fresh processes | `20261030T1334268769873Z-bde0f7c13b564ecea898d6e3137666b1`; `20261030T1342073674849Z-c716a10ea19f41ddb705a62786cfaa00`; `20261030T1349489427492Z-b7057940283d46ac89b03d8de6207e48` |

The ON absence native result was already complete PASS. Its post-result
collector raced an atomic settings-file replacement, so the original collector
ERROR remains retained. A reader-only installed-stack record binds the unchanged
raw result, orchestration, loaded identity, transaction and original error; it
does not rewrite the original record. ON qualified-sequence SHA-256 is
`42c856cf6f20ab7dfd57575903e733b835706a4790d21f4adfbcb39f30c0413b`.
OFF sequence SHA-256 is
`5759dfab3d21a562bb0195a06514f75834f38dc969301c7261fb469b962b38fd`.

Four mechanics groups are explicitly **REUSED_PASS**, not fresh final-byte
executions: profiles/defenses/isolation; Barbazu combat/effects; Kyton
actions/defenses; and AI/actions/cleanup. Their original development sources,
artifacts, profiles, RunIds, raw verdicts and dependency comparisons remain
exact in the public manifest and local qualification ledger. The final artifact
separately supplied fresh bodies/routes, highest-risk healing/chain behavior,
six persistence processes, complete build/package and reproducibility evidence.
Historical failures remain failures and historical NOT_RUN work remains NOT_RUN.

## Art and deferred polish

Source/provenance for both paintings and original bodies remains under the
Sprint 34 original-icons/original-models trees. Raw private captures and game
assets remain outside Git. Native proof showed the intended bodies attached;
donor fallback was not accepted as model completion.

Deferred, non-blocking cosmetics: rough texture seams, simple beard/chain
motion, minor clipping and more elaborate chain choreography. Physics-rope or
general ambient-chain simulation was not selected. Technical acceptance does
not claim personal owner aesthetic approval.

## Exact restoration and stop

After publication, the full intake comparison passed: 967 mod entries, 118 save
entries, 20 private-reference entries, 83 game-setting values, unchanged sound
bank, tasks/processes/leases and no compatibility lock. The user's original mod
installation remains in place; v0.0.172 was not permanently deployed.

Original `Manual_299_KMG_AUTOMATION_WORKING.zks` is restored exactly:

- SHA-256 `650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`
- 8,550,225 bytes
- creation UTC ticks `639208636210035057`
- write UTC ticks `639278868712797783`
- attributes `8224`

`KMG_AUTOMATION_BASELINE` and all other saves remained immutable. Verified
redundant scratch, development packages and public downloads are preserved on
`E:` with hash maps; primary qualified artifacts, unique evidence, references
and original save backups were not deleted. No game process or runtime lease is
active. Sprint 34 stops here; do not infer authority to begin Sprint 35.
