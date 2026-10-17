# Controller load installer repair — alpha 3

Test prerelease `v0.0.157-controller-load-alpha.3` replaces KMG alpha 2. This is
the **Kingmaker Gunslinger / Tabletop Added Rules** update. Keep Buff Planner
0.5.4-controller.1 and CotW unchanged; no companion update or loader change is
part of this repair. Stable v0.0.157 remains Latest; no default-branch merge.

## Install and identify

Download **KingmakerGunslinger-0.0.157-controller-load-alpha.3.zip** from Assets,
not the Source code archives. Install via the normal UMM ZIP workflow on your
test computer, replacing the previous KMG alpha, not adding a second mod folder.
Back up your existing installation and saves first.

**UMM still displays 0.0.157.** This publishes the unchanged tested candidate,
product `39c60fb5e9167f1b2fe35a07b545fa2a2ebaac2e`, without a metadata rebuild.
Use the ZIP name and logged DLL identity to distinguish alpha 3 from alpha 2.

## Why alpha 2 still crashed

The supplied output_log2 shows a bug in KMG's installer diagnostics: the
installed Harmony12 facade threw when asked about an unpatched method. That
stopped installation of the CotW slot-access correction, and the original
conversion crash recurred during controller action-bar creation.

Alpha 3 handles absent metadata correctly and isolates optional diagnostics
from patch installation. A new regression reproduces alpha 2's failure and
executes the real corrected installer, rather than just its transpiler. It
retains CotW's conversion/metamagic/variant logic and native patch owners/order.

Five early first-update consumers now wait for committed blueprint registration:
Shield Other, Teleportation, class-catalog diagnostics, Eastern Weapons and
Favored Class. Waiting does not consume their callback or host retry allowance.
Actual validation and publication failures remain authoritative.

Brown-Fur's lifecycle repair is preserved. The new log independently confirmed
all 25 identities registered successfully; it was not the failing repair.

## Evidence and remaining limits

- Exact packaged DLL: **64/64** headless real-installer/adapter/callback
  assertions and **33/33** focused domain tests; exact-reference build and
  strict package PASS. No full release-suite or native campaign rerun.
- In-game controller save loading, action-bar party/page refresh, deferred
  same-page refresh, physical input and KBP no-cast entry/exit remain **NOT_RUN**.
  A source-tested package is not proof the affected save now loads.
- Four handled ZFavoredClass custom/PrestigiousSpellcaster missing-key errors
  also occur in the older log. They are a separate unresolved data issue; no
  external files, mod settings or saves were edited to hide them.
- Adapter contract remains the observed CotW MVID
  `8caab254-aacf-4811-8093-44b9184e6e53` and Kingmaker 2.1.7b game MVID
  `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

First test loading **without saving over the affected save**, then switching
party members and action-bar pages. Keep metamagic conversion enabled/configured
as before. Look for `cotw-actionbar/installed` and the new assembly hash/MVID.
If that passes, test Buff Planner entry/navigation/Back **without casting first**.

If it fails, retain the fresh output_log and report **Continue or Load Game**
and **controller or mouse/keyboard mode**. Do not clear action bars, respec,
disable CotW or edit saves as an acceptance workaround.

ZIP SHA-256: `6d4ed736a18cfdcac668eb5fad24e6b19001af635fd7e1fc53006834bd507aed`.
DLL SHA-256: `d9fea590a604b81b5a14046ac88454a7eb8d4510e94e0b4ae5de082cbc2d606b`.
DLL MVID: `c1a4d16b-efae-4ea8-b3ef-5966c3db9c6b`.
