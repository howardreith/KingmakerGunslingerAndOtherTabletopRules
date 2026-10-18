# Sprint 27 contract: Griffon

Status: IMPLEMENTATION; NOT QUALIFIED; NOT RELEASED.
Owner mission dated 2026-10-18 selects Sprint 27 through its own stable release,
exact restoration and final-records merge. Sprint 26 remains closed at v0.0.160.
Production root selected before candidate bytes: C:/Dev/KingmakerGunslingerLab/wt/s27.
Product branch: codex/expanded-summoning-sprint27-griffon, directly from current
master 2a0873d8a8848b0f1109415f1e4da14641d9456e. Expected v0.0.161 is unreserved.

## Rules and reconciliation

Primary adult PF1 sources inspected 2026-10-18:
- https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Griffon
- https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Nature%27s%20Ally%204
- https://legacy.aonprd.com/bestiary/universalMonsterRules.html (Pounce, Rake, natural attacks).

Printed baseline: neutral Large magical beast, 5d10+15 (42 hp);
Str16/Dex15/Con16/Int5/Wis13/Cha8; AC17/touch11/flat15 (Dex2, natural6, size-1);
Fort7/Ref6/Will4; BAB5/CMB9/CMD21,25 versus trip; space10/reach5 feet.
Bite +8 1d6+3; two primary talons +7 1d6+3; pounce and two conditional rake
claws +7 1d4+3. All five weapons add the full live signed Strength modifier once.
Weapon Focus applies only to bite. Preserve other native modifiers and damage forms.
Feats: Iron Will, Skill Focus (Perception), Weapon Focus (bite).
Skills: Acrobatics10/Fly6/Perception12, racial +4 Acrobatics/+4 Perception.
Rank/class/attribute/feat/racial attribution must be verified against final native graphs.
No unsupported Fly ranks may become extra combat skills.

The charter's shared word Grab is reconciled against the adult entry: Griffon
has no special Grab. Grant no automatic grab, grapple bonus, carry-off, Rend or
wing/tail attacks. Rake is charge-conditional, never two unconditional limbs.
Retain a genuine existing-grapple exception only where the already-supported
product path reaches it with start-of-turn and exact-target restrictions. Artificial
test grants do not establish reachable Griffon behavior. No generic grapple framework.

Reuse legal native charge/path/run-up/action/interruption/target-loss behavior.
A qualifying pounce normally resolves the ordinary three attacks plus two rake
attacks against a surviving valid target. No hit/grab prerequisites and no retry
on misses. State must remain exact-owner/command/target scoped, clear on abort,
later commands, opportunity attacks, death, dismissal and reload.

Movement: printed land30/fly80 average. Use the existing ordinary-map airborne
navigation convention; where only one effective speed exists, use aerial80 and
explicitly disclose omitted land mode. Never sum modes. No selectable altitude,
wall/cliff bypass, untargetability, carrying, falling or global navigation changes.
Large footprint does not enlarge printed5-foot melee reach; decorative wings
must not enlarge collision or attack range. Printed darkvision60/low-light/scent
retain PASSIVE_CREATURE_SENSES_UNMODELED where no faithful engine consumer exists.

## Publication and assets

Only untemplated SNA IV one, V1d3, VI-IX1d4+1: six roots; no SM, template,
feat gate, hippogriff, mount, companion or polymorph. Keep Griffon suppressed
until required hidden qualification passes. Preserve every released GUID/icon.
Expected arithmetic, to be derived from source:111 distinct creatures;
Monster96/545 unchanged; Nature98/563;1108 generated roots;29 retained native
wrappers;1137 visible choices. Exact new manifests/dependencies advance together.

Intake found no Griffon source/export or model in the existing catalogs.
One built-in image_gen call supplied actual Gorgon/Eagle family pixels and
produced the pinned original recorded in sprint27-generation.json. Export uses
tools/New-ExpandedSummoningIcons.ps1. HumanReview: NOT_PERFORMED_NONBLOCKING.
Fresh guarded research proved no exact native Griffon and one57-bone Manticore
renderer with jaw/legs/tail/wings and native attack events. One original low-detail
prototype is integrated through existing attachment/ownership/rollback machinery.
Static read-only review found no functional recognizability blocker; native body
and combat behavior remain unqualified. Shipped success must show the actual
hybrid body, never donor fallback. No proprietary art redistribution.

## Qualification and stop

Focused edits use nonempty named tests and affected production compilation.
Read coherent changes before one complete Sprint gate at the candidate boundary.
Native batches will be declared with measured harness estimates before launch:
profile/ordinary/signed damage/reach/movement; RTWP and TB charge/rake including
negative state cases; exact-owner AI/body/placement/quantity; lifecycle/module
boundary/targeted persistence; six public roots and real slots/duration/alignment.
Controls follow actual changed seams: released pouncer/raker, released flyer,
chosen native donor; Bulette only if its shared charge seam changes.
Reuse scene/clock/lease/restoration/event writers; no broad creature-test framework.
No old-roster replay, complete five-profile campaign or all16 module states by default.

Every verdict retains source/ZIP/DLL/MVID/observer/environment identity and scope.
Record native failures and unmet preconditions. Preserve KMG_AUTOMATION_BASELINE;
only authorized disposable fixtures/Working may mutate under exact restoration.
No game launch is authorized by this planning record alone: use guarded Steam640820
requests and current transactional intake. Stop on actual safety/source/rules ambiguity.

After hidden PASS, unsuppress Griffon and qualify final-artifact publication changes.
Use publisher refusal tests, preflight and dry-run; merge the product PR, promote
exact qualified bytes, download/verify actual assets and tag lineage, verify exact
restoration, merge record-only RELEASED/STOP branch. Stop before Sprint28.
Visual polish is nonblocking debt; invisibility, unselectability, navigation/attack
stall, missing rules/icon, corrupted save or unbounded leak remains blocking.
