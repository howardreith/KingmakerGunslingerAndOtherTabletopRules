# Feature Modules, Acadamae Graduate, and Cord Qualification

Status: FINAL 0.0.75 GATES IN PROGRESS

Baseline: `7a99ce5ac6d6976212310f997bd39ddfe4a57935`, 0.0.74, 954 deterministic tests. Current deterministic total: 967.

## Proven runtime behavior

- Four standalone fresh-process settings combinations PASS: ON/ON, ON/OFF, OFF/ON, OFF/OFF. Each reports exactly 250 registered identities and the expected class, feat, firearm-menu, capital/BTSL/fixed-loot, Acadamae, and Cord publication state. The settings transaction restored the original SHA-256 `8aa8233b19e69af001d28dc9db51748baf3abb9ffff37ce96754c4addfac7470`.
- Cord standalone PASS 7/7: `20260809T1710388527173Z-7ef88cd7ee634d77a13d1995cc2875fe`. Constitution 10 to 12; fatigue and exhaustion substitutions; inventory-only and unequipped controls; 1 HP floor; cleanup.
- Capital Cord vendor observer PASS with exactly one fixed count-one row on `SmithVendorTable`, exact owner graph, 15,000 gp item, preservation, idempotence, and rollback.
- Consecutive deterministic integrated Acadamae/Cord PASS 7/7: `20260809T1800412559535Z-5388e10cbfb04d1980bcfd98c5cc9115` and `20260809T1802420337279Z-aca388acadfa448a80f2e44ce76771b1`. Both use native Wizard level/spellbook/memorized slot, `SummonMonsterISingle` (`8fd74eddd9b6c224693d9ab241f25e84`), Full-Round to Standard parity, native DC 16 Fortitude rules, cancellation, exact Cord match, one d6 result, no inert retained fatigue buff, later ordinary fatigue after unequip, and cleanup.

## Exact optional profiles

All listed transactions staged exact local bytes and verified restoration:

| Profile | Transaction | Targeted result |
|---|---|---|
| Call of the Wild 1.14.4c-2.1 | `compat-20260809T180508Z-605e9298769e` | ON/ON publication and Acadamae/Cord PASS |
| Arms & Armor 1.0.10 | `compat-20260809T180912Z-5659b0e869da` | publication, mechanics, visuals PASS |
| Toggle Custom Soundpacks 1.0.1 | `compat-20260809T181332Z-976be191b230` | publication, mechanics, Wwise PASS |
| Qualified combined | `compat-20260809T181800Z-27b037d8f332` | publication, mechanics, visuals, Wwise PASS |
| High-risk combined | `compat-20260809T182354Z-275df7b55c5d` | readiness, publication, mechanics PASS |

Exact Call of the Wild DLL SHA-256 is `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`. Managed SoundBank SHA-256 before/after is `0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18`. Historical Call of the Wild final-selector and high-risk timeout evidence remains recorded; the targeted feature-module results do not claim every older comprehensive scenario was rerun.

## Final gates pending

The final 0.0.75 package/DLL hashes, consecutive primary and highest-risk candidate runs, permitted ON/ON working-save smoke, final profile pins, clean tree, and local/remote equality will be appended after execution.

Next concrete action: build the exact 0.0.75 candidate and execute its final runtime gates.
