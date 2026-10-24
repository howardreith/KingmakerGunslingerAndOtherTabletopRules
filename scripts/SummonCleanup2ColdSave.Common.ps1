# Mutable authority is restricted to the existing exact Working filename.
# Caller owns one compatibility lease and must confirm game absence before any
# restore. No existing read-only catalog or legacy persistence guard is widened.
Set-StrictMode -Version Latest

function Assert-KmgCleanup2ColdTree([string]$Root) {
    $full=[IO.Path]::GetFullPath($Root)
    $parent=$full
    while($parent) {
        $item=Get-Item -LiteralPath $parent -Force
        if(($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {throw 'Cold path crosses a reparse point.'}
        $next=Split-Path -Parent $parent
        if($next -ceq $parent){break};$parent=$next
    }
    foreach($item in @(Get-ChildItem -LiteralPath $full -Force -Recurse)) {
        if(($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {throw 'Cold tree contains a reparse point.'}
    }
}

function Get-KmgCleanup2ColdWorkingRecord([string]$Path,[switch]$AllowEmpty) {
    Assert-KmgSummonCleanup2NativeExit
    $full=[IO.Path]::GetFullPath($Path)
    if([IO.Path]::GetFileName($full) -cne 'Manual_299_KMG_AUTOMATION_WORKING.zks') {throw 'Cold Working filename mismatch.'}
    $file=Get-Item -LiteralPath $full -Force
    # This checkpoint supports ordinary files, not a lossy reconstruction of
    # compressed/encrypted/sparse/reparse/offline or named-stream storage.
    # NotContentIndexed is an indexing metadata hint, preserved with the exact
    # original attributes by the existing backup/restoration record.
    $ordinaryAttributes=[int][IO.FileAttributes]::Archive -bor [int][IO.FileAttributes]::Normal -bor [int][IO.FileAttributes]::NotContentIndexed
    if($file.PSIsContainer -or ([int]$file.Attributes -band (-bnot $ordinaryAttributes)) -ne 0 -or
        ($file.PSObject.Properties['LinkType'] -and $file.LinkType)) {throw 'Cold Working must be an ordinary nonlinked file.'}
    $streams=@(Get-Item -LiteralPath $full -Stream '*' -ErrorAction Stop)
    if($streams.Count -ne 1 -or $streams[0].Stream -cne ':$DATA') {throw 'Cold Working has unsupported named data streams.'}
    $record=[ordered]@{path=$full;length=[long]$file.Length;sha256=$null;
        creationUtcTicks=$file.CreationTimeUtc.Ticks;writeUtcTicks=$file.LastWriteTimeUtc.Ticks;
        accessUtcTicks=$file.LastAccessTimeUtc.Ticks;attributes=[int]$file.Attributes;
        securitySddl=(Get-Acl -LiteralPath $full).GetSecurityDescriptorSddlForm(
            [Security.AccessControl.AccessControlSections]::Owner -bor [Security.AccessControl.AccessControlSections]::Group -bor
            [Security.AccessControl.AccessControlSections]::Access)}
    # Prove timestamp settlement is permitted before the data read can touch
    # access time; this writes back the exact current value.
    $file.LastAccessTimeUtc=[DateTime]::new([long]$record.accessUtcTicks,[DateTimeKind]::Utc)
    try {$record.sha256=(Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash.ToLowerInvariant()}
    finally {$file.LastAccessTimeUtc=[DateTime]::new([long]$record.accessUtcTicks,[DateTimeKind]::Utc)}
    # Hashing can update NTFS last access on this machine. This routine is
    # called only without a Working read lease; settle our own read before
    # returning the intake record. No save bytes are changed.
    $file.LastAccessTimeUtc=[DateTime]::new([long]$record.accessUtcTicks,[DateTimeKind]::Utc)
    if($record.length -le 0 -and -not $AllowEmpty) {throw 'Cold Working cannot be empty.'}
    return [pscustomobject]$record
}

function Open-KmgCleanup2ColdReadCatalog([string]$Root,[string]$EvidenceDirectory,$GuardRecord,[string]$GuardJournalPath) {
    Assert-KmgSummonCleanup2NativeExit;Assert-KmgCleanup2ColdTree $Root
    $leases=New-Object 'Collections.Generic.List[IO.FileStream]';$records=New-Object 'Collections.Generic.List[object]'
    try {
        foreach($file in @(Get-ChildItem -LiteralPath $Root -File -Force -Recurse|Sort-Object FullName)) {
            $stream=[IO.File]::Open($file.FullName,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
            $leases.Add($stream);$sha=[Security.Cryptography.SHA256]::Create()
            try {$hash=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','').ToLowerInvariant()}
            finally {$sha.Dispose();$stream.Position=0}
            $file.Refresh()
            $records.Add([ordered]@{path=$file.FullName;length=[long]$file.Length;sha256=$hash;
                creationUtcTicks=$file.CreationTimeUtc.Ticks;writeUtcTicks=$file.LastWriteTimeUtc.Ticks;attributes=[int]$file.Attributes})
        }
        [void][IO.Directory]::CreateDirectory($EvidenceDirectory)
        $intake=Join-Path $EvidenceDirectory 'preexisting-saves.json'
        if(Test-Path -LiteralPath $intake){throw 'Cold catalog evidence collision.'}
        Write-KmgRuntimeAtomicBytes -Path $intake -Bytes ([Text.UTF8Encoding]::new($false).GetBytes((@($records.ToArray())|ConvertTo-Json -Depth 8)))
        return [pscustomobject]@{Directory=$Root;Files=@($records.ToArray());Leases=$leases;EvidenceDirectory=$EvidenceDirectory}
    } catch {
        $failure=$_
        [void](Wait-KmgCleanup2ColdHandleRelease $GuardRecord $GuardJournalPath $null)
        foreach($stream in $leases){$stream.Dispose()}
        throw $failure
    }
}

function Open-KmgCleanup2ColdCatalog([string]$SaveDirectory,[string]$EvidenceDirectory,$GuardRecord,[string]$GuardJournalPath) {
    Assert-KmgSummonCleanup2NativeExit
    Assert-KmgCleanup2ColdTree $SaveDirectory
    $root=(Resolve-Path -LiteralPath $SaveDirectory).Path
    $working=Join-Path $root 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    $files=@(Get-ChildItem -LiteralPath $root -File -Force -Recurse|Sort-Object FullName)
    $workingMatches=@($files|Where-Object {$_.Name -cmatch '^Manual_[0-9]+_KMG_AUTOMATION_WORKING\.zks$'})
    $baselineMatches=@($files|Where-Object {$_.Name -cmatch '^Manual_[0-9]+_KMG_AUTOMATION_BASELINE\.zks$'})
    if($workingMatches.Count -ne 1 -or $workingMatches[0].FullName -cne $working -or $baselineMatches.Count -ne 1 -or
        $baselineMatches[0].FullName -cne (Join-Path $root 'Manual_298_KMG_AUTOMATION_BASELINE.zks')) {throw 'Cold exact Working/baseline filename intake is missing or ambiguous.'}
    # Exact native SaveInfo/header remains the native loader's authority.
    $original=Get-KmgCleanup2ColdWorkingRecord $working
    $catalog=$null
    try {
        # Deliberately own this small acquisition loop: the old read-only
        # catalog closes handles inside its failure catch before a caller can
        # retain them through unexpected-process/query failure.
        $catalog=Open-KmgCleanup2ColdReadCatalog $root $EvidenceDirectory $GuardRecord $GuardJournalPath
        $selection=Assert-KmgSummonCleanup2SaveSelection $catalog
        if($selection.working.path -cne $working -or
            [IO.Path]::GetFileName($selection.baseline.path) -cne 'Manual_298_KMG_AUTOMATION_BASELINE.zks' -or
            $selection.working.sha256 -cne $original.sha256 -or $selection.working.length -ne $original.length -or
            $selection.working.creationUtcTicks -ne $original.creationUtcTicks -or $selection.working.writeUtcTicks -ne $original.writeUtcTicks) {
            throw 'Cold exact Working/baseline identity is missing, ambiguous or changed during intake.'
        }
        $index=-1
        for($i=0;$i -lt $catalog.Files.Count;$i++){if($catalog.Files[$i].path -ceq $working){$index=$i;break}}
        if($index -lt 0){throw 'Cold Working read handle is missing.'}
        $workingLease=$catalog.Leases[$index];$catalog.Leases.RemoveAt($index)
        $all=@($catalog.Files)
        $catalog.Files=@($catalog.Files | Where-Object {$_.path -cne $working})
        return [pscustomobject]@{catalog=$catalog;allFiles=$all;working=$original;workingLease=$workingLease;
            backupLease=$null;backupPath=$null;backupSha256=$null}
    } catch {
        $failure=$_
        [void](Wait-KmgCleanup2ColdHandleRelease $GuardRecord $GuardJournalPath $null)
        if($catalog){foreach($stream in $catalog.Leases){$stream.Dispose()}}
        try {
            # Nothing was writable. Close all data handles first, then settle
            # the timestamp changed by our own hash/catalog reads.
            $now=Get-KmgCleanup2ColdWorkingRecord $working
            foreach($name in @('length','sha256','creationUtcTicks','writeUtcTicks','attributes','securitySddl')) {
                if($now.$name -cne $original.$name){throw ('Cold failed intake changed original Working: '+$name)}
            }
            (Get-Item -LiteralPath $working).LastAccessTimeUtc=[DateTime]::new([long]$original.accessUtcTicks,[DateTimeKind]::Utc)
        } catch {
            $GuardRecord.error=$failure.Exception.Message
            $GuardRecord.preentryWorking=$original;$GuardRecord.recoveryRequired=$true;$GuardRecord.status='RECOVERY_REQUIRED'
            $GuardRecord.restorationErrors=@($GuardRecord.restorationErrors)+@($_.Exception.Message)
            [void][IO.Directory]::CreateDirectory((Split-Path -Parent $GuardJournalPath))
            $early=[ordered]@{schemaVersion=1;kind='Cleanup2ColdPreentryMetadataOnly';runId=$GuardRecord.runId;nativeAdmitted=$false;
                ownerProcessId=$GuardRecord.ownerProcessId;ownerStartedUtcTicks=$GuardRecord.ownerStartedUtcTicks;
                working=$original;saveDirectory=$root;candidate=$GuardRecord.candidate;persistenceScope='giant-humanoids-ii'}
            $earlyPath=Join-Path (Split-Path -Parent $GuardJournalPath) 'cleanup2-cold-ownership.json'
            $immutable=Write-KmgCleanup2ColdOwnershipNew $earlyPath $early
            $GuardRecord.ownershipSha256=$immutable.sha256
            try {Write-KmgCleanup2ColdJournal $GuardJournalPath $GuardRecord}
            finally {[void](Wait-KmgCleanup2ColdHandleRelease $GuardRecord $GuardJournalPath $null);$immutable.lease.Dispose()}
        }
        throw $failure
    }
}

function Assert-KmgCleanup2ColdCatalog($Context) {
    Assert-KmgCleanup2ColdTree $Context.catalog.Directory
    [void](Assert-KmgProtectedSaveCatalog -Catalog $Context.catalog -OwnedPaths @($Context.working.path))
}

function Backup-KmgCleanup2ColdWorking($Context,[string]$RunRoot) {
    Assert-KmgSummonCleanup2NativeExit
    Assert-KmgCleanup2ColdTree $RunRoot
    $path=Join-Path $RunRoot 'original-working.zks'
    if(Test-Path -LiteralPath $path){throw 'Cold original Working backup collision.'}
    $output=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
    try {$Context.workingLease.Position=0;$Context.workingLease.CopyTo($output);$output.Flush($true)}
    finally {$output.Dispose();$Context.workingLease.Position=0}
    if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $Context.working.sha256 -or
        (Get-Item -LiteralPath $path).Length -ne $Context.working.length) {throw 'Durable original Working backup failed verification.'}
    [IO.File]::SetAttributes($path,[IO.FileAttributes]::ReadOnly)
    $Context.backupLease=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
    $Context.backupPath=$path;$Context.backupSha256=$Context.working.sha256
}

function Test-KmgCleanup2ColdWorkingRecord($Before,$After) {
    foreach($name in @('path','length','sha256','creationUtcTicks','writeUtcTicks','accessUtcTicks','attributes','securitySddl')) {
        if($Before.$name -cne $After.$name){return $false}
    }
    return $true
}

function Assert-KmgCleanup2ColdRestoredWorking($Context) {
    Assert-KmgSummonCleanup2NativeExit
    $expected=Join-Path $Context.catalog.Directory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    if($Context.working.path -cne $expected -or $null -eq $Context.workingLease -or -not $Context.workingLease.CanRead -or
        $Context.workingLease.Name -cne $expected){throw 'Restored Working has no exact retained read protection.'}
    $file=Get-Item -LiteralPath $expected -Force
    $sections=[Security.AccessControl.AccessControlSections]::Owner -bor [Security.AccessControl.AccessControlSections]::Group -bor [Security.AccessControl.AccessControlSections]::Access
    $security=(Get-Acl -LiteralPath $expected).GetSecurityDescriptorSddlForm($sections)
    if($file.Length -ne $Context.working.length -or $file.CreationTimeUtc.Ticks -ne $Context.working.creationUtcTicks -or
        $file.LastWriteTimeUtc.Ticks -ne $Context.working.writeUtcTicks -or [int]$file.Attributes -ne $Context.working.attributes -or
        $security -cne $Context.working.securitySddl){throw 'Restored Working metadata/security changed; foreign state is preserved.'}
    $Context.workingLease.Position=0;$sha=[Security.Cryptography.SHA256]::Create()
    try {$hash=[BitConverter]::ToString($sha.ComputeHash($Context.workingLease)).Replace('-','').ToLowerInvariant()}
    finally {$sha.Dispose();$Context.workingLease.Position=0}
    if($hash -cne $Context.working.sha256){throw 'Restored Working bytes changed; foreign state is preserved.'}
    # SetFileTime accepts FILE_WRITE_ATTRIBUTES without data-write access.
    # CreateFile sharing explicitly excludes attribute-only access requests:
    # https://learn.microsoft.com/windows/win32/api/fileapi/nf-fileapi-createfilea
    # This settles only our verification read's access time while the existing
    # read lease continuously denies data writes, deletion and renaming.
    if(-not ('KmgCleanup2ColdAccessTime' -as [type])) {
        Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
public static class KmgCleanup2ColdAccessTime {
    [StructLayout(LayoutKind.Sequential)] private struct FileTime { public uint Low, High; }
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    private static extern SafeFileHandle CreateFileW(string path, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);
    [DllImport("kernel32.dll", SetLastError=true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileTime(SafeFileHandle handle, IntPtr creation, ref FileTime access, IntPtr write);
    public static void Set(string path, long utcTicks) {
        long value = new DateTime(utcTicks, DateTimeKind.Utc).ToFileTimeUtc();
        var time = new FileTime { Low=(uint)value, High=(uint)(value >> 32) };
        using(var handle=CreateFileW(path, 0x100, 1, IntPtr.Zero, 3, 0x80, IntPtr.Zero)) {
            if(handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
            if(!SetFileTime(handle, IntPtr.Zero, ref time, IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
'@ | Out-Null
    }
    Assert-KmgSummonCleanup2NativeExit
    [KmgCleanup2ColdAccessTime]::Set($expected,[long]$Context.working.accessUtcTicks)
    $file.Refresh()
    $security=(Get-Acl -LiteralPath $expected).GetSecurityDescriptorSddlForm($sections)
    $after=[pscustomobject]@{path=$file.FullName;length=[long]$file.Length;sha256=$hash;creationUtcTicks=$file.CreationTimeUtc.Ticks;
        writeUtcTicks=$file.LastWriteTimeUtc.Ticks;accessUtcTicks=$file.LastAccessTimeUtc.Ticks;attributes=[int]$file.Attributes;securitySddl=$security}
    if(-not(Test-KmgCleanup2ColdWorkingRecord $Context.working $after)){throw 'Retained restored Working path/bytes/metadata/security are not exact.'}
}

function Protect-KmgCleanup2ColdRestoredWorking($Context) {
    Assert-KmgSummonCleanup2NativeExit
    Assert-KmgCleanup2ColdTree $Context.catalog.Directory
    if($null -eq $Context.workingLease -or -not $Context.workingLease.CanRead) {
        $Context.workingLease=[IO.File]::Open($Context.working.path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
    }
    # Never dispose this lease on a verification failure: the final native-exit
    # guard owns release, and the journal retains the unresolved restore debt.
    Assert-KmgCleanup2ColdRestoredWorking $Context
}

function Set-KmgCleanup2ColdWorkingMetadata($Context,[string]$VerifiedHash) {
    Assert-KmgSummonCleanup2NativeExit
    $expected=$Context.working.path
    Restore-KmgCleanup2ColdWorkingSecurity $Context
    $sections=[Security.AccessControl.AccessControlSections]::Owner -bor [Security.AccessControl.AccessControlSections]::Group -bor [Security.AccessControl.AccessControlSections]::Access
    $security=(Get-Acl -LiteralPath $expected).GetSecurityDescriptorSddlForm($sections)
    $file=Get-Item -LiteralPath $expected -Force
    $file.CreationTimeUtc=[DateTime]::new([long]$Context.working.creationUtcTicks,[DateTimeKind]::Utc)
    $file.LastWriteTimeUtc=[DateTime]::new([long]$Context.working.writeUtcTicks,[DateTimeKind]::Utc)
    $file.LastAccessTimeUtc=[DateTime]::new([long]$Context.working.accessUtcTicks,[DateTimeKind]::Utc)
    $file.Attributes=[IO.FileAttributes]$Context.working.attributes
    # No data read/handle follows final access-time settlement.
    $file.Refresh()
    $after=[pscustomobject]@{path=$file.FullName;length=[long]$file.Length;sha256=$VerifiedHash;
        creationUtcTicks=$file.CreationTimeUtc.Ticks;writeUtcTicks=$file.LastWriteTimeUtc.Ticks;
        accessUtcTicks=$file.LastAccessTimeUtc.Ticks;attributes=[int]$file.Attributes;securitySddl=$security}
    if(-not(Test-KmgCleanup2ColdWorkingRecord $Context.working $after)) {throw 'Cold original Working path/bytes/metadata/security verification failed.'}
}

function Restore-KmgCleanup2ColdWorkingSecurity($Context) {
    Assert-KmgSummonCleanup2NativeExit
    $expected=$Context.working.path
    $sections=[Security.AccessControl.AccessControlSections]::Owner -bor [Security.AccessControl.AccessControlSections]::Group -bor [Security.AccessControl.AccessControlSections]::Access
    $acl=Get-Acl -LiteralPath $expected
    if($acl.GetSecurityDescriptorSddlForm($sections) -cne $Context.working.securitySddl) {
        $original=New-Object Security.AccessControl.FileSecurity
        $original.SetSecurityDescriptorSddlForm($Context.working.securitySddl,$sections)
        $changed=[Security.AccessControl.AccessControlSections]::None
        foreach($section in @([Security.AccessControl.AccessControlSections]::Owner,[Security.AccessControl.AccessControlSections]::Group,[Security.AccessControl.AccessControlSections]::Access)) {
            if($acl.GetSecurityDescriptorSddlForm($section) -cne $original.GetSecurityDescriptorSddlForm($section)){$changed=$changed -bor $section}
        }
        # Set-Acl persists All through the provider and can request SACL
        # privilege. Persist only modified owner/group/DACL sections through
        # the documented framework/.NET APIs; do not touch an unaudited SACL.
        $replacement=New-Object Security.AccessControl.FileSecurity
        $replacement.SetSecurityDescriptorSddlForm($Context.working.securitySddl,$changed)
        $file=Get-Item -LiteralPath $expected
        if($file.PSObject.Methods['SetAccessControl']){$file.SetAccessControl($replacement)}
        else {[IO.FileSystemAclExtensions]::SetAccessControl($file,$replacement)}
    }
    if((Get-Acl -LiteralPath $expected).GetSecurityDescriptorSddlForm($sections) -cne $Context.working.securitySddl) {throw 'Cold captured owner/group/access ACL restoration failed.'}
}

function Restore-KmgCleanup2ColdUnwrittenWorking($Context) {
    Assert-KmgSummonCleanup2NativeExit
    if($Context.workingLease){$Context.workingLease.Dispose();$Context.workingLease=$null}
    $current=Get-KmgCleanup2ColdWorkingRecord $Context.working.path
    # No native writer was ever admitted: changed bytes/creation/write/ACL are
    # foreign, not a reason to repair them from a partial or absent backup.
    foreach($name in @('path','length','sha256','creationUtcTicks','writeUtcTicks','attributes','securitySddl')) {
        if($current.$name -cne $Context.working.$name){throw ('Unwritten cold Working changed: '+$name)}
    }
    Set-KmgCleanup2ColdWorkingMetadata $Context $current.sha256
    Protect-KmgCleanup2ColdRestoredWorking $Context
    Assert-KmgCleanup2ColdCatalog $Context
}

function Restore-KmgCleanup2ColdWorking($Context) {
    Assert-KmgSummonCleanup2NativeExit
    Assert-KmgCleanup2ColdTree $Context.catalog.Directory
    Assert-KmgCleanup2ColdTree (Split-Path -Parent $Context.backupPath)
    $expected=Join-Path $Context.catalog.Directory 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    if($Context.working.path -cne $expected -or $Context.backupPath -cne (Join-Path (Split-Path -Parent $Context.backupPath) 'original-working.zks')) {
        throw 'Cold restoration path mismatch.'
    }
    if($null -eq $Context.backupLease -or -not $Context.backupLease.CanRead -or
        (Get-FileHash -LiteralPath $Context.backupPath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $Context.working.sha256 -or
        (Get-Item -LiteralPath $Context.backupPath).Length -ne $Context.working.length) {throw 'Cold original Working backup is absent, changed or unprotected.'}
    Assert-KmgSummonCleanup2NativeExit
    if($Context.workingLease){$Context.workingLease.Dispose();$Context.workingLease=$null}
    if(Test-Path -LiteralPath $expected) {Restore-KmgCleanup2ColdWorkingSecurity $Context;[void](Get-KmgCleanup2ColdWorkingRecord $expected -AllowEmpty)}
    $output=[IO.File]::Open($expected,[IO.FileMode]::Create,[IO.FileAccess]::Write,[IO.FileShare]::None)
    try {$Context.backupLease.Position=0;$Context.backupLease.CopyTo($output);$output.Flush($true)} finally {$output.Dispose();$Context.backupLease.Position=0}
    $verifiedHash=(Get-FileHash -LiteralPath $expected -Algorithm SHA256).Hash.ToLowerInvariant()
    if($verifiedHash -cne $Context.working.sha256) {throw 'Cold restored Working bytes differ.'}
    Set-KmgCleanup2ColdWorkingMetadata $Context $verifiedHash
    Protect-KmgCleanup2ColdRestoredWorking $Context
    Assert-KmgCleanup2ColdCatalog $Context
}

function Close-KmgCleanup2ColdCatalog($Context) {
    if($null -eq $Context){return}
    if($Context.workingLease){$Context.workingLease.Dispose()}
    if($Context.backupLease){$Context.backupLease.Dispose()}
    Close-KmgProtectedSaveCatalog $Context.catalog
}
