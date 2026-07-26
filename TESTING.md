# Testing

For the current Sprint 29 build, follow `SMOKE-TEST-GUIDE-0.0.29.md` or the byte-identical `SMOKE-TEST-GUIDE.md`.

The dependency-free C# 7.3 harness is compiled against the exact .NET Framework 4.7 reference surface and must pass 569 tests in three byte-identical runs. The main DLL must compile twice against the exact private Kingmaker 2.1.7b references with warnings as errors and byte-identical DLL/PDB output.

Runtime testing must use a disposable campaign and exercise the actual full-round Overhaul Test Musket ability. Capture the exact Wrecked firearm's repository identity, in-process runtime reference hash, revision, and item state before and after delivery; the Firearm Repair Kit count; the independent second Test Musket; and all overhaul/token fault counters. Missing-kit, non-Wrecked, interruption, successful delivery, repeat rejection, and save/restart behavior are blocking controls.

The development-only immediate overhaul command diagnoses the transaction but does not prove action timing or interruption safety and cannot replace the player-facing ability test.
