using System;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Executes the request's steps one per frame-poll: optionally after loading a save from the main menu, otherwise at
    /// the idle main menu. Stage and load handling adapted from the Kingmaker Cutscene Skip lab harness.
    /// </summary>
    internal sealed class HarnessRunner
    {
        private enum Stage { WaitMainMenu, LoadingSave, Settling, Steps, Finished }

        private enum StepStatus { Running, Done, Failed }

        private const long LoadMarkerMoney = 1234;

        private readonly HarnessRequest request;
        private readonly Evidence evidence;
        private readonly JObject result;
        private readonly double startedAt;
        private Stage stage = Stage.WaitMainMenu;
        private double stageStartedAt;
        private int stepIndex;
        private bool stepBegun;
        private double stepStartedAt;
        private string stepFailure;
        private int quitCountdown = -1;
        private PanelProbe panelProbe;
        private LoadObserver loadObserver;
        private bool saveRequested;
        private bool saveCompleted;
        private long savedMoney;
        private string lastSaveLabel;
        private string craftedGuid;

        public HarnessRunner(HarnessRequest request, Evidence evidence, JObject result)
        {
            this.request = request;
            this.evidence = evidence;
            this.result = result;
            startedAt = Now;
            stageStartedAt = startedAt;
            result["steps"] = new JArray();
        }

        private static double Now => Time.realtimeSinceStartup;

        public void Tick()
        {
            if (quitCountdown >= 0)
            {
                if (quitCountdown-- == 0) Application.Quit();
                return;
            }

            if (stage == Stage.Finished) return;
            if (Now - startedAt > request.TimeoutSeconds)
            {
                Finish(false, "run timeout after " + request.TimeoutSeconds + "s at stage " + stage + " step " + stepIndex);
                return;
            }

            switch (stage)
            {
                case Stage.WaitMainMenu:
                    TickWaitMainMenu();
                    break;
                case Stage.LoadingSave:
                    if (!LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.CurrentlyLoadedArea != null &&
                        Game.Instance.Player != null && Game.Instance.CurrentMode != Kingmaker.GameModes.GameModeType.None)
                    {
                        Enter(Stage.Settling);
                        evidence.Event("saveLoaded", Game.Instance.CurrentlyLoadedArea.name);
                    }

                    break;
                case Stage.Settling:
                    if (Now - stageStartedAt >= 2.0) Enter(Stage.Steps);
                    break;
                case Stage.Steps:
                    TickSteps();
                    break;
            }
        }

        private void Enter(Stage next)
        {
            stage = next;
            stageStartedAt = Now;
        }

        private void TickWaitMainMenu()
        {
            MainMenu menu = UnityEngine.Object.FindObjectOfType<MainMenu>();
            if (menu == null || LoadingProcess.Instance.IsLoadingInProcess || Now - stageStartedAt < 3.0) return;
            evidence.WriteJson("loader-identity.json", LoaderIdentity.Capture());
            evidence.Event("mainMenu", new JObject { ["ummWindowOpen"] = UnityModManager.UI.Instance != null && UnityModManager.UI.Instance.Opened });
            UnityModManager.UI.Instance?.ToggleWindow(false);
            if (string.IsNullOrEmpty(request.Save))
            {
                Enter(Stage.Steps);
                return;
            }

            string path = Path.Combine(Game.Instance.SaveManager.SavePath, request.Save);
            SaveInfo save = Game.Instance.SaveManager.LoadZipSave(path);
            if (save == null)
            {
                Finish(false, "fixture save could not be read: " + path);
                return;
            }

            evidence.Event("loadSave", request.Save);
            menu.LoadGame(save);
            Enter(Stage.LoadingSave);
        }

        private void TickSteps()
        {
            if (stepIndex >= request.Steps.Count)
            {
                Finish(true, null);
                return;
            }

            HarnessStep step = request.Steps[stepIndex];
            if (!stepBegun)
            {
                stepBegun = true;
                stepStartedAt = Now;
                stepFailure = null;
                loadObserver = null;
                saveRequested = false;
                evidence.Event("stepBegin", step.ToString());
            }

            StepStatus status;
            try
            {
                status = Poll(step);
            }
            catch (Exception exception)
            {
                stepFailure = (exception is System.Reflection.TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception).ToString();
                status = StepStatus.Failed;
            }

            if (status == StepStatus.Running && Now - stepStartedAt > step.TimeoutSeconds)
            {
                stepFailure = "step timeout after " + step.TimeoutSeconds + "s";
                status = StepStatus.Failed;
            }

            if (status == StepStatus.Running) return;
            ((JArray)result["steps"]).Add(new JObject { ["step"] = step.ToString(), ["status"] = status.ToString(), ["failure"] = stepFailure });
            if (status == StepStatus.Failed)
            {
                evidence.Event("stepFailed", step + ": " + stepFailure);
                Finish(false, step + ": " + stepFailure);
                return;
            }

            evidence.Event("stepDone", step.ToString());
            stepIndex++;
            stepBegun = false;
        }

        private StepStatus Fail(string message)
        {
            stepFailure = message;
            return StepStatus.Failed;
        }

        private string[] List(string value) => (value ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();

        private StepStatus Poll(HarnessStep step)
        {
            Game game = Game.Instance;
            switch (step.Op)
            {
                case "wait":
                    return Now - stepStartedAt >= step.Seconds ? StepStatus.Done : StepStatus.Running;

                case "identity":
                    evidence.WriteJson("loader-identity-" + (step.Label ?? stepIndex.ToString()) + ".json", LoaderIdentity.Capture());
                    return StepStatus.Done;

                case "expectMods":
                {
                    // Every listed mod is active, loaded and error-free; every "expect" mod is absent or inactive.
                    foreach (string id in List(step.Match))
                    {
                        UnityModManager.ModEntry entry = UnityModManager.modEntries.SingleOrDefault(e => e.Info?.Id == id);
                        if (entry == null || !entry.Active || !entry.Loaded || entry.ErrorOnLoading)
                            return Fail("mod " + id + " is not active and loaded without error" + (entry == null ? " (absent)" :
                                ": active=" + entry.Active + " loaded=" + entry.Loaded + " error=" + entry.ErrorOnLoading));
                    }

                    foreach (string id in List(step.Expect))
                    {
                        if (UnityModManager.modEntries.Any(e => e.Info?.Id == id && e.Active))
                            return Fail("mod " + id + " must be absent but is active");
                    }

                    string[] errors = UnityModManager.modEntries.Where(e => e.ErrorOnLoading).Select(e => e.Info?.Id).ToArray();
                    result["modsWithLoadErrors"] = new JArray(errors.Cast<object>().ToArray());
                    return errors.Length == 0 ? StepStatus.Done : Fail("mods with load errors: " + string.Join(", ", errors));
                }

                case "kmgBootstrap":
                    result["kmgBootstrap"] = ProductProbes.Bootstrap(List(step.Match), List(step.Type));
                    return StepStatus.Done;

                case "kmgModules":
                {
                    JObject state = ProductProbes.ModuleState();
                    result["kmgModules_" + (step.Label ?? stepIndex.ToString())] = state;
                    ProductProbes.AssertActiveModules(state, step.Match ?? string.Empty);
                    if (!string.IsNullOrEmpty(step.Expect))
                    {
                        // The persisted file must hold exactly these values for the named module ids.
                        JObject file = state["file"] as JObject;
                        if (file == null) return Fail("FeatureModules.json was not found");
                        string text = file.ToString();
                        foreach (string pair in List(step.Expect))
                        {
                            string[] parts = pair.Split('=');
                            JToken value = file.SelectTokens("$..['" + parts[0].Trim() + "']").FirstOrDefault();
                            if (value == null || value.Type != JTokenType.Boolean || (bool)value != bool.Parse(parts[1].Trim()))
                                return Fail("FeatureModules.json " + parts[0] + " is " + (value?.ToString() ?? "absent") + ", expected " + parts[1] + ": " + text);
                        }
                    }

                    return StepStatus.Done;
                }

                case "ummPanel":
                    // Opens the loader window and draws the named mod's OnGUI on real IMGUI passes. With "type", one
                    // toggle of that label is clicked; with "save", the loader's own SaveSettingsAndParams (the window's
                    // Save button) then dispatches every mod's OnSaveGUI.
                    if (panelProbe == null)
                    {
                        LoggedExceptions.Arm();
                        if (!string.IsNullOrEmpty(step.Type)) ToggleClick.Arm(step.Type);
                        UnityModManager.UI.Instance?.ToggleWindow(true);
                        GameObject holder = new GameObject("KmgLoaderCompatPanelProbe");
                        UnityEngine.Object.DontDestroyOnLoad(holder);
                        panelProbe = holder.AddComponent<PanelProbe>();
                        panelProbe.ModId = step.Match ?? ProductProbes.ProductId;
                        return StepStatus.Running;
                    }

                    // At least the requested time AND 30 real repaints (or a failure), bounded by the step timeout:
                    // a long frame elsewhere must not turn a fixed window into a vacuous pass or a false failure.
                    if ((Now - stepStartedAt < (step.Seconds > 0 ? step.Seconds : 2.0) || panelProbe.Repaints < 30) &&
                        panelProbe.Failure == null && LoggedExceptions.First == null) return StepStatus.Running;
                    {
                        PanelProbe probe = panelProbe;
                        panelProbe = null;
                        string failure = probe.Failure ?? LoggedExceptions.First;
                        int draws = probe.Draws, repaints = probe.Repaints;
                        UnityEngine.Object.Destroy(probe.gameObject);
                        if (step.Save && failure == null) UnityModManager.SaveSettingsAndParams();
                        failure = failure ?? LoggedExceptions.First;
                        UnityModManager.UI.Instance?.ToggleWindow(false);
                        LoggedExceptions.Disarm();
                        JObject panel = new JObject
                        {
                            ["mod"] = probe.ModId, ["draws"] = draws, ["repaints"] = repaints, ["onGuiCalls"] = probe.Calls,
                            ["exitGui"] = probe.ExitGui, ["gameMode"] = Game.Instance?.CurrentMode.ToString(), ["clicked"] = ToggleClick.Clicked,
                            ["savedThroughLoader"] = step.Save, ["failure"] = failure
                        };
                        result["ummPanel_" + (step.Label ?? stepIndex.ToString())] = panel;
                        if (failure != null) return Fail("panel failed: " + failure);
                        if (repaints < 30) return Fail("panel completed only " + repaints + " repaints");
                        if (!string.IsNullOrEmpty(step.Type) && ToggleClick.Clicked == null) return Fail("toggle '" + step.Type + "' was never drawn");
                        return StepStatus.Done;
                    }

                case "giveWeapon":
                    result["given_" + (step.Label ?? stepIndex.ToString())] = CraftOps.Describe(CraftOps.GiveWeapon(step.Match));
                    return StepStatus.Done;

                case "expectWeapon":
                {
                    // A firearm of this blueprint is in the inventory and carries exactly one item-owned state token.
                    Kingmaker.Items.ItemEntityWeapon weapon = game.Player.Inventory.OfType<Kingmaker.Items.ItemEntityWeapon>()
                        .LastOrDefault(item => item.Blueprint != null && item.Blueprint.AssetGuid == step.Match);
                    if (weapon == null) return Fail("no " + step.Match + " in the inventory");
                    JObject observed = CraftOps.Describe(weapon);
                    result["weapon_" + (step.Label ?? stepIndex.ToString())] = observed;
                    CraftOps.AssertTokens(observed, step.Expect);
                    return StepStatus.Done;
                }

                case "setStateToken":
                {
                    // Puts the newest inventory copy of a firearm into a non-default item-owned state exactly as the
                    // product's token store does for an inventory-only item (no wielder: no parent context).
                    Kingmaker.Items.ItemEntityWeapon weapon = game.Player.Inventory.OfType<Kingmaker.Items.ItemEntityWeapon>()
                        .LastOrDefault(item => item.Blueprint != null && item.Blueprint.AssetGuid == step.Match);
                    if (weapon == null) return Fail("no " + step.Match + " in the inventory");
                    if (weapon.Wielder != null) return Fail("the firearm is wielded; this op covers inventory-only state");
                    var token = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>(step.Type);
                    if (token == null) return Fail("no state token blueprint " + step.Type);
                    if (weapon.AddEnchantment(token, null, null) == null) return Fail("Kingmaker returned no enchantment for " + step.Type);
                    result["stateSet_" + (step.Label ?? stepIndex.ToString())] = CraftOps.Describe(weapon);
                    return StepStatus.Done;
                }

                case "probeBlueprint":
                {
                    // A blueprint member's value, for mods whose effect is a changed blueprint (Kingmaker Bugfixes).
                    const System.Reflection.BindingFlags any = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                    // "match" is "<blueprint class>:<blueprint name>", as verified in the Cutscene Skip lab.
                    string[] key = (step.Match ?? string.Empty).Split(new[] { ':' }, 2);
                    if (key.Length != 2) return Fail("probeBlueprint needs match=<type>:<name>");
                    Kingmaker.Blueprints.BlueprintScriptableObject[] found = Kingmaker.Blueprints.ResourcesLibrary.GetBlueprints<Kingmaker.Blueprints.BlueprintScriptableObject>()
                        .Where(candidate => candidate != null && candidate.GetType().Name == key[0] && candidate.name == key[1]).ToArray();
                    if (found.Length != 1) return Fail(found.Length + " blueprints match " + step.Match);
                    Kingmaker.Blueprints.BlueprintScriptableObject blueprint = found[0];
                    object value = blueprint.GetType().GetProperty(step.Type, any)?.GetValue(blueprint, null) ?? blueprint.GetType().GetField(step.Type, any)?.GetValue(blueprint);
                    string text = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
                    result["probe_" + (step.Label ?? stepIndex.ToString())] = blueprint.name + "." + step.Type + "=" + text;
                    return step.Expect == null || string.Equals(step.Expect, text, StringComparison.OrdinalIgnoreCase)
                        ? StepStatus.Done : Fail(blueprint.name + "." + step.Type + " is " + text + ", expected " + step.Expect);
                }

                case "cmiUpgrade":
                    craftedGuid = CraftOps.Upgrade(step.Match, step.Type, result);
                    result["cmiAfterCraft"] = CraftOps.Verify(craftedGuid, step.Type, step.Match, step.Expect);
                    return StepStatus.Done;

                case "cmiVerify":
                {
                    string guid = craftedGuid ?? CraftOps.CustomGuid(step.Match, CraftOps.Enchantment(step.Type).AssetGuid);
                    result["cmiVerified_" + (step.Label ?? stepIndex.ToString())] = CraftOps.Verify(guid, step.Type, step.Match, step.Expect);
                    return StepStatus.Done;
                }

                case "saveGame":
                    if (!saveRequested)
                    {
                        saveRequested = true;
                        saveCompleted = false;
                        savedMoney = game.Player.Money;
                        lastSaveLabel = step.Label;
                        SaveInfo created = game.SaveManager.CreateNewSave(step.Label);
                        result["savedGame"] = created.FolderName;
                        game.SaveGame(created, () => saveCompleted = true);
                        return StepStatus.Running;
                    }

                    return saveCompleted && !game.SaveManager.CommitInProgress && !LoadingProcess.Instance.IsLoadingInProcess
                        ? StepStatus.Done : StepStatus.Running;

                case "loadLastSave":
                    if (loadObserver == null)
                    {
                        // Load the file the game wrote (the CreateNewSave object is never marked saved), after an
                        // in-memory money marker that only a real load can undo.
                        string written = Directory.GetFiles(game.SaveManager.SavePath, "*_" + lastSaveLabel + ".zks")
                            .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
                        if (written == null) return Fail("the file for save " + lastSaveLabel + " was not found");
                        SaveInfo info = game.SaveManager.LoadZipSave(written);
                        if (info == null) return Fail("the file could not be read: " + written);
                        loadObserver = new LoadObserver(Now, game.Player, savedMoney);
                        game.Player.GainMoney(LoadMarkerMoney);
                        evidence.Event("loadLastSave", Path.GetFileName(written));
                        game.LoadGame(info);
                        return StepStatus.Running;
                    }

                    return PollLoad(game);

                case "expectLoadedFrom":
                {
                    // Fresh process: the save loaded from the main menu is the one the previous process wrote.
                    string name = request.Save ?? string.Empty;
                    result["loadedFromSave"] = name;
                    return name.EndsWith("_" + step.Match + ".zks", StringComparison.Ordinal) ? StepStatus.Done : Fail("loaded " + name);
                }

                default:
                    return Fail("unknown op " + step.Op);
            }
        }

        private StepStatus PollLoad(Game game)
        {
            LoadProgress progress = loadObserver.Observe(Now, LoadingProcess.Instance.IsLoadingInProcess, game.Player,
                game.CurrentlyLoadedArea != null, game.CurrentMode != Kingmaker.GameModes.GameModeType.None, game.Player?.Money ?? -1);
            if (progress == LoadProgress.Waiting) return StepStatus.Running;
            result["load_" + stepIndex] = new JObject
            {
                ["began"] = loadObserver.Began, ["playerReplaced"] = loadObserver.PlayerReplaced, ["money"] = loadObserver.LastMoney,
                ["verdict"] = progress.ToString()
            };
            return progress == LoadProgress.Complete ? StepStatus.Done : Fail(loadObserver.Describe(progress));
        }

        public void Finish(bool passed, string failure)
        {
            if (stage == Stage.Finished) return;
            stage = Stage.Finished;
            result["status"] = passed ? "passed" : "failed";
            result["failure"] = failure;
            result["finishedUtc"] = DateTime.UtcNow.ToString("o");
            result["exitRequested"] = request.ExitAfterCompletion;
            evidence.WriteJson("harness-result.json", result);
            if (request.ExitAfterCompletion) quitCountdown = 30;
        }
    }
}
