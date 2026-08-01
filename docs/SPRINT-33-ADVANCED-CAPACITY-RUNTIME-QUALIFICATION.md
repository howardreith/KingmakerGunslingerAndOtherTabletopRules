# Sprint 33 advanced-capacity runtime qualification

## Qualified revision

- Commit: `f851ae2`
- Version: `0.0.33`
- Package SHA-256:
  `01774ec30fbe71580b3bc3ff7e63337aad0784e7881b868553ed88539d4a284a`
- DLL SHA-256:
  `3e610b43025b2771bd9003396ca469cd10e9028dfb7d5fb180f7f821369622d3`
- Complete domain suite: 685/685 PASS

## Guarded runtime evidence

Exact-commit `mod-load-smoke` passed through Steam App ID 640820:

- Run ID: `20260801T1447365484819Z-10da17eb731c4ba08e76fce5ac216fef`
- Evidence directory:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T1447365313656Z-mod-load-smoke`

Two independent fresh-process `advanced-capacity` runs passed against only
`KMG_AUTOMATION_WORKING`:

- `20260801T1448590698485Z-c8d4099979754579b6bc68fde049bbc5`
- `20260801T1450305002704Z-681c230576604963897eda2927f1c866`

Both runs correlated the same stable fingerprint, loaded mod version `0.0.33`,
and observed no save-writing API. Each reported exact PASS observations:

- atomic reload: `first=6;second=6;inventory=0/0`;
- discharge isolation: `first=4;second=6;records=2`;
- advanced misfire: `NormalToBroken` then
  `AdvancedBrokenRemainsBroken`, four rounds retained, no burst damage.

## Scope and uncertainty

The guarded scenario executes the compiled transaction, repository, state,
misfire, and explosion services inside the real Kingmaker process after exact
working-save correlation. Its firearm objects, inventory, and vault are
request-local fixtures. Therefore this evidence does not claim that native
inventory acquisition, equipped-item interaction, or save-vault mutation was
performed. Save-owned reconstruction and two-item isolation remain supported
by the 685-case domain suite; no save was written during runtime qualification.
