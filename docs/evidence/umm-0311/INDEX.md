# UMM 0.31.1 baseline evidence

Machine-generated outputs exported 2026-11-24 19:54 UTC for candidate commit `49be3fd0e28c551a4a5fafb5e98d87daffb125d5`. Each row names what produced that output (its
source commit, or the reused input it describes); outputs produced at another commit are identified there.
Only local path prefixes were replaced: `<repo>` (this checkout or its temporary clone root), `<temp>`, `<game>`
(the Kingmaker installation, read-only), `<buildtools>`, `<reference-assemblies>`, `<dotnet>`, `<controller>` (the agent-review controller), `<dev>` (the owner's development folder), `<steam>`, `<user-profile>`, `<fleet>` (the owner's fleet mission folder,
outside this repository). Line endings were normalized to LF and a UTF-8 byte-order mark was dropped; nothing else was
edited. Raw SHA-256 is of the original output bytes. Exported SHA-256 is of the committed Git blob of each file in this
folder (LF bytes; a Windows checkout with `core.autocrlf` may show CRLF in the working tree, so hash the blob, not the
checked-out file). To verify one, from the repository root (Git Bash or any POSIX shell with sha256sum; the second
command, with Python, works in Windows PowerShell and cmd.exe):

```
git cat-file blob HEAD:docs/evidence/umm-0311/<file> | sha256sum
python -c "import hashlib,subprocess,sys; print(hashlib.sha256(subprocess.check_output(['git','cat-file','blob','HEAD:docs/evidence/umm-0311/'+sys.argv[1]])).hexdigest())" <file>
```

| File | Produced by | Raw SHA-256 | Exported SHA-256 (Git blob) |
| --- | --- | --- | --- |
| `candidate-admission.json` | 49be3fd0e2 (Test-CandidateAdmission.ps1 in a temporary detached clone of that commit) | `49fedb2d8e45debfeae2ebf2a42730b9cb957d1136b3af62676204b2fd4cf696` | `e4ccb966583456a25e2b08373e76b4793dbe42a846c67ebb74c5e764ab662891` |
| `development-receipt.json` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `9b424d9ffa783254e90f8276186e997f1dc6b5bfb043c16f5f94a048400ff492` | `a1c5f4cf715be6ca16b494e1d180f5dfdc7135f569b927898e0599cfc9a3a381` |
| `development-validation.json` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `6badab24baec22b702c48c5b25f9c8001e4f9844c025dd06cf5aaf72bd7e291d` | `c22f7c6b3fb26f337b084affd1ba1bedb9f9e588e1a048fc0710555b4a572c2c` |
| `development-selected-tests.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `c64daeb3ed567ac255ea137c81ace63d33837f258e87a0d037e621fc8143f923` | `2d28c39dab549ef85553f65da1df5fdb119977b076f62a19908e253ad16c3832` |
| `package-validation.json` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `b17cdbd4c52edb075fc491056ba0cc274cc80ccc82100417fcee9d7a24440bf2` | `34c7b0cf6175b3ad3507c85b5de6849fe887c51fb418db3f66fc9d95d2fc793b` |
| `development-build-console.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `b11b5acb5b9e7cd4d93a43fbed42858d28a78ecf2e5f12bd0c5b14e6491e728c` | `0b77f95dc9aa7dd2898bd34655c86e040491f9040675563d5f41d0cd4a08249a` |
| `api-audit.json` | frozen packaged DLL 3507c755 of the 49be3fd0e2 candidate | `73ba914bbe5dc89d22801a53c617a43f2f18dc72667b2c5b713d6ab9ae9fa8ef` | `7c497fd67d31270e5178706853310dd9dd45395d53d2b4ba4a0176b0411bd443` |
| `complete-suite-unfiltered.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `1a0649705642a3049ba0f4bc555d20c9b96ea559f3c1698aa5a9166ffcecf49f` | `324a23b4b639ed76b1fa2a549dbcb9cc663be090db3dbac77beccf290db0bf57` |
| `pinned-loader-msbuild.json` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `4e42e922a3501cc1191f62b8112d39d3eb4ea43d020da4744dc555b3e3b68551` | `05cc7d415b5254349389c5c12877543ad134c395d229d667b4fe2932feb95b88` |
| `loader-case-selection.json` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `98c1f1a479f95cf45cc034433869504865695dad1a3eae185943339351b0139c` | `5ff834c0d1aed8a159e5ec37e4fa76b3b455a4fc41cb27b9f54aad370ef8fa35` |
| `loader-case-selection.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `ce3cc5cbd83ecd0da4fa63dd3e934ec3596aca0547a9b85fb04e25ea26711d69` | `69285cd5b3f5ab5268c0102545d26b0466c6a5382fdf8468ff93bb59d20a2243` |
| `loader-reference-pins.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` |
| `development-gate.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `8025db2cf25b03cb959c3c728c3ec4f18d0431c2fc56e5882a1d4d2ed11cd2f8` | `45c77e287cbcf1ea9b8033762f3bb0a73fed7e505dd446d520e2a0fe9fd37a31` |
| `focused-gate-loader-filter.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `73ebe9c8bc84619041c147e1f66c7cb2ba87dca69302ebbfcbb5fcc423094347` | `71e3f24635150ea4ac82d4b0727cf22532440cbbd52eb170905021142305f6d2` |
| `mismatched-binding-refusal.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `86a152e1296f9979f348d3bb76037f7ab9bfbf936a9a38722f5907d30812a424` | `5f632ec44a1267513bb89988edd3701e86fa6ae3e6a90de7887f7539525cd6d0` |
| `loader-compat-safety.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `8ef3c6122495aee3d87f1e2b73dd90ba6768c9c449fbc88b20a07b030689a7bc` | `8ef3c6122495aee3d87f1e2b73dd90ba6768c9c449fbc88b20a07b030689a7bc` |
| `pin-diagnostics.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `98200dc04ca42e4100e89f01bde0c73fc7a4071e876ac4567f69ed60968eeb2f` | `a6ed16b7259c3d31e8823ef761790a79f23c33af59b5330a0beeb379133cd47a` |
| `portable-loader-case-probe.txt` | `49be3fd0e28c551a4a5fafb5e98d87daffb125d5` | `e6923ba6dcce7ed8497bfc5b76b17a1ad7c5f0b3e23d622b17a637ec5220bcbe` | `0711667bfbd428bd6a422f2eaa63de5c2a9074635529535b1f551be01ab3ad2d` |
| `portable-loader-case-probe-generator.py.txt` | probe generator (net8 project with the portable runner's properties) | `de61da3a8d36c50534c516f30060158e25c74931efece3b79f85a2a69a3a67ff` | `7dd1358b522aa0efeb802a628830278555a38fe81b5111514e1c1bbbcdb83735` |
| `published-release-verification.json` | reused: published v0.0.188 and v0.0.195 assets verified for 0d767620e7 | `7253932ef2165b619f9d16cc0b5944c2776ce7bb12c7ca454094e43f69316694` (original output, from its first export) | `4358a49a7cd7d0420886cf2bb206434ba2c15a9d91a3d7d993c0e0e42c7e2d03` |
| `onboarding-check-project.json` | controller check-project record from enrollment (reused) | `e6e86dc48e3c7e3b4cf56f7cc3249fe46da10f7643a061bb6977727682aad756` (original output, from its first export) | `112b68ac5ed745a88960acf2bf0da0321be08637e655ab363cf16c619c7f7e21` |
| `loader-profiles.md` | fleet loader-profile record (reused) | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` |
| `loader-api-audit-Program.cs.txt` | audit tool source (reused) | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` |
| `loader-api-audit-csproj.txt` | audit tool project (reused) | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` |

`portable-loader-case-probe.txt` records the loader-reference case compiled with the portable runner's project
properties (.NET 8, C# 7.3, warnings as errors) and run three ways: unbound and unfiltered (NOT APPLICABLE, never a
pass), unbound with a focused selection (refused) and bound (refused there: the assertion runs on the .NET Framework
runner). The portable runner as a whole does not compile at this commit because older test files outside this change
use Newtonsoft.Json, which its generated project does not reference; that is not addressed here.

`development-build-console.txt` is the complete-suite Build-Local console (`-DevelopmentCompleteSuite`): the
development wrapper, the complete suite with the loader case deferred (NOT APPLICABLE), one production candidate
compile, the loader case on that exact output (PASS), one strict package validation, and the candidate digests.
