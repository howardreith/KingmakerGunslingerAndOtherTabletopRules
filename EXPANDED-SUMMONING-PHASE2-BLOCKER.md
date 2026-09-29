# Phase 2 Sprint 11 owner-design blocker, 2026-09-29

Historical record: the owner resolved this blocker later on 2026-09-29 with
an exact automatic, single-response AoO-first policy. The same correction
order reopens Sprint 10 and authorizes continuation through Sprint 21.
The following text records the superseded decision point and must not be
used as the current implementation instruction.

**Blocked requirement:** Aurochs, Bison and Woolly Rhinoceros trample victim
choice. Paizo requires each contacted, smaller target to choose an attack of
opportunity at -4 or a Reflex save for half damage. Sprint 11 and the mission
require the signature trample, including correct saves, damage and cadence.
Rhinoceros itself is a powerful-charge creature and does not have this
trample-choice issue. All four Sprint 11 choices remain hidden. Sprints 12-21
have not started; the Phase 2A draft PR is not ready for owner review.

**Evidence and alternatives:** The [Paizo universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html)
state the target choice. The installed `AbilityCustomOverrun` delivery has a
native forced path but either makes an Overrun CMB check or bypasses it with
`AutoSuccess`; it synchronously invokes `ActionList.Run()` on contact. It has
no AoO/Reflex decision. Read-only installed-assembly inspection also found
`ContextActionProvokeAttackOfOpportunity`, which automatically calls the
combat engagement controller, and `ContextActionSpendAttackOfOpportunity`,
which only decrements an AoO count. Neither offers a defender choice or the
printed -4 attack. The hidden project contact action has already proved the
printed DC 17/20/23 Reflex and damage path, successful half damage, exact
smaller-hostile filtering and same-round replay suppression in guarded RTWP
tests. Its current Reflex-only branch would remove the victim's AoO choice;
it is deliberately unpublished. A preselected toggle or AI preference would
also substitute a new, unapproved decision policy for the target's choice.
An interactive prompt at every path contact would require a new asynchronous
combat/UI control seam outside the native synchronous delivery, with no
bounded project pattern found. These are design/balance adaptations, not a
routine donor or mesh decision.

**Smallest owner decision:** Authorize a specific, disclosed RTWP/turn-based
adaptation for trample victims. One conservative candidate is: when the
defender can make an AoO, automatically grant one attack against the trampler
at -4 and also roll the printed Reflex save for half; otherwise roll Reflex.
This favors defenders relative to choosing only one response, but avoids
silently strengthening the summoned trampler. The owner may instead specify
another exact policy, or authorize a scope split that keeps Aurochs, Bison
and Woolly Rhinoceros hidden while other authorized creatures advance.
Reflex-only publication is not approved by this checkpoint.

**Environment:** Latest pushed visual checkpoint is
`d6e623967840b4bbea03bdcf23c89a5fee1a8995` on
`codex/expanded-summoning-phase2a-sprints9-13`, draft PR #25. The last
guarded run `20260929T1022039564996Z` passed 32/32 assertions after 1,952
domain tests, clean Release and strict package validation; its wrapper
restored the original 136-file live mod tree exactly at
`20260929T1029221524485Z`, SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
No protected save was modified in this run. Ungulate visual quality remains
open separately: clear-floor oblique frames show distinct species but thin,
splayed hoof ends and awkward apparent Rhino leg poses. This finding can be
resolved by reversible art work once the rule decision is supplied.

**Ready-to-paste continuation prompt:**

> Resume Expanded Summoning Phase 2 from the latest pushed Phase 2A branch
> and read `artifacts/phase2/mission.md` plus
> `EXPANDED-SUMMONING-PHASE2-BLOCKER.md`. For Sprint 11 trample, I authorize
> [insert the exact target AoO-versus-Reflex adaptation or explicit split].
> Keep unqualified choices hidden, finish Sprint 11 mechanics/art/runtime
> gates, then continue authorized Sprints 12-21 through the three stacked
> draft PRs. Do not merge, release, permanently deploy or begin Sprint 22.
