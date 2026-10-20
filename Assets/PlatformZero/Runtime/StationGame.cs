#if PLATFORM_ZERO_YARN
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

namespace PlatformZero
{
    public sealed class StationGame : MonoBehaviour
    {
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private StationWalker walker;
        [SerializeField] private StationPresenter presenter;
        [SerializeField] private GameObject notebook;
        [SerializeField] private GameObject exitBarrier;
        private StationHud hud;
        private bool begun;
        private bool paused;
        private bool ended;
        private bool startingDialogue;
        public bool CanExplore => begun && !paused && !ended && !startingDialogue && !runner.IsDialogueRunning;
        public bool Paused => paused;
        public void Configure(DialogueRunner dialogue, StationWalker player, StationPresenter view, GameObject book, GameObject barrier)
        { runner = dialogue; walker = player; presenter = view; notebook = book; exitBarrier = barrier; }

        private void Start()
        {
            hud = gameObject.AddComponent<StationHud>();
            hud.Initialize(this);
            foreach (var sign in FindObjectsByType<StationSign>(FindObjectsSortMode.None)) sign.Initialize(hud.Font);
            presenter.Initialize(hud, this);
            runner.onDialogueComplete ??= new UnityEngine.Events.UnityEvent();
            runner.onDialogueComplete.AddListener(DialogueEnded);
            SetCursor(false);
            hud.ShowMenu("PLATFORM ZERO", "Where the hell are you?", "Begin", Begin);
        }
        private void Update()
        {
            if (hud == null) return;
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true && begun && !ended) SetPaused(!paused);
            if (!CanExplore) { hud.SetPrompt(""); return; }
            StationInteractable target = null;
            if (Physics.Raycast(walker.View.ViewportPointToRay(new Vector3(.5f,.5f,0)), out RaycastHit hit, 3.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                target = hit.collider.GetComponentInParent<StationInteractable>();
            hud.SetPrompt(target != null ? "[E]  " + target.Prompt : "");
            if (target != null && Keyboard.current?.eKey.wasPressedThisFrame == true) RunDialogue(target.Node);
        }
        public void Begin() { begun = true; hud.HideMenu(); SyncWorld(); SetCursor(true); }
        private async void RunDialogue(string node)
        {
            if (!CanExplore) return;
            startingDialogue = true;
            SetCursor(false);
            try { await runner.StartDialogue(node); }
            catch (System.Exception error) { Debug.LogException(error); hud.ShowMenu("Dialogue error", "Check the Unity Console for details.", "Restart", Restart); paused = true; }
            finally { startingDialogue = false; if (CanExplore) SetCursor(true); }
        }
        public bool Flag(string name) => runner.VariableStorage.TryGetValue<bool>(name, out bool value) && value;
        private void DialogueEnded()
        {
            SyncWorld();
            if (Flag("$pz_ended"))
            {
                ended = true;
                hud.ShowMenu("ONE STEP AT A TIME", "Platform Zero\nThank you for playing.", "Play again", Restart);
                SetCursor(false);
            }
            else if (!paused) SetCursor(true);
        }
        private void SyncWorld()
        {
            notebook.SetActive(!Flag("$pz_has_notebook") && !Flag("$pz_notebook_returned"));
            exitBarrier.SetActive(!Flag("$pz_story_complete"));
            hud.SetObjective(Flag("$pz_story_complete") ? "Try the exit together." :
                Flag("$pz_has_notebook") ? "Return the notebook to Nina." :
                Flag("$pz_quest_started") ? "Find the red notebook near the far benches." : "Talk to the girl on the platform.");
        }
        public void SetPaused(bool value)
        {
            paused = value;
            if (paused) hud.ShowMenu("TAKE A BREATH", "WASD / arrows  •  Move\nMouse  •  Look\nE  •  Interact\nSpace / Enter / click  •  Continue dialogue", "Resume", () => SetPaused(false));
            else hud.HideMenu();
            SetCursor(CanExplore);
        }
        private void OnApplicationFocus(bool focus) { if (!focus && begun && !ended && hud != null) SetPaused(true); }
        public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        public static void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void OnDestroy()
        {
            if (runner != null) runner.onDialogueComplete?.RemoveListener(DialogueEnded);
            SetCursor(false);
        }
        private static void SetCursor(bool locked) { Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !locked; }
    }
}

#endif

