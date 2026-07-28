Set-StrictMode -Version Latest

function Select-KmgNamedMethodCandidates {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Methods,

        [Parameter(Mandatory = $true)]
        [string[]]$Names
    )

    return @(
        $Methods |
            Where-Object {
                $_ -ne $null -and
                -not [string]::IsNullOrWhiteSpace($_.Name) -and
                $Names -contains $_.Name
            } |
            Sort-Object `
                @{ Expression = { if ($_.DeclaringType) { $_.DeclaringType.FullName } else { '' } } },
                @{ Expression = { $_.Name } },
                @{ Expression = { if ($_.PSObject.Properties['MetadataToken']) { $_.MetadataToken } else { 0 } } }
    )
}

function Get-KmgRequiredMethodParameters {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Method,

        [Parameter(Mandatory = $true)]
        [string]$ContractName
    )

    try {
        return @($Method.GetParameters())
    }
    catch {
        $declaringType = if ($Method.DeclaringType) {
            $Method.DeclaringType.FullName
        }
        else {
            '<unknown>'
        }
        $assembly = if ($Method.DeclaringType -and $Method.DeclaringType.Assembly) {
            $Method.DeclaringType.Assembly.FullName
        }
        else {
            '<unknown>'
        }
        $message = "Required runtime contract '$ContractName' could not inspect " +
            "parameter metadata; assembly=$assembly; declaringType=$declaringType; " +
            "member=$($Method.Name); exception=$($_.Exception.GetType().FullName): " +
            $_.Exception.Message
        throw [InvalidOperationException]::new($message, $_.Exception)
    }
}
