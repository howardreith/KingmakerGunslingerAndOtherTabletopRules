# UMM 0.31.1 baseline evidence

Machine-generated outputs exported 2026-11-24 08:06 UTC for candidate commit `91139e4368ed5b2b56ff767203431e4dbde27edd`. Each row names what produced that output (its
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
| `candidate-admission.json` | 91139e4368 (Test-CandidateAdmission.ps1 in a temporary clone of that commit) | `5e0aa87d6b8de72332b0554d129f8a6abd89355db67462a86db9f730cf6479db` | `75ed5597d7a2d4618958831dea1bbcfbe2abbdc6e5e782609174db97445117c9` |
| `development-receipt.json` | `91139e4368ed5b2b56ff767203431e4dbde27edd` | `bbc69465c24186d92a6051819f7a0c20b1d02275d157fbe243129f8a22489c42` | `56ea5b5533c2085c0cc62c825cdbc558c939cd5ae577620d98c9488905e69636` |
| `development-validation.json` | `91139e4368ed5b2b56ff767203431e4dbde27edd` | `6badab24baec22b702c48c5b25f9c8001e4f9844c025dd06cf5aaf72bd7e291d` | `c22f7c6b3fb26f337b084affd1ba1bedb9f9e588e1a048fc0710555b4a572c2c` |
| `development-selected-tests.txt` | `91139e4368ed5b2b56ff767203431e4dbde27edd` | `d7c6e1c0f2b6ded4af11270c49dd9fa53733c0e58073b7a628812e5ab311c51a` | `5513757f52d36958f731376890882919d3749f4023411bea40b038040ffdfe4d` |
| `package-validation.json` | 0440edf673 (docs only after 91139e4368) | `75f3e017812df123906ed2d641c305f072ed121c416990dda2b454cee345dfcb` | `54f8ac93f4f436c29283802c3fe389c67f4970f09403da3477d4dca25708f42f` |
| `api-audit.json` | frozen packaged DLL 1ef167cc of the 91139e4368 candidate | `7276a34f98f045c591868737c7fa220491e02e604bad73f9185a6f1f72ecc73c` | `5a00a1f879c665be7d907a2bddb21673afece30d9a73284221a5310363a098ac` |
| `complete-suite-unfiltered.txt` | `91139e4368ed5b2b56ff767203431e4dbde27edd` | `5f5c761fb886d52c54ecf2368097ee3c84141d768f141254018df5a6d8782727` | `205d52cdf67cdecbaeaff8d3b3f2f5662ed5ca1440d74c6b866755a3a0c3a986` |
| `pinned-loader-msbuild.json` | 92539bb24a (91139e4368 changes only Build-Local receipt fields) | `b940eb53f2f2812ae404886d43e28ac2a453a118651e0dd1b5257d02b43d9eff` | `0036311761c6caf4684594c063a7367f1b7fcb7e65152648090fde035ebbd11c` |
| `loader-reference-pins.txt` | 0440edf673 (docs only after 91139e4368) | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` |
| `development-gate.txt` | 0440edf673 (docs only after 91139e4368) | `8fda3dcf376b06450905a9cebf1b47e150ed2eb7501bcd4e8dc26dbf98d08a2b` | `1176ca2f07bf3072fb8e639bd5c8ba81fe2a0309586086e05db83aa5314945a3` |
| `focused-gate-loader-filter.txt` | 0440edf673 (docs only after 91139e4368) | `907ec660ebc41e01dfc105f08e3e3fc4e97047ea09586f37202817d5ca6ba025` | `a3f7df6125377a6e25ac7031cb4168994076ed0c124d62263ce024ae91cb70bc` |
| `mismatched-binding-refusal.txt` | 0440edf673 (docs only after 91139e4368) | `997c8fc3c9051f035dddc1ea1f2d5d0dce08ab235b9c11f1b40d398e7a1faef9` | `520fad47c2aab8cfa84a6f9b63f0c611dec8af96723b0059ead64281411bec86` |
| `loader-compat-safety.txt` | 0440edf673 (docs only after 91139e4368) | `8ef3c6122495aee3d87f1e2b73dd90ba6768c9c449fbc88b20a07b030689a7bc` | `8ef3c6122495aee3d87f1e2b73dd90ba6768c9c449fbc88b20a07b030689a7bc` |
| `pin-diagnostics.txt` | 0440edf673 (docs only after 91139e4368) | `dfffd1dafcb5b40203e88de2dd64cc2cc5cde2fbe54e244d551d35a09a919811` | `cd6e65080191e7d65a549b87eb59a6d79a5f9b7aefc91738f1d7e2a67e986522` |
| `published-release-verification.json` | reused: published v0.0.188 and v0.0.195 assets verified for 0d767620e7 | `7253932ef2165b619f9d16cc0b5944c2776ce7bb12c7ca454094e43f69316694` (original output, from its first export) | `4358a49a7cd7d0420886cf2bb206434ba2c15a9d91a3d7d993c0e0e42c7e2d03` |
| `onboarding-check-project.json` | controller check-project record from enrollment (reused) | `e6e86dc48e3c7e3b4cf56f7cc3249fe46da10f7643a061bb6977727682aad756` (original output, from its first export) | `112b68ac5ed745a88960acf2bf0da0321be08637e655ab363cf16c619c7f7e21` |
| `loader-profiles.md` | fleet loader-profile record (reused) | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` |
| `loader-api-audit-Program.cs.txt` | audit tool source (reused) | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` |
| `loader-api-audit-csproj.txt` | audit tool project (reused) | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` |
