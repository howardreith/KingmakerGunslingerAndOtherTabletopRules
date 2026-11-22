# Known issues and conservative adaptations

- Saving while an Expanded Summoning creature holds a target ends the hold (S6 VER-02, checkpoint C2 finding; open,
  owner decision pending; no change made).
  - Saving is only possible out of combat. When a summon holds a target at that moment, the save releases the hold in
    the running game: the target is freed and the hold does not resume. The saved file never contains the active hold.
  - Cause: the engine's save routine turns every effect off before writing and back on afterwards, and the summon hold
    effects treat being turned off as the end of the hold.
  - How this was established: observed natively for the Giant Flytrap on the accepted development candidate
    `4e11e6c25c`, and consistent with checkpoint C1's single-link save. That the published versions behave the same is
    inferred from their identical source; it was not run on released bytes. Details:
    `docs/GUNSLINGER-SPRINT6-C2-MULTILINK-TERMINAL-CLEANUP.md`, section 4.
- Brown-Fur Powerful Change increase lost after loading when the caster companion was dismissed (S6 T3 / RISK-02,
  checkpoint B, accepted by manual review for its demonstrated development scope; not merged, not released).
  - The scenario: a custom Brown-Fur companion casts a Powerful Change spell on someone and is then dismissed while
    the spell lasts.
  - Before the repair: after the next load the spell came back at its base value (Bull's Strength +4 instead of +6),
    and removing the buff left its persisted record behind.
  - How this was established: the native reproduction used the accepted A2 development candidate `caad210696`. That
    the published versions behave the same is inferred from their identical source; it was not run on released bytes.
  - Repaired on branch `codex/s6-risk02-powerful-change-restoration`: the increase is kept through a fresh load, and
    removal forgets the record. Details: `docs/GUNSLINGER-SPRINT6-B-POWERFUL-CHANGE-RESTORATION.md`.
- Malformed Brown-Fur Powerful Change records in a save (S6 T4 / RISK-03, checkpoint A2, accepted by manual review for
  its development scope; not merged, not released). On branch `codex/s6-risk03-brown-fur-persistence`, a record that
  reaches the mod no longer makes the engine skip the Brown-Fur part.
  - A valid record whose identity no rejected entry could share keeps restoring its exact increase. A valid record that
    a rejected entry in the same save could refer to is withheld (ambiguity policy).
  - A malformed, duplicate-conflicting, unsupported or ambiguous record restores no increase. The spell's base effect
    is kept: Bull's Strength gives +4 instead of +6 until it is cast again with Powerful Change.
  - This does not hold for saves the engine cannot read up to the mod (see the retained limitations below).
  - The record's content stays in the save, and the log warns when a character's condition is first read after a load
    and again when it changes.
  - In the published v0.0.193 (from the decompiled engine, not executed): unit parts are read by the engine's
    `DictionaryConverter`, which skips a part whose value fails to convert. So an unconvertible record silently drops
    the whole Brown-Fur part, every valid increase on it included. Other forms throw in the mod's save and load
    callbacks.
  - Retained limitations, accepted by the review, because the engine changes the data before the mod sees it:
    - a record that repeats a member is read as its last occurrence, and a conflicting repeated `Increase` can grant
      that increase;
    - numbers and date-like strings arrive as the engine's own values;
    - an integer beyond Int64 inside any unit part fails the area load in the engine.
  - No ordinary build has been shown to write such records.
  - Details: `docs/GUNSLINGER-SPRINT6-A2-BROWN-FUR-PERSISTENCE.md`.
- Malformed Controlled Rage data in a save (S6 T4 / RISK-03, checkpoint A1). On branch
  `codex/s6-risk03-controlled-rage-persistence` (not merged, not released), such a save loads. Every unambiguous tier
  keeps its allocation. A tier whose saved entry is invalid or conflicting grants no Controlled Rage bonus until the
  player chooses an allocation with that tier's selector. If the save keeps rejected text but no current allocation,
  no new STR +4 default is made up: the ordinary tier, and any tier the kept text mentions, wait for the player's
  choice. The rejected text stays in the save. The log warns when a character's condition is first read after a load
  and again when it changes. From source, in the published v0.0.193 that data throws in the save and load callbacks
  (not executed on the published bytes).
  No ordinary build has been shown to write it. Brown-Fur's persisted modifiers (the other half of RISK-03) are
  addressed by checkpoint A2 (entry above). Details: `docs/GUNSLINGER-SPRINT6-A1-CONTROLLED-RAGE-PERSISTENCE.md`.
- Buffs built without `FxOnStart` and `FxOnRemove` links. Kingmaker's buff activation dereferences `FxOnStart` after
  the buff is already on the unit, and the rule that applies the buff swallows the error: the buff's components work,
  but `AddBuff` hands nothing back, the "buff added" notice is not raised and no end or tick is scheduled until some
  other buff on the same unit is added or removed.
  - Released: the eleven buffs behind the first missing Gunslinger class report (v0.0.190,
    `docs/GUNSLINGER-MISSING-CLASS-REPAIR.md`) and the eight named Elven Branched Spear buffs (v0.0.191,
    `reports/boughkeeper-buff-icon/`).
  - Repaired on branch `codex/def02-buff-effect-links`, not merged and not released: Deadeye Armed, Gunslinger's
    Dodge, the four Bleeding Wound bleeds applied to a target, Startled (Startling Shot), Targeted Head, Up Close and
    Deadly Armed, Steady Aim Armed, and the four buffs in `EasternWeaponNamedBlueprints` (Falling Petal, the
    Mountain-Sunder round marker and the two retained Wayfarer's Oath and Moonlit Crossing shells). All fourteen have
    the links. That is not the same as every one of them being fully checked in play: how far each was taken through
    its real trigger, and which checks failed, is in `docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md`.
  - **Still in the published v0.0.191**, observed in a loaded game on the published package: a Bleeding Wound bleed is
    put on the target and did not tick in two rounds; Startled did not end in more than two rounds, so the target
    stayed flat-footed; the Mountain-Sunder round marker did not end, so a hit in the next round got no force damage;
    Gunslinger's Dodge works and ends on time only through its own expiry guard, raises no "buff added" notice and is
    not marked not-dispelable. From the engine's code, scheduling resumes the next time another buff on the same unit
    is added or removed, so the effect in play is irregular.
- Deadeye spends grit and arms nothing in the published v0.0.191. From source, this dates from the ability's move to
  native actions in August 2026 (`c2b5f98010`). The ability's apply-buff action had a duration with no dice count;
  Kingmaker cannot calculate it, the action faults inside the game's action list, and Deadeye Armed is never applied. Observed in a loaded game on the
  published package: grit 5 to 4, no buff, no notice. Repaired on branch `codex/def02-buff-effect-links` (not merged,
  not released), where the same use arms the state, an eligible firearm shot consumes it and an unused one ends after
  one round with no refund. The product's own `disposable-gunslinger-deadeye` scenario adds the buff directly and so
  never showed this.
- Steady Aim's armed state is never consumed by a shot and never grants its 10 feet in the published v0.0.191 (from
  source; the published package was not run for this) and on branch `codex/def02-buff-effect-links` (observed).
  `SteadyAimAttackHandler` read `RuleAttackWithWeapon.AttackRoll` before the rule had triggered, where Kingmaker has
  not created the roll yet, and returned. Repaired on branch `codex/new03-steady-aim-attack-roll` (stacked on PR
  #169; not merged, not released): the shot is resolved in the attack-roll prefix, after the discharge step and
  before Deadeye and armor-class selection. In a loaded game, through the real ability and the game's attack
  command, a musket shot resolved at 43.2 ft used ordinary armor class unarmed and touch armor class armed, the
  state was removed once on a hit and on a miss, grit did not change, and the next shot was ordinary again. Record,
  evidence and limits: `docs/GUNSLINGER-STEADY-AIM-RANGE.md`.
- A musket shot ordered from the edge of its range is resolved past its first range increment. The game's attack
  command measures the musket's 40 ft between the two units' edges; the firearm armor-class rule measures between
  their centres. Against a Medium target the shooter stops at about 43 ft by the second measure, so the shot is
  resolved against ordinary armor class unless Steady Aim is armed. Observed in a loaded game on both the branch and
  its predecessor. Not changed; left as a question for the firearm-rules work.
- Gun Training is a three-type adaptation: Pistol, Musket and Blunderbuss, each trained once, over four opportunities
  (5, 9, 13 and 17). A base Gunslinger's opportunity at 17 finds all three trained and grants nothing more: no
  further type and no additional bonus; the level completes normally. This is the owner's selected policy (DEF-04),
  not a defect. A Mysterious Stranger trains its three at 9, 13 and 17; Pistolero and Musket Master use ranked Pistol
  or Musket Training instead. Up to and including the published v0.0.191 the descriptions did not say this; on
  branch `codex/def04-gun-training-progression` (accepted in review at `2fe1b8af2c`; not merged, not released) the Gun Training,
  Mysterious Stranger, Pistolero and Musket Master descriptions state it. Record: `docs/GUNSLINGER-GUN-TRAINING-PROGRESSION.md`.
- With Steady Aim and Deadeye both armed inside the lengthened first increment, the shot uses up Deadeye's state
  (and the grit Deadeye cost to arm) while Deadeye authorizes nothing, because the range already gives touch armor
  class. Observed on branch `codex/new03-steady-aim-attack-roll`. This is Deadeye's existing pricing; not changed.
- Deadeye reports itself available while Deadeye Armed is up. That much was observed in a loaded game, through the
  game's own availability check; a second use was not attempted. From source, the registered ability has no "not
  already armed" restriction, its resource logic spends grit on every use and the armed buff replaces itself, so a
  second use is expected to spend another grit and replace the state. Gunslinger's Dodge, Up Close and Deadly and
  Steady Aim each report unavailable while their state is up. Left as a design question; the policy was not changed.
- Six visible buffs have no icon and draw a blank square where a buff list shows them: Deadeye Armed, the four
  Bleeding Wound bleeds and Falling Petal. With the effect links they are announced on first application, so the blank
  square is now seen reliably on that branch. Not repaired; icon assignments are an art decision and go through the
  icon catalog.
- The optional Suggestion buff (`KMG.Summoning.FeyCelestial.SuggestionBuff`) was listed with the buffs above from its
  source. Without the provider its final blueprint has both effect links (empty), read in the game. With CowWithHat
  Custom Spells present the builder copies the provider's own links into it; that final blueprint has not been read,
  because the provider is not installed on the lab machine. The native Rainbow Pattern buff the provider's buff is
  checked against has a start effect and an empty remove link.
- The registered Gunslinger's Dodge ability applies its buff with the native apply-buff action (`AbilityResourceLogic`,
  `DodgeGritCostCalculator`, `AbilityCasterHasNoFacts`, `AbilityEffectRunAction`). Up to and including the published
  v0.0.191 the source also carries a custom delivery class (`GunslingerDodgeProneAbilityLogic`, with
  `RecordDeliveryEntered`/`RecordDeliveryApplied` diagnostics) and an attack-time runtime (`GunslingerDodgeRuntime`)
  that nothing calls; a real activation leaves the delivery diagnostic at `deliverEntered=false`. Branch
  `codex/int03-dodge-scenario` (not merged, not released) removes those unused classes and their diagnostics and
  keeps `DodgeGritCostCalculator`, which is live, in its own file. Gameplay is unchanged.
- A character saved under version 0.0.65 with the hidden Dodge armed marker (`KMG.Deeds.GunslingerDodgeProneArmed`)
  still has it. The code that removed it has not been called since 2026-08-04 (`93193c5170`). The marker has no
  components and does nothing; its blueprint stays registered so such saves resolve. Nothing removes it.
- The tabletop Dodge reaction policy (`GunslingerDodgeService` and its request and decision types) has domain tests
  and no caller in the product. Left as it is.
- Three hidden Expanded Summoning abilities have no stored description:
  `KMG_Summoning_Infernal_BarbedDevil_OrdersWrath`, `KMG_Summoning_Infernal_BarbedDevil_Pyrotechnics` and
  `KMG_Summoning_Infernal_BoneDevil_DimensionalAnchor`. On a stock game Kingmaker shows `<null>` for such text. On an
  installation where it resolves to a null string, Kingmaker's own ability description getter throws for them. The
  shared grit UI pass used to read every project ability's description and failed there (second missing Gunslinger
  class report, `docs/GUNSLINGER-GRIT-UI-RECIPIENT-REPAIR.md`); it no longer reads any. The three abilities are
  unchanged, so any other code that asks one of them for its displayed description on such an installation would
  still throw. Startup, the class screen and character creation were observed to complete under that condition.
- The shared grit indicator is on Steady Aim, which needs grit and spends none, and not on Quick Clear (standard
  action), Utility Shot, Startling Shot or Lightning Reload, which are in the same position. It also counts remaining
  grit in units of 1 whatever a deed costs. Both are released behaviour, preserved by the recipient repair and left
  as a design question.
- In the disposable loader-compat game copy, the product's own `disposable-gunslinger-dodge` scenario gives no
  verdict on the published packages or on branch `codex/def02-buff-effect-links` (branch
  `codex/new03-steady-aim-attack-roll` has the same scenario code and was not run). It runs from the main menu on a
  unit that is in no loaded area, and Kingmaker interrupts the ability command there before anything is cast. On the published packages the scenario then read the missing buff ("The native Dodge
  command completed without its timed Buff fact"); on `codex/def02-buff-effect-links` it stops at the cause ("Native
  Dodge command produced no execution process; result=Interrupt"). This is the fixture, not the deed
  (`docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md`). Repaired on branch `codex/int03-dodge-scenario` (not merged, not
  released): the scenario now loads `KMG_AUTOMATION_WORKING` through the product's guarded read-only route and uses
  the registered ability through the game's ability command. It passed in the disposable game copy: grit 5 to 4,
  armor class 10 to 12, an incoming attack resolved against 10, 12 and 10, an attempted command refused while active
  and with no grit with nothing spent or applied, the buff ended after one round, and a recast after an early removal
  outlived the earlier deadline (after the review corrections: the later buff was applied 3.46 s before that deadline
  and was still on the unit 0.62 s after it). Record and limits: `docs/GUNSLINGER-DODGE-SCENARIO.md`. It needs the mods the
  working save was made with, and it was not run through the live guarded launcher on the owner's installation.
- `disposable-gunslinger-comprehensive-acceptance` has no Dodge slice on branch `codex/int03-dodge-scenario` (32
  slices, was 33): the slice could not execute the ability at the main menu. The aggregate was not rerun.
- Up to and including the published v0.0.191, two of the product's own diagnostics cannot see what they are named
  for. `disposable-firearm-visual-rigs` is pinned to 14 firearm items; there are 16, so it fails on the count and
  inspects no item's visual binding. `observe-gunslinger-presentation` is pinned to 55 weapons and 56 items; there
  are 57 and 58, so it fails on the count and never checks that the policy-only enchantment is present. Neither is a
  player-facing defect. Repaired on branch `codex/int04-int05-presentation-diagnostics` (not merged, not released):
  the expected identities come from declared rosters, every item is inspected, and each result is its own assertion.
  On that branch all 16 firearm visual bindings and the 57, 58 and 12 censuses pass, and the policy enchantment is
  present with blank text (`docs/GUNSLINGER-PRESENTATION-DIAGNOSTICS.md`).
- Up to and including the published v0.0.191, `disposable-firearm-visual-rigs` fails on
  `pistol-authored-marker-contract` for Duelist's Rebuttal and The Last Word. This is a defect of the diagnostic, not
  of the pistols: it compares the root `Muzzle`'s position in the prefab's own frame with the marker's coordinate in
  the source model's frame, (0, 0, 0.264), and the two pistols have been placed in the held hand frame since
  2026-08-21. Compared in one frame the root markers are within 0.0000001 m of the authored markers, and in a live
  attack both pistols' bores are aligned with the target at the frame the shot fires. Corrected on branch
  `codex/rep03-named-pistol-muzzle` (not merged, not released); no model or bundle changed
  (`docs/GUNSLINGER-NAMED-PISTOL-MUZZLE.md`). How the pistols look was not reviewed.
- On that branch only (the published line does not contain this code): the weapon motion evidence scenarios release a
  paused game while an attack is sampled. As first written on the branch, a run that the runner ended early left the
  game unpaused and its disposable units in place, and a high frame rate could leave an attack command running that
  blocked the next weapon's. The corrected marker diagnostic also let a marker that was not a finite number pass.
  All three are corrected on the branch, and the review of PR #173 at `6f1590d31d` accepted them (not merged, not
  released). These are diagnostics; none of this is player-facing.
- The engine launches a firearm's projectile from the unit's left-palm locator, inherited from the native light
  crossbow's projectile, not from the weapon's muzzle. At the frame a handgun shot fires, that point is 1.4 to 1.8 m
  from the muzzle over two runs (service pistol, both named pistols and the revolver). The mod intends to hide the projectile's
  model; whether it does, and so whether a player sees anything leave from the left hand, has not been examined. Long
  guns were not sampled. Observed, not classified, not changed.
- The 27 generic progression firearms (+2 to +5 and Reliable, for pistol, musket and blunderbuss) receive an item
  visual variant through the same registration helper as the magic firearms, each mapped to its family's Service
  variant. No scenario reads them, so whether those bindings hold in the game is not established. The sixteen-item
  roster of `disposable-firearm-visual-rigs` is the population that scenario reads, not every firearm with a variant.
- Up to and including the published v0.0.191, the `up-close-critical` assertion of `disposable-pistolero-deeds`
  gives a pass or a fail by process on identical inputs (two of three runs on the grit UI recipient candidate failed
  with `6,3`, one passed with `3,3`; the published v0.0.190 passed once). Cause measured on branch
  `codex/rep02-pistolero-damage-consumption` (accepted in review at `9b8a755ec8`; not merged, not released): an invalid assumption in
  the component fixture, not a gameplay defect. Its "hit" stage really hits only when an unseeded d20 is 13 or more
  (then the full packet, 6), and its "critical" stage is always a miss in that order (half packet, 3); the assertion
  compared their hit-point losses. Damage matched each stage's actual outcome and cost and consumption were correct;
  the hit stage's damage amount differed because its outcome did. On that branch the fixture judges outcome, deed damage, grit charge and
  consumption separately, and the registered ability and the attack command were observed on a Pistolero: an
  ordinary hit and an engine-confirmed critical each apply one unmultiplied 1d6 deed packet, a miss applies exactly
  half of its recorded roll, each charges one grit and removes the exact armed instance before the deed's damage; a following unarmed shot and an order cancelled before
  it starts charge nothing, and the cancelled state expires by itself. Record:
  `docs/GUNSLINGER-UP-CLOSE-AND-DEADLY-DAMAGE.md`.
- Firearm proficiency provenance (RISK-05). Rapid Reload's parent gate reads the registered firearm proficiency
  features, and `FirearmProficiencyScopeReconciler` removes this mod's obsolete proficiency scopes by blueprint
  identity whoever granted them (Kingmaker facts carry no general provenance), sparing only the legacy Exotic Weapon
  Proficiency (Firearms) wrapper's full proficiency. No supported independent provider of these facts exists: they are
  granted only by the Gunslinger, Pistolero and Musket Master presentations and the legacy wrapper. On branch
  `codex/risk05-proficiency-provenance` (accepted with Sprint 4 at `d53487a2a6`; not merged, not released) the scope rules are extracted to
  `FirearmProficiencyScopeRules` with tests, and the requirement for any future provider is recorded there and in
  `docs/GUNSLINGER-PROFICIENCY-PROVENANCE.md`: grant a provider-owned fact the proficiency policy is extended to
  recognize, or bring a recorded provenance the reconciler honours; never grant these blueprints directly. A provider
  surviving a Gunslinger's reconciler is a future requirement, not verified compatibility.
- The guarded `disposable-rapid-reload-proficiency-gate` runtime scenario is
  registered, compiled and catalog-synchronised, and both rounds of review
  findings are implemented: a real pending class/archetype change inside one
  live transaction, refusal observed before any test cleanup, a natively built
  Musket Master, a class-identity control with real Gunslinger levels, an
  explicit rebuild-tolerant reservation of the feat slot each case exercises,
  exact-child tracking across an archetype change, declared proficiency scopes
  validated by the evaluators that score each case, and the native
  `LevelUpState.IsComplete` gate required before every claimed confirmation, and
  exception-safe ownership of a level-up controller whose initialisation fails,
  caller-owned cancellation failures reaching the scored failure collection
  rather than only the evidence row, and a non-destructive cleanup injection
  that leaves the controller intact for real teardown. VER-01 / S4 T8: it was
  executed for the first time in a non-elevated session through the supported
  loader-compat product launch, on development candidate `16708bd5e237`
  (session `20261118T014708Z-owner-0.33.0-rapid-reload-transaction`, 11 of 11;
  also 11 of 11 on `fd4087c312bd`). The pending archetype change cleared the
  invalid one-handed child while keeping the parent, all 19 selection-flow
  cases ran without a cleanup error, and party and unit isolation held. No
  defect was found. Accepted with Sprint 4 (2026-11-18); not merged, not released; the level-up screen itself was
  not observed (`docs/GUNSLINGER-SPRINT4-CLOSURE.md`).
- A Musket Master still qualifies for the Rapid Reload parent through
  two-handed firearm proficiency, so a pending archetype change may
  legitimately leave the parent selection in place while clearing the
  now-invalid one-handed child. Both that outcome and the engine removing the
  whole selection are accepted; what is required is that the invalid child is
  neither selected, granted, eligible nor selectable.
- RESOLVED (Z-FIREARM-MAINTENANCE, 0.0.127): the unified repair combat
  question is closed. Repair Firearm is now an out-of-combat, Broken-only
  full-round maintenance action gated by the native party-level combat
  authority; Wrecked firearms are restored automatically only after a
  genuine completed full rest (a participating gunsmith with one reusable
  kit maintains the whole party's carried Broken/Wrecked firearms, and a
  Wrecked gun stays unloaded). The hidden legacy Overhaul alias delegates
  to the same checks, Quick Clear keeps its distinct in-combat Broken
  recovery, and a newly committed break during an attack sequence stops the
  remainder of that sequence and its automatic continuations while a later
  deliberate attack with the Broken gun stays possible. Up to and including
  the published v0.0.191, Quick Clear, Repair Firearm and rest do not lift
  that stop: a companion's own AI does not fire that firearm again until the
  player gives it a new attack order (or the game is saved and reloaded),
  and nothing in the game says so. On branch `codex/pol01-sprint4-policy`
  (owner decision B; accepted in review at `ee65d09e9d`; not merged, not released), a
  successful Quick Clear, Repair Firearm or completed rest that brings the
  exact firearm back to Normal lifts it, so the AI may decide new attacks
  with that gun; the interrupted sequence stays cancelled and nothing is
  ordered by the recovery. The next release notes must state that v0.0.128's
  "Recovery alone grants no new order" is superseded. Record:
  `docs/GUNSLINGER-MISFIRE-AI-RECOVERY-POLICY.md`.
  Native runtime
  qualification status is tracked in
  `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`; human visual/play acceptance
  remains separately pending.
- Brown-Fur `0.0.81` failed human review and is superseded. The repaired
  `0.0.82` artifact passed focused runtime, persistence, optional-mod profiles,
  the authoritative 16-state boundary, and human presentation/play acceptance.
- Brown-Fur fails closed on an unknown or ambiguous future CotW Arcanist
  structure. This preserves all unrelated package modules but may require a new
  compatibility fingerprint and adapter before the archetype can be published.
- Removing Call of the Wild from a save containing a CotW Arcanist or Brown-Fur
  character is unsupported because CotW owns the parent class. Switching only
  the Brown-Fur module OFF while compatible CotW remains installed is supported
  and preserves existing owners.
- Eastern Weapons' subjective equipped-model appearance remains pending human
  review across the rigs, armor, animations, grips, and size-changing cases in
  `docs/EASTERN-WEAPONS-MANUAL-ACCEPTANCE.md`. Automated checks prove bundle,
  prefab, renderer, material, anchor, bounds, fallback, and cleanup structure;
  they are not a claim of aesthetic acceptance.
- `DEFERRED  ENGINE HAS NO RELIABLE COUP-DE-GRACE DC HOOK`. Tabletop Deadly is
  therefore omitted from Wakizashi and Katana. It is not approximated with
  ordinary damage and is not claimed in player-facing item text.
- Nodachi intentionally has no Brace behavior because Kingmaker lacks the
  relevant readied-action system. Its Polearms fighter-group membership does
  not add reach or change its two-handed sword animation.
- The exact qualified Arms and Armor build supplies no overlapping Katana,
  Wakizashi, or Nodachi content, so Eastern Weapons has no known optional-mod
  duplicate-name presentation limitation in the tested profiles.

- Expanded Summoning uses only existing Kingmaker views. Proxy appearance is
  mechanically reconstructed but cannot perfectly match every tabletop body.
  The final manual checklist covers residual aesthetic judgment for scale,
  camera framing, projectile appearance, and animation style.
- The outer Summon Monster and Summon Nature's Ally spellbook parents retain
  their native spell icons. Every visible creature-choice child instead uses
  a project-owned original icon; no Owlcat or optional-mod pixels are used in
  the 77-icon Expanded Summoning set.
- Dire Bat is not published in the player-facing roster. The installed game
  exposes no proven bat-compatible summon rig, and the Roc proxy failed human
  visual acceptance; all frozen identities remain registered for save safety.
- Elephant and Mastodon share Owlcat's Mastodon material. Their view scales
  are differentiated, but Elephant is not recolored gray because a safe
  per-instance material clone was not proven and shared-material mutation
  would alter the native Mastodon.
- Kingmaker stores spell descriptors on shared ability blueprints rather than
  per caster invocation. KMG direct templated roots retain `Summoning`; their
  spawned alignment, Celestial/Fiendish template and smite are caster-correct.
  KMG does not mutate shared descriptors dynamically. Sacred Summons therefore
  remains fail-closed when no exact optional surface is installed.
- Standalone Kingmaker 2.1.7b has no proven Aura of Menace carrier. Lantern
  Archon reuses the exact carrier when Call of the Wild supplies it and
  conservatively omits the aura standalone.
- Shadow Demon possession is omitted because a duration-bounded, save/load-safe
  implementation was not proven. Teleportation, planar travel, and creature
  summoning powers are removed from every adapted summon.
- Succubus has bounded charm and temporary energy-drain combat mechanics. It
  does not grant a permanent profane gift or any effect intended to outlive the
  summon.
- Bebelith's permanent armor destruction is represented as a DC 25 Reflex-gated
  one-round -2 AC effect after two same-target claw hits; equipped items are
  never mutated. Rot is omitted.
- Pixie sleep arrows are non-transferable, zero-weapon-damage attacks with a
  bounded resource. Irresistible Dance uses a project-owned touch-range state
  because the installed native-equivalent carrier belongs to an optional mod.
- Several movement modes and natural-creature secondary abilities lack safe
  native representations. Each omission is listed per creature in
  `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`; no omitted mechanic is
  described as implemented.
- Removing the entire mod is not save-safe for campaigns that have used its
  content. Disabling Expanded Summoning is supported: identities remain
  registered, active summons load and expire safely, and no new KMG variants
  are published after restart.
- The save-free comprehensive Gunslinger runtime fixture has a historical
  limitation: its detached unit has no Swift-action controller, so the
  Gunslinger's Dodge command interrupts before `Start`. This is a test-fixture
  limitation, not a change introduced by Expanded Summoning; focused domain
  coverage and prior live gameplay behavior remain unchanged.
- The same broad aggregate also has order-sensitive forced-roll and detached
  target fixtures for Targeting/Bleeding Wound. Focused domain and production
  regression scenarios remain authoritative; Expanded Summoning does not
  patch firearm or deed execution paths.
- Cleric summon alignment and Races Unleashed (owner-accepted limitation since
  v0.0.181). With Races Unleashed's Spider Summoner feature, "Summon Monster V
  (Ogre Spider)" summons a Bralani or a Redcap, not an ogre spider, in the tested
  installation; clerics are restricted by the creature that actually appears. Giant
  Crab Spider and Giant Black Widow are not classified by the cleric alignment rule
  and keep their native behavior. Black Widow, and by the same rule Crab Spider, is
  not offered in a Herald Caller conversion. The latest native result for this
  feature remains FAIL, 70 PASS / 1 FAIL; those unresolved census findings are an
  explicit owner-accepted exception, not a passing result. This describes the released
  v0.0.181 only. **Resolved in v0.0.182** by the Races Unleashed Spider Summoner
  integration (PR #125): the four spiders appear as themselves, are classified by the
  cleric rule and are offered in Herald Caller conversions. Its evidence is in
  `planning/races-unleashed-compatibility/STATUS.md`.
- Bebelith Dismantle Armor (withdrawn in v0.0.182). A "Dismantled Armor" effect already
  on a character in an older save remains as a retired marker that no longer reduces
  armor class; it is not removed from the save. The withdrawal and the inert marker have
  focused-test and compilation evidence only, not a native observation.
- Saved Greater/Elder Earth Elementals (v0.0.182). A summon saved before v0.0.182 loses
  its Sunder feat, action, toggle and AI action on load but keeps its former ordinary
  attack AI action, whose targeting favors lightly armored or sundered foes; fresh
  summons use the ordinary-attack brain. Autonomous decisions with the retained action
  were not observed, and no actual old save was cold-loaded.
- Oleg's cold iron branched spears (withdrawn in v0.0.183). Shared vendor stock
  persists only item types and counts. If the stocked cold iron spear was bought and a
  cold iron spear was later sold back to Oleg, the engine's update removes that sold
  copy from his stock. Player inventory is never touched. Not observed natively.
- Inherited summon templates: the removal set is a hand-maintained list (v0.0.186). Twelve creature
  keys are declared as needing an inherited celestial or fiendish template stripped. Measured against
  the installed library across all 281 untemplated Summon Monster placements, that list disagrees in
  both directions: 81 placements' registered graphs contain a native template apply action, 52 carry
  the removal, only 16 contain both, 65 apply with no removal and 36 carry a removal with no apply.
  Those are GRAPH observations - the graphs were read, not cast - so they establish that 65 graphs are
  SHAPED to apply a template with nothing removing it, not that each delivers one in play. The
  membership is byte-identical to v0.0.185, so v0.0.186 neither introduces nor worsens it. Tracked
  separately as GS-REP-0002 and not fixed by this release.
  DEF-05 / S5 T1, in progress on `codex/def05-inherited-template-cleanup` (draft PR #186; not merged, not
  released): the census was reproduced on the accepted Sprint 4 product (session `20261118T032921Z`), and it showed the
  donor carrier, selected by parent tier and quantity, decides application whatever the creature. The list is replaced
  by a per-placement policy over the recorded donor, shared by canonical and alternate callers, with an independent
  census evaluator and mutation controls (`docs/EXPANDED-SUMMONING-INHERITED-TEMPLATES.md`). The first candidate
  failed natively on a product defect in that change (alternate families failed closed), and it is repaired in source.
  The review's four census corrections (an independent donor contract, all seven alternate families, donor receipts,
  branch pairing) and a representative native-cast observer were built into `bab7e7dd43fc`, which was never executed.
  The second review's corrections (exact debit, quantity, duration, buff sets and alternate child sets) were built into
  `0af2b30bd4c7`, also never executed. The third review's correction (the deliberate template expectation derived
  independently of the child graph) is built into the frozen final candidate `0d6006ac03a2`. After the owner disabled
  Steam Cloud for Kingmaker, its native census (session `20261118T215938Z`) observed all 5,769 canonical and alternate
  abilities across the seven families, but failed on 170 donor-receipt findings. These are diagnostic: the established
  tier-one menu publication rewrites the two native tier-one parents after cloning. With the owner's tier-one donor
  contract (`67704e6459`), the rerun (session `20261118T225322Z`) failed only one condition of that contract, "preserved
  child exactly once in the menu". The publisher withholds that child by design when its lineage is replaced. With
  condition 3 revised by the owner (withhold on complete replacement coverage, derived independently), the census on
  candidate `540556bbf004` (session `20261119T192806Z`) passed. The first cast session on the same package (session
  `20261119T193054Z`) failed 3 of 15 assertions, while seven of its nine cases passed. Two failures came from the
  observer reading ranks from an unrecalculated context: a tier-VI d4+1 mephit's bonus read 0, and a Master Summoner
  duration expectation read 0 against 420 s observed. The third was a donor assertion not updated for the two approved
  tier-one transitions. After a bounded observer repair (no gameplay change), candidate `8ceb08ccef29` passed both
  the census (session `20261119T201715Z`) and the cast session (`20261119T201938Z`, 15 of 15). With the d4 forced to
  2, `ProjectilesCount` 1 and three Air Mephits prove the d4+1 path. The Master Summoner's Default rank of 7 gives the
  420 s duration. The donor carriers are unchanged across the casts. The quantity is proved for the controlled roll
  only. S5 T1 is accepted and closed for its reviewed development scope (principal review, 2026-11-19; qualified
  product `8ceb08ccef29`). Acceptance is not merge or release: PR #186 stays a draft, so released versions still carry
  this issue.
- Controller Elementals entry with nothing gathered (v0.0.189 disclosure, DEF-06). On a controller, the spell selector
  can offer an Elementals entry even when no original Summon Elemental spell was folded into it, as the v0.0.189 release
  notes disclose. S5 T2 / DEF-06 (`codex/def06-controller-launcher-consolidation`, draft PR #187; not merged, not
  released) publishes the controller entry only after an actual consolidation of an exact original row, at the first
  removed position, and otherwise leaves the native rows untouched. On candidate `f91a80144707`, session
  `20261119T213730Z` observed both lanes in the actual console UI: no entry without consolidation, and one entry in
  place with consolidation. It also observed one exact debit on use, and refusals with no spend for stale and exhausted
  sources (`docs/EXPANDED-SUMMONING-CONTROLLER-LAUNCHER-CONSOLIDATION.md`). S5 T2 is accepted and closed for its
  reviewed development scope (principal review, 2026-11-19; qualified product `f91a80144707`). Acceptance is not merge
  or release: PR #187 stays a draft, so released versions still carry the issue. The candidate session's aggregate
  verdict remains FAIL because of the separate console-learning issue below.
- Console learning: the hidden-option learning list still lists one original Summon Elemental. In the controller
  learning review, with the "show original Summon Elemental spells" option off, the seam-level learning list for a
  Wizard still contained one original Summon Elemental. The revealed list is correct. This reproduces identically on
  the accepted S5 T1 product (session `20261119T213956Z`) and on the S5 T2 candidate (session `20261119T213730Z`), so
  S5 T2 did not cause it. Its mechanism is not traced. It may be a defect in the hidden learning seam, or a stale check
  after the owner's 2026-11-11 and 2026-11-12 withdrawals of the learning disclosure. It is not repaired. It stays an
  unclassified known issue. Both sessions stay FAIL, and their evidence is neither skipped, weakened nor relabeled.
  It is not investigated on the S5 T3 branch.
- Two feature-module publication self-checks fail on a heavily modded installation (v0.0.186):
  `feature-module-expanded-summoning-publication-gate` reports `unclassified=38` against a hard-coded
  expectation of 0, and `feature-module-brown-fur-publication-gate` fails alongside it. A matched
  baseline built from released master reproduces both with identical measured values, so they are
  pre-existing and are not caused by the v0.0.186 repairs. Their mechanism is not traced. They are
  internal consistency checks and do not affect play.
  Since traced for the summoning check (INT-01, 2026-11-16): the 38 entries reproduced on the published v0.0.191 are
  the mod's own generated Spider Summoner
  rows, which are published, correctly, whenever Races Unleashed is installed. The check predates them and had no
  class for them, so it is a defect of the diagnostic and not of publication; it fails the same way on the published
  v0.0.191 with Races Unleashed as the only other mod. Corrected on branch
  `codex/int01-summoning-publication-attribution` (accepted in review at `e2a716c59e`; not merged, not released), where
  every choice under the eighteen summon parents is judged by identity and listed
  (`docs/EXPANDED-SUMMONING-PUBLICATION-BOUNDARY.md`). The original run's raw results, mod list and row identities
  were not recovered; the matching count, Races Unleashed's known presence, the matching Brown-Fur value and the
  source history strongly support this as the cause of the historical report.
  Since traced for the Brown-Fur check (INT-02, 2026-11-16), independently. On the observed twelve-mod stack, with
  Brown-Fur enabled, Call of the Wild 1.14.4c-2.1 and Favored Class 1.3.1, the old gate fails because Brown-Fur
  publishes before Favored Class adds the combined archetypes. Its expected transaction counts are derived from the
  later selector state. The archetype is published correctly, once and in the right place: the transaction found
  six archetypes and the selector holds fourteen foreign ones at the end. A defect of the diagnostic, not of
  publication; it fails the same way on the published v0.0.191. Corrected on branch
  `codex/int02-brown-fur-publication-attribution` (accepted in review at `6ddd4802c2`; not merged, not released),
  where each predicate is its own named result and the transaction is judged against its own record
  (`docs/BROWN-FUR-PUBLICATION-BOUNDARY.md`). Other provider versions and load orders were not run.
- With Favored Class installed, that mod builds three combined Arcanist archetypes from Brown-Fur Transmuter itself
  (Unlettered Arcanist / Spell Specialist / Brown-Fur Transmuter, Unlettered Arcanist / Brown-Fur Transmuter, Spell
  Specialist / Brown-Fur Transmuter), because Brown-Fur is already in the selector when it combines. They are
  Favored Class's blueprints, not this mod's, and by that mod's own list they were present in the owner's
  installation on 2026-11-12. They disappear when the Brown-Fur module is off. Observed with Favored Class 1.3.1
  and Call of the Wild 1.14.4c-2.1; identities `1e94108befebb3728ec03be1a6ee70db`,
  `c152b2a965d9f4f0105d562aeba2ac44` and `753ccc40ae29f6c71c433293315b4fb6`. Decision (NEW-10): Preserve the existing Favored Class-generated combinations. Gameplay
  compatibility qualification is pending; their presence is neither a confirmed defect nor verified gameplay support.
  Nothing is suppressed, renamed, patched or reordered. Whether they work in play has not been examined.
- The identity manifest's note for the Gunsmith's Kit said, up to and including the published v0.0.191, that the kit
  serves "unified Broken-or-Wrecked-to-Normal firearm repair". That was stale developer metadata: field repair has
  been Broken-only since 0.0.127 and a Wrecked firearm is restored only by a completed full rest. No player-facing
  text carried the claim and nothing in play was affected. Corrected under the owner's authorization on branch
  `codex/int07-gunsmith-kit-manifest-note` (accepted in review at `a37a712e85`; not merged, not released), with the historical
  1,913-record prefix hash unchanged and one exact, named exception for this note
  (`docs/BLUEPRINT-MANIFEST.md`). By the owner's later decision the development-candidate validator recognizes this
  one exact transition and accepts the branch again; the v0.0.191 release validator is unchanged, and the next
  release's validator will have to account for the approved note.
- The Expanded Summoning planning report (`planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md`) overstated coverage up to
  and including the accepted INT-07 head: it counted a creature as published in a spell family because its identity
  was registered, and counted any native wrapper in a family as covering every tier. It showed Summon Monster 120 of
  120, and Brachiosaurus as published in Summon Monster, where it has no choice. The committed copy was also stale
  since Stirge's withdrawal on 2026-11-07. Developer documentation only; nothing in play was affected. Corrected on
  branch `codex/int08-summoning-roster-coverage` (accepted in review at `dcea649f5e`; not merged, not released): coverage is now
  compared placement by placement and reads, from source, 1,208 of 1,232 ideal placements published, 9 withheld
  (Stirge, owner withdrawal) and 15 missing (Brachiosaurus in Summon Monster, POL-04; twelve wrapper placements,
  POL-05) (`docs/EXPANDED-SUMMONING-ROSTER-COVERAGE.md`). Both have since been decided as scope exclusions (entries
  below).
- Brachiosaurus is not offered through Summon Monster (POL-04, scope decision, not a defect). The Core Rulebook Summon
  Monster VII table includes it, and the original charter placed it at Summon Monster VII single, VIII 1d3 and IX 1d4+1.
  The owner chose Nature's Ally only on 2026-10-15, because adding Monster placements later is additive while
  withdrawing published ones is not, and reaffirmed it as S5 T3 / POL-04 Alternative A (A-records) on 2026-11-19,
  accepted and closed for its reviewed development scope at `ada0ff2650` (draft PR #188; not merged, not released;
  `docs/EXPANDED-SUMMONING-BRACHIOSAURUS-SCOPE.md`). Those three placements are
  approved out of scope; they are not fulfilled. At `ada0ff2650` the roster report showed 1,208 admitted, 9 withheld,
  12 missing and 3 approved out of scope against the unchanged 1,232 ideal. Brachiosaurus is available through Summon
  Nature's Ally VII, VIII and IX, and the Naturalist, Feral Hunter and Preservationist adapters.
- Twelve higher 1d4+1 placements of five retained native wrappers are not offered (POL-05, scope decision, not a
  defect): Redcap Summon Monster VIII and IX, Axiomite and Soul Eater Summon Monster IX, Manticore Summon Nature's Ally
  VIII and IX, and Mite Summon Nature's Ally IV to IX. The original charter promised them, and the Core Rulebook allows
  1d4+1 creatures from a lower-level list; Owlcat authored these creatures only on a base-plus-two ladder, so no native
  option exists for those exact creature-and-tier combinations. The owner selected S5 T4 / POL-05 Alternative A on
  2026-11-19: keep the base-plus-two ladders and approve exactly those twelve cells out of scope, each recorded
  separately (draft PR #189; reporter and records only; accepted for that development scope at `acd50a042a`; not
  merged, not released;
  `docs/EXPANDED-SUMMONING-WRAPPER-LADDER-SCOPE.md`). They are not fulfilled. Every existing wrapper row of these creatures (15) and its adapters (52)
  are unchanged. The roster report now shows 1,208 admitted, 9 withheld, 0 missing and 15 approved out of scope
  against the unchanged 1,232 ideal; the original ideal is not fully published.
- Dire Narlmarches and Pitax regional weapon stock (withdrawn in v0.0.185). It has the same native limitation as Oleg's spears:
  if a stocked Eastern weapon or spear was bought and a matching one was later sold back to that trader,
  the engine's update removes the sold copy. A save with no persisted known-item history keeps the old
  stocked copy. Player inventory is never touched. Not observed natively.
- Thornstep (retired from acquisition in v0.0.183). In its former Riverine Rise
  container, a copy the party never held (persisted item time still zero) is removed;
  a claimed-and-returned copy, deposits and the container's other loot are kept. A
  Thornstep the engine added to a container generated before the placement existed is
  also never held, so it is removed too. Offline policy tests and a model of the native
  diff only; not observed natively (`planning/gunslinger-playtest-repairs/STATUS.md`).
- With Call of the Wild's "metamagic for spontaneous spell conversion" setting
  enabled, Herald Caller and Druid conversion popups over a prepared slot also
  list Call of the Wild's own rows: the feature's lower-tier quantity spells, with
  metamagic built from the caster's metamagic feats to make up the slot's level
  (not the source's metamagic, as the 2026-11-11 record said). Released versions
  show them as raw native rows, for example "Summon Monster IV (1d4+1 Wolves)" in
  a level 5 popup (recorded 2026-11-11 in
  `planning/races-unleashed-compatibility/STATUS.md`). On branch
  `codex/pol06-cotw-conversion-semantics` (S5 T5 / POL-06, draft PR #190;
  accepted for its reviewed prepared-caster development scope at `c2867a3aa1`,
  product `c5fdcb1431c2`; not merged, not released) they are projected into the published
  creatures of their own tier and quantity, each keeping that row's metamagic,
  the slot's level and one native debit of the source. Rows of the prepared
  parent's own tier stay raw as a conservative presentation fallback (their
  metamagic can differ from the source's). Observed in a disposable game copy
  with an independent per-conversion observer: Extend and Heighten, Single, d3
  and d4+1 casts on both routes, a plain source and a second spellbook
  (`docs/EXPANDED-SUMMONING-COTW-CONVERSION-SEMANTICS.md`). The larger popup (302
  rows for Extend and Heighten Spell; three rows checked for reachability, two
  tooltips rendered), the same creature at two tiers (distinguished only in the
  tooltip) and eight duplicate-looking pairs (alike under the observer's
  comparison key, which does not prove behavioural equivalence) are retained
  presentation limitations within the accepted development scope. Spontaneous
  casters stay outside this scope.
- Mastodon Trample (2d8+18, Reflex DC 29, added in v0.0.180) was reachable
  only by a harness command in every released version: the summon's brain
  supplies only an ordinary attack and summons cannot be commanded by the
  player. On branch `codex/risk06-mastodon-trample-route` (S5 T6 / RISK-06,
  draft PR #191; accepted for its reviewed development scope at `9d2f5e7c4f`,
  product `203b605c108c`; not merged, not released) the Mastodon has a private brain
  whose owned AI action uses the existing Trample as a conservative approach
  maneuver: only when it is unthreatened, has its full round, no enemy is
  within its melee reach, the target is smaller and unclaimed this round, and
  the final straight path crosses the target and ends clear beyond it.
  Otherwise it attacks as before. Qualified natively in real-time and
  turn-based combat on a fixture-created summon, one geometry
  (`docs/EXPANDED-SUMMONING-MASTODON-TRAMPLE-ROUTE.md`). Other tramplers unchanged and not
  examined; summons saved before the change keep attack-only brains. Stale
  dispatch, summon expiry, an attack-of-opportunity stop during an AI
  Trample and the threatened case in turn-based combat were not observed.
- The legacy Advanced Revolver declares six chambers, but saved firearm state holds one round, so its six-round reload
  fails and restores ammunition and weapon state. No published release offered it (vendors only cleaned it up;
  Craft Magic Items only recognized an owned one). On branch `codex/risk01-advanced-revolver` (accepted with Sprint 4 at `e30b19e8fe`;
  not merged, not released) it is contained as unsupported: it stays registered and loadable with its identities, cannot be equipped,
  is no longer recognized by Craft Magic Items, says so in its description, and registration refuses any other firearm
  whose capacity the saved state cannot hold (`docs/GUNSLINGER-ADVANCED-REVOLVER-CONTAINMENT.md`). One already equipped
  through external means stays equipped and keeps failing its reloads safely.
