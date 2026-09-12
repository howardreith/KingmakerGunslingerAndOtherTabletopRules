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
