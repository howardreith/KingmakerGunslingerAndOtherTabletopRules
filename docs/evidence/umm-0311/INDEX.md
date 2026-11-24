# UMM 0.31.1 baseline evidence

Machine-generated outputs for candidate commit `8dfe6d08b5b5dc1fb68408297765ece6357ce7f6`, exported 2026-11-24 03:30 UTC.
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

| File | Raw SHA-256 | Exported SHA-256 (Git blob) |
| --- | --- | --- |
| `pinned-loader-msbuild.json` | `ce629a62765fceb79601f64eba498ee21dfcb425964e58bbfaaaf1d5ec92b355` | `dea598f1848c0e77c0158a0eafe2384f37197f28e0816fec62595887f850df4d` |
| `development-validation.json` | `218131b48d8325f213004a4bd998eedfb216e23336d9611c496b84872ff5cfa8` | `ac3e375ba75d0214e48f0dc1839a1b30876c37753c6a6b67484f9b47d9dc0508` |
| `published-release-verification.json` | `7253932ef2165b619f9d16cc0b5944c2776ce7bb12c7ca454094e43f69316694` | `4358a49a7cd7d0420886cf2bb206434ba2c15a9d91a3d7d993c0e0e42c7e2d03` |
| `onboarding-check-project.json` | `e6e86dc48e3c7e3b4cf56f7cc3249fe46da10f7643a061bb6977727682aad756` | `112b68ac5ed745a88960acf2bf0da0321be08637e655ab363cf16c619c7f7e21` |
