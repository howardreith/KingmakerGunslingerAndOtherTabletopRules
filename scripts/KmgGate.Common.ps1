<#
.SYNOPSIS
The gate level's filter decisions, kept out of the gate script so they can be
tested without running a gate.

.DESCRIPTION
KMG_TEST_FILTER is process-global and inherited. That is exactly what makes it
convenient for an inner loop and dangerous for a qualification: a Sprint or
Tranche invocation that inherited a stale filter would run a subset and report
it as the complete suite. These two functions are the whole of the defence, and
they are separated here so a cheap test can prove them rather than a gate run
having to.
#>

Set-StrictMode -Version Latest

<#
.SYNOPSIS
Decides what KMG_TEST_FILTER must be for one gate level, or refuses.

.OUTPUTS
The filter string to apply. An empty string means "clear it": the complete
suite must run. Throws when the level and the filter disagree.
#>
function Resolve-KmgGateFilter {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Level,
        [AllowNull()][AllowEmptyString()][string]$Filter
    )

    $trimmed = if ($null -eq $Filter) { '' } else { $Filter.Trim() }
    switch ($Level) {
        'Focused' {
            # Required rather than optional. A Focused run with no filter is an
            # unfiltered suite wearing the word "focused", which is the one
            # thing a reader must never have to guess about.
            if ($trimmed -eq '') {
                throw 'The Focused level requires a non-empty -Filter. Run -Level Sprint for the complete suite.'
            }
            return $trimmed
        }
        { $_ -in @('Sprint', 'Tranche') } {
            # Refused rather than ignored. Silently dropping a filter the caller
            # asked for would teach them it had been honoured.
            if ($trimmed -ne '') {
                throw "The $Level level runs the complete suite and does not accept -Filter."
            }
            return ''
        }
        default { throw "Unknown gate level: $Level" }
    }
}

<#
.SYNOPSIS
Runs a body with KMG_TEST_FILTER set to exactly one value, then restores the
caller's environment precisely.

.DESCRIPTION
"Precisely" includes the difference between unset and set-to-empty: a variable
that did not exist before must not exist afterwards. An empty $Filter clears
the variable for the duration, which is how a qualification level guarantees it
cannot inherit a narrowing filter from its caller.
#>
function Invoke-KmgWithTestFilter {
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string]$Filter,
        [Parameter(Mandatory = $true)][scriptblock]$Body
    )

    $hadPrevious = Test-Path Env:\KMG_TEST_FILTER
    $previous = if ($hadPrevious) { $env:KMG_TEST_FILTER } else { $null }
    try {
        if ([string]::IsNullOrEmpty($Filter)) {
            Remove-Item Env:\KMG_TEST_FILTER -ErrorAction SilentlyContinue
        } else {
            $env:KMG_TEST_FILTER = $Filter
        }
        & $Body
    }
    finally {
        if ($hadPrevious) { $env:KMG_TEST_FILTER = $previous }
        else { Remove-Item Env:\KMG_TEST_FILTER -ErrorAction SilentlyContinue }
    }
}
