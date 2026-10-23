using System;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsCleanup2ConsoleReview { get { return IsSummonCleanup2Review &&
            (RuntimeTestRequestParser.Cleanup2ReviewPhase(_request)=="elementals-console" ||
             RuntimeTestRequestParser.Cleanup2ReviewPhase(_request)=="alternate-console"); } }
        private Game.ControllerModeType? _cleanup2OriginalController;
        private string _cleanup2OriginalLoadingScene,_cleanup2ConsoleLoadingScene;
        private BundledSceneLoader.AsyncOperation _cleanup2ConsoleLoadingOperation;
        private int _cleanup2ConsoleLoadingStage;
        private bool _cleanup2ConsoleLoadingBound,_cleanup2ConsoleRestoring,_cleanup2ConsoleRestored;

        // Reuse the qualified native controller/loading-scene APIs before the
        // fresh unsaved world is created. No human save or live campaign is
        // reloaded, and no synthetic console view/model is installed.
        private bool Cleanup2PrepareConsoleLoading()
        {
            if(!IsCleanup2ConsoleReview || !_request.ExitAfterCompletion ||
                Game.Instance.CurrentlyLoadedArea!=null || Game.Instance.Player.Party.Count!=0)
                throw new InvalidOperationException("Exact empty-process console review required.");
            var game=Game.Instance;
            if(_cleanup2ConsoleLoadingStage==0)
            {
                if(game.IsControllerGamepad || _summoningProfileSaveGuard==null || _summoningProfileScenes==null)
                    throw new InvalidOperationException("Console setup requires the protected desktop native-menu intake.");
                _cleanup2OriginalLoadingScene=SceneName.LoadingScreenUI;
                if(!SceneManager.GetSceneByName(_cleanup2OriginalLoadingScene).isLoaded)
                    throw new InvalidOperationException("Original native desktop loading scene is absent.");
                _cleanup2OriginalController=game.ControllerMode;
                game.RootUiContext.DisposeLoadingScreen();
                game.ControllerMode=(Game.ControllerModeType)1;
                _cleanup2ConsoleLoadingScene=SceneName.LoadingScreenUI;
                if(_cleanup2ConsoleLoadingScene==_cleanup2OriginalLoadingScene)
                    throw new InvalidOperationException("Native controller mode did not select its console loading scene.");
                _cleanup2ConsoleLoadingOperation=BundledSceneLoader.UnloadSceneAsync(_cleanup2OriginalLoadingScene);
                _cleanup2ConsoleLoadingStage=1;
                return false;
            }
            if(!game.IsControllerGamepad)throw new InvalidOperationException("Console setup lost its exact controller owner.");
            if(!_cleanup2ConsoleLoadingOperation.IsDone)return false;
            if(_cleanup2ConsoleLoadingStage==1)
            {
                if(SceneManager.GetSceneByName(_cleanup2OriginalLoadingScene).isLoaded)
                    throw new InvalidOperationException("Original loading scene did not unload.");
                _cleanup2ConsoleLoadingOperation=BundledSceneLoader.LoadSceneAsync(_cleanup2ConsoleLoadingScene,LoadSceneMode.Additive);
                _cleanup2ConsoleLoadingStage=2;
                return false;
            }
            if(_cleanup2ConsoleLoadingStage==2)
            {
                if(!SceneManager.GetSceneByName(_cleanup2ConsoleLoadingScene).isLoaded)
                    throw new InvalidOperationException("Native console loading scene failed to register.");
                game.RootUiContext.InitializeLoadingScreen(_cleanup2ConsoleLoadingScene);
                _cleanup2ConsoleLoadingBound=true;_cleanup2ConsoleLoadingStage=3;
                _summoningProfileEvidence["consoleLoadingIntake"]=new JObject {
                    ["originalController"]=_cleanup2OriginalController.ToString(),["actualController"]=game.ControllerMode.ToString(),
                    ["originalLoadingScene"]=_cleanup2OriginalLoadingScene,["consoleLoadingScene"]=_cleanup2ConsoleLoadingScene,
                    ["nativeLoadingContextInitialized"]=true,["worldCreatedBeforeControllerSetup"]=false };
            }
            return _cleanup2ConsoleLoadingStage==3;
        }

        // Native ResetToMainMenu preserves the loading scene selected by the
        // CURRENT controller mode; it does not recreate its context. Load the
        // original scene while the console UI is still intact, then switch and
        // reset in one callback, with no intervening frame on a disposed UI.
        private bool Cleanup2RestoreConsoleLoading()
        {
            if(!_cleanup2OriginalController.HasValue || _cleanup2ConsoleRestored)return true;
            var game=Game.Instance;
            if(!game.IsControllerGamepad && game.ControllerMode==_cleanup2OriginalController.Value && _cleanup2ConsoleLoadingStage==0)
            {
                // Native GameStarter initializes this context on desktop too;
                // its constructor accepts the desktop scene's absent console
                // config. Restore via that same native factory.
                game.RootUiContext.InitializeLoadingScreen(_cleanup2OriginalLoadingScene);
                _cleanup2ConsoleRestored=true;return true;
            }
            if(!game.IsControllerGamepad)
                throw new InvalidOperationException("Foreign controller context prevents console restoration.");
            if(_cleanup2ConsoleLoadingOperation!=null && !_cleanup2ConsoleLoadingOperation.IsDone)return false;
            if(!_cleanup2ConsoleRestoring)
            {
                _cleanup2ConsoleRestoring=true;
                if(!SceneManager.GetSceneByName(_cleanup2OriginalLoadingScene).isLoaded)
                {
                    _cleanup2ConsoleLoadingOperation=BundledSceneLoader.LoadSceneAsync(_cleanup2OriginalLoadingScene,LoadSceneMode.Additive);
                    return false;
                }
            }
            if(!SceneManager.GetSceneByName(_cleanup2OriginalLoadingScene).isLoaded)
                throw new InvalidOperationException("Original desktop loading scene restoration failed.");
            game.RootUiContext.DisposeUiScene();
            if(_cleanup2ConsoleLoadingBound)game.RootUiContext.DisposeLoadingScreen();
            game.ControllerMode=_cleanup2OriginalController.Value;
            game.RootUiContext.InitializeLoadingScreen(_cleanup2OriginalLoadingScene);
            _cleanup2ConsoleRestored=true;
            _summoningProfileEvidence["consoleControllerRestoredBeforeNativeReset"]=true;
            return true;
        }

        private bool Cleanup2ConsoleRestorationExact()
        {
            if(!_cleanup2OriginalController.HasValue)return true;
            bool exact=_cleanup2ConsoleRestored && Game.Instance.ControllerMode==_cleanup2OriginalController.Value &&
                SceneManager.GetSceneByName(_cleanup2OriginalLoadingScene).isLoaded &&
                (string.IsNullOrEmpty(_cleanup2ConsoleLoadingScene) || !SceneManager.GetSceneByName(_cleanup2ConsoleLoadingScene).isLoaded);
            _summoningProfileEvidence["consoleRestoration"]=new JObject {
                ["exact"]=exact,["controller"]=Game.Instance.ControllerMode.ToString(),
                ["desktopLoadingLoaded"]=SceneManager.GetSceneByName(_cleanup2OriginalLoadingScene).isLoaded,
                ["ownedConsoleLoadingUnloaded"]=string.IsNullOrEmpty(_cleanup2ConsoleLoadingScene) || !SceneManager.GetSceneByName(_cleanup2ConsoleLoadingScene).isLoaded };
            return exact;
        }
    }
}
