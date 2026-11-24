# UMM 0.31.1 baseline evidence

Machine-generated outputs exported 2026-11-24 07:05 UTC for candidate commit `04b9384ed1feba02ed13f1dea0a71c384c076da5`. Each row names what produced that output (its
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
| `candidate-admission.json` | 04b9384ed1 (Test-CandidateAdmission.ps1 in a temporary clone of that commit) | `c519345ffc386220a19085e142ee7c3ff44acfc4bb32a90221c9d6f7af05fdee` | `ad7c7ef10ae615c82e732f4bb05d587d5304c0078b54ac3d79eea4c40fd383e1` |
| `development-receipt.json` | `04b9384ed1feba02ed13f1dea0a71c384c076da5` | `5da0a37d6dce8e49849589801925d0911ea2c1b0e5bbc12c26fd992b451df6b5` | `327476c39d78482700cacff337e9ba1f21b565dcd84a670f101363f7d68f47ec` |
| `development-validation.json` | `04b9384ed1feba02ed13f1dea0a71c384c076da5` | `6badab24baec22b702c48c5b25f9c8001e4f9844c025dd06cf5aaf72bd7e291d` | `c22f7c6b3fb26f337b084affd1ba1bedb9f9e588e1a048fc0710555b4a572c2c` |
| `package-validation.json` | `04b9384ed1feba02ed13f1dea0a71c384c076da5` | `f01343c1eccc05f26ce1c96b8562bc0287c9d28a0ed69c248a8e0ff3ed2de4b3` | `7e3de7e242983c3e54fec02cfed68f99a636bff2fcce42ee12408092f75d7a44` |
| `api-audit.json` | `04b9384ed1feba02ed13f1dea0a71c384c076da5` | `693fa79e3fc3a6738583d543692e7c27dce9e1f6db5b52839c89f7b97bd1ed71` | `ed35923bca19c8e329a43f6b91ffe0c6195b07149b54e3735b851d42fcf2153a` |
| `pinned-loader-msbuild.json` | 45b9b77097 (MSBuild inputs unchanged in 04b9384ed1) | `2f308f578ce2d3cc4098f98fe6ecd64817c8558a6cd357c8b75b604fdaa1944a` | `58b264aa50159820e690e1227711c04d0b6c49b6fe4b88d3a7d99d5a4f5cf7c9` |
| `loader-reference-pins.txt` | 45b9b77097 (pin checks unchanged in 04b9384ed1) | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` | `b1409bea4389196a66b07f54ec9c01aa2104dc390a6d096c0396717ed486c516` |
| `development-gate.txt` | working tree committed unchanged as 04b9384ed1 | `39fb7c084a8ead20e1a98f869e4fc4f019cd45ebc17e69fbbd95573234b49851` | `3687e5fd8a8c642ebe24abbcf7290443df426d7aff34090e33092b03e65dd00e` |
| `focused-gate-loader-filter.txt` | working tree committed unchanged as 04b9384ed1 | `8b557c5e2f22c69d7ce9d9310dac485c6bb1d0276a83ecd5e0af927534d17526` | `2d6f1261bd0a7ee6fb7823446f374b50934c5a13e04f551effc5ebeec128457e` |
| `published-release-verification.json` | reused: published v0.0.188 and v0.0.195 assets verified for 0d767620e7 | `7253932ef2165b619f9d16cc0b5944c2776ce7bb12c7ca454094e43f69316694` (original output, from its first export) | `4358a49a7cd7d0420886cf2bb206434ba2c15a9d91a3d7d993c0e0e42c7e2d03` |
| `onboarding-check-project.json` | controller check-project record from enrollment (reused) | `e6e86dc48e3c7e3b4cf56f7cc3249fe46da10f7643a061bb6977727682aad756` (original output, from its first export) | `112b68ac5ed745a88960acf2bf0da0321be08637e655ab363cf16c619c7f7e21` |
| `loader-profiles.md` | fleet loader-profile record (reused) | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` | `3b2883677f537286a0d6f210327fae8ac8734b40ec7cc5850e92819eaeb03d5e` |
| `loader-api-audit-Program.cs.txt` | audit tool source (reused) | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` | `be870827024fd41f63235ebb522c489e8d49c7991de7091ad0e14a1a0e935ea3` |
| `loader-api-audit-csproj.txt` | audit tool project (reused) | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` | `93989652d8252ff54a60ec92c13c1fc41bf94bfa3cf6c9f7f13c877bac761909` |
