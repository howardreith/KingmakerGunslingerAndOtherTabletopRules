# Spider Summoner integration: status

**Incomplete with named blockers.** The fourth review's three acceptance-check weaknesses are corrected and their
four affected phases pass on candidate `04c6030c02` under controlled input. Three phases are carried from earlier
candidates whose code for them is unchanged (below). Four acceptance workstreams still lack required observations:
**RU-16** saved references across save/reload, plus the reload parts of RU-02 and RU-14; **RU-15** rendered SNA and
Druid popups; **RU-10** metamagic, Crab I/Widow IV Herald admission, a cast by the naturally levelled Herald, and its
caster level and duration; and **RU-09** a cast from the multiclass cleric's book. Their observers are now written,
compiled and focused-tested in source commit `65f0acce2e` (fifth review, below), but **not built or run natively**: they
are unobserved, not shown to work. Native execution waits for the owner's decision on the cumulative validation
ceiling, which is about 117 of 120 minutes spent; the request is to raise it to 165 minutes (estimate below). This is
focused compatibility evidence, not release qualification, and owner review is pending. No merge, release or
permanent installation has been performed.

Baseline and its original result, unchanged: [BASELINE.md](BASELINE.md), including the `FAIL, 70 PASS / 1 FAIL`
result on candidate `efc75b8cc0b0`.

## Completion continuation (owner authorization of 2026-11-10)

The owner authorized this continuation with a 90-minute planning target and a 120-minute cumulative ceiling for
builds, checks, preflights and guarded native validation. That replaces the ordinary 30-minute ceiling for this named
continuation only; the general validation policy is unchanged.

### Delivered candidate

| Item | Value |
| --- | --- |
| Source | `04c6030c02fbed53584e73f303795b773761085e`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `068d1131112c19224e5368ddd06f8ae13ddbbf9b5d16d562e038270110b92547` |
| DLL SHA-256 | `452552cce919ee67bbde7cd584e5fec58454351201379a1d0364825cd0824ce9` |
| Purpose | Development candidate `races-unleashed-summoning`; never a release |
| Selected tests | `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`, 34 selected, 0 failures |
| Observer | Same source: phases `summon-cleric-alignment`, `spider-web`, `spider-mechanics`, `spider-routes`, `spider-module-off` |

**What `04c6030c02` changes, relative to `106cda92f3`.** Observer code only, in
`RuntimeTestRunner.SpiderCompletion.cs`, `RuntimeTestRunner.SpiderRoutes.cs`, `SpiderEscapeObservationVerdict.cs`
(one added method) and the new pure `SpiderCompletionVerdicts.cs`. There is no product change.
- **Rerun on `04c6030c02`:** spider-mechanics, spider-web in both profiles, and spider-routes, the phases whose
  verdicts changed.
- **Carried, unchanged code:**
  - `summon-cleric-alignment` from `106cda92f3`. It calls none of the changed methods.
  - `spider-module-off` and the RU-absent control from `deebec4edb39`. Their methods
    (`SpiderModuleOffControl`, `SpiderRacesUnleashedAbsentControl`) and dispatch are unchanged since that candidate.

Earlier delivered candidate, retained: `106cda92f3`, package `a4175372...2249`, DLL `2ecb8761...6f52`. It differed
from `deebec4edb39` only in an optional spawn-count predicate on the Herald/Druid cast observer, and its cleric phase
was rerun on its own bytes.

### Native transactions

| Phase | Profile | Candidate | Run ID | Result | Input |
| --- | --- | --- | --- | --- | --- |
| `spider-mechanics` | combined DATA install | `04c6030c02` | `20261110T1330076949515Z-2ab4adb1a93c4a6d94a7209a38524ee9` | **PASS**, 12 checks | parked, 24/24 samples |
| `spider-web` | combined | `04c6030c02` | `20261110T1332078208524Z-8eb5a86c143c450b9ce2c7bd6a495521` | **PASS**, 5 checks | parked, 24/24 |
| `spider-web` | `gunslinger-races-unleashed` (no Call of the Wild) | `04c6030c02` | `20261110T1335005147378Z-1cde64877ad344c680dded47ce88541f` | **PASS**, 5 checks | parked, 35/35 |
| `spider-routes` | combined | `04c6030c02` | `20261110T1337030918304Z-bb7deec2d63940948c7872e7ab0da9f9` | **PASS**, 21 checks | parked, 43/43 |
| `summon-cleric-alignment` | combined | `106cda92f3` (carried) | `20261110T0656579610169Z-...` | **PASS 186 / 0** | parked, 64/64 |
| `spider-module-off` | combined, Expanded Summoning OFF | `deebec4edb39` (carried) | `20261110T0641112018794Z-67e930f0203d481597f3f8f54ab9c861` | **PASS** | parked |
| `spider-web` as RU-absent control | `gunslinger-only` | `deebec4edb39` (carried) | `20261110T0646424296444Z-80d88f824f904ac28e0a6cc9a4685f43` | **PASS** | pointer moved |

**What the check counts mean.** They are recorded checks, not all gameplay assertions. Each phase includes the
fixture checks (hostile faction, loaded profile, fixture restored). The routes phase also includes three
diagnostic Herald-slot cells, `spider-route-herald-slot-1/4/5`, which pass unconditionally. So its 21 checks are 15
gameplay assertions. The earlier "PASS 22 / 0" counted the summary's lines, which add the native-profile
restoration check. The natural Herald
casting requirement stays open (RU-10).

Superseded runs of the same phases on `deebec4edb39`, retained: routes `20261110T0628118612342Z-c31d6bd0...`,
mechanics `20261110T0631458477894Z-340574a5...`, web combined `20261110T0639091492164Z-2b581e63...` and web without
Call of the Wild `20261110T0643560297139Z-7908c6bc...`, all PASS under their earlier, weaker verdicts. Their
pointer samples moved during the routes and combined-web runs.

Pointer samples are supporting evidence only, sampled every 5 seconds; they do not prove continuous absence of
input. Every `04c6030c02` run read (300, 200) on every sample. The earlier "moved" samples (including 0,0 and 640,360
while the game window started) were in phases with no UI interaction, so those observations are retained.

**Restoration.** Every transaction restored exactly.
- **Scenario transactions:** live tree `5C3E4AD4...BFBB` restored, verified each time.
- **Profile transactions:** `restorationVerified=True`, with Call of the Wild settings and `FeatureModules.json`
  byte-exact. The wrapper does not protect UMM `Params.xml`, which UMM rewrites at launch, so the runner restored it
  each time.
- **`Params.xml`:** ended at its intake hash `E44F4495...30BB` after every run.

**Earlier candidates of this continuation (retained):**

| Candidate | Result | What it showed |
| --- | --- | --- |
| `d94acab01653` (package `3918fffd...c9ef9`) | web FAIL; mechanics FAIL 7/6; module-off FAIL; RU-absent **PASS** | Two summons' CMDs differed by 4 with no web reason, so CMD escapes are now decided within one spider. Poison target died. Module-off observer missed RU's tier I attachment route |
| `5107a283bd20` (package `2cb359c8...8942`) | web **PASS** (both profiles); module-off **PASS**; cleric **PASS 174/0**; mechanics FAIL; routes FAIL | The target was dead, which explained the missing poison states. Herald children are expanded by the popup. Hunter SNA |
| `df4b3e4bc6a9` (package `b592b381...2886`) | mechanics FAIL 1 (Tarantula natural-1 miss; NPC execution); routes FAIL (my lifetime check; Herald slot) | Fixed in `deebec4edb39` |
| `deebec4edb39` | six PASS; cleric FAIL 184/2 | The Herald quantity cast was correct, but the observer assumed one spawn; fixed in `106cda92f3` |
| `106cda92f3` (package `a4175372...2249`) | cleric **PASS 186/0** | Carried. Its six predecessor phases were superseded by `04c6030c02` where the fourth review found weaker verdicts |

### Fifth review: the four workstreams prepared (`65f0acce2e`, not built)

The fifth review accepted the fourth-review corrections, accepted the carried phases, and asked for the persistence
checkpoint sequence to be made explicit and for the remaining fixture preparation to be finished before another
build. Implementation and writing are outside the counted ledger; the compile and focused checks of this pass are
inside it (57 s).

**RU-16 checkpoint sequence.** A `spider-summoner` scope on the existing Working-save stages, four fresh processes,
fixed in `SpiderSummonerPersistencePolicy`:

| Stage | Scenario | Module | Working save |
| --- | --- | --- | --- |
| 1. Prepare | `working-save-expanded-summoning-prepare` | ON | written: the checkpoint |
| 2. Disabled reload | `working-save-expanded-summoning-verify-cleanup`, `spiderCheckpoint=preserve` | OFF | **not written**; the checkpoint stays as prepared |
| 3. Enabled reload and cleanup | `working-save-expanded-summoning-verify-cleanup` | ON | written: the cleaned save |
| 4. Absence | `working-save-expanded-summoning-verify-absent` | ON | not written |

The disabled reload therefore loads the populated checkpoint and runs before the final cleanup, and the enabled
verification is the checkpoint's last consumer. The driver also compares the Working save's hash after stage 2 with
the hash after stage 1, so a write by the disabled reload stops the sequence. Each stage refuses to change anything
when loaded in the other module state. `spiderCheckpoint=preserve` is admitted only for this scope's verify-cleanup,
by the C# request validator and both PowerShell gates, with admission tests. The no-write verify-cleanup follows the
existing roster precedent, where a diagnosis-only cleanup leaves the prepared fixture unwritten. An outer envelope
keeps the original Working bytes and timestamps, restores them after the sequence or any failure, and compares every
other save with its intake hash. It is the same envelope `Invoke-ExpandedSummoningCompatibilityMechanical.ps1` uses.

**The checkpoint.** A Sorcerer 11 fixture caster, spawned from the registered `DefaultPlayerCharacter` with a saved
receipt, holds Spider Summoner and two native `MechanicActionBarSlotSpontaneusConvertedSpell` shortcuts. Each holds the
AbilityData that the variant popup builds from a known Summon Monster spell:
- the original Races Unleashed Giant Black Widow row (IV);
- the generated Giant Crab Spider 1d4+1 row (V), a higher tier than its original.

The Widow cast from its shortcut is the active spider the save carries: one marker, and both poisons at DC 16.

**What each reload asserts.**
- **Both reloads:**
  - both shortcuts resolve to their exact saved row identities, children of their saved known spells in the
    Sorcerer book, so no GUID dangles;
  - the saved Widow is back with its finite lifetime from the caster and exactly one marker;
  - both poisons read the saved DC 16, also after recalculation, never 18.
- **Disabled reload (OFF):**
  - the generated row is registered but in no published menu; its shortcut casts nothing and spends nothing;
  - the original shortcut casts Races Unleashed's own Widow with no benefit, spending one level 4 use.
- **Enabled reload (ON):**
  - the generated shortcut casts three Crab Spiders (die forced to 2), and the original shortcut one Widow;
  - each creature has one benefit and one template, 11 rounds at caster level 11, and exactly one use of its level
    is spent;
  - the saved Widow still has one marker;
  - after the feat is removed, both saved shortcuts are refused with nothing spent;
  - the fixture is retired, and no marked unit remains.

**Decision for owner review: an existing marked summon with the module OFF.** The reload asserts the current
behavior: a spider cast while the module was ON keeps its marker, and with it the poison and skill-branch web DC
benefit, until it expires. That is how saved KMG summons keep their saved state under OFF. The native CMB-branch
web-escape bonus is the exception: it reads the runtime marker identity, which is unset with the module OFF, so it does
not apply. The four Races Unleashed species have no web, so the fixture's Widow cannot observe this. The owner may
prefer every benefit dormant under OFF, or every benefit retained; either is a small product change, so none was
made.

**RU-15, RU-10 and RU-09 in the cleric phase.** These run in the native Default fixture, with their own popup
stopwatch, because the phase already used about 210 of its 240 popup seconds:
- **RU-15:** the rendered Summon Nature's Ally V popup and the Druid conversion popup, for a natural Drow Druid that
  selected the feat and for a Druid without it. With the feat, every published SNA V spider row appears once with an
  icon. Without it, none appears. The popup's row order and no-duplicate checks apply as for every popup.
- **RU-10:**
  - Crab I and Black Widow IV in the Herald popups over unrelated prepared spells;
  - caster level and engine-rule duration (20 rounds at caster level 20) on both existing Herald spider casts;
  - a natural cleric 9/wizard 1 Herald Caller's Ogre through its own Herald popup: 9 rounds at caster level 9.
- **RU-09:** that natural Herald cast spends its cleric slot and leaves its wizard book unchanged.
- **Representative metamagic, corrected plan:** Kingmaker does not carry a source's metamagic through a spontaneous
  conversion. `GetConvertedAbilityData` builds each conversion without it, and `AbilityData.ConvertedFrom` copies
  metamagic only to a variant of the same spell. So the earlier plan, "prepare the unrelated spell with Extend Spell
  and assert a doubled Herald duration", could not hold. Two cells replace it:
  - **Extend Spell Summon Monster V:** the Ogre is a variant of the prepared spell, keeps Extend, and its summon rule
    gives 40 rounds at caster level 20 from the level 6 slot;
  - **Extend-prepared unrelated level 4 source:** the Herald Ogre is a Summon Monster V from that level 5 slot, and like
    the native conversion it carries no Extend: 20 rounds.

Focused results on `65f0acce2e`: the candidate filter selects 35 tests, 0 failures, including the new
`summon-spider-summoner.persistence-stages-duration`. `Test-ExpandedSummoningWorkingSavePersistence.ps1` passed with
37 round trips and 322 fail-closed cases, including the new scope and preservation cases.
`Test-ExpandedSummoningLauncherRouting.ps1` and `Test-SummonOverhaulOffAdmission.ps1` passed. No candidate was built
and nothing ran natively.

### Fourth review: acceptance-check corrections (`04c6030c02`)

The review found three verdicts weaker than the claims they closed. Each is corrected. The existing raw evidence was
audited first; one gap needed a rerun.

1. **Poison attribution (RU-12).**
   - **The weakness:** the old verdict did not require an initial save from the Hunting poison. It accepted any
     continuing DC that equalled some initial DC, without tying a continuing save to its own poison.
   - **Audit of `deebec4edb39` mechanics:** each initial save is attributed by its reason fact and each continuing
     save by its state buff, with the expected states taken from Races Unleashed 1.0.11's `CreateSpiderPoison`
     (feature, buff) pairs.
     - Ordinary Giant Spider, Augment, Crab Spider, Ogre Spider and Tarantula: complete attribution.
     - **Black Widow incomplete:** its Hunting initial save passed (a natural 20 against Fortitude -100), so no
       Hunting state applied and there was no Hunting continuing save. The old PASS did not prove that poison's
       continuing save.
   - **The correction:**
     - Each exposure names its expected poisons by identity: feature, the state its own action tree applies, and the
       state's saves per tick. The four Races Unleashed states are cross-checked against the source pairs.
     - The pure `SpiderPoisonObservationVerdict` requires, for every expected poison, exactly one failed first save,
       its own state applied once, and exactly its per-tick continuing saves from that state at its own first DC,
       with nothing unattributed.
     - A species needs both its own and the Hunting poison. The target's save d20 is forced to 2 so that every state
       applies.
     - Focused negatives cover the review's counterexample and the recorded Widow shape.
   - **Native result:**
     - **Ordinary Giant Spider:** 14 against 12, first and continuing.
     - **Each species:** both poisons 16 against an unbonused 14, each with its own continuing save at 16. The Ogre
       poison's state has two stat-damage saves per tick (Races Unleashed adds a Dexterity copy), and both were
       observed at 16.
     - **Augment Summoning:** 16 against 14.
2. **Route creatures (RU-09, RU-11).**
   - **Audit of `deebec4edb39` routes:** the receipts already showed the right creatures:
     - Druid conversion V: one Ogre Spider;
     - Druid conversion VII: three Crab Spiders;
     - Sorcerer: one Ogre Spider with one Summon Monster template;
     - Hunter: one Crab Spider.

     Each had one marker and a finite lifetime, but the predicates did not enforce any of it.
   - **The correction:** the pure `SpiderRouteObservationVerdict` requires:
     - the expected count, each creature the chosen spider with one marker;
     - one KMG template for Summon Monster, or no template and the caster's alignment for Summon Nature's Ally;
     - no native template, and a live, finite `SummonedUnitBuff` lifetime.

     The `04c6030c02` routes run passed it on the same four casts. The debit checks are unchanged.
3. **Loaded CMB escape (RU-13).**
   - **The weakness:** the loaded escape was always compared across the marked and unmarked summons. Without Call of
     the Wild that escape is the CMD-reading native grapple, so it depended on equal baseline CMDs.
   - **The correction:** the comparison follows the rule the loaded escape actually used.
     - **CMD-reading** (`RuleCombatManeuver`): the same marked spider makes a second real web hit, with its uses
       restored to the starting amount. Its marker is removed after the hit, the loaded escape runs from that real
       state, and the marker is restored.
     - **DC-reading:** the DC is fixed in the web context at the hit, so it is compared with the matched unmarked
       summon's fresh cast.
     - Cross-summon CMD pairs are diagnostics only.

### What the native runs established

- **Real web hit → held state → loaded escape** (RU-13), in both profiles (`04c6030c02`):
  - **The hit:** a ranged-touch attack hit, no initial save was rolled, uses went 2→1→0 on each spider (restored to
    2 for the same-spider baseline hits), and the held state's source, parent context and duration were exact.
  - **Combined profile (Call of the Wild's loaded escape, DC-reading on both branches):** skill and CMB were each 14
    for the feat summon against 12 for the matched unmarked summon.
  - **Without Call of the Wild (native loaded escape):**
    - **Skill branch:** DC-reading, 14 against 12 across matched summons.
    - **CMB branch:** the grapple maneuver against the spider's CMD, 14 with the marker and 12 for the same spider
      after its own second real hit with the marker removed.
  - **Native configured CMB escape:** within one spider, 18 against 16 in the combined profile and 14 against 12
    without Call of the Wild.
  - **Cross-summon pair, diagnostic only:** in the combined profile the unmarked summon's configured CMB escape read
    12, against the marked spider's own 16 without its marker. A cross-summon comparison would have reported +6, not
    +2.
  - **Ordinary grapple:** unchanged by the marker, 16/16 combined and 12/12 without Call of the Wild.
- **Actual poison saves** (RU-12, RU-14), each from one forced-hit bite in an isolated exposure, with the target kept
  alive and unharmed and every save forced to fail: complete per-poison attribution, values in correction 1 above.
  - **Ineligible sources** (Bebelith, Giant Scorpion and others summonable with the feat held): no benefit.
- **Superior Summoning** (RU-05): with the 1d3 die forced to 2, the generated row summoned 2 without Superior
  Summoning and 3 with it, each spider with its summon buff, one template and exactly one benefit.
- **Routes** (RU-02/03/04/07/09/11):
  - **Natural Drow:** a Drow levelled natively from character creation selected Spider Summoner from its level 1
    feat selection.
  - **Prepared SNA:** cast all four originals, and with forced dice 1d3 → 2 and 1d4+1 → 3, each with exactly its
    slot spent and a finite `SummonedUnitBuff` lifetime.
  - **SNA toggle:** under both positions, no Summon Monster template and caster-matched alignment.
  - **Druid conversion** at tiers V and VII: exactly the prepared parent's spider rows, no duplicates, source chain
    kept, exactly the source slot spent, and the creature identities above.
  - **Druid without the feat:** spider rows hidden and unavailable.
  - **Natural Herald:** a cleric/wizard Herald Caller from native progression holds the conversion feature and two
    books. Its three slot cells are diagnostics; no natural Herald cast is claimed.
  - **Sorcerer (spontaneous SM) and Call of the Wild Hunter (spontaneous SNA):** each lost exactly one pool use, with
    the creature identities above, and the stale choice was refused with nothing spent. Each learned its summon spell
    as an ordinary known spell from its class list.
- **Herald spider casts** (RU-10, cleric phase, `106cda92f3`): the repaired Ogre and the Crab 1d4+1 (2 to 5 creatures)
  through the real Herald Summon Monster V popup over an unrelated prepared spell. Both had the native command and
  rule, the retained quantity conversion in the chain, and exactly one original slot spent. Caster level and
  duration were not asserted.
- **Module OFF** with RU installed (RU-17/16 lifecycle, `deebec4edb39`):
  - the eight original rows keep their exact Races Unleashed shape;
  - the tier I Crabs attach through Races Unleashed's own `CustomSpontaneousSpell`;
  - no KMG row is listed;
  - the original Widow casts with its native template and no benefit.
- **RU absent** (RU-17, `deebec4edb39`): the integration reports `absent:`, there are no spider rows or foreign
  identities, and the ordinary Giant Spider casts with no benefit.
- **NPC** (RU-20, bounded control, `04c6030c02`):
  - **Census:** no loaded AI cast action names a spider row.
  - **Ordinary variant:** an NPC without the feat got a cast command from the native AI action, and the AI-built
    ability really summoned.
  - **Gated variant:** it yielded no cast command; the AI returns a move.
  - **Limits:**
    - That no selection or refusal loop arises is an inference from the inspected AI contract (each
      `BlueprintAiCastSpell` names one exact variant) and these observations. It is not an autonomous brain and
      command-queue run.
    - The receipt's expectation text, written before this qualification, still says "so no selection or refusal
      loop".
    - The AI-built ability was executed by the native ability executor; the command-layer spend is not part of this
      control.

## History: third review task


| Candidate | Package / DLL | Run | Result |
| --- | --- | --- | --- |
| `e6723d8723206d1fc213e98cb1132e4833862bb0` (product: CMB web-escape bonus) | `b916bc08...0cb9` / `298b3fa2...452c` | `20261110T0329183919821Z-8081e6929acb4a5b8424e39ad87970fd` | **FAIL, 67 / 2**: guard refusal, before any spider cell |
| same | same | `20261110T0414271665285Z-af430dceb7de452a899db21058d3ef86`, **controlled input** | **PASS, 174 / 0** |
| `655ab8a8ae3e` (observer: rule and branch assertions, real grapple control, in-phase real web hit) | `0483c0ea...46dd` / `79f5e7d0...8c5c` | `20261110T0427495327289Z-81a9b3302a4b4e3d8842109f39574375`, controlled input | **FAIL, 155 / 9**: observer defects, below |
| `182717dd10` (observer fix) | not built | none | none |

**Controlled input.** No human used DATA during the two controlled runs. The pointer was parked at (300, 200), over
fog with no interactive UI, before launch, and sampled every 5 seconds. All 62 samples on the first run and all 56
on the second read (300, 200). Both runs were launched from the clean candidate commit in the main repository; the
development receipt binds each package to that path. The profile is the current DATA combined installation, with
Races Unleashed 1.0.11 and Call of the Wild loaded. Restoration was `verified` after every run: live tree
`5C3E4AD4...BFBB`, `Params.xml` `E44F4495...30BB`.

**Popup-teardown guard refusal.** Recorded as possible input interference, not a confirmed root cause. It stopped run
1 of `8c1734743cce` and the first run of `e6723d872320`, both at the Summon Monster V popup around the repaired Ogre
Spider cast. The two receipts differ:
- In run 1 the first close inside the cast failed, with the popup still open.
- In the `e6723d872320` run, the second, outer close after the cast failed, with the popup already cleared.

Both refused a tooltip context that had no ability and no live trigger. The owner reports probably using the mouse
during those runs. Under controlled input the same unchanged package passed, and the next candidate's run passed
that step too. That supports the interference hypothesis without proving what caused the earlier refusals; both
failures are retained. The guard was not changed.

**`655ab8a8ae3e` run: observer defects, not gameplay.**
- **Assertion bug:** the new rule-problem check compared a Json.NET token type. A null string stored in a `JObject`
  is a `String` token holding null, so the check failed although every escape triggered exactly its expected rule
  and branch. Fixed in `182717dd10`.
- **In-phase real web hit:** the cell created a hostile and fired the web through the correction scenario's
  detached-ability helper. The command was interrupted (`result=Interrupt`), and a second, `Pause`, mode was left on
  the native mode stack. Every later cell then failed its ownership checks, by design. Those later failures are
  consequences, not findings. The cell is withdrawn in `182717dd10`. A feat spider's real web hit needs a fixture
  that owns its own clock and hostile, such as the correction rules scenario.

The recorded values in that run still answer the review's two observer findings:
- **Loaded branches:** every escape attempt triggered exactly the rule and branch its implementation requires,
  identified as `native` or `cotw`. Native skill used `SkillAthletics`, native CMB a `Grapple` maneuver, loaded skill
  `SkillAthletics` and loaded CMB `AdditionalCMB`. Each was 14 with the marker and 12 without.
- **Ordinary grapple:** a real `RuleCombatManeuver` against the spider with no web reason had a target number of 12
  both with and without the marker.

Earlier candidates on this branch are retained, not replaced. Their evidence stays bound to their own bytes:

- `8c1734743cce`, package `919d9753...a250`, DLL `aa2fd336...5f17`, MVID `934bb8d1-e946-4609-ad4d-40ac584f8e9c`:
  - run 1 `20261110T0227019018671Z-...`: **FAIL, 64 / 4**, the same guard refusal as above;
  - run 2 `20261110T0231072443394Z-...`, matched rerun: **PASS, 153 / 0**. The run 2 evidence for RU-01, 02, 03, 04,
    06, 07, 08, 12, 18 and 19 below is bound to this candidate.
- `ae714f04965b`, package `27854009...ec9b`, DLL `511c1d3a...4e4d`, MVID `dc2771c2-05ab-4afc-83ae-46fae8be9a4a`, run
  `20261109T2245072861516Z-...`: **PASS, 139 / 0.** Its "held selection" cell built a new `AbilityData` after the feat
  was removed, so it proves only that a fresh choice without the feat is refused (see RU-02).
- `3e27c5809a9d`, package `461dc4f1...9953`, DLL `a9aac03e...2f1f`, run `20261109T2231457729890Z-...`: **FAIL, 132 / 3.**
  This mission's own quantity check, which assumed a fixed dice form, withheld all 38 generated rows.

Raw evidence stays private under `runtime-evidence\`.

## Review corrections (PR #125 review)

1. **One contract for repair, classification and conversion.** A row is now either the integration's exact
   repaired result, or exactly the known unrepaired input, or unresolved. The permissive fallback, which accepted
   any subset of a whitelist of after-spawn actions, is gone, so a row refused for repair can no longer be classified
   as verified. Conversion routes (Herald Caller, Druid) admit only a verified repaired row.
2. **Variants components.** Only `$KMG_ExpandedSummoning_Variants` on a tier I row is recognized. Races Unleashed
   inherits it by copying the already-published native tier I parent (read from its decompiled
   `CreateSpiderSummon`). The repair removes only that component. Any other variants component, or that one on
   another tier, makes the row unsupported, so it is left untouched and unclassified.
3. **Full buff applications.** The signature records each native buff application's recipient, permanence,
   duration, spell source, dispellability and parenting. The input contract requires them to equal the loaded
   native donor that Races Unleashed copied. The repaired contract requires the summon buff to be applied exactly as
   the donor applies it, with the lifecycle exactly `summon buff, family action, benefit`.
4. **Stale selection observer.** The observer now opens the real popup while the feat is held, keeps the exact
   `AbilityData` offered, removes the feat and executes that same object. It does this for an original row and a
   generated row. The fresh no-feat refusal is kept, under its own name.
5. The manifest now loads only on the integration's single attempt, which resolves the earlier review item.

Focused negatives for each case are in `summon-spider-summoner.contract-agreement`.

## Loaded DC consumers

**Poison.** The initial save (`ContextActionSavingThrow`) and the continuing saves (`BuffPoisonStatDamage`, DC
`Context.Params.DC` plus 2 per additional dose) read the poison feature's calculated parameters through the
context chain. Call of the Wild does not modify these poisons. No product change is indicated. The actual
saves are now observed with per-poison attribution: first and continuing saves from each expected poison, see the
fourth review corrections. Each comparison uses one isolated exposure, because extra doses legitimately raise the
DC; the Bebelith investigation shows how that can be mistaken for a calculation defect.

**Web.**
- **Inheritance:** the shared webbed buff `a719abac...` has no parameter calculator of its own, so its context
  inherits the web ability's parameters. The +2 therefore reaches `context.Params.DC`.
- **Without Call of the Wild:** the installed native break-free action is `ContextActionBreakFree` with **UseCMB on
  and UseCMD off**. This was read offline from the installed `sharedassets1.assets`, following the buff to its
  `AddFactContextActions.NewRound` element. Native `TryBreakFree` uses that DC for the skill check. When the
  target's CMB beats its best of Mobility and Athletics, it instead rolls a grapple maneuver against the spider's
  CMD and reads **no DC at all**, neither the base DC nor the +2.
- **With Call of the Wild (this profile):** its `fixGrappleSpells` replaces the action on this exact buff with
  `ContextActionBreakFreeFromSpellGrapple`, which reads the DC on both branches. Run 2 observed that loaded
  replacement, and a context DC of 12 without the marker and 14 with it.

## Native CMB web escape (RU-13), implemented in e6723d872320

The second review recommended completing the benefit on the native CMB branch, and that is implemented as
`SpiderSummonerWebEscapeDifficulty` on the feat marker. It adds exactly 2 to the one `RuleCalculateCMD` that sets
the escape maneuver's target number, and only when all of these hold:

- the calculation is a grapple CMD for this spider;
- it is directly enclosed by a grapple `RuleCombatManeuver` whose initiator is the same escapee;
- that maneuver's reason context is the webbed state `a719abac...`, whose parent context is the Giant Spider web
  `ea284589...` and whose caster is this spider;
- the spider holds the feat marker.

The component keeps no state between rules. The decision is the pure `SpiderWebEscapePolicy`, tested in
`summon-spider-summoner.web-escape-scope`. Its boundaries are exactly those the review set:
- no persistent or general CMD bonus;
- no change to ordinary grapples or other maneuvers;
- no change for other web sources such as the Bebelith, or for unmarked spiders;
- no second adjustment on paths that read the bonused DC, because the native skill check and Call of the Wild's
  replacement trigger no maneuver;
- no change to the web attack, duration, uses or the shared webbed blueprint.

The CMD baseline is kept; it is not replaced by the web DC.

**Native result (`e6723d872320`, controlled input, PASS).** The summoned ordinary Giant Spider with the feat attempted
each escape with and without the marker. The webbed state was applied in the spider's own web execution context, and
the escapee's BAB or skills selected the branch.

| Path | Rule that ran | Without marker | With marker | Spider without the feat |
| --- | --- | ---: | ---: | ---: |
| Native skill | `RuleSkillCheck`, `SkillAthletics` | 12 | 14 | 12 |
| Native CMB | `RuleCombatManeuver`, `Grapple`, against the spider's CMD | 12 | **14** | 12 |
| Loaded (Call of the Wild) skill | `RuleSkillCheck`, `SkillAthletics` | 12 | 14 | 12 |
| Loaded (Call of the Wild) CMB | `RuleSkillCheck`, `AdditionalCMB` | 12 | 14 | 12 |

The new component therefore raises the native CMB branch's actual target number by exactly 2 through the engine's own
rule chain, and no path is adjusted twice.

Two limits apply to this candidate's verdict. Its pass condition did not itself assert the loaded branches, and its
control was a standalone CMD calculation. The next candidate's observer asserts both, using a real grapple
maneuver, and its recorded values already match (see above). The projectile hit itself is not part of this
observation.

**Profile without Call of the Wild.** The committed profile `gunslinger-races-unleashed` is a valid dependency set.
Races Unleashed declares no requirements, and the three mods that require Call of the Wild (ProperFlanking2,
TweakOrTreat, ZFavoredClass) are not in it.

The earlier statement that a launcher change is needed was wrong. The existing
`scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1` accepts that profile, the
`disposable-expanded-summoning-sprint32-native-review` scenario, its `-Parameters`, and a development candidate
through `-DevelopmentPurpose` with `-ReuseInstalledArtifact` and the exact `-PackagePath`. The next step is a
preflight on that route with the exact candidate. It was not run in this task, because the wrapper stages mod states
and the validation budget would not cover a profile run after the build.

## What the loaded game showed (unchanged findings)

- The Spider Summoner feature has only a Drow prerequisite. **Races Unleashed implements no DC benefit**, so this
  integration owns the +2.
- The four species copy the native Giant Spider unit (7 HD, Mid template band), keep its `HuntingSpiderPoison` and
  add their own copied poison. They have no web, so the ordinary Giant Spider's web is the only web in scope.
- Native quantity rows count as one die plus a rank bonus, the Superior Summoning hook, and from tier VII branch
  into two spawns. Generated rows clone that donor.

## Implementation map

| Concern | Where |
| --- | --- |
| Species, 46-placement matrix, exact DC blueprint set | `Summoning/SpiderSummonerCatalog.cs` |
| Exact input and repaired contracts, donors, conversion admission | `Summoning/SummonForeignRowContracts.cs` |
| Full buff applications in the signature | `Blueprints/SummonSignatureReader.cs`, `Summoning/SummonSignature.cs` |
| Live reading: variants components, donor applications | `Summoning/SummonClericAlignmentGate.cs` (`SummonForeignRowRuntime`) |
| In-place repair, generated rows, publication | `Blueprints/SpiderSummonerIntegration.cs` |
| Per-creature benefit action, DC owner, native CMB web-escape bonus | `Summoning/SpiderSummonerBenefit.cs` |
| Web-escape scope decision | `Summoning/SpiderSummonerCatalog.cs` (`SpiderWebEscapePolicy`) |
| Completion phases: real web hit, poison and summoning feats, NPC, module-off, RU-absent | `RuntimeTesting/RuntimeTestRunner.SpiderCompletion.cs` |
| Working-save persistence scope and its stage policy | `RuntimeTesting/RuntimeTestRunner.SpiderSummonerPersistence.cs`, `RuntimeTesting/SpiderSummonerPersistencePolicy.cs` |
| Rendered Druid menus, Herald admission, metamagic and natural Herald casts (cleric phase) | `RuntimeTesting/RuntimeTestRunner.SpiderInterface.cs` |
| Routes phase: natural Drow and Herald, SNA, Druid conversion, spontaneous books | `RuntimeTesting/RuntimeTestRunner.SpiderRoutes.cs` |
| Escape observation verdicts (named failures) | `RuntimeTesting/SpiderEscapeObservationVerdict.cs`, with poison and route verdicts in `RuntimeTesting/SpiderCompletionVerdicts.cs` |
| Exact module-OFF admission pair | `scripts/ExpandedSummoningOrchestration.Common.ps1`, checked by `scripts/Test-SummonOverhaulOffAdmission.ps1` |
| Conversion admission by spell family | `Summoning/DruidSummonConversionVariantPatch.cs` |
| Late hook, before the existing reorder | `Summoning/AlternateSummoningCoordinator.cs` |

**Who owns the +2.** One hidden marker buff, granted once per eligible creature by the after-spawn action, adds 2
inside `RuleCalculateAbilityParams` for exact poison and web blueprints only. Nothing accumulates across saves,
recalculation or reload. Existing summons from before an upgrade receive no marker.

**Decisions recorded for review.**
- Original rows keep their Races Unleashed names, while generated rows use family-first labels such as "Spider, Ogre (1d3)".
- Summon Nature's Ally spiders follow the existing caster-alignment action and receive no Summon Monster template.
- The cleric rule stays Summon Monster only.
- Only repaired rows are projected into conversion routes. This affects only a failed integration: an unrepaired
  Ogre or Tarantula would then keep its v0.0.181 cleric classification but lose its v0.0.181 Herald projection.

## Acceptance status

Disposition against every subrequirement of `ACCEPTANCE.md`. Native candidates are named per row.

| ID | Implementation | Focused evidence | Native evidence | Candidate | Remaining work |
| --- | --- | --- | --- | --- | --- |
| RU-01 | Done | Contract, matrix and agreement tests | All 8 rows repaired from exact inputs: `repaired=8;generated=38;problems=` | `106cda92f3` | None |
| RU-02 | Done | Input and shape negatives | Natural Drow acquisition at level 1; retained and fresh no-feat choices refused, nothing spent; ordinary Giant Spider without the feat | `106cda92f3`, `deebec4edb39` | **Saved-choice attempt after reload** (RU-16): prepared in `65f0acce2e`, not run |
| RU-03 | Done | Matrix tests | Four SM and four SNA originals cast as the named spider with a finite summon lifetime; repaired Ogre under evil and non-evil casters | `106cda92f3`, `deebec4edb39` | None |
| RU-04 | Done | Every matrix cell; quantity expression | 46/46 census; SM 1d3 and 1d4+1; SNA 1d3 → 2 and 1d4+1 → 3 with forced dice; Herald 1d4+1 | `106cda92f3`, `deebec4edb39` | None |
| RU-05 | Done | Quantity expression | Superior: forced 1d3 → 2 without, 3 with, every spider with one benefit; Augment Con 12→16 and DC interaction | `deebec4edb39` | None |
| RU-06 | Done | Selection and band policy | Celestial and fiendish on all four species, Mid band, no leftover template | `106cda92f3` | LN and CN focused only, as the contract permits |
| RU-07 | Done | None new | Choice kept, toggle changed, cast (fiendish); SNA under both toggle positions with no SM template | `106cda92f3`, `deebec4edb39` | None |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma; deity-derived control | `106cda92f3` | None |
| RU-09 | Done | Membership tests; route-creature verdict | Sorcerer feat holder; ordinary spontaneous SM (Sorcerer) and SNA (Call of the Wild Hunter) pool casts with the chosen spider, exactly one use, stale refused; natural cleric/wizard with two books | `04c6030c02` | **A spider cast from the multiclass cleric's book, with the other book unchanged**: prepared in `65f0acce2e`, not run |
| RU-10 | Done | Family-route and agreement tests | Natural Herald Caller progression (its slot cells are diagnostics); Herald spider casts (repaired Ogre and Crab 1d4+1) through the real popup with exact debit and chain | `106cda92f3`, `04c6030c02` | **Representative metamagic with its expected duration; Crab I and Widow IV Herald admission; a cast by the naturally levelled Herald; caster level and duration asserted alongside chain and debit** (the casts used the cleric phase's granted-feature Herald): prepared in `65f0acce2e`, not run |
| RU-11 | Done | Family-route tests; route-creature verdict | Druid conversion at V and VII compared with prepared SNA and cast as the chosen spider (one benefit, no template, caster alignment, finite lifetime), exact source debit, no duplicates | `04c6030c02` | None |
| RU-12 | Done | Benefit scope; poison attribution verdict | Every expected poison attributed by identity, first and continuing: ordinary 14/12; each species' own and Hunting poison 16 vs 14; Augment 16/14; no cumulative bonus | `04c6030c02` | None |
| RU-13 | Done, with native CMB-branch bonus | Benefit and web-escape scope; escape verdicts, including rule-selected loaded comparison | Real hit → held state → loaded escape, both branches, both profiles, exactly +2, CMD-reading escapes within one spider; no initial save; uses and duration unchanged; ordinary grapple unchanged | `04c6030c02` | None |
| RU-14 | Done | Exact-identity negatives | Feat on/off matched casts; ineligible sources with the feat held get nothing | `04c6030c02` | **No duplication after reload** (RU-16): prepared in `65f0acce2e`, not run |
| RU-15 | Done | Display order tests | Real SM popups including Herald conversion, sorted, no duplicates; SNA and Druid conversion rows at data level; no-feat Druid hidden | `106cda92f3`, `deebec4edb39` | **Rendered SNA and Druid conversion popups, with and without the feat**: prepared in `65f0acce2e`, not run |
| RU-16 | Done (no implementation gap known) | None | Module-OFF lifecycle without a save: dormant rows, originals intact | `deebec4edb39` | **Fresh-process save/reload of an original and a generated shortcut and an active summon; disabled-module reload**: prepared in `65f0acce2e` (four-stage sequence above), not run |
| RU-17 | Done | Module boundary | RU-absent profile; Expanded-Summoning-inactive/RU-present control | `deebec4edb39` | None |
| RU-18 | Done | Agreement tests | Zero unresolved rows | `106cda92f3` | None |
| RU-19 | Done | Observer compiles; escape verdict tests | Per-row semantics; failures retained | `106cda92f3` | None |
| RU-20 | Done | n/a | Draft PR; exact restoration; bounded NPC control (AI cast-action decisions and an executed ordinary cast; no autonomous brain run) | `04c6030c02`, `106cda92f3` | Owner review |

## Remaining native work

All four workstreams are prepared in `65f0acce2e` (see the fifth review). What remains is native:
1. **Build:** one development candidate from `65f0acce2e`'s source, run from the main repository detached at that
   commit.
2. **Cleric phase:** one controlled-input `summon-cleric-alignment` run, which now holds RU-15, RU-10 and RU-09 and
   re-observes every earlier cleric cell on the new bytes. Its process timeout is raised from 480 to 600 seconds for
   the extra casters and popups.
3. **RU-16 sequence:** four fresh processes in the Working envelope, in the order above.

The routes, mechanics, web and module-off phases need no rerun. Their code is unchanged apart from the shared cast
record helper, which this change only adds to.

**Counted-time estimate.**
- **Build and checks:** the build (about 7 minutes) and the focused checks (about 1).
- **Cleric phase:** about 6.5 minutes, about one more than before.
- **RU-16:** four Working-save processes at about 3.5 to 4 minutes each, so 14 to 16.
- **First pass:** about 29 to 31 minutes.
- **Contingency:** each new fixture on this branch has needed one corrective cycle. One rebuild with the cleric phase
  and part of the RU-16 sequence is about 14 minutes.

**Requested: raise the cumulative validation ceiling from 120 to 165 minutes.** That keeps the about 117 minutes
already spent and leaves about 48 for the work above. The remaining about 3 minutes cannot fit a build. This review
request itself does not authorize native execution.

## Validation ledger (completion continuation: 90-minute target, 120-minute ceiling)

| Category | Seconds |
| --- | ---: |
| Focused gates and the OFF admission test | about 223 |
| Candidate builds (`d94acab016`, `5107a283bd`, `df4b3e4bc6`, `deebec4edb`, `106cda92f3`, `04c6030c02`) | 2,625 |
| Guarded native transactions (27 runs, failures included) | 4,098 |
| Fifth-review gates and admission tests | 57 |
| **Total** | **about 7,003 (117 minutes)** |

- **Target and ceiling:** the 90-minute target was exceeded; the 120-minute ceiling was not.
- **Fourth-review pass:** about 1,069 seconds in all:
  - two focused gates, 29 s (the first stopped at two compile errors);
  - the `04c6030c02` build, 410 s;
  - four guarded runs, 630 s.
- **Fifth-review pass:** 57 seconds: two focused gates (19 and 15 s; a first attempt was refused in 1 s for a stray
  parameter), and the persistence admission and routing tests (22 s). Implementation is not counted.
- **Why native work stopped:** the about 3 minutes left cannot fit a build. All four workstreams are now prepared,
  so the binding limit is counted time (estimate above), not a lack of authorization to prepare them.
- **Outside the ledger:** implementation and writing time, and restoration.
- **Separate earlier ledgers:** about 1,600, 920, 800 and 1,110 seconds.

Focused results on the delivered source: the candidate filter above (34 tests, 0 failures), including the new
`summon-spider-summoner.poison-route-verdicts`. The OFF admission test passed on `106cda92f3`; its script and
admission are unchanged. The complete domain suite and release qualification are deferred.

## History: earlier ledgers


Third review task (15-minute target, 30-minute ceiling):

| Step | Seconds |
| --- | ---: |
| Refused launches (records commit checked out; development package path) | about 50 |
| Controlled-input run, `e6723d872320`, PASS | 311 |
| Focused gates, 32 selected, three times | about 60 |
| Candidate build `655ab8a8ae3e` | 410 |
| Controlled-input run, `655ab8a8ae3e`, FAIL (observer defects) | 280 |
| **Total** | **about 1,110** |

The observer fix was not built: another build and run (700 to 900 s) would reach the ceiling. Earlier tasks'
ledgers (about 1,600 s, 920 s and 800 s) are unchanged and separate.

Focused results: `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`, 32 of 2,919
selected, 0 failures, on each of `e6723d872320`, `655ab8a8ae3e` and `182717dd10`. The 785-test related selection was
run on `8c1734743cce`. The complete domain suite and release qualification are deferred.
