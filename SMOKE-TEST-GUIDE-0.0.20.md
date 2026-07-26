# Kingmaker Gunslinger 0.0.20 smoke test

## Safety and scope

Use only a disposable campaign and disposable saves. This build adds inventory ammunition and an atomic manual transaction. It does **not** yet reload the Test Musket or change whether it can fire.

The two ammunition items temporarily use Diamond Dust artwork. Their inventory names should be **Black Powder Charge** and **Lead Ball**.

## Install

1. Exit Pathfinder: Kingmaker completely.
2. Install the standalone `KingmakerGunslinger-0.0.20-ammunition-smoke-test.zip` through Unity Mod Manager over version 0.0.19.
3. Start Kingmaker and confirm the mod shows version 0.0.20 with a green status indicator.
4. Load the same disposable campaign used for the Sprint 19 test, or another disposable campaign.

## A. Bootstrap regression

Open the mod options panel. The heading should identify Sprint 20 and the blueprint state should say initialized.

The log should contain one successful blueprint initialization reporting ten custom blueprints. Any `initialize.failed`, GUID collision, localization exception, or native-source mutation message is a test failure.

## B. Ammunition creation and presentation

1. Click **Remove all basic ammunition from shared inventory**.
2. Click **Print basic-ammunition counts**. Expected: zero powder and zero lead balls.
3. Click **Add 20 Black Powder Charges and 20 Lead Balls**.
4. Open the shared inventory.

Expected:

- one stack named **Black Powder Charge** with quantity 20;
- one stack named **Lead Ball** with quantity 20;
- custom descriptions mentioning an early firearm;
- placeholder Diamond Dust artwork is acceptable;
- the existing Test Muskets remain present and usable exactly as before.

Click **Print basic-ammunition counts**. Expected diagnostic counts are 20 and 20.

## C. Successful atomic consumption

1. Click **Consume one powder + ball pair atomically**.
2. Click **Print basic-ammunition counts**.

Expected:

```text
before: blackPowder=20; leadBalls=20
result: Consumed
after:  blackPowder=19; leadBalls=19
```

The inventory UI should also show 19 and 19.

## D. Missing-component rejection

1. Click **Remove all basic ammunition from shared inventory**.
2. Click **Add one Black Powder Charge**.
3. Print counts: expected 1 powder and 0 lead balls.
4. Click **Consume one powder + ball pair atomically**.
5. Print counts again.

Expected: the result reports insufficient components, and the counts remain exactly 1 and 0.

Then perform the inverse check:

1. Remove all basic ammunition.
2. Add one Lead Ball.
3. Attempt the transaction.

Expected: counts remain exactly 0 and 1.

## E. Save, process restart, and reload

1. Remove all basic ammunition.
2. Add 20 of each.
3. Consume one pair, leaving 19 and 19.
4. Save under a new disposable name.
5. Exit Kingmaker completely to the desktop.
6. Restart Kingmaker and load that save.
7. Print basic-ammunition counts.

Expected: both stacks remain at 19.

## F. Firearm-state carrier regression

After reloading, click **Print visible firearm states**. The Sprint 19 A-D set should still reconstruct as:

- one Loaded / Normal;
- one Empty / Broken;
- one Loaded / Broken;
- one Empty / Normal without a token.

Process-local `kmg-item-*` diagnostic numbers and reference hashes may differ after restart. The state multiset must not change.

## G. Native-item negative control

Inspect any ordinary Diamond Dust already present or add it through an unrelated test tool if convenient. It should retain its normal name, value, description, and behavior. The mod must not rename or convert native Diamond Dust.

## Useful evidence if something fails

Capture:

- the **Last result** line from the mod panel;
- the ammunition stacks and quantities in inventory;
- the `[KMG]` lines in the UMM log;
- `output_log.txt` only for an exception or crash;
- the first step whose observed behavior differed from this guide.
