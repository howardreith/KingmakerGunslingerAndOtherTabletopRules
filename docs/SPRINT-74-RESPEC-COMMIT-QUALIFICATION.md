# Sprint 74 detached respec commit qualification

Source `163320d` added a save-free `Respec`-mode commit on a detached replacement
candidate while retaining a separate Fighter-level-one source. It deliberately
does not invoke `Player.RespecCompanion`, `PrepareRespec`, save APIs, or UI
events.

Run `20260802T1136179100890Z` proved the core replacement result and facts but
failed external isolation because first-level native commit grants starting
inventory. Repair `2eb0090` captures and rolls back exact added references and
quantities, zeros/restores starting gold, and requires shared money stability.
All focused checks, repository validation, 831/831 tests, clean exact-reference
Release build, and strict packaging passed before the corrected run.

Exact mod load `20260802T1140330341524Z` and corrected run
`20260802T1141509870468Z` passed. The source remained Fighter 1/Gunslinger 0;
the replacement reached Fighter 0/Gunslinger 1; success callback,
proficiencies, grit, inventory/gold rollback, and all external snapshots passed.

Exact package/DLL SHA-256 are
`412c258fc4af0bb86041a3b525a074f6abdedf0842b587cf8f4acde3340fec59` /
`e3c7f7eccc23b82c8e23c54e41a481152fcd7aa7f1327d75451345216845f281`.

This is strong single-run evidence because the first of two allowed attempts
identified the rollback defect. It does not qualify the broad global player
replacement callback.
