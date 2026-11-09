> **Activated by owner instruction on 2026-11-09.** This file is the implementation contract for the
> full Spider Summoner integration. The deferred handoff, its `FAIL, 70 PASS / 1 FAIL` result and the
> candidate and environment that result belongs to are preserved unchanged in
> [BASELINE.md](BASELINE.md). Acceptance is tracked in [ACCEPTANCE.md](ACCEPTANCE.md) and [STATUS.md](STATUS.md).

# Races Unleashed Spider Summoner integration

Implement full Spider Summoner integration with Expanded Summoning in `howardreith/KingmakerGunslingerAndOtherTabletopRules`. Eligible characters should receive the four additional spiders through the appropriate Summon Monster and Summon Nature's Ally spell routes, with consistent templates, higher-tier quantities, the feat's DC benefit, and the established cleric restrictions and spell costs.

Owner: Howie Reith. Prepared November 9, 2026. Scope: implementation through a reviewable draft PR. Companion contracts: [acceptance](ACCEPTANCE.md) and [references](REFERENCES.md).

## 1. Activation and execution boundary

When the owner gives this package and its launch prompt to the implementing agent, that instruction activates the previously deferred Races Unleashed task. The old `DEFERRED — NOT AUTHORIZED TO START` marker is historical for this scope. Preserve the old evidence; update the active compatibility mission and resume entry on the implementation branch.

The complete scope includes every requirement below. Template normalization, neutral selection, higher-tier quantities, SNA conversion coverage, and the poison/web benefit are mandatory parts of this mission. Internal phases are checkpoints for coherent work, not requests for renewed permission or opportunities to drop later requirements.

Authorized work includes relevant source and installed-binary inspection, implementation, necessary focused tests and compilation, guarded disposable native fixtures within the current validation policy, documentation, commits, repository-approved pushes, and one draft PR. Routine implementation choices can be resolved autonomously using the established architecture and the explicit gameplay contract.

End at a reviewable development candidate. Merge, release publication, replacement of published artifacts, permanent installation, and edits to the Races Unleashed DLL are outside this task. A true blocker or a required policy limit produces an honest partial handoff with the remaining acceptance items; it does not convert them into optional work.

## 2. Baseline and first reads

PR #122 merged as `2f5703f7c9987bbc25699b205495d53f00101924`. At package preparation, master was `daf87b545693fc5000e7897b3835ccb80037797f`, which also contains PR #123's v0.0.181 preparation. Refresh master and release metadata before starting; preserve any later owner work. Do not work from the obsolete pre-merge branch or reset to the recorded SHA.

Read the current `AGENTS.md`, `docs/VALIDATION-POLICY.md`, `planning/races-unleashed-compatibility/MISSION.md`, `planning/summon-cleric-alignment/STATUS.md`, `planning/summon-alphabetical-menus/STATUS.md`, relevant runtime/save-observation procedures, and the code mapped in REFERENCES. Inspect the current working tree and use a dedicated `codex/*` branch/worktree without disturbing other work.

Preserve the deferred handoff as a historical baseline, either as an unchanged `BASELINE.md` beside the activated mission or through an explicit immutable source reference. Keep its `FAIL, 70 PASS / 1 FAIL` attached to its original candidate and environment. The current resume text may lag the actual merge/release state; reconcile it against Git and release evidence.

The installed RU version was recorded as 1.0.11, while Nexus advertises 1.1.1. Reconcile package metadata, DLL hash, actual assembly, and loaded contracts before mutation. Do not assume these labels establish different behavior or automatically replace the installed mod. Historic binaries and hashes are evidence, not installation pins.

## 3. Required gameplay contract

### 3.1 Access follows the real feat

Use the exact installed Spider Summoner feature and preserve its natural acquisition prerequisites. It is a Drow racial feat, not a general cleric ability. Do not recreate access using a weaker test such as Drow race, cleric class, a deity, a model name, or presence of RU alone.

All four added species and all generated higher-tier choices must require the feature in both visibility and execution. A shortcut, held selection, or open menu must not permit a cast after the feature is lost. A refusal must occur before spending the source slot or starting summon effects. Availability logic may only restrict a native result; it must never re-enable a spell that is otherwise unavailable.

The ordinary Giant Spider already in Expanded Summoning retains its ordinary availability without the feat. The feat changes its qualifying poison/web DCs, not its access requirement. Noncleric feat holders receive the additions without acquiring cleric-only restrictions.

### 3.2 Four species in both spell families

Support Giant Crab Spider at base tier I, Giant Black Widow at IV, Ogre Spider at V, and Giant Tarantula at VII, in both Summon Monster and Summon Nature's Ally. Reuse the verified intended creature assets; every choice must summon the named species. Preserve unrelated expanded and foreign choices.

Inspect all eight original rows, including the previously unclassified SNA rows. The documented Ogre Spider SM row selects Redcap or Bralani because its copied spawn actions are nested. Correct that actual effect. Inspect the SNA Ogre row for the same failure and repair any confirmed mismatch within this feature's scope. Do not assume all eight graphs have the same structure.

Do not globally replace native Redcap/Bralani units or edit their original spells. A legitimate Bralani or Redcap choice elsewhere must remain unchanged. New spider rows should have readable existing icons/portraits and accurate names, counts, and tooltips; use established art guidance if any asset changes prove necessary. New artwork and creature-balance redesign are not required.

### 3.3 Complete higher-tier quantities

Implement the exact matrix in ACCEPTANCE for both families through tier IX. For a spider whose base tier is B, a cast at B summons one; B+1 summons 1d3; B+2 and above summon 1d4+1. No row appears below B.

These are base spell quantities. Preserve Superior Summoning and other applicable existing count modifiers exactly once through the existing summon lifecycle. Preserve Augment Summoning and other established creature buffs separately. Do not bake modifiers into base dice, apply them once per wrapper, or substitute a fixed random result.

There are 23 logical species/tier placements per family, 46 across both, including the eight original own-tier placements. This is semantic coverage, not a demand for 46 registered blueprint objects. One visible choice represents a species and quantity; the template choice must not create duplicate celestial/fiendish menu entries.

### 3.4 Consistent Summon Monster templates

All four SM spiders, including repaired originals and higher-tier choices, must use the existing Expanded Summoning template-selection behavior: good casters select celestial, evil casters select fiendish, and morally neutral casters (LN, N, CN) follow the existing neutral-caster toggle. Evaluate the current choice at execution. Do not cache the toggle value when a menu opens, create a second toggle, or reset the player's existing preference.

Use the established HD-based template bands, smite handling, and alignment machinery with each spider's verified creature HD. Do not choose template strength from cast tier, quantity, caster level, or the donor spell's original creature. Remove or suppress overlapping inherited template actions only where the adapter supplies their intended equivalent. Each spawned unit receives one selected template, with no leftover opposite template or duplicate smite effects.

Preserve the existing distinction between template mechanics, the unit's actual runtime alignment, and the summoning spell's effective alignment classification. Do not rewrite the global alignment behavior of unrelated summons as part of this mission.

SNA spiders receive no SM celestial/fiendish templates. Preserve or correctly follow the existing SNA alignment semantics; do not classify all SNA spiders as universally neutral merely because they have no SM template. This is not authorization to expand the existing SM-focused cleric policy into a general rewrite of every SNA spell.

### 3.5 Cleric alignment remains centralized

Reuse `SummonClericAlignmentRuntime.DecideGovernedRow` and the established policy: disallow effective spell alignment axes opposed to the cleric or the cleric's deity. Preserve both Good/Evil and Law/Chaos, real class-level detection, multiclass behavior across books, and the existing unknown-deity behavior. Do not infer neutrality from missing blueprint descriptors or an unknown identity.

Verified templated spider SM casts use the existing caster-matched spell classification. A celestial buff alone must not make a morally neutral caster's spell Good, nor a fiendish buff alone make it Evil. Classification must describe the corrected effect, not the former Bralani/Redcap donor branches or their union.

Menus and final execution must use the same current decision. Keep the execution check before native spending so held selections and saved shortcuts cannot bypass it. Preserve current treatment of unrelated item uses and class spell-like abilities.

### 3.6 Casting routes and resources

Cover ordinary prepared and spontaneous SM/SNA spells where offered by a valid installed spellbook, Herald Caller conversion from an unrelated prepared spell, and Druid spontaneous SNA conversion. Check prepared summon parents separately, because their variants and flattened conversions can otherwise duplicate the same choices. Ordinary spontaneous slot-pool casting requires its own verification; prepared-spell conversions and class summon SLAs are not substitutes. Do not introduce a new class/provider to create a route absent from the supported installation.

Reuse the existing conversion infrastructure. Preserve the selected casting tier, original prepared source or spontaneous pool, spellbook, caster, `Fact`, metamagic, caster level, duration, and summon lifecycle. A successful cast spends exactly its proper original spell source once. A failed or stale cast spends nothing. No separate spider resource or extra spell preparation is introduced.

Extend foreign admission using verified family/tier/quantity contracts. The old four single-row contracts cannot be indiscriminately reused for new higher-tier rows. Each generated choice must have its own supported casting-tier and multiplicity semantics. Preserve the existing duplicate suppression and alphabetical ordering.

### 3.7 Exactly one poison and web DC benefit

Implement the feat's +2 to qualifying summoned spiders' poison and web effect DCs. Cover all four extra species in both families at every supported quantity, plus the existing ordinary Giant Spider and its applicable spell routes. Every eligible creature produced by a multi-summon cast receives the benefit, including an additional creature supplied by an existing count modifier.

First inspect the actual installed feat implementation. Establish whether it uses a summon listener, a post-spawn action, a unit buff, or another mechanism. Reuse a correct implementation or narrowly replace the overlapping behavior. Do not add a second unconditional listener and assume stacking will be harmless. Record which implementation owns the final +2.

Identify eligible species and their summon provenance explicitly. Do not identify spiders by displayed names, shared rigs, donor blueprints alone, generic poison effects, or the presence of a web ability. Bebelith, Giant Scorpion, ordinary crabs, ants/beetles sharing spider donors, unrelated Web spells, nonsummoned creatures, and summons from casters without the feat must not acquire the bonus merely through shared implementation. Preserve normal feature acquisition and existing summon timing; document any necessary treatment of already active summoned instances during an upgrade.

The ordinary Giant Spider's implemented web is an extraordinary ranged touch attack with no initial saving throw. Its relevant web DC is carried into the held/break-free effect. Add the benefit to the actual applicable web effect/escape DC; do not invent an initial save, improve the attack roll, add uses, or change duration. Do not globally edit the shared webbed buff. Trace the corresponding mechanics of each RU spider rather than assuming all webs work identically.

Poison verification must include the actual first and continuing saving throws, preserving normal Constitution scaling and interactions such as Augment Summoning. Repeated saves, initialization, casts, or reloads must not add another +2. Verify the resolving effect context, not just a tooltip or a pre-cast parameter event. Existing relevant DCs gain the benefit; this task does not invent poison or web abilities that a supported spider does not possess.

## 4. Implementation design and ownership

### 4.1 Extend precise contracts

The current direct-spawn classifier rejects the Crab/Widow template conditional. Do not simply whitelist that action and leave the result as `Fixed(None)`. Represent supported templated direct effects explicitly and return the appropriate governed alignment. Verify relevant buff targets and application properties if the existing signature omits them.

Use exact full identities, expected features, tier, multiplicity, units, conditions, and relevant action structure. Unknown/reversed conditions, altered quantities, hidden spawns, wrong prerequisites, or unexpected effects remain unsupported. Preserve unrelated foreign behavior and emit a concise diagnostic; do not classify an unfamiliar graph as neutral or claim full compatibility. Do not publish generated extensions from an unverified source contract.

Separate the supported legacy input contract from the expected repaired output. Validate before and after mutation. A known bad legacy Ogre payload can be recognized for a scoped repair; that recognition does not make the old effect the final success criterion.

### 4.2 Preserve identity and isolate mutable actions

Preserve all eight original RU ability identities, including their nonstandard full string length. Prefer repairing recognized original rows in place by installing private cloned action components, retaining their identity. Reuse recursive deep-clone and nested-spawn helpers. Never mutate a nested action shared with the original vanilla donor or another mod's unrelated choice.

Create genuinely new higher-tier identities through the repository's stable identity mechanism, keyed by family, casting tier, species, and quantity. Do not generate identities from list position or localized labels. A replacement-row design is acceptable only if it demonstrably preserves old saved references and source context; do not casually delete the old rows or replace them with unresolvable IDs.

Record the component/array ownership of each mutation. Restore only state still owned by this adapter. A later foreign mutation must survive cleanup. Repeated reconciliation must not append more rows, listeners, or buffs.

### 4.3 Initialize after RU and before consumption

Use the demonstrated late-load reconciliation boundary and existing sorting mechanism. Avoid a new per-frame scanner or a broad Harmony patch that affects every spell. Resolve the foreign rows only after the required RU initialization has finished.

Current foreign classification is cached by row and library. Repair and normalization must precede the first classification/projection, or invalidate/version the affected cache deliberately. Reset relevant engine component-provider caches when replacing components. Check that parent/child references and conversion receipts cannot retain a stale uncorrected graph.

### 4.4 Keep optional dependencies optional

Do not add a hard assembly dependency on RU or Call of the Wild. Detect the supported feature/assets and validate the actual graph; a display version string alone is not a behavior contract. RU absent must be a clean no-op for this optional integration.

Publish the extra roster as a feature-gated overlay on the applicable spell routes. Do not add the four species unconditionally to a central roster that is projected into Monster Tactician, Summoner, Master Summoner, Occultist, or other class abilities. This mission does not add Spider Summoner support to those class SLAs.

Integrate new identities with the repository's disabled-module registration and persistence strategy. With RU still installed and Expanded Summoning inactive, original RU behavior must remain available through the supported lifecycle; KMG additions must not leak. New KMG references need safe dormant behavior, and active summons must load/expire consistently with existing module guarantees. Do not promise that uninstalling RU from a Drow save is supported.

## 5. Work phases

### Phase 1: Inspect and activate

Refresh the merged baseline, preserve unrelated work, activate this mission, and capture a concise before-state. Inspect the installed feat and all eight original graphs, spell-route publication, natural prerequisites, DC implementation, and source-unit properties. Map expected units, buff contracts, HD bands, and affected abilities. Reconcile version labels without downgrading. Record any still-unknown interpretation before selecting a mutation.

Deliverable: an evidence-backed contract inventory in the mission status and a concrete implementation outline. Continue to implementation when the required facts are established; this is not a planning-only stopping point.

### Phase 2: Repair and normalize original choices

Implement precise classifier support, correct Ogre payloads, standardize all four SM template paths, establish exactly-one DC-benefit ownership, and preserve original IDs. Use existing helpers and narrow hooks. Add focused behavioral cases for the changed contracts and negative shapes.

Deliverable: coherent implementation of original-row behavior with reviewed diff and focused checks. Do not stop here and label the complete mission done.

### Phase 3: Complete quantities and routes

Generate the required matrix as an optional feature overlay. Integrate ordinary prepared/spontaneous routes, Herald conversion, and Druid conversion while preserving spell provenance, ordering, and duplication guards. Complete module/absence registration and affected save-reference handling. Ensure the ordinary Giant Spider receives only the feat benefit, not a new access restriction.

Deliverable: the full gameplay scope implemented, meaningful focused tests, and a concise code review of the complete change before selecting native qualification.

### Phase 4: Focused native verification and handoff

Use the acceptance matrix to select the smallest real scenarios covering each concrete risk. Cast every original spider in both families; verify representative group quantities, template cases, actual DCs, conversions, and save/module boundaries. Use natural feat/class acquisition in at least one legitimate fixture. Revise the observer to assess the corrected behavior. Package and reuse the same verified candidate where its inputs are unchanged.

Deliverable: one reviewable draft PR and an honest status table for every acceptance ID, with candidate identity, evidence, and restoration outcome. Update current known limitations only to the extent the new evidence resolves them. Preserve old release notes and historical results.

## 6. Validation policy and runtime limits

Follow the current repository validation policy. Its ordinary validation target is 15 minutes and ceiling is 30 minutes, excluding necessary safe cleanup/restoration. This package does not reuse the old mission's 2,400-second ceiling, authorize a broad release campaign, or grant a new larger runtime budget. Do not evade the limit through repeated relabeled phases or unchanged launches.

Implement and review the coherent scope first. Reuse existing test infrastructure and add behavioral tests for actual risks; do not add libraries, assertion-on-source tests, or mock whole modules. Choose named focused filters only after confirming they select relevant tests. Reject a zero-test selection. Compile affected production code. Build unchanged candidate bytes once and reuse their verified artifact.

Use repository-owned guarded runtime entry points, Steam App ID 640820, leases, valid fixtures, transactional staging, and exact restoration. `KMG_AUTOMATION_BASELINE` and human saves are protected. Create or use only the authorized disposable fixture. Preserve the current install, enabled-mod settings, saves, and `Params.xml`. Respect current game/Harmony/UMM reference contracts.

Select valid dependency profiles. Do not disable RU in a full stack while leaving mods that require it enabled. Use the actual combined installation as the main profile, an appropriate minimal comparison only where needed to establish attribution, and focused valid absence/module-inactive controls. These targeted controls do not authorize the entire historic compatibility matrix or all module combinations.

Do not rely on random outcomes alone to prove quantity dice. Verify base expression and actual native counts with applicable modifiers, without repeated fishing for a desired roll. Screenshots establish presentation; structured cast/spawn/resource/DC observations establish mechanics.

If remaining required native checks cannot fit safely within policy, finish authorized safe work, restore exactly, publish the reviewable checkpoint, and list the remaining acceptance IDs with the smallest next test bundle. Do not remove requirements, weaken a failing assertion, claim mission completion, or launch a broad replacement campaign. A substantive engine or source-contract ambiguity is a named blocker, not a reason to alter the gameplay rule silently.

## 7. Observer and regression work

The old native observer encodes the defective Ogre outcome and an obsolete 3-inherent/1-governed census distribution. Replace these expectations with per-row intended semantics. After normalization, the corrected SM rows must not report stale Bralani descriptors, and actual Ogre casts must produce the named spider. Test unknown shapes independently rather than simply marking everything verified.

Retain the requirement that all supported published rows have a valid classification. Keep the old failed result attached to its original candidate. Changed observers/fixtures require their affected scenario again; do not reinterpret missing native observations as success.

Include bounded regression controls for ordinary Giant Spider availability, actual Bralani/Redcap choices, a noncleric, a multiclass book, a deity-derived restriction, existing Augment/Superior Summoning, and unaffected shared poison/web consumers. If parent selection graphs change, include a focused NPC-without-feat selection/AI control against summon-selection loops.

## 8. Documentation and final delivery

Maintain `planning/races-unleashed-compatibility/MISSION.md`, `STATUS.md`, and `ACCEPTANCE.md` on the dedicated branch, retaining baseline provenance and relevant source references. Update only the active part of `AUTONOMOUS-RESUME.md`; preserve historical records. Describe supported versions/contracts, both spell families, full quantity behavior, template selection, exactly-once feat benefit, and any unresolved limitations.

Use the current repository push workflow, including `C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1` while that remains the governing entry point. Do not commit generated candidate archives, raw machine evidence, saves, proprietary game/RU assemblies, credentials, or local configuration.

The final handoff must state: PR/branch/head; complete-for-review or incomplete; behavior implemented; focused/native results and their limits; candidate source/package/DLL identities; affected file areas; acceptance IDs still open; actual restoration result; and the smallest necessary follow-up if blocked. No merge, release, permanent install, or claim of broad release qualification follows from this mission.
