#if PLATFORM_ZERO_YARN
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

namespace PlatformZero.Editor
{
    [InitializeOnLoad]
    public static class StationSmokeTest
    {
        static StationSmokeTest() { EditorApplication.playModeStateChanged += OnPlayMode; }
        [MenuItem("Platform Zero/Run Story Smoke Test")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before testing.");
            EditorSceneManager.OpenScene(StationBuilder.ScenePath);
            SessionState.SetBool("PlatformZero.Smoke", true);
            EditorApplication.isPlaying = true;
        }
        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("PlatformZero.Smoke", false)) return;
            SessionState.SetBool("PlatformZero.Smoke", false);
            new GameObject("Story Smoke Test").AddComponent<StationSmokeDriver>();
        }
    }
    public sealed class StationSmokeDriver : MonoBehaviour
    {
        private readonly List<string> checks = new List<string>();
        private readonly List<string> errors = new List<string>();
        private StationGame game;
        private DialogueRunner runner;
        private StationHud hud;
        private int choice;
        private double deadline;
        private void OnEnable() { Application.logMessageReceived += OnLog; }
        private void OnDisable() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message); }
        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            deadline = EditorApplication.timeSinceStartup + 180;
            yield return new WaitForSecondsRealtime(.5f);
            for (choice=0; choice<2; choice++)
            {
                game = FindAnyObjectByType<StationGame>(); runner = FindAnyObjectByType<DialogueRunner>(); hud = FindAnyObjectByType<StationHud>();
                Check(game != null && runner != null && hud != null && hud.Font != null, "Runtime references and font");
                var book = GameObject.Find("Red Notebook"); var gate = GameObject.Find("Closed Exit Gate");
                Check(book != null && gate != null, "Initial quest objects exist");
                Check(!game.Flag("$pz_quest_started"), "Fresh quest state");
                game.Begin(); game.SetPaused(true); Check(!game.CanExplore, "Pause prevents exploration"); game.SetPaused(false); Check(game.CanExplore, "Resume restores exploration");
                CheckRay("Nina",new Vector3(2,.1f,-.8f),new Vector3(2,1.1f,1.4f));
                CheckRay("Red Notebook",new Vector3(2,.1f,9.3f),book.transform.position);
                CheckRay("Exit Intercom",new Vector3(1.9f,.1f,-13.1f),new Vector3(1.9f,1.35f,-15.2f));
                yield return Talk("Notebook"); Check(!game.Flag("$pz_has_notebook"), "Cannot collect before quest");
                yield return Talk("StationExit"); Check(!game.Flag("$pz_ended"), "Cannot exit early");
                yield return Talk("Nina"); Check(game.Flag("$pz_quest_started"), "Introduction starts quest");
                yield return Talk("Nina"); Check(!game.Flag("$pz_story_complete"), "Reminder preserves quest");
                yield return Talk("Notebook"); Check(game.Flag("$pz_has_notebook") && !book.activeSelf, "Pickup updates Yarn and hides notebook");
                yield return Talk("Nina"); Check(game.Flag("$pz_notebook_returned") && !game.Flag("$pz_has_notebook"), "Return consumes notebook");
                Check(game.Flag("$pz_story_complete") && !gate.activeSelf, "Reveal unlocks exit");
                yield return Talk("Nina"); Check(game.Flag("$pz_story_complete"), "Repeat conversation preserves completion");
                yield return Talk("StationExit"); Check(game.Flag("$pz_ended") && !game.CanExplore, "Ending stops exploration");
                Check(hud.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text=="ONE STEP AT A TIME"), "Ending screen displayed");
                game.Restart(); yield return new WaitForSecondsRealtime(.5f);
            }
            Check(!FindAnyObjectByType<StationGame>().Flag("$pz_quest_started"), "Restart resets Yarn variables");
            Finish();
        }
        private void CheckRay(string name,Vector3 position,Vector3 target)
        {
            var walker=FindAnyObjectByType<StationWalker>(); var controller=walker.GetComponent<CharacterController>();
            controller.enabled=false; walker.transform.position=position; controller.enabled=true;
            var origin=position+new Vector3(0,1.65f,0); Physics.SyncTransforms();
            bool hit=Physics.Raycast(origin,(target-origin).normalized,out var result,3.2f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
            Check(hit && result.collider.GetComponentInParent<StationInteractable>()?.gameObject.name==name,"Interaction reachable: "+name+" (hit "+(hit?result.collider.name:"nothing")+", target "+target+")");
        }
        private IEnumerator Talk(string node)
        {
            game.SetPaused(false);
            typeof(StationGame).GetMethod("RunDialogue",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{node});
            yield return new WaitForSecondsRealtime(.25f);
            float stop=Time.realtimeSinceStartup+30;
            while(runner.IsDialogueRunning && Time.realtimeSinceStartup<stop)
            {
                if(game.Paused) game.SetPaused(false);
                var buttons=hud.GetComponentsInChildren<Button>();
                var selected=buttons.FirstOrDefault(b=>b.name=="Choice "+choice) ?? buttons.FirstOrDefault(b=>b.name=="Continue");
                if(selected!=null) selected.onClick.Invoke();
                yield return new WaitForSecondsRealtime(.18f);
            }
            Check(!runner.IsDialogueRunning,"Dialogue completes: "+node+" / choice "+choice);
            yield return null;
        }
        private void Check(bool valid,string label) { if(!valid) { errors.Add("FAIL: "+label); Finish(); throw new InvalidOperationException(label); } checks.Add("PASS: "+label); }
        private void Update() { if(EditorApplication.timeSinceStartup>deadline) { errors.Add("Smoke test timed out"); Finish(); } }
        private void Finish()
        {
            enabled=false;
            Directory.CreateDirectory("Validation");
            File.WriteAllLines("Validation/runtime-smoke.txt", new[]{errors.Count==0?"PASS":"FAIL",DateTime.UtcNow.ToString("O"),"Unity "+Application.unityVersion,"Checks: "+checks.Count}.Concat(checks).Concat(errors));
            Debug.Log("Platform Zero smoke test: "+checks.Count+" checks, "+errors.Count+" errors.");
            EditorApplication.isPlaying=false;
        }
    }
}
#endif


