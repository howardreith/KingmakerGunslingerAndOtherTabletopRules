# Favored Class Integration - Implementation Report

## COMPLETE LOCALLY - AWAITING OWNER REVIEW

This completion round closed every gap the previous candidate left open.
The final candidate `12651613c` passed all 40 guarded native runs of
its qualification (section 4), and the owner's install was restored and
verified byte for byte afterwards (section 8). Every charter scenario family
is now observed natively (sections 3 and 13): no family is partial or
blocked, and QUALIFIED waits only for the owner's review of this local
candidate.

- The nine partly observed families are native: E10 (permission bounds on
  real level-1 visits), E15 (a stored level plan applied by the native
  constructor, with the scope defect D5 found and fixed), M07 (the full
  misfire matrix), M10, M12, M13 and L07 (real commands in real-time and in a
  fresh turn-based combat), M14 (production bombs thrown natively) and M24
  (every published revelation's own values and every bloodline power).
- The fourteen scheduled rows without a persistence case of their own (G05,
  G06 Dodge, G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04, S06) are
  SAVE TESTED in the fresh-process transaction.
- The four host and dependency states were staged with the owner's
  authorization: a real disabled host through a byte-exact Params.xml stage
  (H02), simulated host defects on cloned live observations (H02 unsupported
  and partial, H04, H05), and a missing-dependency fixture save read in the
  disabled-host profile (L06), for which KMG now shows a precise recovery
  warning.
- The fifteen deferred selected-power targets are published (fourteen
  revelations and Elemental Resistance for both bloodlines) with appended
  identities.
- D1 and D2 are fixed by the tabletop rules on the owner's decision; D5 (the
  stored-plan scope) and an M10 Pistol-Whip leak into the trip's CMB under
  the owner's mod stack were found and fixed; D6 (Pistol-Whip ignores the
  firearm's enhancement bonus, outside the favored-class rows) is recorded
  for the owner (section 9).
- B3 was re-measured: the memory that exhausted a degraded machine before
  its reboot comes from the lanes that drive the native character-build
  screens. Neither the screenshots nor the favored-class integration cause
  it, so it stays an environment risk for qualification batches (section 9).

The branch is pushed to PR #24; nothing was merged, tagged or published. No
CI runs for this branch; every result here is the local guarded evidence
named in section 4.

## 1. Identity

| Item | Value |
| --- | --- |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\favored-class-integration` |
| Branch | `claude/favored-class-integration` (pushed to PR #24 on the owner's instruction; not merged) |
| Baseline | `996105ed9e72259a220be56e2a74e0cc18e5ef47` (master, 0.0.138 release record) |
| Final source commit | `12651613c6bbf8533884e7f73cf1b5dd0a5eaf39` |
| Evidence and records commit | the commit that adds this report (on top of the source commit) |
| Package | `artifacts/local-runtime/0.0.139/KingmakerGunslinger-0.0.139-local-runtime.zip`, SHA-256 `95d57d55c31c829ae4f7175b01df310a2081100af4cc207d49b2e9d9d91d8ec1` (not committed) |
| Loaded DLL | `KingmakerGunslinger.dll` 0.0.139, SHA-256 `ee0a67e40ebf5a926ef7df4d84485d3702f3c52e0677ec91b51a00e83bbf9f8f`, MVID `62badef3-342b-409a-9733-20dc6a664578` |
| Deployment | `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260926T0424273426492Z\deployment.json` |
| Deterministic build | two clean Release builds of `12651613c` each ran the 1,846-case domain suite (0 failures) and produced the identical package and DLL; strict standalone UMM package validation PASS |
| Domain suite | 1,846 deterministic cases, all PASS; repository and icon-catalog validation PASS |

Continuation commits (oldest first), after the first pass's records commit
`fee3f45ec` on the same branch (47 commits since the baseline up to the
continuation's candidate `15c37695c`, whose records commit `eed717046`
follows; 57 up to the first review's candidate `aa298650e`, 60 up to the
second review's `f7b4ae4f5` and 65 up to the third review's candidate
`5636d940b` and 80 up to the final source commit `12651613c`):

- `8a46506df72ce79903a1bff94a0968441d9adc3c` Repair Mostly Human identity, power eligibility, thresholds and icons
- `0138d70b65348d0d2f5ddf2755b70723a95d31d1` Add the O01 performance visual observation
- `e972713925a0d1cb2e54a6b41cc029e7e6b47446` Make O01 range, ring and text agree for each owner
- `08ab4d17c2f270e17bf9a6d7d53315d56de2b68e` Add E11 and respec lanes; fix eidolon and Mostly Human donors
- `b2cf0cf815be129cfaf7562a331b2db7a3fbe621` Add the working-save lifecycle lane; fix E11 and respec subjects
- `ff434c829bb5d35a95f858e7be6bcc725450a231` Add persistence families; custom-companion respec; lifecycle waits
- `1b2d4174dfe6f14cc4a5171afe0aa870c93e2ede` Keep the Mostly Human identity out of respec; fix lifecycle lanes
- `94a00352b3f5b31a3931176c713d7c15f5c8a7f2` Add the native visual census and L03 pet and power respec lanes
- `487dd28ef91ccfe64bd6c81b95be8f9969647781` Pin the registration of every favored-class runtime lane
- `aef882103c23894bbdf942d417240818cf32b279` Regenerate the O01 target manifest from the mechanics classification
- `85d305d53b1aac014ad05984759f3fe7dfe1ea14` Cover a cancelled respec of a Ranger with a projected companion (E13)
- `c6ad460298f7fbe892ea74891337e79b8918ee36` Census only the routes the active profile offers
- `7247451534313b65574eaab4a4d2f0d4803f9991` Hold KMG rows to exact icons and record host rows in the census
- `1ebd83fa90147fb1ca2272bc762c5f620d9d46c2` State the census row rule exactly
- `e3617e224881176d854e346812f122f4737ee2d6` Fix the lane mismatches the verification batch exposed
- `bd8f7c60c28ab6d62870e117756f5a77529f372b` Open the census where the screen routes the reward; fix the L03 subjects
- `77afdde6284cfd2a1af7c7acd573c2b41d1c5b9d` Resolve the bloodline as a progression; re-show the census reward
- `675b117b0934c354d892283136c4b17f36ab75f7` Track the rebuilt level-up state; take class choices before the reward
- `b1b93acad6f72705db6077da0c6504fae0ca05a3` Let a counter target what the same level-up gains
- `cacf3a002cf570ab130a1a898abd4d0d62d378d0` Pin the usable-power wiring as an owned target
- `15c37695c267522ce35cbc021168514d6fb058fc` Reload a selected firearm target in a fresh process

Review commits (PR #24), oldest first, after the records commit `eed717046`:

- `ae0f1dafd436b94ee5e63985f03223e82982f904` Gate owned effects on a committed host publication
- `f9a8e6cfbecd1c80d323e29a98c6b5ea61fe7600` Advance owned powers' own thresholds; exclude the possession BAB
- `078978a4c8e53710ef36de084027f324554bb1ba` Keep pet armor on one qualified desired pet
- `da07c4e07bbc2ea47b63310f33131301e2637654` Widen a performance only together with its ring
- `f4f5b9931b693e4429a3324a92d2ff960e73f627` Close the replay and demoralize scopes in finally blocks
- `719085d518d15c0590e38ba8b122be516d70876b` Count every bootstrap registry in the aggregate
- `7bb4a704085b143225a8067e39bd8f6b5263237b` Pin the live aggregate to its catalog-derived expectation
- `27176f424f4d4f6e794669367a26a2d01e8cdff2` Follow Blast's extra uses through Call of the Wild's gates
- `aa298650ecbc7003ce4c4d0ff992984793362462` Wait only for the pet-transition step's own ranger

Second review commits (PR #24), oldest first, after the records commit `323cf21b7`:

- `7f9c781d86e465e9af8e2784a1db8f29454094d5` Show a performance's range only where its area widened
- `f7b4ae4f535b38f0e13ac498ee2334f84ef0f427` Keep excluded and withheld revelation counters inert

Third review commits (PR #24), oldest first, after the records commit `d13a1589e`:

- `4de114511ccba7f2bbd908d13d85984883b3d11e` Widen a bard's areas only as a whole; verify every rollback
- `a17c2efe7eb673195d2e7eb8f6994c99067e7015` Turn off the toggle whose buff runs an ended area
- `51f56820e90ad126dd5127c51da9d099c8bb073a` Spawn the rollback lane's other areas beside the real toggle
- `5636d940b52c26998f2c5d6e36231956e28dff04` Hold the rollback lane to native outcomes; document the attach retry

Completion round commits (oldest first), after the third review's records commit `4b838b2a3`:

- `bb4dfd674f8cf956e289a7bf9b9b37c8283c3624` Fix Dead Shot confirmation, the level-17 Gun Training pick and True Grit Dodge
- `e7748fff4dd134fb5b36faf50e9d7668e72e9624` Widen the misfire and initiative lanes, add row persistence, fix B3 (its screenshot-poll change did not lower the memory peaks; see section 9)
- `dbe42baa05e4ae5a4bd41853dc28cfe1f6bc3579` Check every published revelation's values and the air bloodline powers
- `54d545e4b4cae841bd1d20164d6ad984b2c3dbad` Add the turn-modes and real-bomb lanes, widen Pistol-Whip, fix removals
- `e191d78e1ff074461dafb4d70b9cf5dc4e2ae7f1` Check permission bounds natively (E10) and fix the first lane runs
- `9e927fb0a47eaa4078c16ae11368ca52a957a7cf` Publish the deferred selected-power targets and end the draw animation
- `60c93b319e105bde9761fb5de0e0c86c50b32be3` Scope stored level plans (E15) and fix two lane defects
- `178225027222e3a9fd7035d2e6b86ad565dc88d5` Simulate host defects (H02/H04/H05) and fix the first lane runs
- `313f5b275f4fe4d9d9467afaa08ab5b2ded1f92d` Warn precisely about missing favored-class dependencies (L06)
- `2609cb60d698215c13811ae874449396fc8965e9` Record every base CMB term of the turn-modes trips (did not compile; fixed forward in `21c92d939`)
- `21c92d9395442595eb75e97f0445d194095e874b` Read the base CMB bonus sources by their native members
- `d8fd0ea4a06d347e49c0af032ae2f62c34e490fa` Keep the Pistol-Whip counter inside the deed's own attack (M10)
- `5ae4eafc76fc63f5cad8b6c8b4d5f70ce6a22422` Check the turn-based Dodge over every observation in its round
- `12651613c6bbf8533884e7f73cf1b5dd0a5eaf39` Grant gate-only bloodline powers in the visual census

## 2. Rows, targets and counts

- Catalog: 54 table appearances, 53 distinct options (46 Paizo, 7 Jon Brazer
  Enterprises), 22 canonical effects.
- Scheduled rows: 30. First-party faithful 22 (G01 G02 G04 G05 G06 G07 G08 G10
  G11 G14, I01 I06 I08, O01 O05 O06 O07 O08, S04 S06, U02 U04); adaptations 3
  (I05 I07 O04); optional third-party 5 (G16 G17 G18 G20 G21, profile OFF by
  default). Alias: I04 = G11 (never a second reward).
- Provider absences: G08 (Goblin) and G20 (Orc) are CODE COMPLETE with their
  routes EXPECTED PROVIDER ABSENCE. No playable Goblin or Orc race exists in
  the qualified profile; G08's counter is G02's firearm confirmation and G20's
  is G05's Pistol-Whip counter, both published and natively tested through
  their other routes. Neither route is ever registered as an empty
  (unrestricted) race array.
- Not published: 23 (provider-deferred G09 G12 G13 G22; engineering-deferred
  G03 G15 G19 S03 S08 U06 U07; excluded I02 I03 O02 O03 S01 S02 S05 S07 U01
  U03 U05 U08); none appears as a placeholder.
- Owned identities: 203 favored-class leaves and helpers registered on every
  load (host present or absent), plus 13 Mostly Human identities; identity
  ledger 2,172 (2,170 active, 2 reserved). The bootstrap's registered
  aggregate counts every registry including Mostly Human's: 2,117 registered =
  2,117 expected in the qualified profile.
- Targets (docs/FAVORED-CLASS-TARGET-MANIFEST.md): I06/S04 66 revelation
  targets, 65 published and scoped with every audited family and their own
  level gates, Spirit of the Warrior excluded (its possession sets base attack
  bonus from oracle level, which a counter must never raise); I08/S06 6 bloodline powers (Fire and Air
  Ray, Blast and Resistance) through the fire/air elemental bloodlines
  and their proven Seeker and Crossblooded aliases; O01 16 registered
  performance counters: 14 published (9 Kingmaker, 5 Call of the Wild), 2
  excluded (Storm Call, Mockery) and 14 classified non-targets.

## 3. Charter scenario families (64)

Ladder: CODE COMPLETE, DOMAIN TESTED, NATIVE TESTED (guarded Steam App ID
640820 runs), SAVE TESTED (fresh-process reload). QUALIFIED needs the owner's
review of this local candidate, so it is `no` for every family. `n/a` means
the gate does not apply to that family; `partial` names what was not observed;
BLOCKED means the state cannot be staged under the mission's rules. NATIVE
TESTED (simulated) means the production gates and planner ran in the game on
the live host's own observations, cloned with one fact changed, because the
defective host itself is not staged (owner-authorized for H04 and H05). Status
is the single highest step fully reached.

| ID | Family | Status | CODE COMPLETE | DOMAIN TESTED | NATIVE TESTED | SAVE TESTED | QUALIFIED | Evidence / note |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| H01 | Host DLL physically absent | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | gunslinger-only isolated profile: host absent, KMG class/firearms/races load, no FCB UI |
| H02 | Host installed but disabled, unsupported or partially initialized | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | disabled host: the host-state lane in a byte-exact Params.xml stage (HostDisabled, every owned identity registered, nothing published); unsupported and partially initialized: the host-defects lane on cloned live observations (UnsupportedBinary, HostIncomplete, nothing planned) |
| H03 | Class-before-host / registration-after-host order | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract and host-state: one Gunslinger favored progression and one reward per qualifying level |
| H04 | Gunslinger absent from host map | NATIVE TESTED (simulated) | yes | yes | yes | n/a | no (owner review) | host-defects lane: a host without a Gunslinger entry (not scanned, not offered, wrong shape) blocks exactly the Gunslinger rows, touches no Gunslinger selection and plans every other family as live; the host's global state is unchanged (no second Core.load or rebuild) |
| H05 | Same version string, changed hash/MVID/shape | NATIVE TESTED (simulated) | yes | yes | yes | n/a | no (owner review) | host-defects lane: the same version label with a changed SHA-256, MVID, Core.load body or required member, another version or a changed dependency is UnsupportedBinary with its own reason and cannot plan; the label alone decides nothing |
| H06 | Repeat registration or readiness twice | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract: repeat publication is a no-op; stable GUID sets, order and component counts |
| H07 | Missing optional race or power | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | AbsentProvidersRemoveOnlyTheirRoute; G08/G20 routes absent natively (no playable Goblin/Orc) |
| H08 | Exception midway through publication | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract: injected fault restores the owned arrays exactly; foreign entries unchanged |
| H09 | Preexisting custom JSON rewards | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract/host-state: host JSON rewards preserved; no foreign file writes |
| H10 | Host generic HP/skill and prestige behavior | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | grit/menus: host HP/skill rewards unchanged; no second automatic-HP patch |
| E01 | Each of the 21 source-addressable race identities | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus race matrix; explicit provider absence for Goblin and Orc |
| E02 | Half-elf human/elf access | SAVE TESTED | yes | yes | yes | yes | no (owner review) | menus; respec Half-elf source; L01 |
| E03 | Half-orc human/orc access | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus: human grit and native Pistol-Whip |
| E04 | Aasimar/Tiefling | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus; Mostly Human bridge (Aasimar keeps host policy) |
| E05 | Four geniekin, Mostly Human OFF/ON | SAVE TESTED | yes | yes | yes | yes | no (owner review) | Mostly Human lane; settings profile OFF run; L01 families |
| E06 | All twelve elemental heritages | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | Mostly Human heritages; elemental core/advanced |
| E07 | Mostly Human dual identity | SAVE TESTED | yes | yes | yes | yes | no (owner review) | Mostly Human dual identity and bridge; lifecycle; L01 families |
| E08 | Visual race/body changes only | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | ancestry from race GUID only (RaceIdentitiesAreExactAndComplete); the host bridge stays closed to a human-looking standard geniekin natively |
| E09 | Verified Racial/Planar Heritage provider | OUT OF SCOPE | n/a | n/a | n/a | n/a | no (owner review) | OUT OF SCOPE: no qualified provider installed |
| E10 | Unknown provider, permission cycle, duplicate facts | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | gunslinger-menus permission bounds on real level-1 visits: an unconditional cycle, the depth bound of four, an unverified fact, a duplicated Mostly Human identity and a stray racial fact; Races Unleashed Suli Mostly Human stays closed; UnknownCyclicAndDuplicateEvidenceIsBounded |
| E11 | Ordinary and Half-elf multiclass progressions | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | disposable-favored-class-multiclass |
| E12 | Future versus permanently replaced feature | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | ImprovedFeatureRuleExcludesOnlyFullReplacement; advanced/menus archetype exclusions |
| E13 | Cancel/backtrack/reopen level-up or respec | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | respec cancel; lifecycle pet respec cancel; grit cancel |
| E14 | Empty child set / capped last target | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus: capped counters close; host HP/skill fallback remains |
| E15 | Mercenary, companion, auto-level and fresh character | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | auto-level lane: a recorded level applied by the native constructor keeps its same-level reward (D5 fixed) while the unscoped native calls drop it; a host-only plan and Linzi's stored plan are identical either way; the Gunslinger has no default build; an NPC summon has no plan or favored class; custom-companion respec, fresh characters and pets as before |
| E16 | Ancestry change with existing fractions | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | respec Half-elf -> Human -> Dwarf; Mostly Human -> Standard |
| M01 | Every supported divisor at N = 0..20 | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | rank policy; grit 20 levels; menus |
| M02 | Capped and uncapped partial capacities | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | UncappedDivisorSixDoesNotStopAtEighteen; menus caps |
| M03 | Switch targets between partial investments | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | oracle/performance per-target ledgers |
| M04 | Duplicate ancestry source routes | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | AliasRoutesNeverDuplicateOrMultiply; menus one counter |
| M05 | Grit with normal and attribute-replacing archetypes | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | grit lane |
| M06 | Grit maximum increase/decrease while spent | SAVE TESTED | yes | yes | yes | yes | no (owner review) | fcb-grit-no-refill; L01 spent grit |
| M07 | Misfire across types, ammo, conditions and reliability | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | mechanics: a 31-case misfire matrix (types, both hands, ammunition, Broken with and without Gun Training, Reliable, Dead Shot and Scatter Shot) |
| M08 | Confirmation below/equal/above Critical Focus | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | mechanics; ConfirmationPreservesTheBetterBonus |
| M09 | Attack categories | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | mechanics: firearm/unarmed/other categories |
| M10 | Pistol-Whip and deed interruption/custom path | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | mechanics: the surrogate attack and its refusals; turn-modes: an interrupted Pistol-Whip spends and attacks nothing, and the counter moves the attack bonus by exactly its steps and never the trip's CMB (scoped to the deed's own attack) |
| M11 | Nimble armor and loss-of-Dexterity | SAVE TESTED | yes | yes | yes | yes | no (owner review) | mechanics; lifecycle; L01 Nimble |
| M12 | Dodge timing in both combat modes | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | turn-modes: the real Dodge command in real time and turn-based: swift, one grit, +2 plus the Halfling steps, no movement, no second use while active, active through its round and gone by the next |
| M13 | Initiative at zero/nonzero grit and True Grit | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | initiative timing and turn-modes: the deed with and without grit and with True Grit; True Grit Dodge refused at 0 grit and free at 1 |
| M14 | Bomb hit/splash/critical/converted damage | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | bombs: production bombs thrown natively; the steps once per damaged creature on a direct hit, splash with failed and passed saves, the miss splash, a critical and converted variants; nothing for Breath Weapon Bomb or the acid bomb's lingering damage |
| M15 | Two Bards with different range investments | SAVE TESTED | yes | yes | yes | yes | no (owner review) | performance range; lifecycle membership; L01 bard |
| M16 | Performance selected target and cap | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | performance range: per-target caps, owner text, ring; exclusions |
| M17 | Two paladins, aura overlap/unlock/replacement | SAVE TESTED | yes | yes | yes | yes | no (owner review) | lifecycle aura overlap; L01 paladin |
| M18 | Pet replacement/resummon/death and Barkskin | SAVE TESTED | yes | yes | yes | yes | no (owner review) | advanced; lifecycle; respec pet; L01 replacement |
| M19 | CMD versus CMB and maneuver subtype | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M20 | Undine Monk at N = 1,2,3,20 | SAVE TESTED | yes | yes | yes | yes | no (owner review) | UndineMonkMixedRateBundle; elemental core; L01 monk |
| M21 | Intimidate/demoralize scope | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M22 | Actual subtype versus geniekin race | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M23 | Undine SR checks | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M24 | Each supported selected-power manifest entry | SAVE TESTED | yes | yes | yes | yes | no (owner review) | oracle: every published revelation (65) equals a native oracle at the effective level at every read point and gate; advanced: six bloodline powers including Elemental Resistance's own gates; L01 revelation and bloodline arithmetic |
| M25 | Neighboring powers and spellbook unchanged | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | oracle/advanced neighbors and Fireball unchanged |
| M26 | Power at unlock boundary and above 20 | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | oracle thresholds; FCB virtual levels never unlock a power |
| M27 | No-scaling power or unsupported branch | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | no (owner review) | manifest dispositions (not offered) |
| M28 | Separate 1/3, 1/4, 1/6 racial variants | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | DistinctVariantsKeepTheirOwnRatesAndProfiles; menus |
| L01 | Fresh-process save/reload | SAVE TESTED | yes | n/a | yes | yes | no (owner review) | Word of Recall FCB transaction: one subject per state/mechanic family, including a selected firearm target with native shots after the reload, and the own persistence case of every remaining scheduled row (G05, G06 Dodge, G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04, S06) |
| L02 | Death/resurrection, area transition, polymorph/return | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | working-save-favored-class-lifecycle |
| L03 | Full respec commit and cancellation | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | respec lane (Gunslinger, Mostly Human, Sorcerer); lifecycle pet respec |
| L04 | Module OFF/ON across restarts | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | settings profiles integration OFF/ON |
| L05 | Third-party profile OFF with earned choices | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | third-party ON/OFF menus and mechanics |
| L06 | Missing external dependency in a saved build | SAVE TESTED | yes | yes | yes | yes | no (owner review) | missing-dependency transaction: a fixture save made with the host is read in the disabled-host profile; the native converter throws on every missing reference (nothing substituted), the fixture and saves folder are unchanged, KMG's identities resolve and the precise warning precedes the native message; the fixture is deleted with hash proof |
| L07 | Real-time-with-pause and turn-based modes | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | turn-modes: initiative, Dodge, Pistol-Whip, demoralize and grit in real time and in a fresh turn-based combat |
| L08 | Unity serialization and release GUID manifest | SAVE TESTED | yes | yes | yes | yes | no (owner review) | manifest tests; the L01 fresh-process reload deserializes the components of every reloaded family subject |
| L09 | Final package versus loaded DLL | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | every guarded run checks commit, package, DLL and MVID |
| L10 | Unsupported level-cap/gestalt/respec/provider | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | no (owner review) | explicit scope statement; no broad compatibility claim |

| Status | Families |
| --- | ---: |
| SAVE TESTED | 13 |
| NATIVE TESTED | 46 |
| NATIVE TESTED (simulated) | 2 |
| DOMAIN TESTED (native n/a) | 2 |
| OUT OF SCOPE | 1 |
| Total | 64 |

## 4. Final-candidate runs

Every run used the guarded launcher (Steam App ID 640820), verified the
commit, package, DLL and MVID before launch, and used only
`KMG_AUTOMATION_WORKING` or request-local disposable fixtures (the persistence and missing-dependency transactions' owned saves were deleted with hash proof).

| Scenario | Run | Result | Note |
| --- | --- | --- | --- |
| observe-favored-class-contract | 20260926T0424285521034Z-observe-favored-class-contract | PASS |  |
| observe-favored-class-host-state | 20260926T0425098817176Z-observe-favored-class-host-state | PASS |  |
| observe-favored-class-host-defects | 20260926T0425504713383Z-observe-favored-class-host-defects | PASS | H02 unsupported and partial, H04 and H05 simulated on cloned live observations |
| disposable-favored-class-elemental-core | 20260926T0426304190835Z-disposable-favored-class-elemental-core | PASS |  |
| disposable-favored-class-elemental-advanced | 20260926T0427143130346Z-disposable-favored-class-elemental-advanced | PASS |  |
| disposable-favored-class-oracle-revelations | 20260926T0427587131736Z-disposable-favored-class-oracle-revelations | PASS |  |
| disposable-favored-class-performance-range | 20260926T0429246463836Z-disposable-favored-class-performance-range | PASS |  |
| working-save-favored-class-lifecycle@KMG_AUTOMATION_WORKING | 20260926T0430061268709Z-working-save-favored-class-lifecycle | PASS | includes fcb-lifecycle-toggle-rollback and fcb-lifecycle-outcome-reload |
| disposable-favored-class-respec | 20260926T0431104013973Z-disposable-favored-class-respec | PASS |  |
| working-save-favored-class-visual-census@KMG_AUTOMATION_WORKING | 20260926T0431532259029Z-working-save-favored-class-visual-census | PASS |  |
| disposable-favored-class-grit | 20260926T0433443953898Z-disposable-favored-class-grit | PASS |  |
| disposable-favored-class-gunslinger-menus | 20260926T0434326926830Z-disposable-favored-class-gunslinger-menus | PASS |  |
| disposable-favored-class-gunslinger-mechanics | 20260926T0435266732898Z-disposable-favored-class-gunslinger-mechanics | PASS |  |
| disposable-favored-class-initiative-timing | 20260926T0436075638631Z-disposable-favored-class-initiative-timing | PASS |  |
| disposable-favored-class-turn-modes | 20260926T0436472719055Z-disposable-favored-class-turn-modes | PASS | M10, M12, M13 and L07 through real commands in real time and a fresh turn-based combat |
| disposable-favored-class-bombs | 20260926T0437279321541Z-disposable-favored-class-bombs | PASS | M14: production bombs thrown natively |
| disposable-favored-class-auto-level | 20260926T0438084070076Z-disposable-favored-class-auto-level | PASS | E15: a recorded level applied by the native plan; D5 scope |
| disposable-favored-class-mostly-human | 20260926T0438487161591Z-disposable-favored-class-mostly-human | PASS |  |
| observe-favored-class-performance-visuals | 20260926T0439381382013Z-observe-favored-class-performance-visuals | PASS |  |
| disposable-favored-class-multiclass | 20260926T0440198691989Z-disposable-favored-class-multiclass | PASS |  |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Ifrit | 20260926T0441000207225Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Ifrit |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Oread | 20260926T0446152587603Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Oread |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Sylph | 20260926T0451067541490Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Sylph |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Undine | 20260926T0456110533371Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Undine |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Sylph | 20260926T0500236878668Z-working-save-elemental-native-respec | PASS | parameters allocation=point-buy;class=Fighter;race=Sylph |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Oread | 20260926T0507046815679Z-working-save-elemental-native-respec | PASS | parameters allocation=point-buy;class=Fighter;race=Oread |
| working-save-smoke@KMG_AUTOMATION_WORKING | 20260926T0514062685374Z-working-save-smoke | PASS |  |
| L01 fresh-process persistence (prepare) | 20260926T0515338811043Z-disposable-word-of-recall-favored-class-persistence | PASS | transaction 20260926T0515255118814Z_3bbf199cd52b4a5eb9d4d430a20064b1 |
| L01 fresh-process persistence (verify) | 20260926T0516336684454Z-disposable-word-of-recall-favored-class-persistence | PASS | one subject per state/mechanic family and the own cases of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04 and S06 reloaded in a fresh process; settings and Mods tree restored |
| observe-favored-class-contract | 20260926T0518031115585Z-observe-favored-class-contract | PASS | settings profile third-party ON (L05; G16/G17/G18/G21 routes offered); the settings file was removed and verified absent |
| disposable-favored-class-gunslinger-menus | 20260926T0518432350673Z-disposable-favored-class-gunslinger-menus | PASS | settings profile third-party ON (L05; G16/G17/G18/G21 routes offered); the settings file was removed and verified absent |
| observe-favored-class-contract | 20260926T0519366390443Z-observe-favored-class-contract | PASS | settings profile Mostly Human OFF (the trait not offered; identities registered); the settings file was removed and verified absent |
| observe-favored-class-host-state | 20260926T0520171147411Z-observe-favored-class-host-state | PASS | settings profile integration OFF (L04; mechanics suppressed, identities registered); the settings file was removed and verified absent |
| observe-favored-class-contract | 20260926T0520572501435Z-observe-favored-class-contract | PASS | settings profile invalid file (schema 2; charter defaults applied and reported); the settings file was removed and verified absent |
| observe-favored-class-host-state | 20260926T0522081723639Z-observe-favored-class-host-state | PASS | real disabled host (H02): ZFavoredClass disabled through a byte-exact Params.xml stage; Params.xml and the regenerated loaded_blueprints.txt files restored byte-exact |
| L06 missing dependency (prepare) | 20260926T0523196350139Z-disposable-word-of-recall-favored-class-persistence | PASS | transaction 20260926T0523169075109Z_328953d2b5034ecb8d9ad783de2b1b80: the fixture saved with the host present |
| L06 missing dependency (absent host) | 20260926T0524188474485Z-observe-favored-class-missing-dependency | PASS | the fixture read with ZFavoredClass disabled: the native loader refuses it, nothing is substituted or written, and KMG's warning names the missing host and its state; Params.xml and the regenerated loaded_blueprints.txt files restored byte-exact; the fixture deleted with hash proof |
| profile gunslinger-only / observe-favored-class-host-state | 20260926T0528022419214Z-observe-favored-class-host-state | PASS | compatibility transaction compat-20260926T052602Z-a75f8c60d59d, restoration verified |
| profile gunslinger-call-of-the-wild / observe-favored-class-host-state | 20260926T0531183716191Z-observe-favored-class-host-state | PASS | compatibility transaction compat-20260926T052931Z-3aacec891548, restoration verified |
| profile gunslinger-call-of-the-wild-favored-class / observe-favored-class-host-state | 20260926T0534505046553Z-observe-favored-class-host-state | PASS | compatibility transaction compat-20260926T053303Z-0acfe3e6e607, restoration verified |

## 5. Adversarial audit

| Area | Result | Evidence |
| --- | --- | --- |
| Optional-host absence | PASS | isolated `gunslinger-only` and CotW-only profiles: host absent, 203/203 leaves registered, core and grit mechanics unaffected, no favored-class UI (H01, H07) |
| Unsupported-host rejection | PASS | a same-version host with a different binary, MVID or shape is rejected (`SameVersionDifferentBinaryIsRejected`); disabled, unloaded or partially initialized hosts stay inactive (H02, H04, H05) |
| Idempotent registration and exact rollback | PASS | contract lane: a repeated publication is a no-op; an injected fault restores the owned arrays exactly and leaves foreign entries unchanged (H06, H08); contained registry rollback domain tests |
| Empty optional-race arrays never unrestricted | PASS | an absent provider removes only its route (`AbsentProvidersRemoveOnlyTheirRoute`); G08/G20 routes are absent natively; no route is ever registered as an empty race array (H07) |
| Duplicate aliases and cap escape | PASS | one canonical ledger per effect and target (`AliasRoutesNeverDuplicateOrMultiply`, M04); the menus lane closes capped counters; the respec lane's ancestry change keeps one counter and no stale access (E16) |
| Spent-resource refill leakage | PASS | `fcb-grit-no-refill`: every completing pick raises maximum grit without refilling spent grit (M06); the L01 reload keeps spent grit and the raised maximum |
| Shared-blueprint and cross-owner leakage | PASS | O01 widens only the casting bard's own area instance, ring and text (other performers and the blueprint native); paladin auras carry each paladin's own bonus; the pet projection follows only the current pet and leaves no orphan |
| Selected-power manifest completeness | PASS | the target manifest classifies every candidate (66 revelations of which 65 are published and Spirit of the Warrior is excluded, 6 bloodline powers, 16 performances plus 14 non-targets); unpublished and excluded targets are withheld with reasons (M24, M27) |
| Host publication gate (review 1) | PASS | owned effects apply only with the integration enabled and the exact host publication committed; host absent, disabled, unsupported, failed or rolled back gives zero while ranks resolve |
| Owned-power thresholds (review 2) | PASS | each published revelation's own level gates and each Blast's extra uses follow the effective level; no other revelation, power, progression, spell, BAB, save or capstone early |
| Pet projection transitions (review 3) | PASS | unlink, relink, qualification loss, dismissal, resummoning, respec and reload leave exactly one projection on the one qualified pet |
| Range transaction (review 4) | PASS | injected ring failures leave the native radius and ring; the cylinder is never widened alone |
| Exception-safe scopes (review 5) | PASS | the replay and Demoralize scopes close in finally blocks; injected failures leave nothing marked and retries work |
| Bootstrap aggregate (review 6) | PASS | the aggregate counts every registry; registered equals expected with the catalogs' 203 and 13 identities live |
| Outcome-following descriptions (second review 1) | PASS | after injected failures, a failed second area and a deferred ring, the feature, toggle and action-bar descriptions match the actual area and ring at every step |
| Inert inactive counters (second review 2) | PASS | a held rank of the excluded Spirit of the Warrior counter or of an injected partial scope changes no read point (including the possession's BAB rank read) and no fact; a complete published target still works |
| Whole-group range outcomes (third review 1) | PASS | while earlier failed or deferred live areas of the same bard's performance are live, a success is held: every area, every ring and all three descriptions converge on native; a randomized domain invariant keeps the live areas all widened or all native and the text truthful |
| Verified rollback (third review 2) | PASS | a ring-restore failure injected into a widened sibling's narrowing ends that area, which is never kept live with native text; the real toggle whose own buff runs it is turned off and spends no round at the next round; an older area that toggle's buff does not run is ended without stopping the current performance |
| No remembered outcome (third review 3) | PASS | after each failed area ends the configured range returns; a same-process forced area unload and reload recreates the area, which widens with its ring and every description, with one live area and nothing left over |
| Stored level plans (E15) | PASS | a recorded level applied by the native constructor keeps its same-level reward; the unscoped native calls drop exactly that reward (D5 fixed); host-only and story-companion plans are accepted identically either way |
| Deed attack scope (M10) | PASS | under the owner's mod stack another mod derives the base CMB from a weapon's attack bonus; the Pistol-Whip counter now applies only inside the deed's own attack, and the trip's CMB is equal with and without it |
| Host and dependency states | PASS | a real disabled host (byte-exact Params.xml stage) and simulated unsupported, partial and Gunslinger-less hosts stay inactive or block only their rows; a save needing the missing host is refused natively with nothing substituted or written, and KMG's warning names the host and its state |
| Save, respec and lifecycle cleanup | PASS | L01 reloads one subject per state/mechanic family in a fresh process, including a selected firearm target; L02 death, polymorph and party area reload; L03 cancelled and committed respecs including the Mostly Human identity, a companion and a selected power |

## 6. Unfinished targets, exclusions and provider absences

No scheduled row is unfinished and no charter scenario family is partial or
blocked. What remains are deliberate exclusions and provider absences:

- O01 exclusions: Storm Call (its text promises 50 feet over a 30-foot area)
  and Mockery (its text is a single selected target while the area hits every
  creature) are registered but never published, because no truthful owner
  range can be displayed.
- I06/S04: every published revelation's own steps, tiers, breakpoints,
  single-level uses and own level gates follow the effective level, so it
  gains its own later abilities and forms earlier; no other revelation,
  revelation choice, class feature, spell, BAB or save is granted. The
  fourteen formerly deferred threshold-only revelations are published (65 of
  66 targets); Spirit of the Warrior is excluded (its possession sets BAB
  from oracle level).
- I08/S06: Elemental Blast's extra uses at 17 and 20 and Elemental
  Resistance's step from 10 to 20 at 9th level follow the effective level
  (at most two steps); Primal Elemental bloodlines are not eligible (not
  fire/air elemental).
- G08 (Goblin) and G20 (Orc): EXPECTED PROVIDER ABSENCE. No playable Goblin
  or Orc exists in the qualified profile; their counters are G02's and G05's,
  published and tested through their other routes, and neither route is ever
  registered as an empty race array.
- E09 (a verified Racial/Planar Heritage provider): OUT OF SCOPE, no
  qualified provider is installed.

## 7. Pre-existing defects D1 and D2 (fixed by the tabletop rules)

Both were pre-existing KMG defects outside the adopted rows. The owner
decided to fix them by the tabletop rules (2026-09-25); both are fixed in
`bb4dfd674` and natively tested (`FAVORED-CLASS-BLOCKERS.md`).

- D1: a base Gunslinger could not complete level 17: the obligatory Gun
  Training selection offered only the three rank-one types, so once all three
  were taken the level never completed. The selection is no longer
  obligatory; the native gate still requires a pick while an official type is
  untrained, and a base Gunslinger's 17th level completes with the empty pick
  (the favored-class grit lane levels a base Gunslinger to 17).
- D2: Dead Shot's critical confirmation was unreachable (its probes were
  immune to critical hits, and the delivery would have replaced every other
  confirmation bonus). Dead Shot now threatens from its natural roll and makes
  one confirmation with the native rules at the highest attack bonus minus 5
  (plus 1 per extra threat, at most 0), adding every other confirmation bonus,
  against the critical AC computed after the firearm AC frame (touch AC when
  the firearm's rule applies); immunity and the party critical setting block
  it as they block a native threat. The firearm confirmation counter (G02 and
  its G08/G16 routes) therefore also applies to Dead Shot's confirmation.

## 8. Installation restoration

The owner's pre-mission KMG install (0.0.136, backup
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`)
was restored through `scripts/Restore-Live-Mod.ps1` (under this lab's lease)
after the last run of the completion round, and verified byte for byte on
2026-09-26 at 05:44 UTC:

- `Info.json` SHA-256 `f66de05d5c6282eece8218b6c4f31d49dfc8ef9efa27dfeceeda712034717e17` (match True)
- `FeatureModules.json` SHA-256 `6e24b2788a0c8f063d6e561a27c93f9c5349f2fc21b5217689da8aefdbb385d0` (match True)
- `KingmakerGunslinger.dll` SHA-256 `c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c` (match True)
- whole tree: 238 backup files, 238 live files, 0 only in the backup, 0 only live
- no `FavoredClassIntegration.json` remains (the default settings apply)
- other mod files unchanged: `CallOfTheWild\settings.json` `24cc3f80...`, `RacesUnleashed\Settings.json` `270899c3...`,
  `ZFavoredClass\settings.json` `bdceed77...` and Tweak or Treat's regenerated `loaded_blueprints.txt` `96e13782...` (match True)
- Unity Mod Manager's `Params.xml`: every disabled-host stage of the round (`20260926T022705Z`, `20260926T030221Z`, `20260926T040626Z`, `20260926T052206Z`) and each L06
  transaction backed it up, disabled only ZFavoredClass and restored the exact pre-stage bytes after
  the game exited (SHA-256 match; backups in `C:\Dev\KingmakerGunslingerLab\runtime-backups\umm-params\`
  and the transaction folders); the regenerated `loaded_blueprints.txt` files were restored byte for byte
  after each disabled-host run; ZFavoredClass is enabled now
- each temporary settings profile removed `FavoredClassIntegration.json` after
  its own runs (verified absent after each profile and at the end)
- the B3 memory diagnostic wrote the integration-off `FavoredClassIntegration.json` only for its one
  run and removed it (verified absent)
- the persistence transaction `20260926T0515255118814Z_3bbf199cd52b4a5eb9d4d430a20064b1` restored the settings and the complete Mods tree;
  its owned save was deleted from the save folder with hash proof (94 preexisting saves preserved;
  the prepared save's evidence copy stays in the machine-local transaction folder)
- the L06 transaction `20260926T0523169075109Z_328953d2b5034ecb8d9ad783de2b1b80` restored `Params.xml` (match True) and deleted its fixture from the save
  folder with hash proof (its evidence copy stays in the machine-local transaction folder); the lab's
  only saves are `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING`
- each compatibility profile (`compat-20260926T052602Z-a75f8c60d59d`, `compat-20260926T052931Z-3aacec891548`, `compat-20260926T053303Z-0acfe3e6e607`) restored the exact original Mods tree and
  FeatureModules bytes before releasing the lock (restoration verified)

Restoration status: VERIFIED.

## 9. Blockers

`FAVORED-CLASS-BLOCKERS.md` keeps only what is genuinely open:

- D6 (owner decision): Pistol-Whip does not add the firearm's enhancement
  bonus to its attack roll (native `enhancedPistolAttackDelta = 0`), which the
  tabletop deed requires. It predates the mission, lies outside the
  favored-class rows and is not changed.
- B3 (environment risk, open): the lanes that drive the native
  character-build screens commit up to 72 GB private while every other lane
  stays under 5 GB. Neither the screenshots (the census and respec lanes save
  none) nor the favored-class integration (an integration-off creator run
  peaked at 70.1 GB against 69.2 and 72.0 GB enabled) cause it. Every run
  passes on this machine; batches start with a low idle commit charge, and
  isolating the cause is a harness follow-up outside the favored-class rows.
- B1 (shared installation, mitigated) and two suspected host defects (H-1,
  H-2; host data, never patched).

B2 is resolved; D1 and D2 are fixed on the owner's decision; D3, D4 and D5
were found and fixed in this mission. Every former owner question
(OD-1 to OD-12) is resolved there with the charter rule, the owner's
authorization or the evidence that decides it. No qualification gap remains;
QUALIFIED waits for the owner's review of this local candidate.

## 10. Review findings (PR #24)

Each finding was addressed in order, then the whole candidate was requalified
on the exact final package (section 4). Native evidence names the lane
assertion and its run under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\`.

| Finding | Commits | Domain tests | Native evidence |
| --- | --- | --- | --- |
| 1. Runtime host gate: owned effects need the integration and a committed qualified host | `ae0f1dafd` | `favored-class.host-activation-transitions`, `favored-class.host-mechanics-truth-table`, `favored-class.host-commit-activates` | `fcb-mechanics-follow-integration`: Published with the host committed gives the two grit steps (`20260925T0442470377650Z-observe-favored-class-host-state`); integration OFF keeps the rank and gives zero (`20260925T0537500689244Z-observe-favored-class-host-state`); host absent in the `gunslinger-only` and CotW-only profiles gives zero (`20260925T0542174833633Z-observe-favored-class-host-state`, `20260925T0545354549902Z-observe-favored-class-host-state`); the host-present profile gives the steps again (`20260925T0549104167941Z-observe-favored-class-host-state`) |
| 2. Selected revelation and bloodline thresholds (charter 8.10) | `f9a8e6cfb`, follow-up `27176f424` | `favored-class.gate-decision-native`, `favored-class.power-use-thresholds`, `favored-class.power-use-gates`, updated revelation and power source tests | `fcb-oracle-level-gates`: 83 own level gates of the published revelations decided at the effective level, 83 moved, none skipped (`20260925T0445118827121Z-disposable-favored-class-oracle-revelations`); `fcb-advanced-bloodline-powers`: Blast's extra uses at 17 and 20 follow the effective level with at most two steps, Ray's uses unchanged (`20260925T0444268317587Z-disposable-favored-class-elemental-advanced`); Spirit of the Warrior is not published (the contract lane's published graphs, `20260925T0442014336065Z-observe-favored-class-contract`); the first requalification run `20260925T0414053062811Z` exposed that Call of the Wild moves Blast's extra uses into the power's own gates, fixed by the follow-up |
| 3. Pet projection follows one qualified desired pet | `078978a4c`, follow-up `aa298650e` | `favored-class.pet-sync-transitions`, `favored-class.pet-sync-plan`, `favored-class.pet-sync-wiring` | `fcb-lifecycle-pet-transitions`: native unlink without replacement, relink, the same pet losing and regaining qualification, native dismissal and a resummoned companion (`20260925T0446375733122Z-working-save-favored-class-lifecycle`); `fcb-lifecycle-respec-pet` (`20260925T0446375733122Z-working-save-favored-class-lifecycle`); persistence verify `word-of-recall-fcb-persistence-family-ranger-replacement-after-reload` and `word-of-recall-fcb-persistence-family-ranger-unlink-after-reload` (`20260925T0532235047813Z-disposable-word-of-recall-favored-class-persistence`) |
| 4. Bard range transaction | `da07c4e07` | `favored-class.range-widening-success`, `favored-class.range-widening-injected-failures`, `favored-class.range-widening-ringless-deferred` | `fcb-performance-injected-failure`: a scaler that scales then throws and one that scales nothing leave the real instance at its native radius with its ring restored exactly; the healthy instance widens both (`20260925T0445550085579Z-disposable-favored-class-performance-range`); radius, ring, text, exclusion and pool-release assertions (`20260925T0445550085579Z-disposable-favored-class-performance-range`); native visuals (`20260925T0455038884902Z-observe-favored-class-performance-visuals`) |
| 5. Exception-safe scopes (level-up replay, Demoralize) | `f4f5b9931` | `favored-class.scope-nested-restores-previous`, `favored-class.scope-exception-closes`, `favored-class.scope-guard-closes-stale-frame`, `favored-class.scope-retry-after-failure`, `favored-class.scope-finally-sources`, updated replay source test | `fcb-elemental-native-mechanics`: a real `[Demoralize, Intimidate check]` action list gives +2 then +0 healthy, +0 after a failure injected inside the frame (depth 0 after each list), and +2 then +0 on retry (`20260925T0443327592809Z-disposable-favored-class-elemental-core`); `fcb-advanced-bloodline-powers`: a failure injected into the reward's replayed pick leaves nothing counted and the retried native replay applies the reward (`20260925T0444268317587Z-disposable-favored-class-elemental-advanced`); every level-up lane replays through the scoped calls |
| 6. Bootstrap aggregate counts | `719085d51`, follow-up `7bb4a7040` | `favored-class.source-invariants` (both sides of the aggregate, all five registries, 171 catalog identities) | `fcb-bootstrap-aggregate`: registered 2,085 = expected 2,085; 171/171 favored-class and 13/13 Mostly Human identities live (`20260925T0442014336065Z-observe-favored-class-contract`); the first run `20260925T0411366030026Z` showed a manifest equality was not a property of the aggregate (see below) |

Intermediate candidates of this round (superseded; each exposed a defect that
the next commit fixed):

- `719085d51` (deployment `20260925T0411348835015Z`): contract
  `20260925T0411366030026Z` FAIL only on the new aggregate pin, whose added
  manifest equality was wrong (the aggregate itself was exact at 2,085);
  host state `20260925T0412292153641Z` and elemental core
  `20260925T0413132204168Z` PASS; elemental advanced `20260925T0414053062811Z`
  FAIL on the Blast use thresholds (finding 2 follow-up); the remaining four
  stage-1 lanes were refused by the guarded launcher before launch because a
  source edit made during the batch left the worktree dirty.
- `27176f424` (deployment `20260925T0426279539132Z`): seven stage-1 lanes
  PASS; lifecycle `20260925T0430574929905Z` ERROR in the new pet-transition
  step, which waited for fixtures an earlier step had destroyed (finding 3
  follow-up); every earlier lifecycle assertion of that run passed.

The installed manifest lists 2,138 active identities: the aggregate's 2,085,
the Brown-Fur extension's 25 (its own registry) and 28 elemental-race
visual assets that are not blueprints (KMG.ElementalRaces.Ifrit.Visual.Body.Male ... KMG.ElementalRaces.Undine.Visual.Head.Female.02).

## 11. Second review findings (PR #24)

Both findings were addressed in order, on top of the records commit
`323cf21b7`, then the whole candidate was requalified on the exact final
package (section 4).

| Finding | Commit | Domain tests | Native evidence |
| --- | --- | --- | --- |
| 1. A Bard's descriptions must follow the actual widening outcome | `7f9c781d8` | `favored-class.range-text-follows-live-outcome`, `favored-class.range-text-failure-memory`, `favored-class.range-text-sources` | `fcb-performance-outcome-text` and `fcb-performance-injected-failure` (`20260925T1326552732020Z-disposable-favored-class-performance-range`), on a fresh invested bard: before any cast the feature, toggle and action-bar descriptions show the configured range; a scaler that scales then throws and one that scales nothing leave the area, its ring and all three descriptions native, and they stay native after the failed area ends; the recovered next cast widens the area, the ring and all three descriptions together; a failing second area narrows the widened first one and every description is native; a deferred ring keeps the area and descriptions native until the late ring widens all of them together |
| 2. Excluded or withheld revelation counters must be mechanically inert | `f7b4ae4f5` | `favored-class.revelation-scope-active`, `favored-class.scope-activity-sources` | `fcb-oracle-inactive-scopes` (`20260925T1326051633899Z-disposable-favored-class-oracle-revelations`): the excluded Spirit of the Warrior counter held at rank 3 on an oracle 14 resolves but equals an uninvested control in every resource, ability parameter, rank, gated fact and feature, and the possession buff's excluded BAB rank read is 14 for both; Fire Breath's live scope gives its benefit, none while a partial reason is injected, and its benefit again afterwards |

The descriptions follow the recorded outcome of the owner's own live areas
of the performance (`FavoredClassPerformanceInstances`); with no live area
the configured range is shown unless the owner's last widening of that
performance failed (the third review removed that remembered failure;
section 12). Every revelation read point (ranks, resources, ability
parameters, level gates, refreshes) goes through the scope's earned steps,
which are zero unless the scope is active: published, complete, admitted to
the read-point indexes and not withheld or unavailable in this process. The
bloodline-power and performance counters apply the same per-target
withholding guard.

## 12. Third review findings (PR #24)

The third review (at `d13a1589e`) found three follow-on problems in the Bard
outcome state machine. They were addressed on top of the records commit
`d13a1589e`, then the whole candidate was requalified on the exact final
package (section 4).

| Finding | Commits | Domain tests | Native evidence |
| --- | --- | --- | --- |
| 1. A later success could coexist with earlier failed or deferred live areas | `4de114511` | `favored-class.range-group-success-beside-native`, `favored-class.range-group-success-beside-deferred`, `favored-class.range-group-invariant` | `fcb-performance-injected-failure` and `fcb-performance-outcome-text` (`20260925T2045379719640Z-disposable-favored-class-performance-range`): widen A; fail B (A and B native); while both are live a success C is held, so A, B, C, every ring and all three descriptions converge on native; a success beside a live deferred area is held and the late ring keeps both native |
| 2. A failed sibling's rollback was not exception-safe | `4de114511`; follow-ups `a17c2efe7`, `51f56820e`, `5636d940b` | `favored-class.range-group-unverifiable-rollback`, `favored-class.range-group-toggle-lineage`, `favored-class.range-text-sources` | `fcb-performance-injected-failure` and `fcb-performance-outcome-text` (`20260925T2045379719640Z-disposable-favored-class-performance-range`): a ring-restore failure injected into the narrowing of a widened sibling ends that area (not counted live; the descriptions follow the live failed area); `fcb-lifecycle-toggle-rollback` (`20260925T2046176703078Z-working-save-favored-class-lifecycle`): a real Inspire Competence toggle started natively widens its area (40 feet, ring x1.333); when that area's rollback cannot be verified, it is ended, the toggle is turned off and at the next round stops without spending a round (1 round before and after); an older area that the toggle's current buff does not run is ended while the current performance keeps running on the same buff, narrowed with the failed area |
| 3. Failure memory outlived its area | `4de114511` | `favored-class.range-text-no-memory`, `favored-class.range-text-sources` (no remembered outcome) | `fcb-performance-injected-failure` and `fcb-performance-outcome-text` (`20260925T2045379719640Z-disposable-favored-class-performance-range`): after each failed area ends, the descriptions return to the configured range; `fcb-lifecycle-outcome-reload` (`20260925T2046176703078Z-working-save-favored-class-lifecycle`): an invested bard's area fails before a same-process forced area unload and reload (`Game.ReloadArea`: the performance buff's native `AddAreaEffect` ends its area on unload and spawns a new one on load); the recreated area widens with its ring, every description follows it, one live area, nothing left over |

The owner's live areas of one performance form a group that widens only as a
whole (`FavoredClassRangeGroup`): an area may attempt widening only while every
other live area of the group is widened, and otherwise stays native (Held); a
widening recorded beside a native sibling is rolled back; an area that ends up
native narrows every widened sibling. Narrowing restores the ring and the
radius independently, then verifies both; an area that cannot be verified
native is ended. No outcome is remembered after its area ends: with no live
area the descriptions show the configured range.

Defect found in this pass's own review of `4de114511` (fixed in `a17c2efe7`):
the unverifiable-rollback path is meant to turn off the toggle whose own
current buff runs the ended area, but it compared the toggle's buff context
with the area's own context. `AreaEffectsController.Spawn` runs every area
under `parentContext.CloneFor(blueprint)`, a new context whose `ParentContext`
is the buff's, so the comparison could never match: the area was ended while
the performance kept running and spending rounds with no area. The toggle's
current buff is now found among the ancestors of the area's context
(`FavoredClassContextLineage`, bounded); saves preserve that reference (the
save serializer keeps object references and `ParentContext` is serialized).
The performance lane could not have caught it (it spawns areas under fresh
contexts), so the lifecycle lane now starts the real toggle. Its first form
added a second buff of the same blueprint, which `BuffCollection.AddBuff`
replaces rather than stacks; `51f56820e` spawns the failing and older areas
directly instead, before `a17c2efe7` was ever run.

The native attach (`AreaEffectEntityData.OnViewAttached`) calls `SpawnFxs`
after every spawn and load, so the late-ring hook also attempts once more an
instance the initialization left native beside its unscaled ring, under the
same group rule; a failing area beside a native sibling therefore records
Failed at its initialization and Held at its attach. Both are native.
`5636d940b` documents it and holds the lane to native outcomes.

Intermediate candidates of this round (superseded):

- `4de114511` (deployment `20260925T1532361168725Z`, before a machine
  reboot): the sixteen favored-class lanes, the Oread, Sylph and Undine
  creator lanes, both respec lanes, the persistence transaction and the
  smoke PASS. The Ifrit creator lane ended three times with the game
  process exiting before a result (`20260925T1547323536185Z`,
  `20260925T1625508985738Z`, `20260925T1637499765483Z`) and one Sylph run
  reported Out of memory (`20260925T1557284483521Z`; its rerun
  `20260925T1631393278584Z` passed): the machine's commit charge was
  exhausted (66 GB committed with no game running; the game's private
  bytes reached 60 GB when the system commit hit its 127.6 GB ceiling;
  B3). The machine was rebooted (15 GB committed with nothing running).
  This candidate's cycle was not resumed: this pass's review found the
  toggle-lineage defect above first.
- `51f56820e` (deployment `20260925T2029203755841Z`): seven stage-1 lanes
  PASS; the lifecycle lane `20260925T2033303720915Z` FAIL only on the new
  `fcb-lifecycle-toggle-rollback`, whose first scenario passed in full (the
  real toggle's area was ended, the toggle turned off, and the next round
  spent nothing). Its two failures were the lane's own expectations: the
  left-over count was sampled after the second scenario's areas existed, and
  the failing area's attach-time retry is held (native) rather than failed.
  Fixed in `5636d940b`.

## 13. Completion round

The owner's directive was to finish every remaining item. The owner decided
D1 and D2 (fix by the tabletop rules) and authorized the host and dependency
stagings: a disabled host (H02), simulated host defects (H04/H05) and a
missing-dependency save (L06), each with a byte-exact restore.

### Families observed natively

- **E10**: a runtime-test seam extends the verified permission graph for one
  disposed scope (a domain test proves no production path calls it). Real
  level-1 Gunslinger visits: an unconditional cycle terminates and opens
  exactly the reachable ancestries' counters; a five-edge chain stops at the
  depth bound of four; an edge on an unverified fact never applies; a
  duplicated Mostly Human identity opens each counter once; a stray racial
  fact proves nothing; one favored-class selection per visit.
- **E15**: a stored level plan (auto-level for companions, a pregen, an
  imported companion) is applied by the level-up controller's own
  constructor, outside the scoped replay, so a reward listed before its
  same-level target was dropped (D5). Fixed by a transpiler that scopes the
  plan's one AddAction call. The save-free, settings-free auto-level lane
  uses the native imported-companion trigger: a recorded Ifrit Sorcerer level
  is applied whole (the reward kept, the visit complete and automatic, the
  counter and ray confirmed), while the same native calls without the scope
  reject exactly the reward; a host-only plan and Linzi's own stored plan are
  accepted identically either way and never pick a KMG counter; the
  Gunslinger has no default build; an NPC class leveled by the game (the
  Erinyes summon) has no plan, favored class, reward or KMG counter.
- **M07**: a 31-case native misfire matrix (types, both hands, ammunition,
  Broken with and without Gun Training, Reliable, Dead Shot and Scatter
  Shot).
- **M10, M12, M13, L07**: real commands in real time and in a fresh
  turn-based combat whose initiative is rolled under turn-based rules. The
  Initiative deed and its steps with and without grit and with True Grit;
  Gunslinger's Dodge (swift, one grit, +2 plus the Halfling steps, no
  movement, no second use while active, a one-round buff that covers the
  rest of its round and ends at the next round's start); an interrupted
  Pistol-Whip that spends and attacks nothing, and two real ones whose attack
  bonus differs by exactly the steps while the trip's CMB does not; the
  native demoralize with and without the Rogue steps; grit never refilling.
  The lane found that another mod in the owner's stack computes the base CMB
  from a weapon's attack bonus, so the Pistol-Whip counter (keyed to the
  surrogate weapon) reached the trip's CMB; it is now keyed to the deed's own
  attack only.
- **M14**: the production bombs thrown natively (direct hit, splash with
  failed and passed saves, the miss splash, a critical, converted variants
  and the Arcane Bomber's fire bomb): the steps once per damaged creature,
  nothing for Breath Weapon Bomb or the acid bomb's lingering damage.
- **M24**: every published revelation (65 of 66; Spirit of the Warrior
  excluded) equals a native oracle at the effective level at each read point
  and gate, including the chosen weapon's own gates for Weapon Mastery; the
  six bloodline powers, including Elemental Resistance's gates.

### Persistence

The fresh-process transaction reloads the own persistence case of every
remaining scheduled row: G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04,
O05, O08, U02, S04 and S06, each with its numerical effect after the reload.

### Host and dependency states (owner-authorized)

- **H02 (disabled host)**: Unity Mod Manager's Params.xml is backed up, only
  ZFavoredClass is disabled, the host-state lane runs, and the exact original
  bytes are restored and verified by SHA-256 after the game exits; mods that
  list their loaded blueprints at startup regenerate a different list while
  the host is disabled (Tweak or Treat), so those generated files are
  restored byte for byte too. The integration is HostDisabled with every owned
  identity registered and nothing published.
- **H02 (unsupported, partial), H04, H05 (simulated)**: the live host's own
  observations are cloned and one fact changed per case; the production gates
  and planner reject each specifically (UnsupportedBinary, HostIncomplete,
  GunslingerMissing with only the Gunslinger rows blocked) and nothing live
  changes.
- **L06**: a transaction-owned fixture save made with the host is read in the
  disabled-host profile. The game's own converter throws on every missing
  reference (nothing is substituted, nothing written), KMG records them and
  shows a precise recovery warning before the native "Cannot load game"
  message, KMG's identities in the save still resolve, and the fixture is
  deleted with hash proof.

### Deferred targets published

Charter 8.10 counts effect thresholds among the level-dependent effects, so
the fourteen threshold-only revelations and Elemental Resistance (fire and
air) are published with 32 appended identities (2,172: 2,170 active, 2
reserved); every earlier identity keeps its position.

### Memory (B3) re-measured

The game's committed memory was sampled every 5 s through both final cycles.
Only the lanes that drive the native character-build screens exceed 5 GB:
the census (no screenshots) 41.9 and 43.0 GB, the creator lanes 57-72 GB and
the respec lanes (no screenshots) 46-47 GB, all released when the game exits.
The screenshot-poll change in `e7748fff4` did not lower these peaks, so the
earlier attribution was withdrawn. One extra guarded run after the
qualification, the Ifrit creator lane with the integration disabled by the
integration-off profile
(`20260926T0536579357364Z-working-save-elemental-character-creation-regression`,
PASS; the game log reports `IntegrationDisabled`, 0 leaves published), peaked
at 70.1 GB against 69.2 and 72.0 GB enabled: the favored-class integration
does not cause it. B3 stays an environment risk for qualification batches
(`FAVORED-CLASS-BLOCKERS.md`).
