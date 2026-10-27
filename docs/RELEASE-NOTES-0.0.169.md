# 0.0.169 - Satyr and Lillend Azata

Expanded Summoning adds two adult support summons:

- **Satyr**, on Summon Nature's Ally IV-IX, supplies useful control without
  requiring pipes, skips immune/ineligible targets and uses bow, dagger or horns
  when control is unsuitable.
- **Lillend Azata**, on Summon Monster VI-IX, supplies bard buffs, healing,
  performances and safe control, with sword/tail attacks and grab/constrict as
  an offensive fallback.

Both retain legal quantities, normal summoning cost and duration, independent
resources and owner-bound cleanup. The new routes preserve existing summon
identities and routes. Affected action/AI paths were qualified in real-time with
pause and turn-based play, without reaction prompts or outcome-aware rewinds.

## Approved adaptations and limitations

Suggestion is available only with active **CowWithHat Custom Spells** (tested
with1.4.5). It uses the accepted avoid-combat/fascinated-until-harmed adaptation,
normal Will/spell-resistance checks and minutes-per-caster-level duration.
Removing the provider makes Satyr's saved grants unavailable and removes
Lillend's dependent entries/effects safely. No new dependency is required to use
the other summon abilities.

Lillend's noncombat performance dispatcher and bardic Suggestion memory exception
are limited to this creature and its own fascinated targets. Ordinary offensive
reactions remain. They are not a general performance or AI expansion.

- Lullaby uses a standard action and its immediate-release, seven-round tail;
  sustained concentration is not implemented.
- Hold Person retains native automatic repeat saves without charging a
  full-round action. Native hearing limitations remain: deafness is not queried.
- Countersong and Darkness are omitted. Light, Dancing Lights, Ghost Sound,
  Hallucinatory Terrain and Detect Magic are also omitted where applicable.
- Flight uses the accepted ordinary-map/Airborne adaptation, without general
  flight or navigation changes. Native passive-sense limitations remain.
- Models are technically usable and recognizable; cosmetic polish debt remains.
  Technical acceptance does not claim the owner's personal aesthetic approval.

These are explicit owner-approved dispositions, not substituted rules.

## Qualification and compatibility

Qualified development source `efd237d1cf12338d8d5eea50403af203785c3dc0` passed
the full2,756-test source/build/package gate. Its final fresh bardic native case
passed20 assertions. Source-bound reviewed reuse covers public casting routes,
quantities/costs, control/support, both modes, technical presentation, cleanup,
fresh-process persistence, module/provider transitions and affected integrations.
The technical presentation run passed46 assertions with21 agent-inspected images.
Independent review found no remaining concrete blocker in the agreed Sprint32 scope.

Release validation pins all qualified gameplay code, compiled observers, project,
assets/source art and blueprint ledger. Release0.0.169 changes only version
attributes and generated build stamps in the DLL; its package/DLL/MVID differ
from the development artifacts. Earlier native execution retains its original
artifact identity and is **reviewed reuse**, not fresh execution of the published
DLL. The normal publisher validates the complete release and compares two
deterministic builds. The published checksums and manifest identify those bytes.

The actual human installation and protected saves were restored after native
testing; `KMG_AUTOMATION_BASELINE` remained immutable. DATA's active/unaccepted
visual work was not included. Accepted v0.0.168 dependencies are included.

Prior v0.0.167 gaps remain **NOT_RUN**: the grouped all30 public-route/body/icon
check, standalone final integration and highest-risk combined final integration.
The separately disclosed v0.0.168 cold/public-route/final integration gaps also
remain NOT_RUN. Sprint32's focused evidence does not close those older campaigns
or claim whole-roster/all-profile qualification.

See the [active Sprint32 record](../planning/SPRINT32-STATUS.md) for the finite
acceptance table, exact evidence identities, additional observation limits,
original failures and current publication status. No Sprint33 content is included.
