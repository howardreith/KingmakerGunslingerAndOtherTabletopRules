# Agent-review continuation: $mission_title

$situation

$correction

You remain the Claude Opus developer for draft PR #$pr_number in `$worktree`
(branch `$branch`). The working rules from the start of this session still
apply: guarded push only (`$push_command`), focused validation with honest
limits, no edits to workflow, transcript, instruction or policy files, no
weakening of requirements, stop file `$stop_file`, and no GitHub posting.
Shell permissions match the start of each command: run one simple command per
call (no `cd`, `&&`, `;` or pipes). Allowed prefixes:
$allowed_commands

Mission (sha256 `$mission_sha256`): `$mission_path`

## Active checkpoint: `$checkpoint_id` ($checkpoint_type)

Scope: $checkpoint_scope

Exit conditions:
$exit_conditions

Later checkpoints:
$later_checkpoints

## Findings

For every pending finding, decide on the evidence:
- `fixed`: correct it in new commits (never amend or rewrite history), run the
  failing reproduction and affected neighbors first, and cite the commits and
  evidence.
- `rebutted`: the finding is wrong; give concrete evidence (file/line, test,
  observation). The reviewer decides whether to accept it.
- `deferred_optional`: only for genuinely optional polish, with rationale.
- `unresolved_blocker`: you cannot resolve it; explain precisely.
Do not silently drop a finding or mark your own rebuttal accepted.

When done, return the structured report (`agent-review.developer.v1`) with
`status` `checkpoint_ready`, `candidate_commit` equal to the pushed clean HEAD,
`checkpoint_id` `$checkpoint_id`, a disposition for every pending finding,
validation results and `runtime_restoration`.
