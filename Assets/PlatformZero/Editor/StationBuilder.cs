#if PLATFORM_ZERO_YARN
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Yarn.Unity;

namespace PlatformZero.Editor
{
    public static class StationBuilder
    {
        public const string ScenePath="Assets/Scenes/PlatformZero.unity";
        private const string DialoguePath="Assets/Dialogue/PlatformZero.yarnproject";
        private static Transform world;
        private static Material teal,cream,concrete,dark,amber,red,skin,cloth,glow;

        [InitializeOnLoadMethod]
        private static void QueueFirstBuild()
        {
            if (!File.Exists(ScenePath)) EditorApplication.delayCall += FirstBuild;
        }
        private static void FirstBuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=FirstBuild; return; }
            if (AssetDatabase.LoadAssetAtPath<YarnProject>(DialoguePath)==null) return;
            if (EditorSceneManager.GetActiveScene().isDirty) return;
            BuildScene();
        }

        [MenuItem("Platform Zero/Create Station Scene")]
        public static void BuildScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var project=AssetDatabase.LoadAssetAtPath<YarnProject>(DialoguePath);
            if(project==null || project.Program==null) throw new InvalidOperationException("Import and compile PlatformZero.yarnproject first.");
            Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/Materials");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            world=new GameObject("Platform Zero Station").transform;
            teal=Mat("Deep Teal",new Color(.075f,.22f,.24f)); cream=Mat("Ivory Tile",new Color(.62f,.66f,.61f));
            concrete=Mat("Concrete",new Color(.27f,.33f,.34f)); dark=Mat("Charcoal",new Color(.035f,.055f,.065f));
            amber=Mat("Safety Yellow",new Color(.83f,.55f,.13f)); red=Mat("Notebook Red",new Color(.74f,.055f,.045f));
            skin=Mat("Warm Skin",new Color(.64f,.40f,.29f)); cloth=Mat("Nina Jacket",new Color(.25f,.32f,.40f));
            glow=Mat("Light Strip",new Color(.8f,.85f,.72f),true);
            Box("Platform",new Vector3(0,-.25f,0),new Vector3(12,.5f,34),concrete);
            Box("Back Wall",new Vector3(5.9f,2.5f,0),new Vector3(.4f,5,34),cream);
            Box("End Wall",new Vector3(0,2.5f,16.8f),new Vector3(12,5,.4f),teal);
            Box("Entrance Wall",new Vector3(0,2.5f,-16.8f),new Vector3(12,5,.4f),teal);
            Box("Ceiling",new Vector3(0,5.1f,0),new Vector3(12,.3f,34),dark);
            Box("Track Wall",new Vector3(-5.9f,2.5f,0),new Vector3(.3f,5,34),dark);
            Box("Track Bed",new Vector3(-5,.015f,0),new Vector3(1.5f,.03f,33),dark);
            for(int i=0;i<33;i++) Box("Sleeper",new Vector3(-5,.04f,i-16),new Vector3(1.5f,.06f,.15f),concrete,false);
            Box("Track Rail",new Vector3(-5.4f,.1f,0),new Vector3(.07f,.1f,33),cream,false);
            Box("Track Rail",new Vector3(-4.7f,.1f,0),new Vector3(.07f,.1f,33),cream,false);
            Box("Platform Edge",new Vector3(-3.95f,.025f,0),new Vector3(.25f,.05f,33),amber,false);
            var safety=Box("Track Boundary",new Vector3(-4.15f,1.5f,0),new Vector3(.1f,3,34),dark); safety.GetComponent<Renderer>().enabled=false;
            for(int z=-16;z<=16;z+=2)
            {
                Box("Floor Joint",new Vector3(.8f,.007f,z),new Vector3(9.6f,.01f,.018f),dark,false);
                Box("Wall Joint",new Vector3(5.68f,2.7f,z),new Vector3(.01f,4.3f,.025f),concrete,false);
            }
            for(int x=-3;x<=5;x+=2) Box("Floor Joint",new Vector3(x,.008f,0),new Vector3(.018f,.01f,33),dark,false);
            Box("Wall Stripe",new Vector3(5.66f,1.5f,0),new Vector3(.02f,.36f,34),teal,false);
            for(int z=-12;z<=12;z+=6)
            {
                Box("Pillar",new Vector3(-2.8f,2.5f,z),new Vector3(.55f,5,.55f),teal);
                Box("Pillar Base",new Vector3(-2.8f,.25f,z),new Vector3(.75f,.5f,.75f),dark);
                Box("Overhead Light",new Vector3(1,4.85f,z),new Vector3(4,.07f,.18f),glow,false);
                LightAt(new Vector3(1,4.5f,z),new Color(.82f,.91f,.84f),4,10);
            }
            Bench(3.9f,0); Bench(3.9f,10);
            Box("Station Sign",new Vector3(1,3.5f,15.8f),new Vector3(7,1.5f,.2f),dark,false);
            Sign("PLATFORM 0",new Vector3(1,3.6f,15.67f),Vector3.zero,7,new Color(.94f,.80f,.50f));
            Sign("NO SERVICE  /  00:00",new Vector3(1,2.95f,15.65f),Vector3.zero,2.2f,Color.white);
            Sign("PLEASE MIND THE GAP",new Vector3(5.62f,3.1f,4.5f),new Vector3(0,-90,0),2.1f,Color.white);
            Clock();

            var nina=new GameObject("Nina"); nina.transform.SetParent(world); nina.transform.position=new Vector3(2,0,1.4f);
            Part("Jacket",nina.transform,PrimitiveType.Capsule,new Vector3(0,.94f,0),new Vector3(.52f,.55f,.35f),cloth);
            Part("Head",nina.transform,PrimitiveType.Sphere,new Vector3(0,1.62f,0),new Vector3(.36f,.43f,.36f),skin);
            Part("Hair",nina.transform,PrimitiveType.Sphere,new Vector3(0,1.77f,.035f),new Vector3(.39f,.26f,.37f),dark);
            Part("Left Leg",nina.transform,PrimitiveType.Capsule,new Vector3(-.14f,.36f,0),new Vector3(.18f,.36f,.20f),dark);
            Part("Right Leg",nina.transform,PrimitiveType.Capsule,new Vector3(.14f,.36f,0),new Vector3(.18f,.36f,.20f),dark);
            Part("Left Arm",nina.transform,PrimitiveType.Capsule,new Vector3(-.32f,.96f,0),new Vector3(.17f,.36f,.19f),cloth);
            Part("Right Arm",nina.transform,PrimitiveType.Capsule,new Vector3(.32f,.96f,0),new Vector3(.17f,.36f,.19f),cloth);
            var ninaCollider=nina.AddComponent<CapsuleCollider>();ninaCollider.center=new Vector3(0,.9f,0);ninaCollider.height=1.85f;ninaCollider.radius=.45f;
            nina.AddComponent<StationInteractable>().Configure("Nina","Talk to Nina");
            Box("Backpack",new Vector3(2.6f,.3f,1.6f),new Vector3(.45f,.6f,.3f),teal);

            var book=Box("Red Notebook",new Vector3(3.5f,.73f,9.3f),new Vector3(.42f,.12f,.58f),red);
            book.AddComponent<StationInteractable>().Configure("Notebook","Pick up the red notebook");
            var pages=Box("Notebook Pages",new Vector3(3.5f,.73f,9.3f),new Vector3(.39f,.065f,.53f),cream,false);pages.transform.SetParent(book.transform,true);
            LightAt(new Vector3(3.5f,1.5f,9.3f),new Color(1,.65f,.30f),1.4f,3);

            var barrier=Box("Closed Exit Gate",new Vector3(0,1.35f,-15.5f),new Vector3(2.8f,2.7f,.12f),dark);
            for(int i=0;i<6;i++) Box("Exit Step",new Vector3(0,i*.14f,-15.7f-i*.15f),new Vector3(2.8f,.2f,.3f),concrete);
            Box("Exit Sign",new Vector3(0,3.4f,-15.6f),new Vector3(3.5f,.8f,.15f),teal,false);
            Sign("EXIT  /  WAY OUT",new Vector3(0,3.4f,-15.48f),new Vector3(0,180,0),3,Color.white);
            var exit=Box("Exit Intercom",new Vector3(1.9f,1.35f,-15.2f),new Vector3(.5f,.8f,.16f),amber);
            exit.AddComponent<StationInteractable>().Configure("StationExit","Try the exit");
            var clockButton=Box("Station Map",new Vector3(5.6f,1.9f,-5),new Vector3(.15f,1.8f,2),teal);
            clockButton.AddComponent<StationInteractable>().Configure("StationMap","Read the station map");

            var gameObject=new GameObject("Game"); var game=gameObject.AddComponent<StationGame>();
            var dialogueObject=new GameObject("Yarn Dialogue");
            var storage=dialogueObject.AddComponent<InMemoryVariableStorage>();
            var presenter=dialogueObject.AddComponent<StationPresenter>();
            var runner=dialogueObject.AddComponent<DialogueRunner>(); runner.autoStart=false; runner.VariableStorage=storage;runner.SetProject(project);runner.DialoguePresenters=new[]{presenter};
            var player=new GameObject("Player");player.layer=2;player.transform.position=new Vector3(0,.1f,-9);
            var controller=player.AddComponent<CharacterController>();controller.height=1.8f;controller.radius=.3f;controller.center=new Vector3(0,.9f,0);controller.stepOffset=.25f;
            var cameraObject=new GameObject("Player Camera");cameraObject.transform.SetParent(player.transform,false);cameraObject.transform.localPosition=new Vector3(0,1.65f,0);cameraObject.tag="MainCamera";
            var camera=cameraObject.AddComponent<Camera>();camera.fieldOfView=70;camera.nearClipPlane=.05f;camera.farClipPlane=80;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.06f,.075f);cameraObject.AddComponent<AudioListener>();
            var walker=player.AddComponent<StationWalker>();walker.Configure(camera,game);game.Configure(runner,walker,presenter,book,barrier);
            var sun=new GameObject("Soft Fill").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.45f;sun.color=new Color(.65f,.76f,.81f);sun.transform.rotation=Quaternion.Euler(65,-30,0);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.21f,.27f,.29f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.055f,.09f,.10f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.025f;
            PlayerSettings.productName="Platform Zero";PlayerSettings.companyName="Platform Zero";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();
            Debug.Log("Platform Zero: station scene created and fully wired.");
        }
        private static Material Mat(string name,Color color,bool emissive=false)
        {
            string path="Assets/Materials/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path); }
            material.color=color;material.SetFloat("_Smoothness",.25f);
            if(emissive) { material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*3); }
            return material;
        }
        private static GameObject Box(string name,Vector3 position,Vector3 scale,Material material,bool solid=true)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(world);obj.transform.position=position;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());return obj;
        }
        private static void Part(string name,Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {var obj=GameObject.CreatePrimitive(type);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());}
        private static void Bench(float x,float z)
        {Box("Bench Seat",new Vector3(x,.58f,z),new Vector3(1.2f,.16f,3),teal);Box("Bench Back",new Vector3(x+.5f,1.05f,z),new Vector3(.15f,.85f,3),teal);foreach(float dz in new[]{-1f,1f})Box("Bench Leg",new Vector3(x,.25f,z+dz),new Vector3(.8f,.5f,.12f),dark);}
        private static void LightAt(Vector3 position,Color color,float intensity,float range)
        {var obj=new GameObject("Platform Light");obj.transform.SetParent(world);obj.transform.position=position;var light=obj.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=range;light.shadows=LightShadows.None;}
        private static void Sign(string text,Vector3 position,Vector3 rotation,float size,Color color)
        {var obj=new GameObject("Sign "+text);obj.transform.SetParent(world);obj.transform.position=position;obj.transform.eulerAngles=rotation;obj.AddComponent<StationSign>().Configure(text,size,color);}
        private static void Clock()
        {Box("Clock Housing",new Vector3(5.58f,3.5f,-1),new Vector3(.2f,1.25f,1.25f),dark,false);Sign("00:00",new Vector3(5.43f,3.5f,-1),new Vector3(0,-90,0),4,new Color(.94f,.70f,.35f));}
        [MenuItem("Platform Zero/Build Windows Game")]
        public static void BuildWindows()
        {
            if(!File.Exists(ScenePath)) BuildScene();
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth=1280;
            PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.resizableWindow=true;
            PlayerSettings.bundleVersion="1.0.0";
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/Windows");
            var report=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Windows/PlatformZero.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
            Directory.CreateDirectory("Validation");
            File.WriteAllText("Validation/windows-build.txt",$"Result: {report.summary.result}\nUnity: {Application.unityVersion}\nTarget: Windows x86_64\nScene: {ScenePath}\nDuration: {report.summary.totalTime}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nSize: {report.summary.totalSize} bytes\n");
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Windows build failed: "+report.summary.result);
        }
    }
}

#endif


