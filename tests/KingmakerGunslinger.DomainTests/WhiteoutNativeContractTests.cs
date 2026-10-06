using System;
using System.IO;
using System.Text.RegularExpressions;
namespace KingmakerGunslinger.DomainTests
{
    // Read-only installed-engine contracts. These tests establish neither a
    // reversible scene fixture nor attack-patch/gameplay qualification.
    internal static class WhiteoutNativeContractTests
    {
        private static string Native(string method, int occurrence = 0)
        {
            string s=LungeEngineContractTests.Native(method, occurrence);
            // Native coroutine/WeatherData members are nested and indented.
            // Restrict every assertion to the last exact method body.
            var starts=Regex.Matches(s,@"(?m)^ +\.method ");
            Assertions.True(starts.Count>0,"Exact native method header: "+method);
            return s.Substring(starts[starts.Count-1].Index);
        }
        internal static void ActualWeatherCarrier()
        {
            string s=Native("WeatherData::get_ActualWeather");
            Assertions.True(s.Contains("WeatherSystemBehaviour::WeatherType") && s.Contains("WeatherSystemBehaviour::RainIntensity") && s.Contains("WeatherSystemBehaviour::SnowIntensity"), "ActualWeather uses the existing visual singleton and exact precipitation fields.");
            Assertions.False(s.Contains("NextWeatherChange") || s.Contains("CurrentWeather"), "Changing saved schedule is unnecessary for ActualWeather.");
        }
        internal static void ExactThresholds()
        {
            string s=Native("WeatherData::get_ActualWeather");
            Assertions.True(s.Contains("RainIntensitites") && s.Contains("SnowIntensitites") && s.Contains("9.9999998e-003") && s.Contains("ldc.i4.4"), "Native five-level threshold mapping and epsilon are explicit.");
        }
        internal static void NativeNotificationRoute()
        {
            string s=Native("WeatherController::OnUpdateWeatherSystem");
            Assertions.True(s.Contains("IWeatherChangeHandler") && s.Contains("EventBus::RaiseEvent") && s.Contains("m_Overridden") && s.Contains("m_SeasonData"), "Native update dispatch owns the two exact controller values a future fixture must restore.");
            Assertions.False(s.Contains("NextWeatherChange") || s.Contains("StartCoroutine") || s.Contains("::Tick("), "Native notification alone neither advances saved weather nor starts transitions.");
        }
        internal static void ControllerIntensitySetter()
        {
            string s=Native("WeatherController::set_Intensity");
            Assertions.True(s.Contains("WeatherSystemBehaviour::RainIntensity") && s.Contains("WeatherSystemBehaviour::SnowIntensity"), "Native setter uses the exact captured float fields.");
            string accessor=Native("Game::GetController");
            Assertions.True(accessor.Contains("includeInactive") && accessor.Contains("m_GameModes") && accessor.Contains("GameMode::GetController") && !accessor.Contains("newobj"), "Native accessor returns an existing paused mode-owned controller, without replacement.");
        }
        internal static void PointIndoorPredicate()
        {
            string s=Native("LocalMapArea::IsIndoor");
            Assertions.True(s.Contains("LocalMapArea::GetClosest") && s.Contains("BlueprintAreaPart::get_IsIndoor"), "Loaded-point indoor status comes from the exact native area part.");
            Assertions.True(s.Contains("IL_0010:  ldc.i4.0") && s.Contains("IL_0020:  ldc.i4.0") && s.Contains("brfalse.s"), "A missing-map false result is not qualified outdoor evidence.");
        }
        internal static void ExistingConditionHazard()
        {
            string s=Native("UnitPartPartyWeatherBuff::OnWeatherChange");
            Assertions.True(s.Contains("RemoveFact") && s.Contains("AddBuff") && s.Contains("m_LastBuff"), "Native weather dispatch can replace exact existing condition identities.");
            Assertions.True(Native("AddBuffInBadWeather::OnWeatherChange").Contains("xor"), "Conditional weather grants use the exact WhenCalmer/threshold predicate, not a broad descriptor assumption.");
        }
        internal static void SynchronousNotificationContract()
        {
            string s=Native("WeatherController::OnUpdateWeatherSystem");
            Assertions.True(s.Contains("Code size       105") && s.Contains("::set_Intensity") && s.Contains("brtrue"), "overrideWeather=true skips the intensity setter but retains exact native event dispatch.");
            Assertions.False(s.Contains("WeatherSystemBehaviour::Update") || s.Contains("WeatherSystemBehaviour::Init") || s.Contains("YieldInstruction"), "Synchronous notification does not itself tick the visual scene.");
        }
        internal static void ClosedAreaRoute()
        {
            string s=File.ReadAllText("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.WhiteoutWeather.cs");
            Assertions.True(s.Contains("WordOfRecallDestinationPolicy.OlegId") && s.Contains("AutoSaveMode.None") && s.Contains("typeof(SaveInfo)"), "The inherited read-only observer has only its existing exact no-save Oleg route; this test does not requalify it against the stricter mission.");
        }
        internal static void NoScheduleWriterInNotification()
        {
            string s=Native("WeatherController::OnUpdateWeatherSystem");
            Assertions.False(s.Contains("stfld      valuetype Kingmaker.Controllers.InclemencyType Kingmaker.Player/WeatherData::CurrentWeather") || s.Contains("NextWeatherChange"), "Exact native override notification has no saved schedule writer.");
            Assertions.True(Native("WeatherController::Tick").Contains("NextWeatherChange"), "The periodic controller remains excluded because it can advance the schedule.");
        }
        internal static void NativeAttackStage()
        {
            string s=Native("RuleAttackRoll::TryOvercomeTargetConcealmentAndMissChance");
            Assertions.True(s.Contains("instance bool") && s.Contains("Code size       149") && s.Contains("RuleConcealmentCheck::get_Success") && s.Contains("get_IgnoreConcealment") && s.Contains("get_MissChance"), "Exact private zero-argument success stage exists; inspection is not gameplay qualification.");
        }
        internal static void StageRemainsUnbound()
        {
            string bootstrap=File.ReadAllText("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs");
            string policy=File.ReadAllText("src/KingmakerGunslinger/ElementalRaces/WhiteoutPolicy.cs");
            Assertions.False(bootstrap.Contains("Whiteout"), "No Whiteout bootstrap, trait or acquisition path is installed.");
            Assertions.False(policy.Contains("HarmonyPatch") || policy.Contains("BlueprintBuff") || policy.Contains("LocalizedString"), "Retained Whiteout policy creates no marker, patch, localization or icon consumer.");
            Assertions.False(File.Exists("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.WhiteoutNativeWeatherFixture.cs"), "The rejected weather writer is absent from final source.");
        }
        internal static void NativeAttackOrdering()
        {
            string s=Native("RuleAttackRoll::OnTrigger");
            int stage=s.IndexOf("RuleAttackRoll::TryOvercomeTargetConcealmentAndMissChance",StringComparison.Ordinal);
            int ac=s.IndexOf("RuleCalculateAC::.ctor",StringComparison.Ordinal);
            int mirror=s.IndexOf("UnitPartMirrorImage::TryAbsorbHit",StringComparison.Ordinal);
            Assertions.True(stage>0 && ac>stage && mirror>ac && s.Contains("brfalse    IL_0464"), "Native stage failure precedes AC/hit/mirror-image resolution and has its own miss branch.");
        }
        internal static void CrossAreaStashDisposesOriginals()
        {
            string unload=Native("'<UnloadAreaCoroutine>d__35'::MoveNext");
            string stash=Native("AreaDataStash::StashAreaState");
            Assertions.True(unload.Contains("AreaDataStash::StashAreaState") && stash.Contains("AreaPersistentState::Dispose"), "Native cross-area unload stashes then disposes original scene objects.");
            Assertions.True(Native("SceneEntitiesState::RemoveEntityData").Contains("EntityDataBase::Dispose"), "Removal disposes an original entity rather than reversibly hiding it.");
        }
        internal static void ReloadDeserializesReplacementObjects()
        {
            string load=Native("'<LoadAreaCoroutine>d__24'::MoveNext");
            string unstash=Native("AreaDataStash::UnstashAreaState");
            Assertions.True(load.Contains("AreaDataStash::UnstashAreaState") && unstash.Contains("DeserializeAreaJson") && unstash.Contains("SetDeserializedSceneState"), "Reload rehydrates scene objects; equal unit IDs do not imply exact reference restoration.");
            Assertions.True(Native("EntityPool`1::get_All").Contains("HashSet"), "Native unit pool is unordered; the blocker concerns replaced references, not enumeration order.");
        }
        internal static void OriginalFactCollectionsAreDisposed()
        {
            Assertions.True(Native("UnitEntityData::Dispose").Contains("UnitDescriptor::Dispose") && Native("UnitDescriptor::Dispose").Contains("FactCollection::Dispose"), "Native unit disposal irreversibly disposes original facts and subscriptions.");
            Assertions.True(Native("BuffCollection::Dispose").Contains("FactCollection::Dispose"), "Re-granting a matching blueprint would not restore the original buff identity.");
        }
    }
}
