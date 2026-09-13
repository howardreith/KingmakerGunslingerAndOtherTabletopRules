# Firearm postrelease hotfix qualification — 0.0.128

This report records the native mechanical qualification of the unpublished
`codex/firearm-postrelease-hotfix` candidate. It supersedes no historical release
qualification or waiver. The PR records the final committed package identity.

## Root causes and implementation

Repair's shared blueprint component returned its full requirements paragraph
from parameterless `IAbilityAvailabilityProvider.GetReason()`. Native
`AbilityData.GetUnavailableReason()` first calls `IsAvailableFor(this)` and then
that parameterless method. Native action-bar action-economy warnings can precede
that route. The repair-only adapters now evaluate the concrete `AbilityData`
afresh and return one contextual reason; no selected-caster/failure cache or
global notification UI patch is used. Combat takes precedence and returns exactly
`Cannot repair firearms during combat.` The legacy alias uses the same component.
Eligibility, command-start binding, reusable kit, and delivery checks remain.

The old interruption latch had no durable representation of an accepted player
order. Its one-shot consent required the click handler on the stack and the same
frame, then was cleared when the handler returned. Harmony's generated handler
frames make declaring-type stack detection unsuitable. Native empty desktop
attacks can create an auto-use reload without calling the attack factory at all;
the eventual attack arrives after the consent was discarded. The old eligibility
check also reversed `CanAttack`'s actor/target receiver, and reload completion
mistook native `OnEnded(bool raiseEvent)` for an interruption flag.

The replacement attaches an exact actor, firearm, target and degradation epoch to
commands at construction. Only the actual native desktop/controller attack
submission accepts its proposed order after native `Run` owns the command (or its
native merged survivor). A per-actor submission number rejects superseded reload
callbacks. Automatic construction never grants consent. Successful reloads use
`ResultType.Success`; continuation can be the scheduled native callback or the
same accepted order's ordinary native AI attack. The authority has no frame expiry
and survives a real pause. Cancellation, supersession, changed item/target/context,
faulting input and subsequent degradation invalidate the relevant order. Stale
cleanup cannot revoke a newer order. Records are transient and weakly owned.

The real full-rest check also exposed an existing empty-slot exception in firearm
collection: native `ItemSlot.Item` throws when empty. Collection now uses
`MaybeItem`. This one-line compatibility fix allows the native completed-rest
boundary to restore the same naturally Wrecked firearm. Misfire probabilities,
penalties, ammunition/save IDs, explosion rules, Dead Shot and scatter mechanics
are unchanged.

## Production input and evidence boundary

Desktop input invokes actual `ClickUnitHandler.OnClick` with the selected native
actor and clicked root `UnitEntityView` object. Native `actor.CanAttack(target)`
and the ordinary attack versus auto-use branches run. Simulated prediction,
selection, interaction, child-view resolution and a fault after submission are
negative cases. Controller cursor input uses the shared click handler; native
`InGameInputLayer.OnInteract` uses its actual console selection and resolved view
for the separate out-of-combat direct-order path. Both loaded and empty controller
orders fire. Native call-site counts are checked at patch installation.

The guarded Steam App 640820 request runs in a save-free disposable scene. The
fixture owns actors, native controller dependencies, clocks and animation act
cues; actual input adapters, command/AI processing, reload delivery, action costs,
weapon rules, misfire transitions and burst resolution execute. Positive proof
uses no authorization Bridge/helper, direct suppression reset or preloaded Broken
weapon. Manual reload supplies the loaded Broken cases.

Paused RTWP owns/restores both native Pause mode stack and registration count;
`Game.IsPaused` is asserted during the click and after three later frames, with no
command pumping until unpause. Earlier attempts that merely requested pause are
not counted as paused proof. Native full sleep advances eight hours and invokes
the real completion hook; only the subsequent UI/autosave coroutine is deferred.

## Executed checks

| Layer | Result |
| --- | --- |
| Focused firearm orders / interruption / field repair | 28/28 PASS within full suite: 17 behavioral policy cases and 11 structural source contracts |
| Compiled input-wrapper return/exception/finally cases | 10/10 PASS; synthetic control-flow proof, separate from native proof |
| Complete domain suite | 1,618/1,618 PASS, zero failures |
| Repository and applicable asset validators | PASS |
| Clean exact-reference Release build | PASS |
| Strict standalone UMM package validation | PASS, 135 files |
| Guarded native extended scenario | 145/145 PASS, zero failures |

Native coverage:

- Exact combat rejection before and after action expenditure in desktop and
  controller-cursor RTWP/TB; no repair, resource, kit, command or cooldown mutation.
  All other requested short reasons, unknown context, legacy alias and alternating
  selected characters pass through native availability/action-bar warnings.
- Four core lanes (desktop/controller cursor × RTWP/TB): a real initial misfire
  consumes the loaded round, commits Broken and stops the old sequence; native AI
  retries without input cannot reload or shoot. A new empty-gun order reloads with
  normal Move action cost and fires a non-misfire while still Broken. RTWP remains
  valid across an actual pause. TB retains its normal Standard-action gate.
- The accepted Broken order later misfires through weapon rules, naturally becomes
  Wrecked and applies its burst once. Further native fire/reload is rejected.
- Manual reload and Quick Clear in RTWP/TB do not revive the old order. Later new
  input fires; manual-reload cases prove loaded Broken attacks remain Broken.
- Old attack and AI-reload command objects cannot borrow new authority. Wrong-target
  AI work is rejected. A completed old reload callback is actually queued, then a
  different-target order and repeated same-enemy paused order supersede it; draining
  that callback preserves the newest command, which shoots the exact new target.
- Another actor's ordinary weapon order, selection, preview and real interaction
  commands do not authorize this firearm. A native handler fault after accepted
  reload submission revokes exactly that invocation's order.
- Native out-of-combat action-bar repair preserves the exact item and reusable kit;
  repair alone leaves the cancelled sequence suppressed, and later input works.
  Native full sleep/completion restores a naturally Wrecked firearm once, including
  a body with empty equipment slots. Incomplete/time-skip/encounter rest is rejected.
- All fixtures restore original world, inventory, clock and controller state.
  Save mutation attempts: zero. The original installed mod was restored and no
  owner gameplay process was terminated.

## Recorded passing candidate

Run: `20260912T2042111791865Z-disposable-firearm-break-interruption` (run 25).
Loaded identity: `KingmakerGunslinger, Version=0.0.128.0`; UMM version `0.0.128`.
This run tested the uncommitted source on base
`db1fccf648b25baa53667dd00ac566e2a0d7f6b9`, source fingerprint
`5b782b0381990b193d235a6cc6e418050015cf25e72694ee4aed3ac8d4ca489e`.

| Artifact | SHA-256 |
| --- | --- |
| Qualified local-runtime ZIP | `ebeedd42f42c1498320546cdbed033aab7eb0af3f27c9b1c3bf86199ce85c9b7` |
| Loaded DLL | `a9bfebfeebbe73eba8376f70e37bab86b0cc435c4d063081c500df4f3818ff24` |
| Structured runtime result | `4cf166ac8b38ba622dec0efb99a4398fa7909c5e5bee8544281bf50603908a81` |
| Native input trace | `ecd267a3144b9f4405c49c77b7626f0cb43f84f181df76f492988c96c63ca96c` |

DLL MVID: `5c100927-a932-4d67-be03-ad6182add455`.
Raw logs/assemblies/packages remain local. After the documentation and validator
changes, all required gates are rerun before commit. The committed build is
requalified and its final commit/package/DLL hashes are recorded in the PR; the
above earlier artifact is not substituted for that final identity.

## NOT RUN and limits

Human visual/physical-device acceptance, save-backed load/persistence, the camp
UI/autosave tail and an exhaustive optional-mod configuration matrix are NOT RUN.
This guarded save-free fixture has no combat-log view; presentation-only annotation
errors are not evidence of visual acceptance. The generic runner reports game
version `UNKNOWN`; exact installed-reference provenance and loaded mod/DLL identity
are checked by the build/deploy harness. Dead Shot/scatter and broader misfire,
ammunition and identity compatibility retain their domain regressions; this native
scenario exercises ordinary production firearm discharges. No historical waiver
is applied to the mandatory hotfix input paths.

Earlier unsuccessful attempts remain local and are not counted as PASS. Run 2
was NOT RUN because Steam did not provide the required launch environment;
later fresh Steam launches succeeded. Subsequent failures narrowed fixture
controller/animation, native argument, pause-registration, faction-cache and
callback-timing contracts; final assertions require their actual native effects.

## Owner smoke

1. In combat, click Repair Firearm and verify exactly `Cannot repair firearms during combat.`
2. Misfire a normal firearm: the shot resolves and uses ammunition, the gun becomes
   Broken, and the existing sequence stops before another shot or reload.
3. Explicitly attack the same enemy again. With an empty gun and legal ammunition,
   one click must reload normally and then fire; repeat with a paused RTWP order.
4. Manually reload a Broken gun: it must stay idle until a new attack. That new
   loaded attack must fire while the gun remains Broken on a non-misfire roll.
5. Repeat the new-order/reload case in turn-based mode and check normal action cost.
6. Cause another applicable misfire: the gun naturally becomes Wrecked, its burst
   occurs once, and further firing/reloading is rejected.
7. Complete a full rest with a participating gunsmith and reusable kit; the same
   firearm returns to Normal. Outside combat, Broken-only field repair also works.


## PR #18 review revision — 2026-09-13

**CR-01 FIXED; CR-02 FIXED; CR-03 FIXED with the native limits below.**
This continues the reviewed `f38848fbc4b60354e8e35c9d0b5ea043bf36e0a1`
on the existing branch. Master and the published 0.0.127 tag were preserved;
0.0.128 remains unpublished. Earlier qualification records above remain
historical. Revised before/after DLL, MVID, source and trace identities are in
[the curated review evidence](FIREARM-POSTRELEASE-HOTFIX-REVIEW-EVIDENCE.json).

### CR-01 — actual native ownership and coexistence

The reviewed prefix cancelled an order and advanced its actor-wide submission
number before native `Run` decided whether a command was rejected, merged,
queued or coexisted. Native Focused Aim legally occupied the Swift slot alongside
a paused Standard attack and spent one grit; both native commands survived,
but the ledger rejected the attack and its reload callback.

Submission now advances only for an actually retained bound command, after the
native outcome. The order records its native owner and real command container.
Native slot removal, queue replacement and interruption reconcile ownership;
unrelated surviving slots do not supersede it. `UnitCommands.Temporary`
prediction cannot cancel the real container or a completed reload's callback.
A legitimate native AI continuation can arrive before the scheduled callback;
the stale callback then yields to its accepted successor without revoking it.

Twelve native lanes cover Normal, newly accepted loaded Broken, and newly
accepted empty Broken orders, pending/running, RTWP/TB. RTWP Focused Aim uses the
ordinary action bar and exactly one grit and the native Swift cost while the
attack/reload continues. The TB action bar requires an empty command container:
Focused Aim executes first on the actual turn, then the attack; an otherwise
ready Clipping Shot attempt while pending/running is rejected without cost or
mutation. This preserves native coexistence rules. Additional tests cover
completed/pending reloads, wrong-executor rejection, prediction, replacement,
cancellation, Quick Clear and stale callbacks with exact costs/resources.

### CR-02 — native retarget and merged survivors

The reviewed target equality check rejected a surviving native command after
`UnitAttack.UpdateTarget` selected another legal remembered enemy. Only an
observation around that exact native method may now update the current owned
order's target. Actor/item/epoch/target checks remain on foreign construction;
no new automatic-retarget policy is introduced.

Normal and legitimately reaccepted Broken pistol sequences kill A through native
projectile damage/life processing, select B natively, and fire the remaining
allowed shot with an ordinary free cartridge reload. Wrong-target AI stays
rejected. Misfires remain mandatory misses: the degradation/dead-target negative
uses a separate genuinely selected ally's ordinary sword attack to kill A after
the firearm breaks. Advancing native command/AI processing cannot restart it.

An additional running-reload regression observed actual
`UnitUseAbility.TryMergeInto=true`, but the reviewed adapter kept the old attack
intent. The correction owns that actual survivor and transfers the newly
accepted immutable pending reload intent onto it. It preserves elapsed progress
and costs and never rewrites an already scheduled old callback. Paused unstarted
replacement, running reload merge, stale native AI reloads, and same/different
enemy orders pass. A running attack merge after a real coexisting paper reload
preserves its already-spent Standard cost, attack index, last rule and elapsed
progress. Native costs, including the existing RTWP Free-slot cooldown behavior,
are observed rather than changed.

### CR-03 — action prediction and native terminal boundaries

`ActionsStates.Standard.CanUse` includes UI hover prediction. Actual native
pointer handler selection, hover notification, path calculation and prediction
reproduced premature cancellation of an accepted reload order. The callback now
submits one continuation to the native queue on the exact actor's turn; actual
native cooldowns determine execution. There is no retry loop or cross-turn feature.

| Legitimate powder/ball configuration | Native TB charge | Qualified continuation |
| --- | --- | --- |
| Pistol with Rapid Reload | Move +3 | Same-turn reload then Standard +6 attack, one click |
| Pistol without Rapid Reload | Standard +6 | Reload once; no remaining Standard means no shot |
| Musket without Rapid Reload | Standard +6 and Move +3 | Full-round reload once; no premature shot; native turn can auto-end |

Installed `TickCommandTurnBased` can reject an unstarted action using
`ForceFinishForTurnBased(Success)`, setting `IsActed` despite no discharge or
cost. The adapter observes that actual terminal owner event and revokes its
authority. The fixture records the call and zero attack cost; `IsActed` alone
is not evidence of firing. Ordinary native turn end also interrupts the queue
and has no firearm exception. Standard/FullRound reloads therefore do **not**
promise an automatic attack next turn. A new explicit attack uses the retained
round and the next turn's real action.

Ten cost/boundary cases and three real-hover cases cover Move, Standard,
FullRound, callback completion after native turn end, no premature shot/cost,
and cancellation/replacement while truly pending before native termination.
The save-free host supplies request-owned native Recast/Grid navigation; no
path result, action availability, ledger acceptance or target is assigned.

### Revised evidence and retained acceptance

The evidence JSON records the before run for each reviewed mechanism and
precisely identifies which other fixes were already present. These are native
mechanical reproductions, not claims that the reviewer independently ran them.
The before CR-02 run also failed a corpse-cleanup expectation; external restore
passed. Corrected fixtures restore exactly. Earlier failed instrumentation is
retained locally and is not counted as PASS.

Development run `20260913T0530333854811Z-a4e1dec760e54283aef75edb8c34b789`
passed **413/413 native assertions**, including all **145 original checks** and
268 added checks. The comparison preserves multiplicity and normalizes only
request-owned character GUIDs in assertion names. Complete domain suite:
**1,622/1,622 PASS**, including four added behavioral ledger tests. Compiled
input-wrapper control flow: **10/10 PASS**, explicitly synthetic. Repository and
asset validation, clean exact-reference Release and strict 135-file installable
package validation PASS. Exact dirty-source/DLL identity is in the evidence JSON;
the committed candidate receives its own subsequent qualification record.

Original native desktop/controller orders, paused RTWP, real Broken interruption,
empty reload-first and loaded/manual reattacks, natural Wrecked/burst-once,
contextual repair/legacy alias/character isolation, Quick Clear and completed
rest remain covered. No Bridge, test authorization helper, direct suppression
reset, manual ledger acceptance, direct Broken/Wrecked assignment or direct
retarget supplies positive proof. Ammo/item/save IDs, modifiers, probabilities,
penalties and production Dead Shot/scatter code are unchanged; their domain
coverage is retained.

NOT RUN: physical devices/human visual acceptance, save-backed persistence,
camp UI/autosave tail and exhaustive optional-mod configurations. Exact limits
are in the evidence JSON. No required review mechanical lane uses a historical
waiver. Generic runtime game version remains UNKNOWN; exact installed native
Assembly-CSharp SHA/MVID and loaded mod identity are recorded instead.

Every completed or recovered run restored actual pre-test installation/settings.
The original installed mod was 0.0.126, DLL
`99a8ab3e454c5cb4b8b74fc13bee09e6f00893c79d0cc065da27ce3b857681fc`.
Hung disposable navigation probes were recovered under explicit owner permission
after PID/request/Steam-parent verification. No owner gameplay session was
terminated; save mutation attempts in qualified fixtures were zero.

### Revised owner smoke

Misfire to Broken and wait: the old sequence must stop before another shot/reload.
Explicitly attack the same or another enemy, including manual-loaded and empty
auto-reload cases and paused RTWP/unpause. In RTWP, use legal Focused Aim alongside
a pending/running order and check its normal cost. Where native full attack
permits it, kill A and let the surviving command select B. In TB, observe the
cost table: Move reload can attack in the same turn; Standard/FullRound cannot
borrow another action or promise a next-turn automatic shot. Cause a later
applicable misfire to naturally reach Wrecked with one burst; further fire/reload
must fail. Complete a full rest to restore the same item. Combat Repair must say
exactly `Cannot repair firearms during combat.`


## Final committed review candidate

The clean build from `fc6c5cb220f7501fa20fc1709b085fd10b58b7a6` passed the complete guarded native scenario
again: **413/413 PASS, zero failures**, all 145 original checks retained. Complete
domain suite **1,622/1,622**, focused ledger behavioral tests **10/10**, and compiled
input-wrapper cases **10/10** PASS. Repository/assets, clean exact-reference
Release and strict 135-file package validation PASS. No production change follows
this qualified source commit; the later PR-tip commit records evidence only.
This artifact supersedes the development candidate for delivery.

| Identity | Value |
| --- | --- |
| Version | `0.0.128-firearm-postrelease-hotfix` |
| Branch | `codex/firearm-postrelease-hotfix` |
| Compiled source commit | `fc6c5cb220f7501fa20fc1709b085fd10b58b7a6` |
| Clean source fingerprint | `2721b45eedca565de6bad343ff2df38eced3cdb13fe6c13bd6d6d3a5c3b13f81` |
| Package SHA-256 | `c0c2c0c83db19f2b0bd71cd4393f5a0b09b1a8adadb29a9725d8541d257960e6` |
| DLL SHA-256 | `ce65373e73f40573fc9f715d30ef181da0d5b07db4be38d9be8261b6ce59b8b8` |
| DLL MVID | `2a1e57fe-63ef-49ce-b69c-3cbf7de95372` |
| Native run ID | `20260913T0544024994395Z-62c3a21ad3864b25b953d8da52c67568` |
| Runtime result SHA-256 | `90ffa6f65684af3a2e7b9770d03c3684b9776577ac66e1261d702bd0047abcb5` |
| Native trace SHA-256 | `6a0c9495fdee722708b99cd2c884e480066ac391b2357369b8b55069baafc78b` |

Installable package: `artifacts/packages/KingmakerGunslinger-0.0.128-firearm-postrelease-hotfix.zip`. The local-runtime copy
has the identical ZIP/DLL identity. The guarded runner verified the installed
mod version, source fingerprint, package hash and loaded DLL/MVID before the
mechanical conclusions above. Native and synthetic evidence remain distinct.

Final restoration independently compared every file against the actual pre-test
backup `20260913T0543590921304Z`: **138/138 identical**, including the original
0.0.126 DLL and FeatureModules settings. No Kingmaker process remained. The
[JSON evidence](FIREARM-POSTRELEASE-HOTFIX-REVIEW-EVIDENCE.json) includes additional
manifest/build/control-flow hashes and restoration identities. The earlier NOT
RUN boundaries remain explicit. PR #18 is updated for review; it is not merged,
and no public release or tag is created.

Exploratory configuration limit: an earlier legacy-revolver batch-reload probe
failed before reaching the running-merge boundary (no loaded round and no powder
spent). It is not counted as passing coverage. The final running/already-spent
merge proof uses the production pistol plus a real native paper reload. Legacy
batch-reload behavior was not changed or qualified by this focused hotfix; its
failed run identity is retained in the curated evidence.
