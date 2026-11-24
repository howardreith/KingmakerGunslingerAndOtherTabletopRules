# UMM 0.31.1 baseline evidence

Machine-generated outputs exported 2026-11-24 17:46 UTC for candidate commit `4f196b26d112640ace1eaa49dea355d16e210127`. Each row names what produced that output (its
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
| `candidate-admission.json` | 4f196b26d1 (Test-CandidateAdmission.ps1 in a temporary clone of that commit) | `96ad97c71c4b7e614e893e28d9d08581f3ae40319f6222e34a35238251060a95` | `cfbf68d5fab4d860a45ce7249b074d3c24eb4b3ba0880e03303b845baeffe4a2` |
| `development-receipt.json` | `4f196b26d112640ace1eaa49dea355d16e210127` | `07f7bc02dca3dc6a5b0f540118931739fdc698059a5a0534b996e76883eea47f` | `398c3c8a528f7b41dbd54aede4d3a02051f6f412ffabaeccedcc9a6c32d3e897` |
| `development-validation.json` | `4f196b26d112640ace1eaa49dea355d16e210127` | `6badab24baec22b702c48c5b25f9c8001e4f9844c025dd06cf5aaf72bd7e291d` | `c22f7c6b3fb26f337b084affd1ba1bedb9f9e588e1a048fc0710555b4a572c2c` |
| `development-selected-tests.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `d7c6e1c0f2b6ded4af11270c49dd9fa53733c0e58073b7a628812e5ab311c51a` | `5513757f52d36958f731376890882919d3749f4023411bea40b038040ffdfe4d` |
| `package-validation.json` | `4f196b26d112640ace1eaa49dea355d16e210127` | `1dfe37df6366d775ea9c91f02be03cff5c33242a1b42707620c5e8d2930aa086` | `156b129910f3c9831af7969f615c66f4673f607bc2b1c701342a5c35b9cd158a` |
| `development-build-console.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `adf838ef1aa5488a950cd84578e31356325e893552977c61dd07dd794a1fb7aa` | `9086d0b90c885786a17aa299184e3d48be37be84d11b07381c340135b2588df7` |
| `api-audit.json` | frozen packaged DLL b1e07c2a of the 4f196b26d1 candidate | `4b3bf0ac20cfd0d493bdefdf6c82e078ee90a11eff6bae442e88a14d1e254c8d` | `7e96ed45a94d05b408998db15fa327e1a954c2937f5a19f25511652d94bc289d` |
| `complete-suite-unfiltered.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `a8a6e84c2a62099be51766e97e77f039715715d84f35c81f4e62d6dd4da65eb3` | `046de58be07aed597ee8cbc92c67efc1f783ac7292a44938efec0ad901f7f678` |
| `pinned-loader-msbuild.json` | `4f196b26d112640ace1eaa49dea355d16e210127` | `e832ae1c6f3ab27e021e889c77ffdb8398e479d560c58175cab4923f7f1a5195` | `7e4752eefc38d4a80c45dde2a5e4f8d81222fd2d8618898505e5fbf47a452165` |
| `loader-reference-pins.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` |
| `development-gate.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `9dbf336a6e8026a12fd06ad772fee7b606b9dcef46739277381a271f334a5510` | `99dfc58679b8f1a7255fe1d4b6856682e4135aa591766e95c14e31b4978271e3` |
| `focused-gate-loader-filter.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `e5358a9328416cda70c3c4fe9790b249c57e38b9b919d6c0ccaa3830d52b0526` | `1492b9870ad7ffded7e80e1c83ba1caa74170aafc4633fd402ba5d7ad8489ec2` |
| `mismatched-binding-refusal.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `a9c3197013df13284e4fc4b46f4266d6904574bb5d9970c28f63194dd0715d1c` | `5b0b41e4cb99e92c567eecbf8fae596e567cb516dae92e1bb12fdc76734f51d0` |
| `loader-compat-safety.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `8ef3c6122495aee3d87f1e2b73dd90ba6768c9c449fbc88b20a07b030689a7bc` | `8ef3c6122495aee3d87f1e2b73dd90ba6768c9c449fbc88b20a07b030689a7bc` |
| `pin-diagnostics.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `98200dc04ca42e4100e89f01bde0c73fc7a4071e876ac4567f69ed60968eeb2f` | `a6ed16b7259c3d31e8823ef761790a79f23c33af59b5330a0beeb379133cd47a` |
| `portable-loader-case-probe.txt` | `4f196b26d112640ace1eaa49dea355d16e210127` | `2956c44fdbf0efb87c68aaff95b4e03446ef404e15076aa7e0ba1276ce0cd050` | `261983197f3d0abc2d02afc17ed698315f50e93269e753531babfabf70b4ba03` |
| `portable-loader-case-probe-generator.py.txt` | probe generator (net8 project with the portable runner's properties) | `f3e29ecdb54cf1231dbcbdf224fbaf8d2e679272e516d0625d0833f6f283f5c0` | `0ffc5fb0d67c828ccdb0843f9a08aaa8554887333689763c62766d47a17eb4b5` |
| `published-release-verification.json` | reused: published v0.0.188 and v0.0.195 assets verified for 0d767620e7 | `7253932ef2165b619f9d16cc0b5944c2776ce7bb12c7ca454094e43f69316694` (original output, from its first export) | `4358a49a7cd7d0420886cf2bb206434ba2c15a9d91a3d7d993c0e0e42c7e2d03` |
| `onboarding-check-project.json` | controller check-project record from enrollment (reused) | `e6e86dc48e3c7e3b4cf56f7cc3249fe46da10f7643a061bb6977727682aad756` (original output, from its first export) | `112b68ac5ed745a88960acf2bf0da0321be08637e655ab363cf16c619c7f7e21` |
| `loader-profiles.md` | fleet loader-profile record (reused) | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` |
| `loader-api-audit-Program.cs.txt` | audit tool source (reused) | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` |
| `loader-api-audit-csproj.txt` | audit tool project (reused) | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` |

`portable-loader-case-probe.txt` records the loader-reference case compiled with the portable runner's project
properties (.NET 8, C# 7.3, warnings as errors) and run three ways: unbound (not applicable), selected by name
without a binding (refused) and bound (refused there: the assertion runs on the .NET Framework runner). The portable
runner as a whole does not compile at this commit because older test files outside this change use Newtonsoft.Json,
which its generated project does not reference; that is not addressed here.
