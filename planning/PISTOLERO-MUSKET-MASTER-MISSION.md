# Pistolero and Musket Master Mission

## Authority and baseline

This file is the durable operational contract for the user-issued **Codex
Autonomous Work Order: Pistolero and Musket Master, Revision 2** received on
2026-08-07. The full work order in the task record remains authoritative; this
copy preserves every operative requirement needed to resume safely.

- Work only on Pistolero, Musket Master, and the bounded base-starting-firearm
  investigation.
- Base all work on clean merge commit
  `10b792735db5d685b46749dc08ea819f31fa8052` or a clean descendant containing
  its optional-mod compatibility framework and current-donor Evasive repair.
- Preserve existing blueprint identities, base Gunslinger behavior, Mysterious
  Stranger behavior, exact compatibility profile identities/dispositions, and
  save semantics.
- Do not claim runtime correctness from builds or deterministic tests.
- Use only the guarded Steam App ID 640820 runtime harness and disposable
  fixtures. Never use UI automation, OCR, screenshots, or direct executable
  launch as mechanical evidence.

## Mandatory feature contract

Pistolero receives one-handed scoped proficiency; an exact production Pistol,
powder, ball, and kit; Up Close and Deadly at 1 in place of Deadeye; the exact
existing Deadeye at 7 in place of Startling Shot; Twin Shot Knockdown at 11 in
place of Bleeding Wound; and Pistol Training ranks at 5/9/13/17 in place of all
base Gun Training selections. Its starter is always the exact production
Pistol and never a Musket or duplicate firearm.

Musket Master receives two-handed scoped proficiency; native
`ReplaceStartingEquipment = true`; an archetype starting array containing
exactly production Musket, black powder, lead ball, and gunsmith kit; Steady Aim
and exact Rapid Reload (Musket) at 1 in place of Gunslinger's Dodge; Fast Musket
at 3 in place of Utility Shot; and Musket Training ranks at 5/9/13/17 in place
of all base Gun Training selections. The live starter transaction must observe
exactly one native production Musket, no production Pistol, top exact powder and
ball stacks to 20/20, and bind battered ownership to that exact Musket and
receiver without duplication on later callbacks.

Up Close and Deadly is a free-action one-shot arming adaptation, spends exactly
1 grit only after an eligible one-handed non-scatter hit or miss, deals
1d6/2d6/3d6/4d6/5d6 precision damage at levels 1/5/10/15/20, halves the same
roll on a miss, is unmultiplied on criticals, excludes misfires/scatter and
precision immunity, and is never reduced by True Grit.

Twin Shot Knockdown is a targeted free action after two distinct same-target
one-handed firearm hits in the actor's current turn/action cycle. It revalidates
target, turn, immunity, prone state, and grit; applies the proven no-save/no-CMB
prone delivery; is one opportunity per target per turn; and rolls grit back on
unexpected delivery failure.

Steady Aim is a move-action one-shot arming ability with no spend and a
positive-grit requirement removable only by its True Grit choice. It adds 10
feet to one exact two-handed direct shot's effective range increment, stacks
before Deadeye/touch-AC/range-penalty calculations, excludes scatter, expires
at turn end, and never mutates shared weapon metadata.

Fast Musket is passive, uses the authoritative positive-grit/True Grit state,
and extends the central reload policy: treat qualifying two-handed firearms as
the corresponding one-handed profile, then apply matching Rapid Reload, then
existing higher-priority Lightning Reload rules. Presentation, command creation,
execution, and full-attack auto-reload must revalidate state.

Pistol/Musket Training share one idempotent training service. Correct-family
damage is Dexterity plus rank-1 at 5/9/13/17, with Dexterity applied at most
once and highest entitlement selected across stale/overlapping facts. Broken
misfire increase is +2; normal is unchanged; Wrecked fails closed. Base
exact-kind training and Dead Shot use the same service without behavior change.

## Required shared architecture

- One fail-closed project-owned handedness policy classifies Pistol/Revolver as
  one-handed and Musket/Blunderbuss/Rifle as two-handed.
- Preserve full `KMG.Firearms.FirearmProficiency` identity and old-save meaning.
  Add stable one-handed and two-handed facts, exact restriction support, proper
  Reload/Scatter action grants, and one public EWP (Firearms) feat granting the
  existing full fact.
- Firearm feat prerequisites must accept full or matching scoped proficiency,
  reject donor-category leakage, remain duplicate-free, and preserve exact
  Rapid Reload (Musket) identity.
- Generalize the starting-firearm observer/resolver with precedence: Musket
  Master Musket; Pistolero Pistol; optional explicit base choice if safely
  implemented; otherwise base/Mysterious Stranger Pistol. Observe native grants
  only, preserve detached no-delta behavior, bind exact item/receiver, and roll
  back only project-added ammunition on binding failure.
- Add archetype-aware ownership prerequisites and real runtime consumption for
  True Grit choices: Twin Shot, Steady Aim, Fast Musket, Focused Aim, and
  existing owned deeds. Never add Up Close and Deadly or Clipping Shot.
- Substitute truthful archetype deed summaries and recursively traverse base
  progression, selections, archetype additions, abilities, and buffs for
  localization/icon publication.
- Append exactly one Mysterious Stranger, Pistolero, and Musket Master while
  preserving unrelated current archetypes and exact rollback snapshots.
- Preserve the merged optional-mod framework, current-donor ordered-component
  validation, root/catalog/chargen observers, sentinel-owned transactions, and
  historical evidence classifications.

## Secondary investigation

Only after mandatory archetypes are source-qualified, inspect the exact
level-up transaction for an obligatory base/Mysterious Stranger Pistol-or-
Musket choice with old-save Pistol default and exact starter binding. Implement
only if it avoids delayed inventory replacement, global mutation, duplicates,
and respec/save risk. Weapon Focus is not the selector unless every timing,
uniqueness, rollback, and no-prior-grant condition is proven. Otherwise defer
with exact evidence; this does not block core completion.

## Verification and publication

For each coherent phase: focused tests, repository validation, the complete
domain suite, clean exact-reference Release build, build-output and SoundBank
validation, package creation and strict validation, relevant guarded runtime
scenario, coherent commit, approved push script, remote-SHA verification, and
durable journal/resume update.

Final evidence must include archetype catalog/starter, Pistolero mechanics,
Musket Master mechanics, persistence/respec, and all materially touched legacy
scenarios; two fresh-process archetype-comprehensive passes; and two working-
save smokes when the inherited baseline permits. Run required exact compatibility
profiles (standalone, Arms & Armor, Toggle Custom Soundpacks, qualified combined)
through the existing transaction wrapper and exactly one bounded Call of the
Wild sequence. Preserve the public CotW `CONFLICT-CONFIRMED` classification
until required human chargen confirmation.

Advance the patch version from the actual base and update Info/package/schema
pins transactionally. Commit no packages, proprietary assemblies, saves,
credentials, raw machine-local evidence, or transient compatibility state.
Never merge, force-push, rewrite history, or work on master.

## Stopping and completion

Continue through failures while a safe reversible evidence-supported strategy
exists. Stop only for a work-order hard stop: unsafe/no valid baseline; genuinely
missing required proprietary contract/asset; exact runtime proof that safe
implementation is impossible; forbidden Steam/DLC/UI state after independent
source work; contradictory authoritative local rules unresolved by the work
order; or policy violation.

Completion requires stable identities; exact progression, starter, proficiency,
training, reload, range, True Grit, presentation, save/reconciliation, bootstrap,
manifest, package, runtime, and compatibility contracts; clean published branch;
exact hashes/run IDs; implementation and qualification reports; a short human
checklist; and a current `AUTONOMOUS-RESUME.md`. If only the inherited detached
Dodge defect blocks the aggregate, report exactly: **Pistolero and Musket Master
independently qualified; full Gunslinger aggregate remains blocked by the
inherited Dodge defect.**
