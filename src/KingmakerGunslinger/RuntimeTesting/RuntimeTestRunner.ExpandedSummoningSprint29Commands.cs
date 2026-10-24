using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool _cleanup2EttinOrdinaryFullAttackProved;
        // Native RTWP commands, rather than queried plans: one disposable
        // owner and an awake visible hostile per case. Rolls remain random;
        // the fixture neither issues an attack nor supplies a hit.
        private IEnumerable<int> ReviewSprint29Commands(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            GameMode[] cleanup2BaseModes = null;
            int[] cleanup2BaseCounts = null;
            GameMode cleanup2SetupPause = null;
            if (IsSummonCleanup2Review)
            {
                // The outer request fixture also pauses during setup. Retain
                // that exact inherited Pause receipt; no arbitrary overlay or
                // human-loaded world is admitted by this command caller.
                if (!ReferenceEquals(fixture, _sprint25Fixture) || fixture.Caster == null ||
                    fixture.Caster.Blueprint != fixture.CasterBlueprint || fixture.UnitsBefore.Any(value => ReferenceEquals(value, fixture.Caster)) ||
                    (string)_request.Parameters["reviewPhase"] != "ettin-commands" || !Cleanup2NoNarrativeOverlay())
                    throw new InvalidOperationException("Exact owned Ettin command fixture required before pause settlement.");
                var stack = (Stack<GameMode>)typeof(Game).GetField("m_GameModes", Cleanup2Fields).GetValue(Game.Instance);
                GameMode[] entry = stack.ToArray();
                cleanup2SetupPause = entry.Length > 0 && entry[0].Type == GameModeType.Pause ? entry[0] : null;
                cleanup2BaseModes = cleanup2SetupPause == null ? entry : entry.Skip(1).ToArray();
                cleanup2BaseCounts = ((int[])typeof(Game).GetField("m_ModesCount", Cleanup2Fields).GetValue(Game.Instance)).ToArray();
                if (cleanup2SetupPause != null) cleanup2BaseCounts[(int)GameModeType.Pause]--;
                if (cleanup2BaseModes.Length == 0 || cleanup2BaseModes.Any(value =>
                    value.Type != GameModeType.Default && value.Type != GameModeType.Dialog) ||
                    cleanup2BaseCounts[(int)GameModeType.Pause] != 0)
                    throw new InvalidOperationException("Unexpected Ettin fixture mode stack; no pause is removed.");
            }
            Game.Instance.IsPaused = true;
            if(!IsSummonCleanup2Review || fixture.Hostile==null || fixture.Hostile.Destroyed)
                CreateExpandedSummoningCorrectionHostile(fixture);
            else ResetExpandedSummoningHostile(fixture);
            Cleanup2NativeDefaultScope nativeMode = null;
            string cleanup2Stage = "fixture-setup";
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                if (IsSummonCleanup2Review)
                    foreach (int frame in Cleanup2SettleEttinOwnedPause(fixture, cleanup2BaseModes,
                        cleanup2BaseCounts, cleanup2SetupPause, true, "fixture-setup")) yield return frame;
                else Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (var spec in new[] {
                    new { Key="ettin", Name="flail-single", Set=0,
                        Full=false, Count=1, Off=0,
                        Main="KMG_Summoning_Giant_Sprint29_EttinMainFlail2d6",
                        Secondary="KMG_Summoning_Giant_Sprint29_EttinOffFlail2d6" },
                    new { Key="ettin", Name="flail-full", Set=0,
                        Full=true, Count=4, Off=2,
                        Main="KMG_Summoning_Giant_Sprint29_EttinMainFlail2d6",
                        Secondary="KMG_Summoning_Giant_Sprint29_EttinOffFlail2d6" },
                    new { Key="ettin", Name="javelin-full", Set=1,
                        Full=true, Count=2, Off=1,
                        Main="KMG_Summoning_Giant_Sprint29_EttinMainJavelin1d8",
                        Secondary="KMG_Summoning_Giant_Sprint29_EttinOffJavelin1d8" },
                    new { Key="cloud-giant", Name="morningstar-full", Set=0,
                        Full=true, Count=3, Off=0,
                        Main="KMG_Summoning_Giant_Sprint29_CloudMorningstar4d6",
                        Secondary="" },
                    new { Key="cloud-giant", Name="slam-full", Set=1,
                        Full=true, Count=2, Off=0,
                        Main="KMG_Summoning_Giant_CloudSlam2d6",
                        Secondary="" },
                    new { Key="cloud-giant", Name="rock-full", Set=2,
                        Full=true, Count=1, Off=0,
                        Main="KMG_Summoning_Giant_CloudRock2d6",
                        Secondary="" },
                    new { Key="storm-giant", Name="greatsword-full", Set=0,
                        Full=true, Count=3, Off=0,
                        Main="KMG_Summoning_Giant_Sprint29_StormGreatsword4d6",
                        Secondary="" },
                    new { Key="storm-giant", Name="slam-full", Set=1,
                        Full=true, Count=2, Off=0,
                        Main="KMG_Summoning_Giant_Sprint29_StormSlam2d6",
                        Secondary="" },
                    new { Key="storm-giant", Name="bow-full", Set=2,
                        Full=true, Count=3, Off=0,
                        Main="KMG_Summoning_Giant_Sprint29_StormCompositeBow3d6",
                        Secondary="" }
                }.Where(value => !IsSummonCleanup2Review || value.Key == "ettin" && value.Name == "flail-full")
                .Where(value => !turnBased ||
                    value.Key == "ettin" &&
                        (value.Name == "flail-full" ||
                         value.Name == "javelin-full") ||
                    value.Key == "cloud-giant" &&
                        value.Name == "morningstar-full" ||
                    value.Key == "storm-giant" &&
                        value.Name == "bow-full"))
                {
                    cleanup2Stage = "qualified-synchronous-summon";
                    UnitEntityData owner;
                    try { owner = CastExpandedSummoningOwnTier(fixture, spec.Key); }
                    catch (Exception error)
                    {
                        if (IsSummonCleanup2Review)
                        {
                            var expected = ExpandedSummoningUnit(fixture.Blueprints,
                                ExpandedSummoningOwnTierVariant(spec.Key, SummonMultiplicity.One));
                            foreach (var partial in ExpandedSummoningRuleCapture.Where(value => value != null &&
                                ReferenceEquals(value.Blueprint, expected) && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)))
                                if (!fixture.Created.Contains(partial)) fixture.Created.Add(partial);
                            Cleanup2Check("ettin-command-summon-setup-failed-" + (turnBased ? "turn" : "rtwp"), false,
                                new JObject { ["stage"] = cleanup2Stage, ["originalException"] = error.ToString(),
                                    ["qualifiedCastExecution"] = _expandedSummoningLastAbilityExecution,
                                    ["nativeDefaultLeasePending"] = Cleanup2NativeDefaultPending,
                                    ["mode"] = Game.Instance.CurrentMode.ToString() },
                                "A synchronous fixture-summon error remains visible before any command-only native mode lease or cleanup refusal.");
                        }
                        throw;
                    }
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        cleanup2Stage = "appearance-and-native-toggle";
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        foreach (int frame in Sprint22SetPowerAttack(owner, false))
                            yield return frame;
                        if (spec.Set != 0)
                        {
                            var change = new UnitSwitchHandEquipmentSet(spec.Set);
                            owner.Commands.Run(change);
                            int frames = 0;
                            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                            while (!change.IsFinished && frames++ < 1000 &&
                                DateTime.UtcNow < deadline) yield return 0;
                            bool ready = change.IsFinished &&
                                owner.Body.CurrentHandEquipmentSetIndex == spec.Set;
                            Sprint25Check(spec.Key + "-" + spec.Name + "-switch",
                                ready, new JObject { ["owner"] = owner.UniqueId,
                                    ["finished"] = change.IsFinished,
                                    ["result"] = change.Result.ToString(),
                                    ["active"] = owner.Body.CurrentHandEquipmentSetIndex,
                                    ["frames"] = frames },
                                "Native command selected the private armament before combat.");
                            if (!ready) throw new InvalidOperationException(
                                "Sprint 29 command hand switch did not finish.");
                        }
                        UnitEntityData target = fixture.Hostile;
                        ResetExpandedSummoningHostile(fixture);
                        cleanup2Stage = "native-lane-survey";
                        string lane, direction; JObject cleanup2Lane = null;
                        if (IsSummonCleanup2Review && spec.Key == "ettin" && spec.Name == "flail-full")
                        {
                            // Oleg has overlapping native graphs. Reuse the
                            // qualified query-owned walking constraint and
                            // actual post-placement range/LOS proof instead of
                            // treating an unconstrained nearest node as ground.
                            // Both cases are still RTWP here; TB admission is
                            // deliberately performed only after placement.
                            cleanup2Lane = Cleanup2CrabAttackLane(fixture, owner, target,
                                "ettin-flail-full-" + (turnBased ? "turn" : "rtwp"));
                            lane = "cleanup2-native-connected-walking-graph-and-range";
                            direction = (string)cleanup2Lane["direction"];
                        }
                        else
                        {
                            Vector3 far;
                            Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                                owner, target, 8f, out far, out lane,
                                out direction);
                            Vector3 forward = (far - origin).normalized;
                            PlaceExpandedSummoningUnit(owner, origin);
                            float separation = owner.Corpulence +
                                target.Corpulence + .8f;
                            PlaceExpandedSummoningUnit(target,
                                spec.Set == 2 || spec.Key == "ettin" &&
                                    spec.Set == 1 ? far :
                                    origin + forward * separation);
                        }
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        var probe = new UnitAttack(target);
                        probe.Init(owner);
                        bool legal = owner.HasLOS(target) &&
                            probe.IsUnitEnoughClose && owner.IsEnemy(target) &&
                            !target.Descriptor.State.IsDead;
                        Sprint25Check(spec.Key + "-" + spec.Name + "-lane",
                            legal, new JObject { ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["lane"] = lane, ["direction"] = direction,
                                ["cleanup2ConstrainedLane"] = cleanup2Lane,
                                ["distance"] = owner.DistanceTo(target),
                                ["los"] = owner.HasLOS(target),
                                ["inRange"] = probe.IsUnitEnoughClose,
                                ["enemy"] = owner.IsEnemy(target) },
                            "A visible hostile and a legal native lane precede the attack command.");
                        if (!legal) throw new InvalidOperationException(
                            "Sprint 29 command has no legal visible lane.");
                        owner.JoinCombat(); target.JoinCombat();
                        owner.Memory.Add(target); target.Memory.Add(owner);
                        // Qualified synchronous fixture summoning, appearance,
                        // toggle and placement finish before native progression
                        // owns the command. Never carry its lease across the
                        // fixture cast/manual-clock lifecycle.
                        if (IsSummonCleanup2Review && !turnBased)
                        {
                            // Joining this exact owned pair can invoke native
                            // PauseNewEnemyAppeared. Settle that caller-owned
                            // pause before entering the strict Default lease.
                            foreach (int frame in Cleanup2SettleEttinOwnedPause(fixture, cleanup2BaseModes,
                                cleanup2BaseCounts, null, true, "owned-pair-joined-combat")) yield return frame;
                            cleanup2Stage = "native-command-mode-entry";
                            nativeMode = Cleanup2EnterNativeDefault(fixture, "ettin-ordinary-rtwp");
                            for (int frame = 0; frame < 8; frame++)
                            {
                                if (!nativeMode.Current || !Cleanup2NoNarrativeOverlay())
                                    throw new InvalidOperationException("Ettin command-only native Default context changed before command issue.");
                                yield return 0;
                            }
                        }
                        JObject admission = null;
                        if (turnBased)
                        {
                            admission = new JObject();
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                                new[] { owner }, target, admission))
                                yield return frame;
                            for (int frame = 0; frame < 500 &&
                                !Sprint18OwnerIsActing(owner); frame++)
                                yield return 0;
                            bool acting = (bool)admission["admitted"] &&
                                Sprint18OwnerIsActing(owner);
                            Sprint25Check(spec.Key + "-" + spec.Name +
                                "-turn-admission", acting,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["admission"] = admission,
                                    ["turn"] = Sprint22TurnState(owner, target) },
                                "The exact hidden owner receives a native turn before its own command.");
                            if (!acting) throw new InvalidOperationException(
                                "Sprint 29 native owner turn admission failed.");
                        }
                        int before = target.Descriptor.Damage;
                        ItemEntityWeapon main = owner.Body.PrimaryHand.MaybeWeapon;
                        ItemEntityWeapon off = owner.Body.SecondaryHand.MaybeWeapon;
                        var observer = new Sprint24AttackObserver {
                            Owner = owner, Target = target };
                        var costs = new JArray();
                        var endings = new JArray();
                        var images = new JArray();
                        int photographedEvents=0;
                        var command = new UnitAttack(target) {
                            ForceFullAttack = spec.Full,
                            IsSingleAttack = !spec.Full };
                        cleanup2Stage = "ordinary-native-command";
                        EventBus.Subscribe(observer);
                        try
                        {
                            using (var economy = new FirearmReviewCosts(owner,
                                costs))
                            using (var trace = new Sprint26CommandTrace(owner,
                                endings))
                            {
                                owner.Commands.Run(command);
                                int frames = 0;
                                DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                                while (frames++ < 3500 &&
                                    DateTime.UtcNow < deadline &&
                                    (!command.IsFinished ||
                                     observer.WeaponAttacks.Count < spec.Count))
                                {
                                    if (nativeMode != null && (!nativeMode.Current || !Cleanup2NoNarrativeOverlay()))
                                        throw new InvalidOperationException("Ettin ordinary native command lost its exact Default lease.");
                                    if(IsSummonCleanup2Review && observer.WeaponAttacks.Count>photographedEvents)
                                    {
                                        photographedEvents=observer.WeaponAttacks.Count;
                                        string file="ettin-ordinary-"+(turnBased?"turn":"rtwp")+"-event"+photographedEvents+".png";
                                        images.Add(new JObject { ["file"]=file,["result"]=Cleanup2OrdinaryAttackImage(owner,target,file),
                                            ["actualWeaponEventCount"]=photographedEvents,["plannedIndex"]=command.GetAttackIndex() });
                                    }
                                    yield return 0;
                                }
                                int hits = observer.WeaponAttacks.Count(value =>
                                    value.AttackRoll != null &&
                                    value.AttackRoll.IsHit);
                                int impactFrames = 0;
                                DateTime impactDeadline =
                                    DateTime.UtcNow.AddSeconds(10);
                                while (observer.Damage.Count < hits &&
                                    impactFrames++ < 600 &&
                                    DateTime.UtcNow < impactDeadline)
                                    yield return 0;
                                for (int frame = 0; frame < 12; frame++)
                                    yield return 0;
                                int mainPackets = observer.WeaponAttacks.Count(
                                    value => value.Weapon?.Blueprint?.name ==
                                        spec.Main);
                                int offPackets = observer.WeaponAttacks.Count(
                                    value => value.Weapon?.Blueprint?.name ==
                                        spec.Secondary && spec.Off > 0);
                                bool exact = command.IsFinished &&
                                    (command.Result == UnitCommand.ResultType.Success ||
                                     trace.Exhausted(command)) &&
                                    command.AllAttacks.Count == spec.Count &&
                                    observer.OwnerWeaponAttacks.Count == spec.Count &&
                                    observer.WeaponAttacks.Count == spec.Count &&
                                    observer.Rolls.Count == spec.Count &&
                                    mainPackets == spec.Count - spec.Off &&
                                    offPackets == spec.Off &&
                                    ReferenceEquals(main?.Owner, owner.Descriptor) &&
                                    (spec.Off == 0 || ReferenceEquals(off?.Owner,
                                        owner.Descriptor)) &&
                                    observer.Damage.Count == hits &&
                                    economy.Count(command) == 1 &&
                                    economy.Exact(command, turnBased) &&
                                    !command.Cutscene &&
                                    !command.IsIgnoreCooldown &&
                                    !target.Descriptor.State.IsDead;
                                if(IsSummonCleanup2Review)
                                {
                                    _cleanup2EttinOrdinaryFullAttackProved=exact;
                                    var offInfo=new AttackHandInfo(owner.Body.SecondaryHand,0,0);
                                    JObject selection=Cleanup2EttinAttackSelection(offInfo);
                                    offInfo.CreateAnimationHandleForAttack();
                                    Cleanup2Check("ettin-ordinary-offhand-contract-"+(turnBased?"turn":"rtwp"),exact,
                                        new JObject { ["unit"]=owner.UniqueId,["nativeSelection"]=selection,
                                            ["separateOffhandHandleAvailable"]=offInfo.AnimationHandle!=null,
                                            ["ordinaryNativeCommandResolved"]=exact,["mainPackets"]=mainPackets,["offhandPackets"]=offPackets,
                                            ["oneNativeCost"]=economy.Count(command)==1 && economy.Exact(command,turnBased),
                                            ["nativeImages"]=images,["replacementBody"]=EttinSecondHeadVisual.Describe(owner.View),
                                            ["nativeMode"]=nativeMode?.Describe(),
                                            ["limits"]=turnBased ?
                                                "Native turn-admitted command/action/rule proof under the established fixture clock. No substituted handle, clip or forced rule/hit." :
                                                "Ordinary native Default RTWP progression with the manual fixture clock suspended. No substituted handle, clip or forced rule/hit." },
                                        "An ordinary paired-flail full attack resolves two actual main-hand and two actual off-hand weapon events with one native action cost; optional action lookup is recorded separately.");
                                }
                                Sprint25Check(spec.Key + "-" + spec.Name +
                                    "-resolved", exact,
                                    new JObject { ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["turnBased"] = turnBased,
                                        ["admission"] = admission,
                                        ["weapon"] = main?.Blueprint?.name,
                                        ["offhand"] = off?.Blueprint?.name,
                                        ["finished"] = command.IsFinished,
                                        ["result"] = command.Result.ToString(),
                                        ["frames"] = frames,
                                        ["planned"] = command.AllAttacks.Count,
                                        ["expected"] = spec.Count,
                                        ["ownerAttacks"] = observer.OwnerWeaponAttacks.Count,
                                        ["targetAttacks"] = observer.WeaponAttacks.Count,
                                        ["mainPackets"] = mainPackets,
                                        ["offPackets"] = offPackets,
                                        ["rolls"] = new JArray(
                                            observer.WeaponAttacks.Select(value =>
                                                new JObject {
                                                    ["weapon"] = value.Weapon?.Blueprint?.name,
                                                    ["hit"] = value.AttackRoll?.IsHit,
                                                    ["natural"] = value.AttackRoll?.Roll.Value,
                                                    ["attackBonus"] = value.AttackRoll?.AttackBonus,
                                                    ["nativeDamageRule"] =
                                                        value.MeleeDamage != null })),
                                        ["damage"] = new JArray(observer.Damage.Select(
                                            Sprint16DamageEvent)),
                                        ["hits"] = hits,
                                        ["damagePackets"] = observer.Damage.Count,
                                        ["impactWaitFrames"] = impactFrames,
                                        ["targetDamageBefore"] = before,
                                        ["targetDamageAfter"] = target.Descriptor.Damage,
                                        ["costs"] = costs,
                                        ["nativeEnds"] = endings,
                                        ["forcedRolls"] = 0,
                                        ["fixtureAttackCommands"] = 0 },
                                    "One issued native command spends its action and resolves only its selected weapon packets against the visible hostile.");
                            }
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                            false;
                        Game.Instance.TurnBasedCombatController.Activate();
                        owner.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat();
                        ResetExpandedSummoningHostile(fixture);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
                }
            }
            finally
            {
                try
                {
                    if (nativeMode != null)
                    {
                        try { nativeMode.Dispose(); }
                        catch (Exception cleanupError)
                        {
                            // A failed cleanup remains a FAIL assertion and
                            // retains its guard/lease. It must not replace an
                            // exception already unwinding from native setup or
                            // command observation.
                            Cleanup2Check("ettin-command-native-default-cleanup-failed", false,
                                new JObject { ["lastStage"] = cleanup2Stage, ["cleanupException"] = cleanupError.ToString(),
                                    ["qualifiedCastExecution"] = _expandedSummoningLastAbilityExecution,
                                    ["nativeDefaultLeasePending"] = Cleanup2NativeDefaultPending, ["nativeMode"] = nativeMode.Describe() },
                                "Exact native mode restoration must succeed; refusal preserves pending ownership and never replaces the original command/setup exception.");
                        }
                    }
                }
                finally
                {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                    Game.Instance.TurnBasedCombatController.Activate();
                    Game.Instance.IsPaused = oldPause;
                }
            }
        }

        private IEnumerable<int> Cleanup2SettleEttinOwnedPause(ExpandedSummoningCorrectionFixture fixture,
            GameMode[] baseline, int[] counts, GameMode knownPause, bool callerRequestedPause, string stage)
        {
            var game = Game.Instance;
            var modesField = typeof(Game).GetField("m_GameModes", Cleanup2Fields);
            var countsField = typeof(Game).GetField("m_ModesCount", Cleanup2Fields);
            var pendingField = typeof(Game).GetField("m_WillBePaused", Cleanup2Fields);
            var pendingActions = typeof(Game).GetField("m_BeforeTickActions", Cleanup2Fields);
            bool waiting = _sprint25WaitingForUnityAppearance;
            bool stopped = false;
            var rows = new JArray();
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);
            _sprint25WaitingForUnityAppearance = true;
            try
            {
                for (int frame = 0; frame < 90 && DateTime.UtcNow < deadline; frame++)
                {
                    var stack = (Stack<GameMode>)modesField.GetValue(game);
                    int[] actual = (int[])countsField.GetValue(game);
                    bool pending = (bool)pendingField.GetValue(game);
                    bool baseExact = stack.SequenceEqual(baseline) && actual.SequenceEqual(counts);
                    bool onePause = stack.Count == baseline.Length + 1 && stack.Peek().Type == GameModeType.Pause &&
                        stack.Skip(1).SequenceEqual(baseline) && actual.Length == counts.Length &&
                        actual.Select((value, index) => value == counts[index] + (index == (int)GameModeType.Pause ? 1 : 0)).All(value => value);
                    bool safe = ReferenceEquals(fixture, _sprint25Fixture) && !Cleanup2NativeDefaultPending &&
                        Cleanup2NoNarrativeOverlay() && game.SelectedAbilityHandler?.Ability == null &&
                        (baseExact || onePause && (knownPause == null ? callerRequestedPause : ReferenceEquals(knownPause, stack.Peek())));
                    if (rows.Count < 6) rows.Add(new JObject { ["frame"] = Time.frameCount, ["mode"] = game.CurrentMode.ToString(),
                        ["paused"] = game.IsPaused, ["pendingOwnPause"] = pending, ["baseStackAndCountsExact"] = baseExact,
                        ["oneExactPauseAboveBase"] = onePause, ["safe"] = safe });
                    if (!safe)
                    {
                        Cleanup2Check("ettin-owned-pause-refused-" + stage, false, new JObject {
                            ["stage"] = stage, ["samples"] = rows, ["mode"] = game.CurrentMode.ToString(),
                            ["selectedAbility"] = game.SelectedAbilityHandler?.Ability?.Blueprint.AssetGuid },
                            "An unexpected stack/count/input or narrative state refuses native unpause and preserves the original failure evidence.");
                        throw new InvalidOperationException("Ettin owned pause boundary changed; foreign mode/input is retained.");
                    }
                    if (onePause && knownPause == null) knownPause = stack.Peek();
                    bool immediate = !(bool)typeof(Game).GetField("m_GameModeTicking", Cleanup2Fields).GetValue(game) &&
                        !(bool)typeof(Game).GetField("m_SwitchingModes", Cleanup2Fields).GetValue(game) &&
                        ((System.Collections.ICollection)pendingActions.GetValue(game)).Count == 0;
                    if (!pending && immediate && baseExact && !game.IsPaused)
                    {
                        stopped = true;
                        break;
                    }
                    // Native setter refuses false while m_WillBePaused=true.
                    // Wait for its own queued push, then remove only that exact
                    // top instance through the ordinary native unpause setter.
                    if (!pending && immediate && onePause) game.IsPaused = false;
                    yield return 0;
                }
                Cleanup2Check("ettin-owned-pause-settled-" + stage, stopped, new JObject {
                    ["stage"] = stage, ["samples"] = rows, ["ownedPauseObserved"] = knownPause != null,
                    ["nativeUnpaused"] = !game.IsPaused, ["mode"] = game.CurrentMode.ToString(),
                    ["manualPulsesSuspendedDuringSettlement"] = true,
                    ["contract"] = "Native set_IsPaused ignores false while m_WillBePaused. Exact fixture pause settles and native unpause restores previous stack/count references before strict Default admission." },
                    "Only the exact request fixture's setup/join-combat Pause is settled and removed; foreign overlays/inputs and native timing remain untouched.");
                if (!stopped) throw new InvalidOperationException("Owned Ettin fixture pause did not settle within five seconds.");
            }
            finally { _sprint25WaitingForUnityAppearance = waiting; }
        }
    }
}
