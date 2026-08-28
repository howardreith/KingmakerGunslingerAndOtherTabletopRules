# Kingmaker Gunslinger 0.0.106

Release archive:
`KingmakerGunslinger-0.0.106-fatigue-authority-repair.zip`.

## Fatigue authority repair

- Ordinary Kingmaker applications of canonical Fatigued now retain the exact
  native result. A refresh or reapplication cannot become Exhausted merely
  because KMG observed a pre-existing Fatigued fact.
- Native canonical Exhausted remains Exhausted, and an incoming Fatigued fact
  cannot downgrade an already Exhausted unit.
- Acadamae Graduate retains its accepted failed-save consequence: its exact
  adapter applies permanent/rest-removable Fatigued, or Exhausted when that
  same request begins on an already Fatigued caster.
- The Acadamae escalation permission is thread-local, one-shot, exact-unit and
  exact-blueprint correlated, nested-call safe, and disposed even on failure.
- Cord of Stubborn Resolve remains global and post-success. Ordinary repeated
  Fatigued reaches Cord as Fatigue; native Exhausted and explicit Acadamae
  escalation reach it as Exhaustion. Its direct damage, exactly-once routing,
  exhaustion substitution, recursion guard, and 1-HP floor are unchanged.

Installed Kingmaker 2.1.7b IL shows that native weariness reapplies canonical
Fatigued hourly while the weariness stack remains 1, using the canonical
blueprint's `Prolong` stacking. Only a later native weariness stack selects
canonical Exhausted. The prior global KMG escalation therefore converted a
routine travel refresh into premature Exhausted.

This release does not patch travel classes and does not change Overhaul
Firearm, Expanded Summoning, summon rosters/durations, Acadamae eligibility or
action economy, firearm systems, blueprint identities, Cord acquisition, or
Cord presentation.

Optional Craft Magic Items compatibility remains reflection-only; the package
does not link or include `CraftMagicItems.dll`.

This release retains the 1,288-test 0.0.103 baseline, the 1,307-test 0.0.104
summon repair, and the 1,315-test 0.0.105 presentation baseline. The complete
dependency-free deterministic suite contains 1,323 tests.
Mechanical and guarded runtime evidence is recorded in
`docs/FATIGUE-AUTHORITY-REPAIR-QUALIFICATION.md`. World-map travel remains a
supervised acceptance boundary because the guarded harness does not use input
automation or infer UI state.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
