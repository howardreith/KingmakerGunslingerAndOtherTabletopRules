# Sprint 35 — RELEASED v0.0.173; stop before Sprint 36

[Stable release](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.173) ·
[Product PR #86](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/86) ·
[Public manifest](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/download/v0.0.173/release-manifest.json)

Sprint 35 is complete. Barbed Devil (Hamatula) and Bone Devil (Osyluth) are
released through five untemplated Summon Monster routes. Sprint 34 remains
closed; no Nature's Ally entries, Ice Devil, Sprint 36, DATA, Mounted Combat,
Buff Planner, permanent deployment or historical replay was included.

- Tested/promoted source: `e42ae90092dc3f929ffcbda13ebd001281a839dd`
- Source fingerprint: `f387eee79849a012eb3bcd9f1ebadfb7f3fb15f0d4fb598ef9c4d48e7400799a`
- Product merge: `40296fd8db4b47109ed0f81e4705c4b37ceab0ab`
- Reviewed base preserved as the first merge parent: `aee691127b9ade4bbfde55247d54652acf85957c`
- Annotated tag object/target: `2046af0172eead15ae42b6948ff46265fe0e257b` → product merge
- Public ZIP SHA-256: `c5a3f7b852fdde85bb59f2e25c6d22cbb2cacaaa2dbcb0f7fa85710203db2a99`
- DLL SHA-256: `4ebfbd254fe6943c794e37e4965dff02472c93046f357ce920e2c720bb4e2f85`
- DLL MVID: `e52bb61e-22e9-4e32-99a0-5ffdd6ad0654`
- Public manifest SHA-256: `d343a0bfb66c91f9b4115d1d7315d2b39d88d9264445a1e53cede4f78c0c7d89`
- Public checksums SHA-256: `55899912cc381cf243f5f5e1ae8f8f0add1713a14e13356cf6b428dcadcc00df`

The release is stable, non-draft, non-prerelease and Latest. Its actual ZIP,
manifest and checksum were downloaded from GitHub. The 440-member public ZIP
has Info version 0.0.173, one expected DLL, the hashes/MVID above, and passes
strict standalone package validation. Publication rebuilt nothing.

## Delivered scope

| Creature | Routes | Delivered behavior |
|---|---|---|
| Barbed Devil | SM VIII one; SM IX 1d3 | Medium two-claw profile; DR 10/good, SR 22 and printed defenses; ordinary Power Attack/Cleave/critical handling; independent incoming Barbed Defense with the actual reach-weapon exception; wounding-claw fear; Medium-or-smaller grab; exclusive successful-maintain Impale; exact-owner anti-recursion; Hold Person; exactly two Scorching Ray rays; one-use Unholy Blight with native alignment/SR/save effects. |
| Bone Devil | SM VII one; SM VIII 1d3; SM IX 1d4+1 | Large 10-foot-reach bite/claw/claw/sting routine without imported secondary penalties; DR 10/good, SR 20 and printed defenses; wounding-sting finite Strength poison; real 5-foot fear aura; distinct ordinary and three-use quickened self-invisibility with attack break/action/resource persistence; Airborne ordinary-map movement; bounded destructible Wall of Ice control. |

Both creatures have distinct original rigged bodies, real painted medallion
icons, native movement/selection/contact observations, exact private-resource
ownership and disabled-module-safe saved identities. Bone's body includes its
own visible six-segment stinging tail; Barbed's broad body carries twenty
authored rigid thorns. Sprint 34 bodies and icons were not modified.

Final source-derived totals:

| Surface | Count |
|---|---:|
| Unique project creatures | 128 |
| Monster entries / placements | 106 / 587 |
| Nature's Ally entries / placements | 105 / 588 |
| Generated placements | 1,175 |
| Retained wrappers | 29 |
| Visible choices | 1,204 |

Released identities remain append-only. Only the Barbed/Bone suppression keys
were removed for the final public candidate.

## Explicit adaptations and limitations

- Wall of Ice is the owner-approved wall-only adapter: each cast creates one
  grounded visible 10-foot planar section. It owns temporary GridGraph node
  claims, 36 HP, native object-damage halving except full fire damage, breach
  cold for the remaining duration, and ten-minute native fog after lethal fire.
  Breach, expiry, source removal, disable and teardown restore exact navigation.
- It does not implement player-drawn multi-section planes, a hemisphere,
  Strength DC 27 breaking, or line-of-effect blocking. Movement obstruction and
  targeting remain distinct. It is not a general terrain/NavMesh framework.
- Unsupported Barbed Major Image, Produce Flame, Pyrotechnics and Order's
  Wrath, and Bone Major Image and Dimensional Anchor, are omitted with no
  substitute. Summoning and planar/self travel remain precisely suppressed.
- Bone flight uses the established Airborne effective movement representation;
  separate land/fly mode selection and vertical navigation are not claimed.
- Poison and applied Strength damage retain their independent native lifetime;
  source disappearance is not a cure. Active summon-owned walls, aura, grants,
  grapple links and private views have exact owners and idempotent cleanup.
- Owner aesthetic review was not performed and is nonblocking. Simplified
  wings, coarse textures, imperfect seams and nonessential choreography remain
  cosmetic backlog rather than mechanical claims.

The full print/engine/adaptation contract remains in
[SPRINT35-RULES.md](SPRINT35-RULES.md).

## Qualification and attribution

The complete source/build/package boundary passed **2,776/2,776** tests.
Supported exact-reference reproducibility produced the identical ZIP, DLL and
MVID while preserving the primary artifact.

| Mandatory row | Verdict | Exact execution |
|---|---|---|
| Early hidden bootstrap, bodies/tail and Wall obstruction/removal | PASS, observer-only reuse | `20261031T1935352924893Z-0a7d61cc571942c18a8bd58b2defaf8f` |
| Barbed supported SLA outcomes | PASS, observer-only reuse | `20261031T1942328036808Z-9642fe13043e437f88d7f7a4ead98eed` |
| Barbed contact/fear/grapple/impale/two-ray combat | PASS, observer-only reuse | `20261031T1955599337554Z-731a4bd5db0f4d34a81d4687f4ea7248` |
| Bone poison/aura/invisibility/ownership | PASS, observer-only reuse | `20261031T2003119169625Z-47dcd89853e74c3fa7689595853ec656` |
| Distinct presentation and all five public routes | PASS, observer-only reuse | `20261031T2011229339963Z-66eecd2a460e4f51b8ae8d2c853a49fd` |
| Mixed ON persistence, three fresh processes | PASS | `20261101T0350052664096Z-9d61aae86c704ef3901857155b8a9d4d`; `20261101T0356580095915Z-3ab723deefc342b59dea18a9a20e1baf`; `20261101T0403564697020Z-112b147a03ad402b81388c851cebd280` |
| Mixed OFF persistence, three fresh processes | PASS | `20261101T0410289054919Z-b1aaface274141d1add389a401702d23`; `20261101T0417180789851Z-f74ea5ba034a4add93784a3d8f64d21d`; `20261101T0424091142959Z-e9c38cb2b29840c49df24336b736b7a4` |
| Highest-risk installed-stack integration and unchanged S32 neighbor | PASS | ON prepare execution above |

The five reused rows remain attributed to development source
`f785c3035662c7baeae5e0c5f82e85973e03e489`, ZIP
`83b8432b04257615d79c66489ac162e4860ffbcdf50db86aaa7a0162fb953b53`,
DLL `3c1bd7a69b65b6b7fe0f6f2e9de83f6db2a7201a5b8a3f615eb000ae7efd6bc1`
and MVID `e50d50e9-9ada-446f-8f6a-28699fa1735a`. Reuse is limited to
observer-only final changes in the resource-receipt and persistence observers;
the final source separately passed both persistence envelopes, installed-stack
integration, complete build/package and reproducibility. No old verdict was
rewritten; normalized execution receipts link the immutable raw results,
deployments and restoration records.

## Restoration and stop condition

The post-download audit proves the 968-entry Mods manifest, all 118 protected
saves and metadata, original Working and baseline, 83 settings, 21 reference
entries and sound bank exact to the captured intake. Kingmaker is exited;
Steam remains running; no Sprint 35 lock, active journal, task or live staging
remains. Fifteen stale Active-labelled generic journals all predate the intake
and were preserved rather than destructively reconciled.

The audit retains raw aggregate FAIL receipt SHA-256
`0e6d597c9c6af612cb5f6ee761d9ccf8c75992234d7ccde10dd25683a1fa0111`.
Every raw admission fact was PASS, but its aggregate compared the outer
one-element Working/baseline array `Length` (1) to file length. A separate
reader-only correction, SHA-256
`13a834234d55f6be64119f2f1f2dec4df43ce416e42a4ac79d46ece38aaf8b28`,
retains that verdict and records PASS from the immutable facts. Before the
final fact set, the two launch-owned Unity session values and sound-bank
creation timestamp were restored exactly; restoration receipt is retained.

Sprint 35 is **RELEASED / STOP**. Do not start Sprint 36 from this record.
