using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.ElementalRaces
{
    // One post-LoadDictionary callback, then exact settings/host callbacks.
    // No frame polling, unit registry, unrelated unit scan or save mutation.
    internal static class ElementalCharacterTraitPublicationCoordinator
    {
        private static ModContext _context;
        private static DormantElementalCharacterTraitGraph _graph;
        private static ElementalCharacterTraitPublicationTransaction<BlueprintFeature> _publication;
        private static bool _attached, _reconciling;
        private static UnityModManager.ModEntry _host;
        private static Action<UnityModManager.ModEntry> _hostGui, _hostSave;
        private static Func<UnityModManager.ModEntry,bool,bool> _hostToggle;
        internal static string Failure { get; private set; }
        internal static bool GraphRegistered { get { return _graph!=null; } }
        internal static DormantElementalCharacterTraitGraph Graph { get { return _graph; } }
        internal static bool Published { get { return _publication?.IsCommitted==true; } }
        internal static bool ProviderModuleEnabled
        {
            get
            {
                ModContext context;
                return ModContext.TryGet(out context) && context.IsReady && !context.IsFailed &&
                    context.ModEntry.Active && context.FeatureModules?.Pending?.ElementalRaces==true;
            }
        }
        internal static void Attach(ModContext context)
        {
            if(_context!=null && !ReferenceEquals(_context,context)) throw new InvalidOperationException("Foreign DATA publication context.");
            if(_context==null)
            {
                _context=context;
                context.FeatureModules.ElementalCharacterTraitsChanged+=OnModuleChanged;
            }
            Schedule();
        }
        private static void Schedule()
        {
            if(_attached || _context==null) return;
            _attached=true; _context.ModEntry.OnUpdate+=FirstUpdate;
        }
        private static void FirstUpdate(UnityModManager.ModEntry entry,float delta)
        {
            _attached=false; entry.OnUpdate-=FirstUpdate;
            Reconcile("first-update-after-complete-blueprint-postfix-chain");
        }
        private static void OnModuleChanged() { Reconcile("elemental-module-setting-changed"); }
        private static UnityModManager.ModEntry HostEntry()
        {
            var field=typeof(UnityModManager).GetField("modEntries",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            var list=field?.GetValue(null) as IEnumerable;
            if(list==null) throw new InvalidOperationException("Exact UMM entry collection unavailable.");
            var matches=list.Cast<object>().OfType<UnityModManager.ModEntry>().Where(e=>e.Info?.Id==FavoredClassTraitResolver.ModId).ToArray();
            if(matches.Length>1) throw new InvalidOperationException("Ambiguous Favored Class ownership.");
            return matches.SingleOrDefault();
        }
        internal static AidAnotherContractResolution<FavoredClassTraitContract> ResolveHost()
        { return FavoredClassTraitResolver.Resolve(HostEntry(),BlueprintBootstrap.BodyguardFeats?.HelpfulCombat); }
        private static void ObserveHostCallbacks(UnityModManager.ModEntry host)
        {
            if(host==null || ReferenceEquals(_host,host)) return;
            if(_host!=null) throw new InvalidOperationException("Foreign host replacement during the current process.");
            _host=host; _hostGui=host.OnGUI; _hostSave=host.OnSaveGUI; _hostToggle=host.OnToggle;
            host.OnGUI=entry=> { try { _hostGui?.Invoke(entry); } finally { Reconcile("favored-class-settings-gui-completed"); } };
            host.OnSaveGUI=entry=> { try { _hostSave?.Invoke(entry); } finally { Reconcile("favored-class-settings-save-completed"); } };
            host.OnToggle=(entry,enabled)=>
            {
                bool accepted=_hostToggle==null || _hostToggle(entry,enabled);
                if(accepted)
                {
                    // UMM commits Active after returning from this callback.
                    // Acquisition OFF is immediate; ON waits for that exact
                    // one-shot post-toggle callback, never a polling loop.
                    if(!enabled) RemoveAcquisition();
                    Schedule();
                }
                return accepted;
            };
        }
        internal static bool Reconcile(string checkpoint)
        {
            if(_reconciling || _context==null || !BlueprintBootstrap.IsInitialized) return false;
            _reconciling=true;
            DormantElementalCharacterTraitGraph created=null;
            ElementalCharacterTraitNativeRegistration registration=null;
            try
            {
                var library=BlueprintBootstrap.Library; var races=BlueprintBootstrap.ElementalRaces;
                var wings=BlueprintBootstrap.ElementalFeats.Require<BlueprintBuff>(ElementalCharacterTraitCatalog.WingsOfAirGuid);
                var hostEntry=HostEntry();var host=ResolveHost();
                if(host.IsCompatible) ObserveHostCallbacks(hostEntry);
                var resolved=ElementalCharacterTraitNativeGraph.ResolveCanonicalGraphIfFullyRegistered(library,races,wings);
                bool existing=resolved!=null;
                var graph=resolved ?? (created=DormantElementalCharacterTraitFactory.CreateDetached(library,races,wings,host,ProviderModuleEnabled,ProjectAssetIcons.RequireIcon));
                ElementalCharacterTraitNativeGraph.Validate(graph,races.OrderedRaces(),wings);
                if(host.IsCompatible) ElementalCharacterTraitNativeGraph.BindExactRacePrerequisites(graph,races.OrderedRaces(),host.Contract);
                registration=new ElementalCharacterTraitNativeRegistration(library,BlueprintManifest.Load(_context.ModEntry.Path),graph,existing);
                bool ready=host.IsCompatible && host.Contract.TraitsEnabled && ProviderModuleEnabled;
                if(!ready)
                {
                    RemoveAcquisition();
                    registration.Apply();
                    _graph=graph; Failure=null;
                    _context.Logger.Info("elemental-character-traits","identities-resolvable-acquisition-off",checkpoint);
                    return true;
                }
                if(_publication!=null)
                {
                    ElementalCharacterTraitNativeGraph.Validate(graph,races.OrderedRaces(),wings);
                    var selection=host.Contract.RaceTraits;
                    for(int i=0;i<4;i++)
                        if(selection.AllFeatures.Count(f=>ReferenceEquals(f,graph.Features[i]))!=1)
                            throw new InvalidOperationException("Owned selector membership changed after publication.");
                    registration.Apply();
                    _graph=graph; Failure=null;return true;
                }
                var target=host.Contract.RaceTraits;
                var conditions=new ElementalCharacterTraitPublicationConditions(true,true,true,true,true,target.AssetGuid,
                    ElementalCharacterTraitAssetGate.Evaluate(ElementalCharacterTraitAssetCatalog.Current()));
                var transaction=new ElementalCharacterTraitPublicationTransaction<BlueprintFeature>(conditions,
                    ()=>target.Features,()=>target.AllFeatures,a=>target.AllFeatures=a,graph.Features,f=>f.AssetGuid,
                    registration.Preflight,()=>
                    {
                        var live=ResolveHost();
                        if(!live.IsCompatible || !live.Contract.TraitsEnabled || !ProviderModuleEnabled ||
                            !ReferenceEquals(live.Contract.RaceTraits,target))
                            throw new InvalidOperationException("Host/module/selection changed immediately before publication.");
                        ElementalCharacterTraitNativeGraph.Validate(graph,races.OrderedRaces(),wings);
                    },new CharacterTraitPublicationStep(registration.Apply,registration.Rollback));
                transaction.Commit();
                _graph=graph; _publication=transaction; Failure=null;
                _context.Logger.Info("elemental-character-traits","all-four-published",checkpoint+";registered=11;racial_traits=4;foreign-preserved=true");
                return true;
            }
            catch(Exception error)
            {
                Failure=error.ToString();
                // The transaction retains native resolutions if selector rollback
                // fails. Never dispose an object still in the library.
                if(created!=null && !ElementalCharacterTraitCatalog.Nodes().Any(n=>
                    BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(n.Guid)))
                    try { created.Dispose(); } catch(Exception cleanup) { Failure+="\n"+cleanup; }
                _context.Logger.Failure("elemental-character-traits","publication-blocked",checkpoint,error);
                return false;
            }
            finally { _reconciling=false; }
        }
        internal static void RemoveAcquisition()
        {
            if(_publication==null) return;
            _publication.WithdrawAcquisition();_publication=null;
        }
    }
}
