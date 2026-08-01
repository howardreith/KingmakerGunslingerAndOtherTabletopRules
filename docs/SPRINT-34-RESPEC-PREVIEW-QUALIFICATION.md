# Sprint 34 respec preview qualification

This save-free checkpoint proves native Gunslinger respec preview using the
detached replacement-unit architecture used by Kingmaker's exact
`Player.RespecCompanion` contract.

- Exact source commit: `3d4ba8f`.
- Original disposable source after seeding: Fighter 1, Gunslinger 0.
- Fresh detached replacement before selection: Fighter 0, Gunslinger 0.
- Native `Respec` preview after selection: Fighter 0, Gunslinger 1.
- Original source after preview: Fighter 1, Gunslinger 0.
- Exact queued actions: 2.
- Controllers canceled and both detached entities disposed; party and global
  unit reference snapshots remained unchanged.
- No `PrepareRespec`, `Commit`, global unit creation, UI event, save selection,
  save load, or save write was invoked.

Exact installed-contract inspection proved that Kingmaker creates a fresh unit
from the original blueprint, copies experience and selected preserved state,
then initiates respec on that replacement. Earlier single-unit attempts were
retained as negative evidence and were not relabeled as success.

Source gates passed: 8 focused checks, repository validation, 691/691 domain
tests, exact private-reference Release compile, output validation, and strict
package validation.

Runtime evidence:

- Mod-load: `20260801T1836154433116Z-mod-load-smoke`.
- PASS: `20260801T1837314150470Z-disposable-gunslinger-respec-preview`.
- PASS: `20260801T1838472989503Z-disposable-gunslinger-respec-preview`.
- Package SHA-256:
  `fffd41f772c7b8b3668c7b8c4d2e8364a16a19cb118dccba112a67d826721e05`.
- DLL SHA-256:
  `5616dfd5da3eb32431c28501fa53289d6866364e818fa37a9568f235a9e6f36e`.

This checkpoint proves reversible respec preview and class selection. It does
not invoke Kingmaker's broad replacement callback or claim persisted respec
commit qualification.
