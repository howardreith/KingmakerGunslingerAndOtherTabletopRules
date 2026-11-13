# History rewrite of 2026-11-13

On 2026-11-13 the owner had every branch and release tag on GitHub rewritten
to remove `Co-Authored-By: Claude ... <noreply@anthropic.com>` trailers from
commit messages. Nothing else changed: every rewritten commit keeps its file
tree, author, committer and dates. The changes are the message (the trailer
line is gone, and git-filter-repo replaced abbreviated commit IDs quoted in
messages with their new IDs) and therefore the commit ID. The first commit
affected is the 2026-09-22 "Require firearm proficiency for Rapid Reload". All
94 `v*` tags were moved to the rewritten commits; release assets did not change.

[commit-id-map-2026-11-13.tsv](commit-id-map-2026-11-13.tsv) lists all 4,058
rewritten commits as `old<TAB>new`. Use it to translate commit IDs in older
records, evidence and release notes, which keep the IDs that were true when
they were written.

## Which IDs were updated

- Executable checks that pass a commit ID to git (`git show <id>:<path>`,
  `git diff <id>`, `merge-base --is-ancestor`, `rev-parse`) now use the new ID.
  The old object no longer exists in a fresh clone.
- IDs that name built artifacts keep the old value. Development and release
  packages stamp their source commit into the DLL, receipts and folder names
  when they are built, so a package built before the rewrite still carries the
  old ID.
- Records, evidence, planning notes, release notes and source comments keep
  their original IDs as historical text. Translate them with the map.

GitHub's read-only `refs/pull/*` refs still hold the original commits of
merged pull requests; only GitHub support can purge those.
