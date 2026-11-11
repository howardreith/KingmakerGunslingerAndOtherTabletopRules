# Spider Summoner integration: status

**Owner-accepted and complete (2026-11-11).** The owner accepted the final candidate and asked for PR #125 to be
merged and the project closed. That merge authorization supersedes the mission's original "no merge" boundary for
this one action. It is not a release: no tag, publication or permanent installation has been performed, and released
v0.0.181 keeps its documented limitation (`KNOWN-ISSUES.md`) until a release includes this work.

On the final candidate `0908e3415f`, every RU-01 to RU-20 row is closed (acceptance table below):
- **Reload defect fixed (RU-14, RU-16).** The native `Fact.TurnOn` calls a component's `OnTurnOn` before it starts
  that component listening for rules, so the marker's recalculation went unanswered and stored the base DC.
  `0908e3415f` starts the marker listening first.
- **All four persistence stages pass** in fresh processes on `KMG_AUTOMATION_WORKING`:
  - the populated checkpoint;
  - the module-OFF reload, which leaves the checkpoint unchanged;
  - the enabled reload and cleanup;
  - fresh absence.
  Reloaded spiders read the saved poison DCs (Widow 16/16, Giant Spider 14), before and after recalculation.
- **The cleric/interface phase (234/234) and the no-Call-of-the-Wild real-web phase (6/6)** pass again on the same
  bytes.

This is focused compatibility evidence, not release qualification; a release would need its own qualification. The
Races Unleashed DLL is untouched.

## Reload fix and completion (owner instruction "Go ahead and finish up. Take the time you need.")

**Diagnosis**, from the installed 2.1.7b `Assembly-CSharp`, decompiled:
- **Load order:** `UnitDescriptor.TurnOn` turns on `Progression.Features` before `Buffs`. A poison feature with
  `RecalculateOnStatChange` therefore recalculates in `Fact.TurnOn` before the marker buff answers rules.
- **Listening order:** for the marker itself, `Fact.TurnOn` calls each component's `OnTurnOn()` and only then
  `StartListenEvents()`. The `e0c4edd35b` recalculation inside `OnTurnOn` was triggered while
  `SpiderSummonerDcBonus` was not yet subscribed, so it stored the base DC again. That matches the identical values
  observed on `4328070cbb` and `e0c4edd35b`.
- **Fix (`0908e3415f`):** `SpiderSummonerDcBonus.OnTurnOn` calls the public, idempotent `StartListenEvents()` before
  `RecalculatePoison`. The native step then skips the second subscription. Each calculation is still one new rule
  answered by the one marker, so the result carries exactly one +2. No integration state is needed, so it also works
  with the module OFF.

**Final candidate.**

| Item | Value |
| --- | --- |
| Source | `0908e3415fa4`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `ec6fd46eec3bb3be393cf2a3b10289e3eab1c58bc9f14bdabc7849c10000e442` |
| DLL SHA-256 | `d464950288ddc76f07b13eba1a06c21b4dfc60a4f96d06ef2def1569e06cbbfd` |
| Selected tests | candidate filter, 35 selected, 0 failures; focused gate (`gate19`) PASS |

**Native transactions on `0908e3415f`.** All controlled input; the pointer was parked at (300, 200) on every
stage.

| Phase | Profile | Evidence directory | Result |
| --- | --- | --- | --- |
| RU-16 prepare (attempt 1) | combined DATA install | `20261111T0032030066203Z-...prepare` | PASS 10/10 |
| RU-16 disabled reload (attempt 1) | combined, module OFF | `20261111T0034143193382Z-...verify-cleanup` | **launch failure, no result**: Kingmaker exited 4 s after accepting the request, before any save interaction. Its log shows `steam_api64.dll` failing to load from `Kingmaker_Data/Plugins`; the file was present and unchanged (2 September). Treated as a transient host failure (commit headroom was about 10 GB), not product evidence. The Working save and everything else were restored, so the sequence was retried once from prepare |
| RU-16 prepare | combined | `20261111T0036043767138Z-...prepare` | **PASS** 10/10 |
| RU-16 disabled reload | combined, module OFF | `20261111T0038083175696Z-...verify-cleanup` | **PASS** 18/18; checkpoint hash `F8759038...5E86` before and after (not written) |
| RU-16 enabled reload and cleanup | combined | `20261111T0040102373144Z-...verify-cleanup` | **PASS** 15/15 |
| RU-16 fresh absence | combined | `20261111T0042335888409Z-...verify-absent` | **PASS** 6/6 |
| `summon-cleric-alignment` | combined | `20261111T0045385494739Z-...sprint32-native-review` | **PASS 234/234** |
| `spider-web` | `gunslinger-races-unleashed` (no Call of the Wild) | `20261111T0052479415395Z-...sprint32-native-review` | **PASS 6/6**, loaded escape `native` |

**What the persistence stages established.**
- **Disabled reload:** both reloaded spiders hold one marker, with the poison DCs at the saved values (Widow 16/16,
  Giant Spider 14), also after recalculation and never 18. Every web, shortcut and lifecycle check from the earlier
  candidates passes again: native CMB escape 14 vs 12 within the same spider; ordinary grapple unchanged; DC/skill
  +2; nothing doubled; the generated shortcut dormant and refused; the original RU Widow cast natively.
- **Enabled reload:** the same DCs; casting again adds no second marker to either saved spider. After feat removal,
  both saved shortcuts (generated Crab and original Races Unleashed Widow) are refused with nothing spent and nothing
  summoned (RU-02). Cleanup retires the fixture caster, pins and spiders: no receipt units, no marked units.
- **Fresh absence:** a fresh load of the cleaned Working save has no fixture caster, no fixture spider and no marked
  unit.

**Restoration, every transaction**, checked independently afterwards in Windows PowerShell 5.1 (`final_check.ps1`):
- live tree 254 files `5C3E4AD4...BFBB`;
- `Params.xml` `E44F4495...30BB`;
- Working save `B29C1CCB...55B2` with its original timestamp;
- all 95 saves equal their intake;
- no Kingmaker process.

The profile transaction restored the Call of the Wild settings and `FeatureModules.json` byte-exact. The runner also
restored UMM's launch-time rewrite of `Params.xml`.

**Drivers (scratch, not committed).** SHA-256 values:
- `run_persist.ps1` `7D1F9DB4...32AAA`;
- `run_persist_stage.ps1` `8D4B6789...2EA9FF`;
- `run_parent.ps1` `89526AFA...7AF0`;
- `run_child.ps1` `85F45E54...C8F7`;
- `run_final2.ps1` `153A8F05...8831`;
- `final_check.ps1` `2711C8E6...F12A`.

A first `run_final2.ps1` attempt under Windows PowerShell 5.1 launched nothing: its parent uses PowerShell 7's
`Start-ThreadJob`. It was rerun from PowerShell 7, whose children are 5.1.

The sections below are the earlier record of this continuation, kept as written.

## Final continuation (owner allowance 165, then 170 minutes; record as of `e0c4edd35b`)

**Product changes in this continuation.**
- **`4328070cbb`, module-off web benefit (owner decision §2):** `SpiderSummonerWebEscapeDifficulty` recognizes its
  own attached marker fact (`SpiderWebEscapePolicy.OwnMarkerHeld`) instead of the integration's static
  `SpiderSummonerRuntime.Marker`. Every escape-scope restriction is unchanged: web/held-state ancestry, owning spider,
  target and maneuver identity, the ordinary-grapple exclusion, and exactly +2. The full integration is not activated
  under OFF.
- **`e0c4edd35b`, attempted reload fix:** `SpiderSummonerDcBonus.OnTurnOn` recalculates the owner's eligible poison
  contexts. Natively it did **not** change the reloaded value (below).

**Fixture changes.**
- **RU-16:** a saved ordinary Giant Spider (published Summon Monster II row, one marker) joins the checkpoint. The
  disabled reload adds the web cell (below).
- **Extend Spell legality:** the fixture holds the native Extend Spell feat, and the unrelated source is chosen from
  spells that accept Extend.
- **Natural Herald:** it selects Desna in the native deity selection, chaotic good. Earlier, the automatic choice
  left it unable to see any spider row, and its deity was not recorded.
- **Popup checks:**
  - the Extend Spell Summon Monster V popup expects the level 6 slot's projected Herald Summon Monster VI rows
    followed by its own Summon Monster V rows;
  - rows identified exactly as Call of the Wild's metamagic conversions, or as the cleric's non-summon conversions, are
    recorded by name in `setAside` and kept out of the summon comparison.

**Final candidate.**

| Item | Value |
| --- | --- |
| Source | `e0c4edd35b3a`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `b512253a2164d4dc498f0bc01d4c4f76ef09f81476be1af6dc5385227467b132` |
| DLL SHA-256 | `2dd277f7f22ad71710c5b93a6bd80b595ce70c3f4dcb7c06f4b7b9a60adc6ad7` |
| Selected tests | candidate filter, 35 selected, 0 failures |
| Previous candidate of this continuation | `4328070cbbe2`, package `31989eeb...505f`, DLL `62ce103a...f1be` |

**Native transactions.** All controlled input; the pointer was parked at (300, 200) on every sample.

| Phase | Profile | Candidate | Evidence directory | Result |
| --- | --- | --- | --- | --- |
| `summon-cleric-alignment` | combined DATA install | `4328070cbb` | `20261110T2123080587962Z-...sprint32-native-review` | FAIL 227/231 (popup expectations; corrected) |
| RU-16 prepare (pwsh 7 host) | combined | `4328070cbb` | `20261110T2130541391075Z-...prepare` | scenario PASS 10/10; wrapper `Params.xml` receipt refused (below) |
| RU-16 prepare | combined | `4328070cbb` | `20261110T2134450063093Z-...prepare` | **PASS** 10/10 |
| RU-16 disabled reload | combined, module OFF | `4328070cbb` | `20261110T2136538798314Z-...verify-cleanup` | FAIL 16/18: reloaded poison DC |
| RU-16 prepare | combined | `e0c4edd35b` | `20261110T2157149527883Z-...prepare` | **PASS** 10/10 |
| RU-16 disabled reload | combined, module OFF | `e0c4edd35b` | `20261110T2159297583907Z-...verify-cleanup` | FAIL 16/18: reloaded poison DC |
| `summon-cleric-alignment` | combined | `e0c4edd35b` | `20261110T2201349112957Z-...sprint32-native-review` | **PASS 234/234** |
| `spider-web` | `gunslinger-races-unleashed` (no Call of the Wild) | `e0c4edd35b` | `20261110T2208571459564Z-...sprint32-native-review` | **PASS 6/6**, loaded escape `native` |

**Disabled-module reload, both candidates (16 of 18 checks pass).**
- **Checkpoint:** module state; ownership of the caster, saved Widow and saved Giant Spider through their receipts;
  both saved shortcuts resolve to their exact rows, books and source chains.
- **Generated row:** in no published menu; its shortcut casts nothing and spends nothing.
- **Original row:** its shortcut casts Races Unleashed's own Widow with no KMG marker, spending exactly one level 4
  use.
- **Web, from the saved Giant Spider:**
  - real hits (native ability executor) leave the held state from that spider's own web;
  - the **native CMB escape**, executed explicitly from the web-produced state under the loaded profile
    (`loadedEscape=cotw`), is exactly +2 against the same spider and live state with its marker removed;
  - an ordinary grapple is unchanged;
  - the web DC on the skill escape, loaded and native, is +2 against the same spider's hit with the marker absent at
    capture;
  - Call of the Wild's loaded CMB-branch escape (reading the captured DC) is +2;
  - one marker throughout, nothing applied twice.

  The no-Call-of-the-Wild web phase above is the naturally loaded native path on the final bytes.
- **The checkpoint was not written:** the driver's hash before and after the disabled stage is identical
  (`C0A2230A...` on `4328070cbb`, `764EAE94...` on `e0c4edd35b`).
- **The two failures:** `off-reloaded-active-spider` and `off-reloaded-web-spider`. The stored poison parameters after
  the load read Widow 14/14 and Giant Spider 12. After one recalculation they read 16/16 and 14, which equal the saved
  values, so a recalculation adds exactly one +2. Each spider holds exactly one marker. The values were identical on
  both candidates, so the `OnTurnOn` recalculation had no observable effect.

**The open defect and its next step (superseded: diagnosed and fixed in `0908e3415f`, above).** The stored parameters are what the poison save reads (RU-12). A reloaded
spider would therefore poison at the base DC until something recalculates it. Either the buff's `OnTurnOn` is not
called on load, or the load recalculates the poison features after it with the marker not yet answering
`RuleCalculateAbilityParams`.
1. Record the order once, natively (marker `OnTurnOn`, `OnFactActivate` and `PostLoad` against the feature's
   recalculation), or recalculate after the area load completes.
2. One build (about 7 minutes).
3. The four persistence stages (about 9 minutes; prepare is unchanged code).

That is about 16 counted minutes, beyond the remaining 6, so it needs a new owner allowance. The enabled-reload
and absence stages have not run on any candidate. They would also assert the reloaded DC.

**Cleric/interface phase on `e0c4edd35b` (234/234).**
- **RU-15:** the rendered SNA V and Druid conversion popups for the natural Drow Druid 13 with the feat list every
  published spider row once, each with an icon. For the Druid 9 without it, they list none.
- **RU-10:**
  - Crab I and Black Widow IV are admitted in the Herald popups, chained to the unrelated prepared source.
  - **Extend Spell Summon Monster V:** the Ogre keeps Extend, gets 40 rounds at caster level 20, and its level 6
    slot is the one spent.
  - **Herald spider from an unrelated level 4 spell prepared with Extend Spell:** the source is from the spell list
    and accepts Extend, and the fixture holds the native Extend Spell feat. The spider is a Summon Monster V from that
    level 5 slot with no Extend, matching the native conversion: 20 rounds at caster level 20, exact debit.
  - **Natural Herald** (cleric 9 Herald Caller of Desna, chaotic good, plus wizard 1): casts the Ogre through its own
    Herald popup from its cleric book, 54 seconds (9 rounds) at caster level 9.
- **RU-09:** that natural Herald's cleric slot is spent and its wizard book is unchanged.
- **Set aside, by exact identity:**
  - Call of the Wild's metamagic conversions (Summon Monster V, and IV, `Single`/`d3`/`d4plus1`, carrying Extend);
  - the natural Herald's Cure Light Wounds, Mass and Cure Serious Wounds conversions.

**Separate finding for the owner (not Spider Summoner scope).** Call of the Wild's
`metamagic_for_spontaneous_spell_conversion` setting (read in its `SetToggleAdditionalSpells` replacement IL) adds
conversions at the source's base level, carrying its metamagic. They are not in the native
`GetSpontaneousConversionSpells` list. Expanded Summoning's projection handles only that native list, so these show
as raw native quantity rows, for example "Summon Monster IV (Dire Wolf)" in a level 5 Herald popup. This affects every
Herald popup over a metamagic source under that setting. Projecting or hiding them is a design choice for the owner;
nothing was changed.

**Restoration, every transaction.**
- **Installation and settings:** live tree `5C3E4AD4...BFBB` (Windows PowerShell 5.1 fingerprint); `Params.xml`
  `E44F4495...30BB`; Call of the Wild settings and `FeatureModules.json` byte-exact in the profile transaction.
- **Working save:** restored to its original `B29C1CCB...55B2` with timestamps, verified. Every other save was compared
  with its intake: unchanged.
- **Host lesson:** the first prepare attempt called the scenario wrapper inside PowerShell 7. Its `Params.xml` step
  refused to complete the receipt ("Only the live owner ... completes it in place"). The driver restored the bytes,
  and `Restore-UmmParams.ps1` then closed the receipt ("unchanged"). Every later stage ran in a Windows PowerShell 5.1
  child.

**Drivers (scratch, not committed).** SHA-256 values:
- `run_persist.ps1` `7D1F9DB4...32AAA`, the original-Working envelope;
- `run_persist_stage.ps1` `8D4B6789...2EA9FF`, one 5.1 stage;
- `run_parent.ps1` `89526AFA...7AF0`;
- `run_child.ps1` `85F45E54...C8F7`;
- `run_final.ps1` `4A14E871...AE3DD`.

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
| RU-02 | Done | Input and shape negatives | Natural Drow acquisition at level 1; retained and fresh no-feat choices refused, nothing spent; ordinary Giant Spider without the feat. Saved shortcuts after reload: OFF reload refuses the generated shortcut with nothing spent; after feat removal on the enabled reload, the saved generated Crab and original RU Widow shortcuts are refused with nothing spent and nothing summoned | `106cda92f3`, `deebec4edb39`, `0908e3415f` | None |
| RU-03 | Done | Matrix tests | Four SM and four SNA originals cast as the named spider with a finite summon lifetime; repaired Ogre under evil and non-evil casters | `106cda92f3`, `deebec4edb39` | None |
| RU-04 | Done | Every matrix cell; quantity expression | 46/46 census; SM 1d3 and 1d4+1; SNA 1d3 → 2 and 1d4+1 → 3 with forced dice; Herald 1d4+1 | `106cda92f3`, `deebec4edb39` | None |
| RU-05 | Done | Quantity expression | Superior: forced 1d3 → 2 without, 3 with, every spider with one benefit; Augment Con 12→16 and DC interaction | `deebec4edb39` | None |
| RU-06 | Done | Selection and band policy | Celestial and fiendish on all four species, Mid band, no leftover template | `106cda92f3` | LN and CN focused only, as the contract permits |
| RU-07 | Done | None new | Choice kept, toggle changed, cast (fiendish); SNA under both toggle positions with no SM template | `106cda92f3`, `deebec4edb39` | None |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma; deity-derived control | `106cda92f3` | None |
| RU-09 | Done | Membership tests; route-creature verdict | Sorcerer feat holder; ordinary spontaneous SM and SNA pool casts; natural cleric 9 (Desna)/wizard 1 Herald casts the Ogre from its cleric book, cleric slot spent, wizard book unchanged | `04c6030c02`, `e0c4edd35b`, `0908e3415f` | None |
| RU-10 | Done | Family-route and agreement tests | Crab I and Widow IV Herald admission; Extend Spell SM V Ogre keeps Extend, 40 rounds at CL 20, level 6 slot spent; Herald spider from an Extend-prepared unrelated level 4 spell: no Extend like the native conversion, 20 rounds at CL 20, level 5 slot debit; natural Herald cast at CL 9, 9 rounds; earlier repaired-Ogre and Crab 1d4+1 Herald casts | `0908e3415f`, `e0c4edd35b`, `04c6030c02` | None (Call of the Wild metamagic-conversion rows are a separate owner finding) |
| RU-11 | Done | Family-route tests; route-creature verdict | Druid conversion at V and VII compared with prepared SNA and cast as the chosen spider (one benefit, no template, caster alignment, finite lifetime), exact source debit, no duplicates | `04c6030c02` | None |
| RU-12 | Done | Benefit scope; poison attribution verdict | Every expected poison attributed by identity, first and continuing: ordinary 14/12; each species' own and Hunting poison 16 vs 14; Augment 16/14; no cumulative bonus | `04c6030c02` | None |
| RU-13 | Done, with native CMB-branch bonus | Benefit and web-escape scope; escape verdicts, including rule-selected loaded comparison | Real hit → held state → loaded escape, both branches, both profiles, exactly +2, CMD-reading escapes within one spider; no initial save; uses and duration unchanged; ordinary grapple unchanged | `04c6030c02` | None |
| RU-14 | Done | Exact-identity negatives; own-marker recognition | Feat on/off matched casts; ineligible sources get nothing; after reload with the module OFF and ON, each spider holds exactly one marker and stored poison DCs equal the saved values (Widow 16/16, Giant Spider 14), also after recalculation, never 18; casting again adds no second marker; module-OFF web: native CMB +2, DC/skill +2, ordinary grapple unchanged, nothing doubled | `04c6030c02`, `0908e3415f` | None |
| RU-15 | Done | Display order tests | Real SM popups including Herald conversion; rendered SNA V and Druid conversion popups for the natural Drow Druid with the feat (every spider row once, with icons) and a Druid without it (none) | `106cda92f3`, `e0c4edd35b`, `0908e3415f` | None |
| RU-16 | Done | Stage policy | Four fresh processes on `KMG_AUTOMATION_WORKING`: prepare 10/10; OFF reload of the populated checkpoint, not written (hash identical), 18/18: exact shortcut identities, generated row dormant and refused, original RU Widow cast with no marker, saved poison and web benefit retained; enabled reload and cleanup 15/15; fresh absence 6/6 | `0908e3415f` | None |
| RU-17 | Done | Module boundary | RU-absent profile; Expanded-Summoning-inactive/RU-present control | `deebec4edb39` | None |
| RU-18 | Done | Agreement tests | Zero unresolved rows | `106cda92f3` | None |
| RU-19 | Done | Observer compiles; escape verdict tests | Per-row semantics; failures retained | `106cda92f3` | None |
| RU-20 | Done | n/a | Draft PR; exact restoration; bounded NPC control (AI cast-action decisions and an executed ordinary cast; no autonomous brain run) | `04c6030c02`, `106cda92f3`, `0908e3415f` | None: owner-accepted 2026-11-11; PR #125 merged on the owner's instruction |

## Remaining native work

None. The project is closed as owner-accepted. Deferred by the validation policy to any future release, not by this
mission:
- the complete domain suite;
- release qualification.

Separate owner decision, unchanged: Call of the Wild's metamagic-conversion rows in Herald popups (see the final
continuation section).

The earlier routes, mechanics and combined-web phases ran on earlier candidates (named per row). The changes since
then touch only the escape handler's marker recognition and the marker's reload recalculation. The final candidate
reran the phases covering both changes: persistence, cleric/interface, and no-Call-of-the-Wild web.

## Validation ledger (ceiling raised by the owner to 165, then 170 minutes; then "Take the time you need")

| Category | Seconds |
| --- | ---: |
| Earlier passes of this continuation (below) | about 7,003 |
| Final continuation: focused gates (`gate17` 4 s stopped at a compile error, 18 s; `gate18` 17 s) | 39 |
| Final continuation: builds (`4328070cbb` 421 s; `e0c4edd35b` stopped by the harness at low memory, about 374 s; `e0c4edd35b` 412 s) | about 1,207 |
| Final continuation: guarded native runs (cleric 376 and 381; prepare 111, 129 and 132; disabled reload 122 and 124; no-Call-of-the-Wild web 195) | 1,570 |
| Subtotal at the 170-minute stop | about 9,819 (164 minutes) |
| Completion: build `0908e3415f` | 433 |
| Completion: persistence attempt 1 (prepare 132; disabled-reload launch failure 47) | 179 |
| Completion: persistence (prepare 123, disabled reload 122, enabled reload and cleanup 143, absence 113) | 501 |
| Completion: cleric 376; no-Call-of-the-Wild web 180 | 556 |
| Completion: focused gate (`gate19`) | 16 |
| **Total** | **about 11,504 (192 minutes)** |

- **Completion allowance:** the owner's instruction "Go ahead and finish up. Take the time you need." covered the
  diagnosis, one build and the runs above, about 28 counted minutes beyond the 170-minute stop.

- **Earlier passes:** gates and admission tests about 280; six builds 2,625; 27 guarded runs 4,098.
- **The stopped build:** the session harness stopped it for low system memory during packaging. On the owner's
  instruction the ceiling was raised to 170 minutes and the build restarted.
- **Why native work stopped at 170 minutes:** the about 6 minutes left could not fit a build. The owner then
  allowed the completion.
- **Outside the ledger:** implementation, writing, diagnosis reads (including the decompiles), restoration (including
  the `Params.xml` receipt recovery), and the zero-launch `run_final2.ps1` attempt under 5.1.

Focused results on the final source `0908e3415f`: the candidate filter (35 tests, 0 failures), including
`summon-spider-summoner.web-escape-scope` with the own-marker cases; focused gate PASS. The complete domain suite and release
qualification are deferred.

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
