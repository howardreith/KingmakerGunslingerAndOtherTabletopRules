import io

def patch(p, old, new, count=1):
    s = io.open(p, encoding='utf-8-sig').read()
    assert s.count(old) == count, (p, old[:70], s.count(old))
    io.open(p, 'w', encoding='utf-8-sig', newline='').write(s.replace(old, new))

# ---- transaction model: activation outcome + new states ----
p = 'src/KingmakerGunslinger/Spells/Teleportation/TeleportCastTransaction.cs'
patch(p,
r'''    internal enum TeleportTransactionState
    {
        Pending, Cancelled, Invalidated, Validating, Committed, Completed,
        TechnicalFailureUnspent, TechnicalFailureCompensated, TechnicalFailureSpent, AmbiguousExpenditure
    }''',
r'''    internal enum TeleportTransactionState
    {
        Pending, Cancelled, Invalidated, Validating, Committed, Completed,
        TechnicalFailureUnspent, TechnicalFailureCompensated, TechnicalFailureSpent, AmbiguousExpenditure,
        // Native item activation refused or failed its rules check. This is a
        // legitimate rules outcome, not a technical failure: a refused/UMD-failed
        // activation spends nothing and never teleports; a failed cast that the
        // native path still consumed stays spent with no teleport and no refund.
        ActivationRefused, ActivationFailedSpent
    }
    internal enum TeleportActivationOutcome { NotAttempted, Succeeded, RefusedUnspent, FailedSpent }''')

patch(p,
r'''        void Spend();
        TeleportExpenditure ObserveExpenditure();
        bool RestoreAndVerifyExactResource();
        object Evidence();
    }''',
r'''        void Spend();
        TeleportExpenditure ObserveExpenditure();
        bool RestoreAndVerifyExactResource();
        object Evidence();
        // Inventory-backed resources report their native activation outcome so a
        // refused or rules-failed activation is never mistaken for a spent cast.
        TeleportActivationOutcome ActivationOutcome { get; }
    }''')

patch(p,
r'''                spendAttempted = true;
                resource.Spend();
                TeleportExpenditure expenditure = resource.ObserveExpenditure();''',
r'''                spendAttempted = true;
                resource.Spend();
                TeleportActivationOutcome activation = resource.ActivationOutcome;
                if (activation == TeleportActivationOutcome.RefusedUnspent)
                {
                    State = TeleportTransactionState.ActivationRefused;
                    Diagnostic = "Native item activation was refused; nothing was spent and no teleport occurred.";
                    return;
                }
                if (activation == TeleportActivationOutcome.FailedSpent)
                {
                    State = TeleportTransactionState.ActivationFailedSpent;
                    Diagnostic = "Native item activation failed its rules check and the item was consumed; no teleport occurred.";
                    return;
                }
                TeleportExpenditure expenditure = resource.ObserveExpenditure();''')

# spellbook resource: activation not attempted
p = 'src/KingmakerGunslinger/Spells/Teleportation/TeleportationNativeCastResource.cs'
s = io.open(p, encoding='utf-8-sig').read()
anchor = '        public object Evidence() { return NativeEvidence(); }'
assert s.count(anchor) == 1
s = s.replace(anchor, anchor + '''
        public TeleportActivationOutcome ActivationOutcome
        { get { return TeleportActivationOutcome.NotAttempted; } }''')
io.open(p, 'w', encoding='utf-8-sig', newline='').write(s)

print('ok')
