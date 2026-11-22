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
        private ChargenClassScreenProbe chargenProbe;
        private BuffUiProbe buffProbe;
        private OpportunityBuffUiProbe opportunityProbe;
        private TimedBuffProbe timedBuffProbe;
        private readonly ProbeSlot probes = new ProbeSlot();
        private readonly System.Collections.Generic.List<string> deferredFailures = new System.Collections.Generic.List<string>();

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
                Finish(deferredFailures.Count == 0, deferredFailures.Count == 0 ? null : string.Join(" || ", deferredFailures.ToArray()));
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

            // The step is over, however it ended: by its own verdict, by timeout or by an exception. Whatever it
            // started is ended here, under this step's identity, before another step is considered.
            bool isolationRestored = EndStepProbes(step, ref status);
            string expectedFailure = null;
            if (!string.IsNullOrEmpty(step.ExpectFailure))
            {
                if (status == StepStatus.Failed && isolationRestored && (stepFailure ?? string.Empty).IndexOf(step.ExpectFailure, StringComparison.Ordinal) >= 0)
                {
                    expectedFailure = stepFailure;
                    stepFailure = null;
                    status = StepStatus.Done;
                }
                else if (status == StepStatus.Done)
                {
                    stepFailure = "the expected failure '" + step.ExpectFailure + "' did not occur";
                    status = StepStatus.Failed;
                }
            }

            ((JArray)result["steps"]).Add(new JObject
            {
                ["step"] = step.ToString(), ["status"] = status.ToString(), ["failure"] = stepFailure, ["expectedFailure"] = expectedFailure,
                ["isolationRestored"] = isolationRestored
            });
            if (status == StepStatus.Failed)
            {
                evidence.Event("stepFailed", step + ": " + stepFailure);
                deferredFailures.Add(step + ": " + stepFailure);
            }
            else
            {
                evidence.Event("stepDone", step.ToString());
            }

            if (ProbeSlot.Decide(status == StepStatus.Failed, step.ContinueOnFailure, isolationRestored) == StepContinuation.StopLaunch)
            {
                if (!isolationRestored)
                {
                    evidence.Event("isolationNotRestored", step.ToString());
                    deferredFailures.Add(step + ": the launch stops here; what the step's probe owned could not all be released");
                }

                Finish(false, string.Join(" || ", deferredFailures.ToArray()));
                return;
            }

            stepIndex++;
            stepBegun = false;
        }

        /// <summary>The identity a step's probe and its result are kept under: the step's position and its result key.</summary>
        private string StepIdentity(HarnessStep step)
        {
            return stepIndex + "|" + step.Op + "_" + (step.Label ?? stepIndex.ToString());
        }

        /// <summary>Writes a probe's record and how it ended under the step that started it, never under the current one.</summary>
        private void RecordProbe(ProbeOutcome outcome)
        {
            JObject recorded = outcome.Result as JObject ?? new JObject();
            recorded["termination"] = new JObject
            {
                ["step"] = outcome.Owner, ["testFailure"] = outcome.TestFailure, ["passed"] = outcome.Passed,
                ["isolationRestored"] = outcome.IsolationRestored,
                ["cleanupAttempted"] = new JArray(outcome.Cleanup.Attempted.Cast<object>().ToArray()),
                ["cleanupFailures"] = new JArray(outcome.Cleanup.Failures.Cast<object>().ToArray())
            };
            result[outcome.Owner.Substring(outcome.Owner.IndexOf('|') + 1)] = recorded;
        }

        /// <summary>
        /// Ends everything the finished step started. Idempotent for a probe that already reported its verdict. Returns
        /// whether the game is back to what it was before the step's probe.
        /// </summary>
        private bool EndStepProbes(HarnessStep step, ref StepStatus status)
        {
            if (status == StepStatus.Failed)
            {
                // These probes clear themselves when they report a verdict. One left behind by a timeout or an
                // exception must not be picked up by a later step.
                chargenProbe = null;
                buffProbe = null;
                opportunityProbe = null;
                timedBuffProbe = null;
            }

            ProbeOutcome ended = probes.Terminate(StepIdentity(step), stepFailure ?? "the step ended without its probe's verdict");
            if (ended != null)
            {
                RecordProbe(ended);
                if (!ended.Passed)
                {
                    stepFailure = ended.Failure;
                    status = StepStatus.Failed;
                }
            }

            // Not reachable while every step ends its own probe; kept so that an active probe is never left behind.
            ProbeOutcome orphan = probes.Abandon("left active when step " + StepIdentity(step) + " ended");
            if (orphan != null)
            {
                RecordProbe(orphan);
                stepFailure = (stepFailure == null ? string.Empty : stepFailure + "; ") + "a probe of step " + orphan.Owner + " was still active";
                status = StepStatus.Failed;
            }

            return probes.IsolationRestored;
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

                case "kmgInitialization":
                {
                    // The product's own state after its one initialization attempt: expect=completed, or expect=failed
                    // with "match" naming text the retained failure chain must contain.
                    JObject state = PresentationProbes.InitializationState();
                    state["environment"] = PresentationProbes.Environment();
                    result["kmgInitialization_" + (step.Label ?? stepIndex.ToString())] = state;
                    string problem = PresentationProbes.AssertInitialization(state, step.Expect, step.Match);
                    // "type" names the locale this launch must actually be running in.
                    string locale = (string)state["environment"]["currentLocale"];
                    if (problem == null && !string.IsNullOrEmpty(step.Type) && !string.Equals(locale, step.Type, StringComparison.Ordinal))
                        problem = "the active locale is " + locale + ", expected " + step.Type;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgClassCatalog":
                {
                    // "match" is a character-class guid; "expect" is how often the native class catalog must hold it.
                    JObject catalog = PresentationProbes.ClassCatalog(step.Match);
                    result["kmgClassCatalog_" + (step.Label ?? stepIndex.ToString())] = catalog;
                    int wanted = int.Parse(step.Expect, System.Globalization.CultureInfo.InvariantCulture);
                    if ((int)catalog["publishedCount"] != wanted) return Fail("class " + step.Match + " is published " + catalog["publishedCount"] + " times, expected " + wanted);
                    if ((bool)catalog["blueprintResolves"] != (wanted > 0)) return Fail("class " + step.Match + " blueprint resolves=" + catalog["blueprintResolves"]);
                    return StepStatus.Done;
                }

                case "kmgPresentationCensus":
                {
                    // "match" is the class guid; "type" lists unit facts that must be reachable, visible, carry assigned
                    // name and description text and an icon. No visible project fact may rely on placeholder text.
                    JObject census = PresentationProbes.Census(step.Match, List(step.Type), step.Hidden, step.Invariants, out string problem);
                    evidence.WriteJson("presentation-census-" + (step.Label ?? stepIndex.ToString()) + ".json", census);
                    JObject summary = new JObject();
                    foreach (string name in new[] { "environment", "class", "visitedFacts", "projectFacts", "visibleProjectFacts", "hiddenProjectFacts", "offenderCount", "offenders", "required" })
                        if (census[name] != null) summary[name] = census[name];
                    result["kmgPresentationCensus_" + (step.Label ?? stepIndex.ToString())] = summary;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgPresentationContract":
                {
                    JObject contract = PresentationProbes.PresentationContract(out string problem);
                    result["kmgPresentationContract_" + (step.Label ?? stepIndex.ToString())] = contract;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgChargenClassScreen":
                {
                    // "match" is the class guid; "contains" lists text its drawn label must contain. Runs at the main
                    // menu: opens the real new-character creator, reads its class list, clicks the class, presses Next.
                    if (chargenProbe == null) chargenProbe = new ChargenClassScreenProbe(evidence.Directory, step.Label ?? stepIndex.ToString(), step.Match, step.Contains);
                    string outcome;
                    try { outcome = chargenProbe.Poll(); }
                    catch (Exception exception) { outcome = exception.ToString(); }
                    if (outcome == null) return StepStatus.Running;
                    result["kmgChargenClassScreen_" + (step.Label ?? stepIndex.ToString())] = chargenProbe.Result;
                    chargenProbe = null;
                    return outcome.Length == 0 ? StepStatus.Done : Fail(outcome);
                }

                case "kmgBuffUi":
                {
                    // "type" lists buff guids applied to the loaded save's main character; "contains" lists the names
                    // the character sheet must draw; "expect" names one of them that must end by itself at the next
                    // round. Reads the party bar and the sheet's Buffs and Conditions rows.
                    if (buffProbe == null) buffProbe = new BuffUiProbe(evidence.Directory, step.Label ?? stepIndex.ToString(), List(step.Type), step.Contains, step.Expect);
                    string outcome;
                    try { outcome = buffProbe.Poll(); }
                    catch (Exception exception) { outcome = exception.ToString(); }
                    if (outcome == null) return StepStatus.Running;
                    result["kmgBuffUi_" + (step.Label ?? stepIndex.ToString())] = buffProbe.Result;
                    buffProbe = null;
                    return outcome.Length == 0 ? StepStatus.Done : Fail(outcome);
                }

                case "kmgOpportunityBuffUi":
                {
                    // "type" is the weapon the loaded save's main character is armed with; "match" is the buff its
                    // attack-of-opportunity hit must grant; "expect" names the icon that buff must show; "contains"
                    // lists text the buff's tooltip must draw. Reads the portrait's buff slot and the tooltip it opens.
                    if (opportunityProbe == null) opportunityProbe = new OpportunityBuffUiProbe(evidence.Directory, step.Label ?? stepIndex.ToString(), step.Type, step.Match, step.Expect, step.Contains);
                    string outcome;
                    try { outcome = opportunityProbe.Poll(); }
                    catch (Exception exception) { outcome = exception.ToString(); }
                    if (outcome == null) return StepStatus.Running;
                    result["kmgOpportunityBuffUi_" + (step.Label ?? stepIndex.ToString())] = opportunityProbe.Result;
                    opportunityProbe = null;
                    return outcome.Length == 0 ? StepStatus.Done : Fail(outcome);
                }

                case "kmgTimedBuffs":
                {
                    // "type" lists buff guids put on the loaded save's main character; "match" is their duration in
                    // seconds; "expect", when given, names the icon every visible one must draw. Each must activate
                    // fully, show in the portrait with its icon only if it is visible, and end by itself.
                    // "invariants": { "whileActive": "unchanged" } adds, for buffs that must be inert, that armor class,
                    // speed, Reflex and Initiative are the same while the buffs are on as before them.
                    if (timedBuffProbe == null)
                    {
                        string whileActive;
                        timedBuffProbe = new TimedBuffProbe(evidence.Directory, step.Label ?? stepIndex.ToString(), List(step.Type),
                            double.Parse(step.Match, System.Globalization.CultureInfo.InvariantCulture), step.Expect,
                            step.Invariants != null && step.Invariants.TryGetValue("whileActive", out whileActive) && whileActive == "unchanged");
                    }
                    string outcome;
                    try { outcome = timedBuffProbe.Poll(); }
                    catch (Exception exception) { outcome = exception.ToString(); }
                    if (outcome == null) return StepStatus.Running;
                    result["kmgTimedBuffs_" + (step.Label ?? stepIndex.ToString())] = timedBuffProbe.Result;
                    timedBuffProbe = null;
                    return outcome.Length == 0 ? StepStatus.Done : Fail(outcome);
                }

                case "kmgBuffCensus":
                {
                    // "type" lists buffs that must resolve; "own" lists buffs this stack may not publish. Records what
                    // each final blueprint holds. With "expect": "linked", an absent effect link is a failure.
                    var census = new JObject();
                    System.Collections.Generic.List<string> failures = BuffCensus.Run(List(step.Type), step.Own, step.Expect == "linked", census);
                    result["kmgBuffCensus_" + (step.Label ?? stepIndex.ToString())] = census;
                    return failures.Count == 0 ? StepStatus.Done : Fail(string.Join("; ", failures.ToArray()));
                }

                case "kmgVariantCensus":
                {
                    // "match" lists variant parents. Records every entry under each, in order; changes nothing.
                    var variantCensus = new JObject();
                    System.Collections.Generic.List<string> unreadableParents = VariantCensus.Run(List(step.Match), variantCensus);
                    result["kmgVariantCensus_" + (step.Label ?? stepIndex.ToString())] = variantCensus;
                    return unreadableParents.Count == 0 ? StepStatus.Done : Fail(string.Join("; ", unreadableParents.ToArray()));
                }

                case "kmgArchetypeCensus":
                {
                    // "match" lists character classes. Records the archetypes each offers, in order; changes nothing.
                    var archetypeCensus = new JObject();
                    System.Collections.Generic.List<string> unreadableClasses = ArchetypeCensus.Run(List(step.Match), archetypeCensus);
                    result["kmgArchetypeCensus_" + (step.Label ?? stepIndex.ToString())] = archetypeCensus;
                    return unreadableClasses.Count == 0 ? StepStatus.Done : Fail(string.Join("; ", unreadableClasses.ToArray()));
                }

                case "kmgDeedLifecycle":
                {
                    // "match" names the script (dodge, armed, bleeds, condition, fallingPetal, mountainSunder);
                    // "invariants" carries the blueprint identities and expected costs that script needs. Each runs
                    // the effect's real trigger on disposable units in the loaded save, with time running.
                    // The probe lives in the slot under this step's identity. The slot ends it when it reports a verdict
                    // or throws; TickSteps ends it when the step times out.
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new DeedLifecycleProbe(evidence.Directory, step.Label ?? stepIndex.ToString(), step.Match, step.Invariants));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgBrownFur":
                {
                    // S6 T4 / RISK-03 (A2): "match" names the mode; invariants.saveLabel names the save it writes.
                    // Needs condition observeBrownFurPersistence on the launch.
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new BrownFurProbe(request.ProfileDir, step.Match, step.Invariants));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgGrapple":
                {
                    // S6 checkpoint C1 / VER-02: "match" is terminals or reload (see GrappleProbes.cs).
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new GrappleProbe(step.Match));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgMultiLink":
                {
                    // S6 checkpoint C2 / VER-02: "match" is multilink or multilinkReload (see MultiLinkProbes.cs).
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new MultiLinkProbe(step.Match));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgSaveSuspension":
                {
                    // S6 checkpoint C2a / VER-02 F1: "match" is reach or reachReload (see SaveSuspensionProbes.cs).
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new SaveSuspensionProbe(step.Match));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgHoldLifecycle":
                {
                    // S6 checkpoint C2a R1 / VER-02 F1: "match" is removal, cadence or afterLoad (see HoldLifecycleProbes.cs).
                    // Needs condition observeHoldLifecycle on the launch.
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new HoldLifecycleProbe(step.Match));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgControlledRage":
                {
                    // S6 T4 / RISK-03: "match" is create, reload or final; invariants.saveLabel names the save create
                    // writes. Needs condition observeControlledRagePersistence on the launch.
                    string identity = StepIdentity(step);
                    if (!probes.IsActive) probes.Begin(identity, new ControlledRageProbe(request.ProfileDir, step.Match, step.Invariants));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgGritUiCensus":
                {
                    // "match" is the grit resource guid; "type" lists the abilities that must carry the shared grit
                    // indicator exactly once; "own" lists those that must keep their own grit resource logic. No
                    // other project ability may carry either. With the observeGritUiPass condition, the product's
                    // first pass must also have replaced exactly the shared recipients' component arrays, and "expect"
                    // is the most project ability descriptions that pass may have read.
                    JObject census = GritUiProbes.Census(step.Match, List(step.Type), step.Own.ToArray(), out string problem);
                    if (!string.IsNullOrEmpty(step.Expect))
                    {
                        int reads = GritUiPass.DescriptionReads.Values.Sum();
                        string excess = !GritUiPass.Installed ? "the grit UI pass was not observed (observeGritUiPass)" :
                            reads > int.Parse(step.Expect, System.Globalization.CultureInfo.InvariantCulture)
                                ? "the grit UI pass read " + reads + " project ability descriptions, at most " + step.Expect + " allowed" : null;
                        if (excess != null) problem = problem == null ? excess : problem + "; " + excess;
                    }
                    evidence.WriteJson("grit-ui-census-" + (step.Label ?? stepIndex.ToString()) + ".json", census);
                    JObject summary = new JObject();
                    foreach (string name in new[] { "gritResolves", "pass", "projectAbilities", "shared", "own", "changedByFirstPass", "unstoredDescriptions" })
                        if (census[name] != null) summary[name] = census[name];
                    result["kmgGritUiCensus_" + (step.Label ?? stepIndex.ToString())] = summary;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgGritUiReapply":
                {
                    // "match" is the grit resource guid; "expect" is the number of shared-indicator recipients the
                    // repeat must return. Runs the product's integration again on the live blueprints and then offers
                    // it malformed recipients; see GritUiProbes.Reapply.
                    if (string.IsNullOrEmpty(step.Expect)) return Fail("kmgGritUiReapply needs expect=<shared recipient count>");
                    JObject reapplied = GritUiProbes.Reapply(step.Match, int.Parse(step.Expect, System.Globalization.CultureInfo.InvariantCulture), out string problem);
                    result["kmgGritUiReapply_" + (step.Label ?? stepIndex.ToString())] = reapplied;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgWeaponTypeProfile":
                {
                    // Spear startup repair. "match": owned | absent | empty; "type": weapon types that must still hold
                    // their pre-product profile (captureWeaponTypes); "expect": the native Longspear's old-release
                    // clone predicate failures ("none" or a comma list).
                    JObject profile = WeaponTypeProfiles.Probe(step.Match, List(step.Type), step.Expect, out string problem);
                    result["kmgWeaponTypeProfile_" + (step.Label ?? stepIndex.ToString())] = profile;
                    // invariants: ownedValidations / corruptionsApplied (exact) and getterThrowsAtLeast, so a session
                    // proves its condition actually fired where it meant to.
                    var counts = new System.Collections.Generic.Dictionary<string, int>
                    {
                        ["ownedValidations"] = SpearRepairConditions.OwnedValidations, ["corruptionsApplied"] = SpearRepairConditions.CorruptionsApplied,
                        ["getterThrowsAtLeast"] = SpearRepairConditions.GetterThrows,
                        ["guidGetterThrowsAtLeast"] = SpearRepairConditions.GuidGetterThrows,
                        ["rangeOverridesAtLeast"] = SpearRepairConditions.RangeOverrides,
                        ["subCategoryRemovalsAtLeast"] = SpearRepairConditions.SubCategoryRemovals,
                        ["subCategoryAdditionsAtLeast"] = SpearRepairConditions.SubCategoryAdditions,
                        ["enchantmentOverridesAtLeast"] = SpearRepairConditions.EnchantmentOverrides,
                        ["subCategoryListAdditionsAtLeast"] = SpearRepairConditions.SubCategoryListAdditions,
                        ["itemDamageOverridesAtLeast"] = SpearRepairConditions.ItemDamageOverrides
                    };
                    foreach (var pair in step.Invariants ?? new System.Collections.Generic.Dictionary<string, string>())
                    {
                        // "longspearGetter:Name" / "ownedGetter:Name": the observer's recorded effective value, exact text.
                        if (pair.Key.StartsWith("longspearGetter:", StringComparison.Ordinal) || pair.Key.StartsWith("ownedGetter:", StringComparison.Ordinal))
                        {
                            string getter = pair.Key.Substring(pair.Key.IndexOf(':') + 1);
                            JToken getters = pair.Key.StartsWith("longspear", StringComparison.Ordinal) ? profile["nativeLongspear"]?["getters"] : profile["ownedSpear"]?["getters"];
                            string recorded = getters?[getter] == null ? "<absent>" : (string)getters[getter];
                            if (recorded != pair.Value) problem = (problem == null ? "" : problem + "; ") + pair.Key + " recorded " + recorded + ", expected " + pair.Value;
                            continue;
                        }

                        if (!counts.TryGetValue(pair.Key, out int observed)) { problem = (problem == null ? "" : problem + "; ") + "unknown invariant " + pair.Key; continue; }
                        int wanted = int.Parse(pair.Value, System.Globalization.CultureInfo.InvariantCulture);
                        bool held = pair.Key.EndsWith("AtLeast", StringComparison.Ordinal) ? observed >= wanted : observed == wanted;
                        if (!held) problem = (problem == null ? "" : problem + "; ") + pair.Key + " is " + observed + ", expected " + pair.Value;
                    }

                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgItemPricing":
                {
                    // Ordinary weapon sale price and tooltip fill; see ItemPricingProbe.
                    JObject pricing = ItemPricingProbe.Run(List(step.Type), step.Expect, step.Match, step.Contains, step.Invariants, out string problem);
                    result["kmgItemPricing_" + (step.Label ?? stepIndex.ToString())] = pricing;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgSorcererSummon":
                {
                    // A new level-1 sorcerer's Summon Monster I menu and one cast from it; see SorcererSummonProbe. An
                    // owned probe: the slot releases what it created on success, failure, exception, step timeout and
                    // run end, and a failed release stops the launch.
                    string identity = StepIdentity(step);
                    if (!probes.IsActive)
                        probes.Begin(identity, new SorcererSummonProbe(evidence.Directory, step.Label ?? stepIndex.ToString(), step.Expect, step.Contains, step.Invariants));
                    ProbeOutcome outcome = probes.Poll(identity);
                    if (outcome == null) return StepStatus.Running;
                    RecordProbe(outcome);
                    return outcome.Passed ? StepStatus.Done : Fail(outcome.Failure);
                }

                case "kmgSorcererResidue":
                {
                    // Nothing a sorcerer probe created is left: no such unit in the area and none in the party.
                    var residue = new JObject
                    {
                        ["units"] = new JArray(game.State.Units.Where(u => u != null && u.Blueprint != null && u.Blueprint.name == "KmgLoaderCompat_NewSorcerer" && u.IsInState)
                            .Select(u => (object)u.UniqueId).ToArray()),
                        ["partyCount"] = game.Player.PartyCharacters.Count,
                        ["partyHasSorcerer"] = game.Player.PartyCharacters.Any(r => r.Value != null && r.Value.Blueprint != null && r.Value.Blueprint.name == "KmgLoaderCompat_NewSorcerer"),
                        ["targeting"] = game.SelectedAbilityHandler?.Ability?.Blueprint?.name
                    };
                    result["kmgSorcererResidue_" + (step.Label ?? stepIndex.ToString())] = residue;
                    if (((JArray)residue["units"]).Count != 0 || (bool)residue["partyHasSorcerer"] || !string.IsNullOrEmpty((string)residue["targeting"]))
                        return Fail("sorcerer probe residue: " + residue.ToString(Newtonsoft.Json.Formatting.None));
                    return StepStatus.Done;
                }

                case "kmgSetTargeting":
                {
                    // Puts an ability ("match": guid) of the main character into Kingmaker's targeting handler, as an
                    // action-bar click does, so a later step inherits a non-idle UI.
                    var ability = Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(step.Match);
                    if (ability == null) return Fail("no ability " + step.Match);
                    game.SelectedAbilityHandler.SetAbility(new Kingmaker.UnitLogic.Abilities.AbilityData(ability, game.Player.MainCharacter.Value.Descriptor));
                    result["kmgSetTargeting_" + (step.Label ?? stepIndex.ToString())] = game.SelectedAbilityHandler.Ability?.Blueprint?.name;
                    return StepStatus.Done;
                }

                case "kmgTargetingState":
                {
                    // "match": the blueprint name the targeting handler must hold, or "none". "type" = "drop" then clears it.
                    string held = game.SelectedAbilityHandler?.Ability?.Blueprint?.name ?? "none";
                    result["kmgTargetingState_" + (step.Label ?? stepIndex.ToString())] = held;
                    if (step.Type == "drop") game.SelectedAbilityHandler?.DropAbility();
                    return held == step.Match ? StepStatus.Done : Fail("the targeting handler holds " + held + ", expected " + step.Match);
                }

                case "kmgItemDamage":
                {
                    // "match": "guid=1xD8;..." - each item's effective BaseDamage (see ItemDamageProbe).
                    JObject damage = ItemDamageProbe.Run(step.Match, out string problem);
                    result["kmgItemDamage_" + (step.Label ?? stepIndex.ToString())] = damage;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgSetSelection":
                    // Fixture setup for the sorcerer probe's selection ownership: "match" empty | main.
                    result["kmgSetSelection_" + (step.Label ?? stepIndex.ToString())] = SelectionFixture.Set(step.Match);
                    return StepStatus.Done;

                case "kmgSelectionState":
                {
                    // "match" empty: no unit selected; main: the main character alone. Also records the current character.
                    JObject selection = SelectionFixture.Current();
                    result["kmgSelectionState_" + (step.Label ?? stepIndex.ToString())] = selection;
                    var units = (JArray)selection["units"];
                    var main = game.Player.MainCharacter.Value;
                    string mainName = (main.Blueprint?.name ?? "<no blueprint>") + "#" + main.UniqueId;
                    bool held = step.Match == "empty" ? units.Count == 0 :
                        step.Match == "main" ? units.Count == 1 && (string)units[0] == mainName : false;
                    return held ? StepStatus.Done : Fail("the selection is " + selection.ToString(Newtonsoft.Json.Formatting.None) + ", expected " + step.Match);
                }

                case "kmgSpearSelectorState":
                {
                    // Native spear selectors, familiarity and selector runtime against their pre-product capture.
                    JObject state = SpearSelectorState.Probe(step.Expect, out string problem);
                    result["kmgSpearSelectorState_" + (step.Label ?? stepIndex.ToString())] = state;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgSpearPersistFixture":
                    result["kmgSpearPersistFixture_" + (step.Label ?? stepIndex.ToString())] = SpearPersistence.Fixture(request.ProfileDir);
                    return StepStatus.Done;

                case "kmgSpearPersistedItems":
                {
                    JObject persisted = SpearPersistence.Verify(request.ProfileDir, out string problem);
                    result["kmgSpearPersistedItems_" + (step.Label ?? stepIndex.ToString())] = persisted;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

                case "kmgRollbackResidue":
                {
                    // Needs the captureOriginals condition: exact restoration is judged against identities recorded
                    // before the product ran.
                    JObject residue = PresentationProbes.RollbackResidue(out string problem);
                    result["kmgRollbackResidue_" + (step.Label ?? stepIndex.ToString())] = residue;
                    return problem == null ? StepStatus.Done : Fail(problem);
                }

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
                        PanelText.Arm();
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
                        PanelText.Disarm();
                        JObject panel = new JObject
                        {
                            ["mod"] = probe.ModId, ["draws"] = draws, ["repaints"] = repaints, ["onGuiCalls"] = probe.Calls,
                            ["exitGui"] = probe.ExitGui, ["gameMode"] = Game.Instance?.CurrentMode.ToString(), ["clicked"] = ToggleClick.Clicked,
                            ["savedThroughLoader"] = step.Save, ["failure"] = failure,
                            ["labels"] = new JArray(PanelText.Labels.Select(text => (object)(text.Length > 400 ? text.Substring(0, 400) : text)).ToArray())
                        };
                        result["ummPanel_" + (step.Label ?? stepIndex.ToString())] = panel;
                        if (failure != null) return Fail("panel failed: " + failure);
                        if (repaints < 30) return Fail("panel completed only " + repaints + " repaints");
                        if (!string.IsNullOrEmpty(step.Type) && ToggleClick.Clicked == null) return Fail("toggle '" + step.Type + "' was never drawn");
                        foreach (string wanted in step.Contains)
                            if (!PanelText.Labels.Any(text => text.IndexOf(wanted, StringComparison.Ordinal) >= 0)) return Fail("no panel label contains '" + wanted + "'");
                        foreach (string unwanted in step.Absent)
                            if (PanelText.Labels.Any(text => text.IndexOf(unwanted, StringComparison.Ordinal) >= 0)) return Fail("a panel label contains '" + unwanted + "'");
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

            // A run that ends while a probe is active (run timeout, harness exception) still releases what it owns.
            ProbeOutcome left = probes.Abandon("the run ended: " + (failure ?? "no failure"));
            if (left != null)
            {
                RecordProbe(left);
                passed = false;
                failure = (failure == null ? string.Empty : failure + " || ") + "probe of step " + left.Owner + " was active when the run ended" +
                    (left.IsolationRestored ? string.Empty : ": " + left.Failure);
            }

            result["isolationRestored"] = probes.IsolationRestored;
            result["status"] = passed ? "passed" : "failed";
            result["failure"] = failure;
            result["finishedUtc"] = DateTime.UtcNow.ToString("o");
            result["exitRequested"] = request.ExitAfterCompletion;
            evidence.WriteJson("harness-result.json", result);
            if (request.ExitAfterCompletion) quitCountdown = 30;
        }
    }
}
