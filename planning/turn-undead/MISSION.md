# Mission: add Turn Undead to Tabletop Rules Expansion

**Owner:** Howie Reith  
**Prepared:** 2026-11-27  
**Product repository:** [howardreith/KingmakerGunslingerAndOtherTabletopRules](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules)  
**Target:** Pathfinder: Kingmaker, the repository's supported build and loader profile  
**Requested end state:** a complete, tested Turn Undead feature with both supplied icons, in a draft PR ready for owner review through the current development process.

This is an implementation assignment for Claude when the owner supplies the packet as the task. Package preparation has not enrolled a controller run, created a branch or PR, changed game files, or implemented the feat. The packet does not authorize bypassing current project permissions or changing an existing mission.

## 1. Objective and owner direction

Add the Pathfinder general feat **Turn Undead** as an alternative use of a character's existing positive-energy channel. Its purpose is to make undead flee. The owner selected this feature alone after a broader feasibility investigation and asked for two original images: a channel-feat-style emblem and a distinct execution icon. They prefer the execution action under the existing Channel Positive Energy submenu where the loaded game supports that presentation.

The user's source is [Turn Undead on d20PFSRD](https://www.d20pfsrd.com/feats/general-feats/turn-undead/); the corresponding [Archives of Nethys feat entry](https://aonprd.com/FeatDisplay.aspx?ItemName=Turn%20Undead) is the rules reference. Use the full technical contract to implement the rules, including their practical Kingmaker adaptations.

This repository is a **Kingmaker** mod. The original suggestion mentioned Wrath mods, but this packet does not target Wrath of the Righteous or its different engine/API.

## 2. Read the complete task and current project instructions

The following files are appendices to this mission, not optional background:

| File | Purpose |
| --- | --- |
| [DEVELOPMENT_PROCESS.md](DEVELOPMENT_PROCESS.md) | Foreground Claude development, independent Codex review, guarded publication, limits, finalization |
| [TECHNICAL_CONTRACT.md](TECHNICAL_CONTRACT.md) | Behavior, native implementation leads, channel sources, classification, persistence, compatibility |
| [ACCEPTANCE_MATRIX.md](ACCEPTANCE_MATRIX.md) | 36 bounded acceptance items and native evidence requirements |
| [ICON_INTEGRATION_NOTES.md](ICON_INTEGRATION_NOTES.md) | Correct source/export paths, exact icon consumers, cache, catalog, build/package intake |
| [SOURCES.md](SOURCES.md) | Primary-source map, pinned versions, known limitations |
| [assets/asset-manifest.json](assets/asset-manifest.json) | The two art concepts, exact filenames/hashes, formatting and provenance |
| [assets/GENERATION_PROMPTS.md](assets/GENERATION_PROMPTS.md) | Actual image-generation instructions and reference inputs |

Use [resources/IMPLEMENTATION_RECORD_TEMPLATE.md](resources/IMPLEMENTATION_RECORD_TEMPLATE.md) to record new observations. It is a template; blank fields are not evidence. The optional [turn-undead.plan.json](turn-undead.plan.json) defines two checkpoints in one enrollment.

At intake, read the current repository's `AGENTS.md`, `CLAUDE.md` if present, `docs/VALIDATION-POLICY.md`, `docs/AGENT-REVIEW-WORKFLOW.md`, relevant runtime-testing instructions, and icon-art guide. Read the installed development-process operating guide, protocol, and Kingmaker project configuration. The owner's current instructions and actual enforced interfaces govern; dated research snapshots are navigation aids. If an interface changed, reconcile it explicitly. Do not weaken a protection or invent a permission to match this packet.

Commit the mission and necessary appendices in an appropriate planning location so the reviewer can read the exact task governing the branch. Keep their relationships and hashes traceable. Production art belongs in the established asset tree; the entire mission ZIP does not belong in the installed mod.

## 3. Scope

### Deliver in this mission

- A selectable general feat with a real positive-channel prerequisite, localized display/description text, stable blueprint identities, and the appropriate dedicated feature-module integration.
- One Turn Undead execution path for each supported native positive-channel source the character actually possesses: positive cleric, ordinary paladin, Hospitaler, and Empyreal sorcerer. Verify each source contract before binding it.
- Correct channel expenditure, action cost, targeting, saving throws, flight, expiration, and intelligent-only repeat saves through native rules.
- Explicit source choice for characters with multiple independent qualifying pools.
- Safe save/load and module publication behavior, preserving existing learned facts and action references according to the project's conventions.
- Existing-menu integration where the supported loaded profile provides a suitable positive-channel wrapper; a discoverable direct action where it does not.
- Narrow optional Call of the Wild compatibility for the selected supported profile: relevant wrappers, channel resistance and legitimate DC modifiers, without making CotW a required dependency.
- Both supplied icons at their actual user-facing consumers, with source provenance, catalog/build/package entries, and real in-game presentation evidence.
- Focused production-path tests, necessary guarded native observations, independent review dispositions, and a final verified owner-review handoff.

The source-coverage table must distinguish the four native sources from additional CotW-only classes/archetypes. Discover and document the owner's selected CotW profile early. Extra third-party positive-channel providers are not an unbounded completion requirement; integrate those justified by that supported profile and disclose any unsupported providers accurately.

### Outside this mission

Command Undead, the Command Undead or Control Undead spells, Mythic Turn Undead, rebuking/cowering control variants, permanent servants, undead hit-dice budgets, necromancer domination, new channel-damage modes, quick/swift/smite/cone/line Turn variants, unrelated wizard fixes, new classes, a general channel UI rewrite, a global fear overhaul, equipment dropping/disarming, unrelated compatibility remediation, and development-controller changes are outside scope.

Do not add a library for this feature unless a concrete need replaces substantial code and satisfies current project/owner policy. Existing engine and repository facilities should suffice. Do not fold unrelated cleanup into the feature PR.

## 4. Gameplay contract at a glance

| Decision | Required behavior |
| --- | --- |
| Prerequisite | Actual positive-energy channel class feature; no good-alignment-only or cleric-class-only shortcut |
| Action | Standard action, supernatural; one use of the selected source, translated to its real underlying cost |
| Area | 30 feet around the caster, using the verified native delivery semantics |
| Targets | Actual undead, including friendly undead unless a specifically supported selection rule excludes them |
| Initial save | Will for every eligible undead, including mindless creatures |
| Failure | One minute of native fleeing behavior representing panic |
| Repeat saves | Intelligent undead only, once at each verified native round opportunity, ending the owned effect on success |
| Ordinary cleric DC | `10 + floor(cleric level / 2) + Charisma modifier`, with genuine applicable bonuses |
| Other sources | Verified source-specific level/stat/resource mapping; damage dice are not a substitute for DC level |
| Damage/healing/control | None; no automatic destruction and no Hit Dice ceiling |
| Resources | Existing pool only; no new daily pool or refills from acquiring/reloading the feat |
| Multiple sources | Keep pools, effective levels, costs and DCs separate; no silent fallback |
| Existing wizard ability | Preserve its identities, scaling, resource and behavior |

The technical contract owns the detailed implementation requirements. In particular, the package does not claim that a generic fear buff will work on undead or that a native buff's default repeat-save behavior already satisfies this feat.

## 5. Decisions already made for implementation

### Use the existing native turning pattern as a starting point

The historical donor is `TurnUndeadNecromancy`, not a generic damaging channel or a domination spell. Create private owned definitions and replace its wizard-specific binding. Inspect current engine contracts before cloning components. Never edit the shared donor or its shared buff merely to implement the general feat.

### Treat mindlessness as a real data problem

Ordinary skeletons and zombies in the inspected historical data have Intelligence 10. A zombie lord shares the ordinary zombie's broad type and AI brain. An Intelligence threshold, display-name test, or generic zombie marker is therefore inadequate. Find a reliable actual runtime characteristic; otherwise use a small, reviewed classification inventory with stable identities and explicit exceptions. Keep unsupported third-party classification and its actual behavior visible. The core supported native content must not silently inherit the wrong repeat-save rule.

### Preserve a narrow immunity exception

Undead's ordinary mind/fear immunities must remain intact. Use the native undead-turning exception, with the correct native rule context, and verify separate explicit condition/story protections. Initial and repeat saves must receive appropriate channel resistance and legitimate modifiers. New ability identities are not automatically included in ability-specific modifier lists.

### Use the menu actually present in the loaded profile

The inspected native 2.1.4 channel abilities are separate heal/harm actions. CotW constructs wrappers that supply channel submenus. Prefer the appropriate existing positive-harm wrapper when present, adding only the owned feat-gated child. Preserve existing variants, saved references and parent icons. Use source-specific child identities when required; do not assign one child's `Parent` to multiple incompatible wrappers. For a profile without a suitable submenu, a normal pinnable action is the scoped fallback.

Do not blindly call CotW's `storeChannel` to register Turn as a normal damage channel. That route also creates unrelated channel transformations. Inspect and use only the narrow integration necessary for this feature.

### Use the supplied artwork as the production concepts

| Consumer | Supplied runtime file | Format |
| --- | --- | --- |
| General feat and visible feat fact | `assets/icons/turn-undead-feat.png` | 64×64 RGBA PNG, transparent terracotta emblem |
| Executable Turn Undead action/variant | `assets/icons/turn-undead-action.png` | 128×128 RGBA PNG, painted positive-energy repulsion |

Each has a byte-preserved 1254×1254 original under `assets/source/`. The full-resolution action original is RGB; its runtime export is RGBA as required. These are two concepts with two originals, not four separate icon requests.

The package's runtime PNGs were resized mechanically with Pillow. They meet the size/encoding requirements but are not asserted to match the repository's Windows GDI+ exporter bytes. Intake the originals and actual prompts, run the scoped canonical exporter where available, and record the resulting real hashes. Retain the art; no fresh generation is required for ordinary implementation or builds. Technical/thumbnail review has occurred; native UI verification and the owner's subjective visual approval are separate statuses.

## 6. Work sequence and checkpoints

1. **Intake and isolate.** Verify repository/remotes/current instructions; discover active missions and installed controller; create an unused `codex/*` branch/worktree from the actual target tip. Record supported game/loader/mod profiles. Preserve other work and runtime locks.
2. **Establish contracts.** Inspect the installed engine and loaded blueprints. Complete the source-mapping table, immunity/modifier route, classification strategy, menu strategy, persistence route, and visible-icon inventory. Convert any historical assumptions into actual evidence or clearly identified open questions.
3. **Build one meaningful path.** Implement a private cleric Turn action through the real rules, with proper expenditure and one verified native victim. Establish the new feat/module definitions and a focused nonzero test path. Submit the optional intermediate controller checkpoint with remaining coverage clearly pending.
4. **Complete the feature.** Bind the supported native sources, intelligent-only recovery, duration/persistence, resistance/modifiers, scoped compatibility, and presentation. Integrate both art concepts using current repository conventions. Add only meaningful regression coverage for observed hazards.
5. **Verify and review.** Cover the acceptance matrix with focused production-path checks and a small number of grouped native fixtures. Preserve exact artifact/evidence identities and restoration receipts. Run the accepted independent-review loop and correct findings coherently.
6. **Finalize the handoff.** Follow the controller's actual finalization procedure. Report the draft PR, reviewed artifact, accepted reviews, evidence, adaptations, limits and remaining subjective art review. Stop at verified `AWAITING_OWNER_REVIEW`.

The current proposed plan is one focused PR with an early checkpoint and a final checkpoint. A separate prerequisite PR is appropriate only if discovery identifies a truly independent bounded dependency; it requires a separate explicit mission/enrollment, not a fiction that checkpoints create separate PRs.

## 7. Validation and finish criteria

Use `ACCEPTANCE_MATRIX.md` as the finite checklist. It is not 36 separate runtime campaigns. Several rows should share one meaningful encounter or fixture. Fast tests should exercise production policy/adapters with realistic records; native observations must establish actual rules/UI behavior where unit tests cannot.

Required native evidence includes ordinary feat acquisition, the actual execution/menu route, correct resource debit, successful and failed initial saves, actual flight, mindless persistence, intelligent recovery, expiration, save/load during an effect, both combat modes, and the two real icon consumers. Verify representative supported source boundaries and compatibility seams rather than rerunning unrelated campaigns. Follow current validation budgets and complete safe restoration of any started transaction.

The implementation is complete only when required behavior is supported by appropriate evidence, required checks and accepted reviews are current, the scoped PR is reviewable, runtime transactions are restored, and finalization reports the proper owner-review state. A compile, a screenshot, an assumed native API, or an informal reviewer opinion cannot replace missing required proof.

If a required environment, rule resolution, or controller permission is unavailable, continue useful independent work and report the exact blocked item and its impact. Do not claim final completion, fabricate evidence, substitute unrelated historical authorization, or widen access. Ordinary implementation choices and accepted-review corrections do not require repeated owner confirmation.

## 8. Research baseline and update discipline

This packet inspected TRE `master` at `52755b617c79328a972ae5b74bc05784422e332d`, development-process `main` at `a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b`, and CotW source at `1332fb0db844b7863f484ca978bea2349fe49769`. Native blueprint observations came from a community 2.1.4 archive; the expected lab target has been Kingmaker 2.1.7b. Confirm the actual installation, loader and reference identities rather than treating those historical labels as proof.

No new gameplay GUIDs, reviewer receipts, owner approvals, active enrollment IDs, native test outcomes, or release permissions are preallocated or asserted by this packet. Record real values as work proceeds. Use the linked source map and current files to resolve drift while preserving the selected feature's scope.
