# Optional-mod compatibility implementation report

> Historical-scope note (2026-08-24): this report describes the 0.0.71
> optional-mod framework checkpoint. Craft Magic Items is no longer merely a
> source-only/static profile; the separate 0.0.98 implementation and exact
> guarded qualification are documented in
> `CRAFT-MAGIC-ITEMS-COMPATIBILITY-REPORT.md`.

## Status

Framework implementation is in progress on
`codex/postbase-archetypes-compatibility`. The unchanged 0.0.71 baseline passes
911 deterministic tests and all exact-reference Release/package gates. This is
not yet compatibility qualification.

## Architecture

Implemented foundations include a canonical read-only reference inventory, a
logical catalog/schema, a deterministic standard-library GUID/Harmony/bootstrap
scanner, and committed exact-profile definitions/schema with truthful
static-only and unavailable dispositions.

The profile resolver binds only committed logical keys to canonical local
roots, reports complete exact identities and manifests, and refuses ambiguous
or missing runtime roots. Public transaction entry accepts a committed profile
ID rather than caller-provided source paths.

Transactional staging records the exact original Mods tree and managed
`KMG_Firearms.bnk` state, atomically renames the original Mods directory,
creates a sentinel-owned isolated directory, copies the validated Gunslinger
package and allowlisted references, and restores from `finally`/explicit
recovery. Restoration quarantines staged data, returns the original directory,
verifies exact metadata and hashes, restores the bounded SoundBank side effect,
and deletes only the sentinel-owned quarantine after verification.

The guarded optional-mod observer is implemented through the established
request parser and runner. It accepts only committed runtime-capable profile
IDs, inventories exact ordered UMM entries and assemblies, records Harmony12
owners/order on all Gunslinger-patched methods, and verifies the base class,
20-level progression, Mysterious Stranger rows/Charisma binding, production
firearm pairs, Wwise state, and save-free boundary.

The standalone observer passed twice. Arms & Armor 1.0.10 passed isolated load,
exact observer, visual-rig, and production-switching scenarios. Toggle Custom
Soundpacks 1.0.1 passed isolated load, exact observer, and Wwise discharge.
Call of the Wild 1.14.4c-2.1 did not reach guarded readiness within 300 seconds;
it is recorded as `CONFLICT-OBSERVED`, not compatibility-qualified. Every
transaction restored the exact original Mods tree and bounded SoundBank state.

Pending: combined-profile execution, remaining standalone/profile scenarios,
working-save qualification where eligible, and final exact-profile reports.

The Arms & Armor plus Toggle Custom Soundpacks extension passes load, exact
observer, presentation, visual rigs, switching, Targeting Arms, Wwise, Scatter,
and reload. Comprehensive acceptance is not qualified: standalone and combined
runs reproduce a detached Gunslinger's Dodge command ending `Interrupt` without
the timed buff. This is `GUNSLINGER-REPAIR-REQUIRED`, not attributed to either
optional mod. Working-save smoke was not run while standalone comprehensive
qualification remained failed.

The profile invocation wrapper accepts only the six runtime-capable committed
profiles and the established guarded scenario allowlist. It stages through the
transaction core, launches each scenario through the existing Steam harness in
a fresh process, waits boundedly for automatic exit, and restores from
`finally`. A PASS result is not returned unless the transaction record confirms
exact restoration.

## Safety and dependency result

The design admits no third-party compile or runtime dependency and no
third-party payload in the Gunslinger package. Runtime staging will operate on
copied profile roots and restore the exact original Mods-directory state.

## Verification

Inventory and scanner behavior fixtures pass. Repository validation, the
unchanged 911-test suite, exact-reference Release, SoundBank validation, and
strict 0.0.72 packaging passed at the first checkpoint. The current rebuilt
package SHA-256 is
`5FD8DC95EAA96B4DCAF225C41AEBE700816D3B41FD4D12D70A5E69B6DE2CA0D1`.
Current DLL/AssetBundle/SoundBank SHA-256 are
`B22C9ED4FE76E61C0152CFFF376CC19EE2A9380DB32BD10E8AA178168DB1A80A`,
`F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`, and
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

Human evidence now confirms that exact Call of the Wild 1.14.4c-2.1 reaches
character creation and publishes its own added classes while Gunslinger is
absent from the selector. This is `CONFLICT-CONFIRMED`, not merely slow startup.
Root/selector lifecycle forensics are pending; no production repair has yet
been selected.

The diagnostic candidate snapshots all required lifecycle boundaries and the
exact Kingmaker 2.1.7b chargen collection getter without mutating the catalog.
Installed IL proves CotW writes through `Main.library.Root`, chargen reads
through `Game.Instance.BlueprintRoot`, and pre-repair Gunslinger publishes
through `BlueprintRoot.Instance`. The next materially different CotW run will
determine whether those objects or arrays actually diverge at runtime.

The run proved the roots do not diverge. Bootstrap instead failed and rolled
back during Evasive donor validation. The bounded repair validates that each
project clone preserves its current donor's ordered component types rather than
requiring vanilla-only counts. It neither references nor detects CotW and does
not alter Evasive's level, Grit, grant, removal, or True Grit behavior.

The repaired candidate passed fresh CotW load evidence
`20260807T2146571019519Z-mod-load-smoke` and strengthened observer run
`20260807T2149121927539Z-a37fb450a1164ec9b664812be3073704`. The observer
proved exact UMM identities/order, all 46 compiled CotW helper classes retained,
Gunslinger singular in the 48-entry final root catalog and chargen selector
input, exact progression and Mysterious Stranger structure, firearm identities,
Wwise readiness, Harmony inventory, and no save-writing API. Both transactions
restored exactly. Current repair package/DLL hashes are
`37B4C25A45BD69EC19B20248BA14539AF38FAF7AF3C5C649E6FD3AC01AC0DBE5` /
`B8799B41D9C74F0D1EB1484F6BFE9353285CF0DFD34507977E37CA72E3A69E8B`.
Human chargen confirmation remains pending, so the prior human conflict is not
yet promoted to final runtime qualification.
