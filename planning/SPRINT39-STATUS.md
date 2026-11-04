# Sprint 39 status — Nalfeshnee

**IN PROGRESS.** Not released, not qualified, nothing claimed as proven on a
frozen artifact. Sprint 38 is closed at v0.0.176 and is not reopened: its
artifact, its original four-armed body, its native adaptations, its approved
Reverse Gravity and Veil omissions and its completed Vrock spell-resistance
finding all stand.

## Working identities

| Identity | Value |
|---|---|
| product branch | `codex/expanded-summoning-sprint39-nalfeshnee` |
| records branch | `codex/expanded-summoning-sprint39-release-records` (not yet created) |
| product/build/native-test/promotion root | `C:/Dev/KingmakerGunslingerLab/wt/s39` |
| base commit | `c69212d6c7871c8f3ebf043bfffa53ee3851455c` (= `origin/master`, verified by `git ls-remote`) |
| expected version | `v0.0.177`, only if actually available at publication time |

The promotion root is fixed **now**, before any native qualification, because
this repository's exact-reference build is byte-deterministic within a
repository root but not across roots, and the publisher can only promote from
the root where the artifact was built.

Sprint-number collision, checked rather than assumed: `planning/SPRINT-39-ENTRY-CRITERIA.md`
and `tools/validate_sprint39.py` already exist and belong to the **Gunslinger**
series' Sprint 39, Pistol-Whip (`INFORMATIONAL_VERSION = "0.0.39-s39-pistol-whip"`).
They are unrelated to this sprint and are not touched. This sprint follows the
Expanded Summoning naming convention instead — `planning/SPRINT39-STATUS.md`,
`planning/SPRINT39-RULES.md`, `planning/SPRINT39-RELEASE-EVIDENCE.json` and
`tools/validate_expanded_summoning_sprint39_release177.py` — which is how
Sprints 32 through 38 are named.

## Intake completed

* `AGENTS.md` and `docs/VALIDATION-POLICY.md` read. This is an explicitly
  selected sprint/release mission, so the ordinary 30-minute validation ceiling
  does not gate each authorized stage.
* `planning/SPRINT38-STATUS.md` read in full as the structural template, and
  its stale active banner corrected as an authorized documentation-only
  carry-forward (below).
* Worktree created at `origin/master` from an **unelevated** shell, so no
  `BUILTIN\Administrators` dubious-ownership repair was needed. Both tool
  shells are unelevated in this session; this was verified rather than assumed,
  and it is also the state the guarded runtime harness requires.
* The two gitignored local build inputs copied in from `wt/s38`:
  `GamePath.props` and `artifacts/inspection/bodyguard-native` (99 MB, read by
  the native-contract tests). Neither is in git, and `artifacts/` is ignored,
  so the worktree still reads clean.
* Promotion root's toolchain confirmed: `scripts/Invoke-KmgGate.ps1 -Filter
  'expanded-summoning.sprint38'` at `c69212d6` — production compiled, 2,801
  registered tests, 5 selected, 0 failures, 30.6s.
* The charter document itself is **not on this machine**. The mission's quoted
  Sprint 39 charter row (page 27) is used as supplied.

### Sprint 38 documentation carry-forward (documentation only)

Three corrections to `planning/SPRINT38-STATUS.md`, with no rebuild, no new
Sprint 38 release and no change to any released byte. Dated contents and
recorded failures are preserved verbatim.

1. The **active top banner** read "IN PROGRESS. Not released, not qualified,
   nothing claimed as proven on a frozen artifact." That was true when written
   and went stale when the sprint released. It now states the released identity
   and points at the appended `Release qualification, 2026-11-04` section and
   `SPRINT38-RELEASE-EVIDENCE.json` as the authority, and it records what the
   banner previously said.
2. The **Reverse Gravity impossibility claim** was narrowed. The recorded
   search result is unchanged and retained — zero occurrences of `gravit` or
   `levitat` in 103 MB of IL against working positive controls — but the
   inference drawn from it ("The engine has no gravity and no levitation
   concept at all… A faithful implementation is impossible") overstated what a
   two-name substring search can establish. Reverse Gravity was omitted by the
   recorded owner decision of 2026-11-03 as a balance and fidelity choice, and
   that decision does not rest on the narrowed inference.
3. The forward-looking **Early bounded probe**, **Review corrections** and
   **Progress** sections are marked historical, because their "still UNKNOWN",
   "outstanding" and "pending" statements describe the open sprint rather than
   the closed one.

## The printed creature

Transcribed from the live Archives of Nethys entry on 2026-11-03, not from
recall and not from a sibling demon's constants. Nalfeshnee CR 14, *Pathfinder
RPG Bestiary* pg. 65.

```
CE Huge outsider (chaotic, demon, evil, extraplanar)
Init +5; Senses darkvision 60 ft., true seeing; Perception +31
Aura unholy aura (DC 23)
AC 29, touch 13, flat-footed 28 (+4 deflection, +1 Dex, +16 natural, -2 size)
hp 203 (14d10+126)
Fort +22, Ref +9, Will +21
DR 10/good; Immune electricity, poison; Resist acid 10, cold 10, fire 10; SR 25
Speed 30 ft., fly 40 ft. (poor)
Melee bite +23 (3d8+11/19-20), 2 claws +23 (2d6+11)
Space 15 ft., Reach 15 ft.
Special Attacks unholy nimbus
Spell-Like Abilities (CL 12th)
  Constant - true seeing, unholy aura (DC 23)
  At will  - call lightning (DC 18), feeblemind (DC 20),
             greater dispel magic, slow (DC 18),
             greater teleport (self plus 50 lbs. of objects only)
  1/day    - summon (level 5, 1 nalfeshnee 20%, 1d4 hezrous 40%,
             or 1d4 vrocks 50%)
Str 32, Dex 13, Con 29, Int 23, Wis 22, Cha 20
Base Atk +14; CMB +27; CMD 42
Feats Awesome Blow, Cleave, Improved Bull Rush, Improved Critical (bite),
      Improved Initiative, Iron Will, Power Attack
Skills Bluff +22, Diplomacy +22, Fly +10, Intimidate +19,
       Knowledge (arcana) +23, Knowledge (planes) +23,
       Knowledge (any one other) +20, Perception +31, Sense Motive +23,
       Spellcraft +23, Stealth +10, Use Magic Device +22;
       Racial Modifier +8 on Perception
Languages Abyssal, Celestial, Draconic; telepathy 100 ft.

Unholy Nimbus (Su): Three times per day as a free action a nalfeshnee can
create a nimbus of unholy light, causing nauseating beams of writhing colour
to play around its body. One round later, the light bursts in a 60-foot
radius. Any non-demon creature caught within this area must succeed on a DC 22
Will save or be dazed for 1d10 rounds as visions of madness hound it. The save
DC is Charisma-based.
```

### Every printed total derives; nothing is pinned as a result

The point of this table is that no difficulty class, save total, attack bonus
or armour class is hard-coded as an outcome anywhere in the implementation.

| Printed | Derivation | Checks |
|---|---|---|
| Init +5 | +1 Dex, +4 Improved Initiative | yes |
| AC 29 | 10 +16 natural +1 Dex −2 size **+4 deflection from its own unholy aura** | yes |
| racial AC without the aura | 10 +16 +1 −2 = **25** | the +4 is the buff's, not the blueprint's |
| Fort +22 | good base 2+14/2 = 9, +9 Con, **+4 aura resistance** | yes |
| Ref +9 | poor base 14/3 = 4, +1 Dex, **+4 aura resistance** | yes |
| Will +21 | good base 9, +6 Wis, +2 Iron Will, **+4 aura resistance** | yes |
| racial saves without the aura | Fort +18, Ref +5, Will +17 | the +4s are the buff's |
| bite/claw +23 | BAB +14, +11 Str, −2 size | yes |
| damage +11 | Str 32 → +11; bite and both claws are primary | yes |
| CMB +27 | +14 BAB, +11 Str, +2 Huge | yes |
| CMD 42 | 10 +14 +11 +1 Dex +2 size **+4 deflection** | yes |
| Perception +31 | 14 ranks, +6 Wis, +8 racial, +3 class skill | yes |
| call lightning / slow DC 18 | 10 + 3 + 5 Cha | yes |
| feeblemind DC 20 | 10 + 5 + 5 Cha | yes |
| unholy aura DC 23 | 10 + 8 + 5 Cha | yes |
| unholy nimbus DC 22 | Su: 10 + ½(14 HD) + 5 Cha | yes |
| SR 25 | CR 14 + 11, the creature's **own general** SR | yes |

Two consequences worth stating before any code exists.

**The aura is already in the printed defences, so it must not be added twice.**
AC 29, all three save totals and CMD 42 each already include the constant
*unholy aura*'s +4. The blueprint therefore carries natural armour 16 and the
racial save bases above, with **no** deflection and **no** resistance bonus of
its own, and the aura supplies them. Deriving the racial values separately is
the whole reason the table above exists.

**General SR 25 and the aura's conditional SR 25 are different facts that
happen to print the same number.** The creature's own SR 25 is CR + 11 and
applies to everything. *Unholy aura* separately grants SR 25 **against good
spells and spells cast by good creatures**. They coincide numerically here,
which is exactly why collapsing them into one would be easy and wrong.

**14 racial HD but caster level 12.** The spell-like caster level is not the
hit dice, as it was not for the Glabrezu either, and the two are used in
different places. 14 HD drives BAB, save bases, skill ranks and the Su DC's
half-HD term; CL 12 drives the spell-like DCs, spell resistance penetration
and caster-level-dependent ranges.

**Not this creature's rules, though a sibling's cloning would suggest them.**
No rend, no Great Cleave, no Vital Strike, no extra limbs, and 14d10+126 with
CL 12 rather than the Glabrezu's 12d10+120 with CL 14. The feat list is
exactly Awesome Blow, Cleave, Improved Bull Rush, Improved Critical (bite),
Improved Initiative, Iron Will, Power Attack. The bite's 19–20 threat range is
*Improved Critical (bite)* and must be delivered **once**, not through both
weapon metadata and a feat.

## One creature and one public route

`key = nalfeshnee`, adult, untemplated, Summon Monster IX only — exactly the
Glabrezu's registration shape, which contributes one placement at quantity One
because `GenerateVariants` admits a tier-9 creature for parent 9 alone.

| Quantity | Reviewed baseline | With Nalfeshnee |
|---|---|---|
| project creatures (`UnitCount`) | 132 | 133 |
| Monster roster / placements | 110 / 598 | 111 / 599 |
| Nature's Ally roster / placements | 105 / 588 | 105 / 588 (unchanged) |
| generated roots + wrappers = choices | 1,186 + 29 = 1,215 | 1,187 + 29 = 1,216 |

No Nature's Ally row, no lower-tier route, no alignment-template children, no
Ice Devil, no Sprint 40 content. All counts are derived from source and
re-derived before the candidate freeze; legitimate concurrent additions are
preserved rather than overwritten.

`SummonDemonIdentityCatalog` goes from seven project demons to **eight**,
adding `nalfeshnee` ("CE Huge outsider (chaotic, demon, evil, extraplanar)")
with every existing member and every recorded exclusion preserved. That same
classification serves both the Nimbus's non-demon eligibility and the Vrock's
existing screech exemption, so the two cannot drift. The catalog's recorded
campaign/third-party taxonomy limitation stays a disclosed limitation and is
not treated as permission to omit a known project member.

## Spell-like and special-ability disposition

One explicit table. "Excluded by charter" means the Sprint 39 charter row
itself excludes it; it is not a new omission, and Sprint 38's Reverse
Gravity/Veil and Sprint 37's Holy Water omissions authorise nothing here.

| Ability | Disposition | Basis |
|---|---|---|
| unholy nimbus (Su, 3/day) | **in scope — signature** | charter required outcome |
| feeblemind (at will) | **in scope — signature** | charter required outcome |
| greater dispel magic (at will) | **in scope — signature** | charter required outcome |
| unholy aura (constant) | **in scope** | printed constant SLA, already in printed defences |
| true seeing (constant) | **in scope — reuse** | released S38 native identity |
| call lightning (at will) | **in scope — reuse** | released S29 native implementation |
| slow (at will) | **in scope — reuse** | released S36 native Slow |
| greater teleport | **excluded by charter** | planar travel |
| summon (1/day, demons) | **excluded by charter** | summonable demon reinforcements |

## Reuse / gap / smallest change / proof map

Read from the actual contracts of the released code, not from file names.

| Requirement | Existing consumer | Actual gap | Smallest change | Proof |
|---|---|---|---|---|
| registration + one SM IX route | `ExpandedSummoningCatalog`, Glabrezu's tier-9 shape | one row | one `C(...)` row; `ValidateFamily` 110/598 → 111/599; `UnitCount` 132 → 133 | domain counts + public route observation |
| demon identity | `SummonDemonIdentityCatalog` | 7 pinned | add key, pin 7 → 8, add membership assertion | domain + Nimbus eligibility + Vrock exemption control |
| profile / saves / skills | S36–38 demon chassis | none | new policy constants + exact racial profile | live-profile observation vs printed table |
| bite + 2 claws, primary | released natural-weapon modifier handling | none | three weapons, bite crit edge 19 **only** | live attack routine; no doubled crit range |
| Cleave, Awesome Blow, Improved Bull Rush | released private ordinary Cleave + Awesome Blow / Improved Bull Rush | none | reuse at printed identities | native action-cost observation |
| flying representation | established ordinary-map flying, one effective movement mode | none | reuse; record printed land/fly split | movement observation |
| constant true seeing | S38 `NativeTrueSeeingBuff38` = `09b4b69169304474296484c74aa12027` | none | reuse exact identity | persistence: constant sense after cold load |
| slow | S36 `NativeSlow36` = `0bc608c3f2b548b44b7146b7530613ac` | none | reuse graph; CL 12 DC 18 | save/SR/duration at actual consumers |
| call lightning | S29 Storm Giant call + native bolt action | at-will vs S29's one-per-summoning | reuse graph, at-will frequency; honest finite bolt supply per casting | cast + bolt-action observation |
| dispel (targeted) | S31 `92681f181b507b34ea87018e8f7a528a`; S38 `143775c49ae6b7446b805d3b2e702298`; released useful-enemy-buff heuristic | **greater** form and its 3-effect cap unresolved | see Risk D below | dispel-count + eligibility observation |
| daze condition | S32 clone of native daze buff `9934fedff1b14994ea90205d189c8759` | S32 adds a 1-minute immunity the Nimbus has **no** basis for | reuse condition, **not** the immunity marker | applied-condition observation |
| victim effect attribution | `AstralVictimEffectContext.Create` | none | reuse | effect ownership after source loss |
| descriptor immunity | released `SummonDescriptorImmunity` pair | none | reuse | immune-vs-eligible control |
| explicit SLA range | S38 `AbilityRange.Custom` + `CustomRange` from `CloseRangeFeet(CL)` | none | reuse; CL 12 | targeting distance at actual consumer |
| AI actor + per-target split | S38 `…Eligibility38` / `…TargetEligibility38` pattern | none | reuse shape per ability | AI selects actual usable candidate |
| nested summon / planar travel exclusion | established summon sanitization policy | none | reuse | sanitization observation |
| body + icon | project-owned Blender pipeline, S37/S38 generators, unified S38 view-policy predicate | new creature | one generator + one icon | strict inventory + owned-resource receipt |

## The three risks, settled before implementation

The mission requires these settled before a final-candidate campaign, and two
of the three are now resolved offline from the private reference assembly with
**no guarded transaction spent**, using reflection plus `ildasm`.

### Risk 1 — Unholy Nimbus: delayed resolution, limited uses, ownership — **RESOLVED, carrier selected**

What the installed engine actually provides, and what it does not:

* **`Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverDelay` exists** and
  is a real delayed-delivery mechanism. Its coroutine captures
  `Game.Instance.TimeController.GameTime`, yields `null` while
  `GameTime - start < DelaySeconds`, then yields **exactly one**
  `AbilityDeliveryTarget(context.MainTarget)`. Game time advances in real-time
  play and by one round duration per turn-based round, so its deadline is
  correct in **both** combat modes with no mode test.
* **But it cannot cross a save.** `Kingmaker.Controllers.AbilityExecutionProcess`
  is a bare `System.Object` with no interfaces and no serialization attributes,
  and its entire pending state is a compiler-generated
  `IEnumerator<AbilityDeliveryTarget> m_Process`. A windup in flight through
  `AbilityDeliverDelay` is therefore **lost** on a cold load — not duplicated,
  but lost.
* **Neither a feature nor a buff receives `ITickEachRound`.** Both dispatchers —
  `Kingmaker.Controllers.Units.UnitTicksController.TickNextRound` and
  `TurnBased.Controllers.TurnController.Prepare` — call
  `unit.Logic.CallFactComponents<ITickEachRound>`, and `Logic`
  (`UnitLogicCollection`) and `Buffs` (`BuffCollection`) are **separate sibling
  collections**, both deriving from `OwnedFactCollection<UnitDescriptor>`. The
  real-time path is additionally gated on `unit.IsInCombat`. So the Sprint 38
  lesson generalises further than Sprint 38 stated it: a per-round callback
  cannot be hung on a buff either.
* **Buff duration state *is* serialized, with an absolute deadline.**
  `Kingmaker.UnitLogic.Buffs.Buff.m_EndTime` carries `[JsonProperty]`, as does
  its `m_Context`. `BuffCollection` owns `m_NextEvent`, `UpdateNextEvent`,
  `PostLoad`, `m_EndTimeOverride` and `m_IsOwnerAlive`, so expiry is event-driven
  and re-armed after load.

**Selected carrier: a project-owned pending-nimbus buff on the Nalfeshnee
itself, with the burst fired from its expiry.** Because `m_EndTime` is an
absolute game time, the remaining windup survives a cold load correctly and the
burst then fires **once**, at the right moment — which is genuine exactly-once
continuation rather than the weaker "pending work is cleaned up on load"
behaviour the native delay would have forced. `AbilityDeliverDelay`'s own clock
derivation is what justifies game time as the right measure; it is not itself
used as the carrier.

Still to settle in implementation, each explicitly and none from a fixture:

* `AddFactContextActions` offers `Activated`, `Deactivated` and `NewRound`, and
  `OnFactDeactivate` fires on **any** deactivation — natural expiry, dispel,
  manual removal and owner death alike. So the burst **must** be guarded to
  distinguish genuine expiry (game time has reached the recorded deadline) from
  cancellation, and must additionally require a live, non-destroyed owner.
  Detonating on a dispelled or dead-owner windup would be a duplicate-source
  bug, and the mission forbids pending work firing from a destroyed summon.
* Target eligibility is decided at the **real burst boundary**, not from an
  activation snapshot, so a creature that enters or leaves during the windup is
  resolved against the world as it is when the light bursts.
* Burst centre: the Nalfeshnee's own position at burst time, since the printed
  light plays around its body and then bursts. The actual behaviour will be
  recorded rather than asserted.
* Three independent uses per summoning, debited at activation, spent uses
  preserved across reload, no global shared resource and no silent refill. No
  refill on cancellation or on reload.
* **One outstanding question to freeze before any scheduler is written:**
  what a second activation does while a windup is already pending. The printed
  text does not say. No unlimited stacked explosions, no invented concentration
  requirement and no invented cooldown.

The condition itself is **dazed for 1d10 rounds**, which is neither the Vrock's
instant Screech stun nor a nausea aura. The native daze condition buff is
already cloned by Sprint 32 at `9934fedff1b14994ea90205d189c8759`; Sprint 32
also attaches a one-minute daze immunity because the *Daze cantrip*'s own rule
says so, and the Nimbus has no such clause, so that marker is deliberately not
reused. Non-demon eligibility resolves through
`SummonDemonIdentityCatalog`. Collateral against eligible non-demons is
retained for the manual effect, with conservative ally-safe AI and a useful
fallback rather than faction-based immunity; because the burst is delayed, allies
cannot be guaranteed to stay clear after activation, and that difference is
documented rather than papered over.

### Risk 2 — Feeblemind: real disability, lasting state, supported cure — **UNRESOLVED input, routed to the bounded probe**

No `Feeblemind` identity exists anywhere in this project's source, so whether
the installed 2.1.7b library ships a native feeblemind graph at all is a
genuinely unresolved input. It is routed to the single bounded installed-library
observation below. No GUID is guessed and no absence is asserted from a name
search.

The contract to meet, once the input is known: Intelligence and Charisma to 1,
spellcasting prevented, relevant mental skills prevented, Will save with the
arcane caster's special penalty, spell resistance applies, instantaneous with
lasting consequences and named high-level cures — not a short stun and not an
ordinary dispellable timed debuff. Ability-score changes must not silently fail
to restrict the casting behaviour they are supposed to restrict, and arcane
casting must not be inferred from high Intelligence, a class label or the mere
presence of spell-like abilities. The project's established summon-lifetime
contract and the printed lasting result may conflict; if they do, that exact
issue is settled before testing rather than silently nerfed or silently made
permanent. Note that the released
`RemoveBuffIfCasterIsMissing` component used by Sprint 32's daze is therefore
**not** automatically right here.

### Risk 3 — constant Unholy Aura: recipients and defences without double-counting — **UNRESOLVED input, routed to the bounded probe**

No `UnholyAura` identity exists in this project's source either, so the native
graph is the second unresolved input.

The arithmetic is already settled above and is the part that does not need the
library: the printed AC, all three saves and CMD **already include** the aura's
+4, so racial base values are derived separately and the buff supplies the
bonuses. What the probe must establish is the native consumer's actual shape,
because *unholy aura* is four different branches and not an AC marker: +4
deflection AC, +4 resistance on saves, conditional SR 25 against good spells
and good casters, alignment-related protection, and a Strength-damage response
to good melee attackers. A creature's **constant** feature is also not the
spell's normal multi-target cast, so the intended recipient and radius
behaviour is verified rather than converted into unlimited permanent party-wide
protection. Bonus types, non-stacking with equivalent sources, exact-source
grants, retaliation attribution, and reconstruction-once-on-load without
accumulating bonuses all apply. The ordinary spell's focus or material
components are **not** by themselves a reason to drop a printed constant SLA;
the summon policy is applied deliberately, as Sprint 38 did, rather than a
price being read and the feature quietly omitted.

### Risk D — Greater Dispel Magic is not ordinary Dispel renamed — **UNRESOLVED input, routed to the bounded probe**

The project has the **targeted ordinary** dispel at two pinned identities
(S31 `92681f181b507b34ea87018e8f7a528a`, S38 `143775c49ae6b7446b805d3b2e702298`)
and a released useful-enemy-buff heuristic. What it does not have is any
`GreaterDispel` identity. At CL 12 the targeted greater form removes up to
**three** eligible effects under the PF1 one-per-four-levels rule, so it is not
a one-effect Babau dispel with a higher name. The probe must establish whether a
native greater graph exists and which of its targeted, area and counterspell
forms the installed game actually exposes. Any native limitation is enumerated
exactly; no blanket claim of completeness, and no new counterspell subsystem if
no suitable one exists — the precise disposition is surfaced instead. The
released useful-enemy-buff heuristic is reused without pretending it is a
perfect global buff taxonomy, distinguishing at least useful-beneficial-effect,
harmful-only and no-useful-effect targets.

## The one bounded installed-library observation

Four genuinely unresolved inputs, routed to a single research-only scenario on
the established Sprint 37/38 pattern — `observe-expanded-summoning-sprint39-native-inputs`,
which creates no actor and loads no save:

1. a native **feeblemind** graph, and what implements its condition, ability-score
   change, casting restriction, save penalty, cure and persistence;
2. a native **unholy aura** graph and its actual constant-feature consumer, with
   its deflection, resistance, conditional SR, protection and
   Strength-damage branches told apart;
3. a native **greater dispel magic** graph and which variant forms are exposed;
4. whether any native **Nalfeshnee** unit or body exists at all.

A name-search miss is a result about that name at that moment and is not
universal nonexistence; base-game blueprints, mod-added graphs and native graphs
altered by installed mods are distinguished. Per the established mechanics this
scenario needs **three** registrations, not one — the C#
`RuntimeTestScenarioCatalog`, the PowerShell `$script:KmgRuntimeScenarioMetadata`
table in `scripts/RuntimeAutomation.Common.ps1`, and the documented `$expected`
union in `scripts/Test-RuntimeScenarioPreflight.ps1` — or the harness refuses
the scenario as not allowlisted *after* snapshotting.

## Where this sits now, and exactly what remains

Branch `codex/expanded-summoning-sprint39-nalfeshnee` at `71559d1c`, pushed.
The worktree is clean and the **complete unfiltered domain suite is green:
2,809 registered, 2,809 executed, 0 failures.**

### Done and verified

| Item | State |
|---|---|
| intake, worktree, promotion root, allowlist, push | complete |
| Sprint 38 documentation carry-forward | complete; three corrections, no rebuild |
| printed contract + derivations | complete; 8 domain tests |
| all four risk designs | selected, with the native evidence for each |
| bounded installed-library observation | PASS, live tree restored |
| original icon | authored, exported, manifest 146 -> 147 |
| roster registration, suppressed route, demon catalog 7 -> 8 | complete |
| builder, production components, identity block, ledger, counts | complete, suite green |
| original body, view wiring, packaging, strict inventory | complete |
| persistence scope `NalfeshneeScope` | added to the one authoritative list |
| review phases + core observer | declared, admitted, compiling |

The 51 failures the earlier checkpoint carried were **one incomplete
dependency graph**, not 51 defects; completing it closed all of them. Two real
defects the full suite caught along the way were fixed rather than pinned
around: the four caller-adapter GUIDs had to be the tool's deterministic
RFC-4122 v5 allocation rather than freshly allocated ones, and the two new
natural weapons needed rows in both the JSON and the narrative weapon-visual
audits.

### Remaining, in dependency order

Nothing below has been attempted yet, and **no gameplay behaviour has been
observed on a live unit.** The designs are selected and the code compiles; that
is not qualification.

1. **Three more observers**, on the core observer's shape:
   - *Nimbus*: activation debits one of three uses; a second activation while
     pending is refused and costs nothing; the burst resolves one round later
     and dazes a non-demon while sparing a demon; early removal does not
     burst; destroying the owner while pending does not burst.
   - *Spells*: feeblemind against an arcane and a divine caster, with real
     spell commands rather than a component list; effective Intelligence and
     Charisma actually reaching 1; ordinary lawful actions still available;
     the cure proven through native Heal against the project clone; greater
     dispel with one carried check over multiple ordered effects, a failed
     higher entry, successful lower entries and an eligible effect beyond the
     cap; call lightning and slow at caster level 12.
   - *Public*: the one Summon Monster IX route, slot, quantity and duration.
2. **Persistence observer** plus its hooks in
   `RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs`, mirroring the
   Sprint 38 `Sprint38PersistenceScope` shape. What crosses the save is the
   pending windup with its absolute deadline and the spent uses; unlike Sprint
   38 the pending effect is **not** withdrawn first, because a windup does not
   stop its owner acting and so needs no readiness waiver.
3. **Phase dispatch** in `RuntimeTestRunner.ExpandedSummoningSatyr32.cs` and a
   review scenario, which needs the **four** registrations this sprint
   established: the C# catalog, the PowerShell metadata table, the preflight
   union, and `Test-KmgExactSummoningRuntimeBoundary`.
4. **Guarded mechanics qualification** against a development candidate, then
   repair whatever it finds.
5. **Offline scope/selection/publisher checks**, then the candidate freeze:
   version bump to 0.0.177, a new
   `tools/validate_expanded_summoning_sprint39_release177.py` with the final
   product head pinned, its entry in `tools/validate_repository.py`, and the
   full unfiltered gate.
6. **One ON and one OFF envelope across six fresh processes**, then the
   installed-stack integration.
7. **Release**: reproducibility, publisher preflight, merge, stable
   publication of the exact qualified ZIP, public-download verification, exact
   restoration and the final-records merge.

### Mid-sprint mechanics worth not rediscovering

* The repository wrapper is **version-gated** and still selects Sprint 38's
  validator at `Info.json` 0.0.176, so it refuses in-progress source. That is
  the boundary working. Guarded scenarios before the freeze need a development
  candidate: build with `-DevelopmentPurpose <lower-case-hyphenated>` plus
  `-DevelopmentTestFilter`, then pass that package to the scenario launcher
  with `-PackagePath`, which it **requires** for a development purpose - the
  opposite of the release path, where `-PackagePath` is batch reuse only.
* `Build-Local.ps1` fingerprints the source at the start and re-checks it at
  the end, counting untracked files anywhere in the tree. Writing a planning
  document during a build fails it *after* packaging. Commit first, build,
  then leave the tree alone.
* Both shells are **unelevated** this session, which the guarded harness
  requires and which has been the other way before. Verify, do not assume.
* The ledger's caller-adapter GUIDs are **deterministic**: RFC-4122 v5, SHA-1
  over namespace `5f1c8e0b-6d63-4c55-9a41-2b7e3f9c0d18` plus the symbol, with
  version and variant bits set. Every other ledger GUID is freshly allocated.
* The measured rig capture lives on the owned `E:` archive at
  `KingmakerGunslingerLab-archive/runtime-evidence-20261101/20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census/sprint24-theropod-census.json`,
  SHA-256 `727645bb...`; no new census is needed.

## Not started

Everything after this point. Sprint 40 and the Ice Devil are NOT_STARTED and
out of scope.
