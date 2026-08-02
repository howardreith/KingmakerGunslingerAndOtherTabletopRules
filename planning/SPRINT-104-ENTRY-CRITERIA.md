# Sprint 104 entry criteria - Targeting Arms

## Authority and adaptation

The level-7 PnP deed requires a full-round firearm attack, one grit, no
damage, sneak-attack-immunity exclusion, and a chosen carried item drop.
Kingmaker 2.1.7b exposes no safe general carried-item choice/drop transaction.
Its exact native Disarm authority instead disables weapon use through
`DisarmMainHandBuff` and `DisarmOffHandBuff`.

The narrow adaptation is therefore one no-damage firearm hit that applies
only `DisarmMainHandBuff` for exactly six seconds. Main hand deterministically
represents the chosen item and includes native two-handed weapons. Off hand is
left untouched so the adaptation cannot disable more than one item.

## Qualification contract

- level 7 grants one full-round weapon-range ability;
- an eligible use spends one grit and one loaded chamber;
- the native firearm attack hits but dispatches no damage rule;
- an eligible hit applies the exact installed main-hand Disarm buff for one
  nonpermanent six-second round;
- miss and sneak-attack-immunity policy branches apply no rider;
- cleanup removes the buff, item state, and disposable units without changing
  party or global-unit collections.
