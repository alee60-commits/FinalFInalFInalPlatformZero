#if PLATFORM_ZERO_YARN
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Yarn.Unity;

namespace PlatformZero
{
    public sealed class StationHud : MonoBehaviour
    {
        private TMP_FontAsset font;
        public TMP_FontAsset Font => font;
        private RectTransform canvas;
        private GameObject dialogue;
        private GameObject menu;
        private GameObject choices;
        private TMP_Text speaker, line, objective, prompt, menuTitle, menuBody;
        private UnityEngine.UI.Button advance, primary;
        private TMP_Text primaryLabel;
        public bool ContinueRequested { get; set; }
        public int SelectedChoice { get; private set; } = -1;
        private readonly List<GameObject> choiceObjects = new List<GameObject>();
        private static readonly Color Cream = new Color(.94f,.92f,.84f);
        private static readonly Color Amber = new Color(.96f,.69f,.28f);

        public void Initialize(StationGame game)
        {
            font = TMP_Settings.defaultFontAsset;
            var root = new GameObject("Station UI", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<RectTransform>();
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600,900); scaler.matchWidthOrHeight = .5f;
            var events = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            Text("Brand", canvas, "PLATFORM  /  0", 21, Amber, new Vector2(40,-36), new Vector2(400,35), new Vector2(0,1));
            objective = Text("Objective", canvas, "", 22, Cream, new Vector2(40,-77), new Vector2(900,40), new Vector2(0,1));
            Text("Controls", canvas, "WASD  MOVE     MOUSE  LOOK     E  INTERACT     ESC  PAUSE", 17, Cream, new Vector2(40,30), new Vector2(1000,30), Vector2.zero);
            Text("Crosshair", canvas, "+", 22, Cream, Vector2.zero, new Vector2(25,25), new Vector2(.5f,.5f)).alignment = TextAlignmentOptions.Center;
            prompt = Text("Interaction", canvas, "", 24, Cream, new Vector2(0,-65), new Vector2(900,45), new Vector2(.5f,.5f));
            prompt.alignment = TextAlignmentOptions.Center;

            var panel = Panel("Dialogue", canvas, new Color(.025f,.06f,.075f,.98f));
            panel.anchorMin = new Vector2(.10f,.075f); panel.anchorMax = new Vector2(.90f,.36f); panel.offsetMin = Vector2.zero; panel.offsetMax = Vector2.zero;
            dialogue = panel.gameObject;
            var strip = Panel("Accent", panel, Amber); Stretch(strip); strip.anchorMax = new Vector2(.005f,1);
            speaker = Text("Speaker", panel, "", 23, Amber, new Vector2(32,-20), new Vector2(1100,35), new Vector2(0,1));
            line = Text("Line", panel, "", 29, Cream, new Vector2(32,-67), new Vector2(1100,105), new Vector2(0,1));
            line.textWrappingMode = TextWrappingModes.Normal;
            advance = Button("Continue", panel, "CONTINUE  [SPACE]", new Vector2(-28,20), new Vector2(250,48), new Vector2(1,0), () => ContinueRequested = true, out _);
            var choiceRoot = new GameObject("Choices",typeof(RectTransform));
            choiceRoot.transform.SetParent(panel,false); choices = choiceRoot; Stretch(choiceRoot.GetComponent<RectTransform>());
            dialogue.SetActive(false);

            var overlay = Panel("Menu", canvas, new Color(.015f,.035f,.045f,.96f)); Stretch(overlay); menu = overlay.gameObject;
            Text("Eyebrow", overlay, "A SHORT NARRATIVE GAME", 20, Amber, new Vector2(0,225),new Vector2(1100,35),new Vector2(.5f,.5f)).alignment = TextAlignmentOptions.Center;
            menuTitle = Text("Title", overlay,"",68,Cream,new Vector2(0,130),new Vector2(1300,100),new Vector2(.5f,.5f)); menuTitle.alignment = TextAlignmentOptions.Center;
            menuBody = Text("Description",overlay,"",25,Cream,new Vector2(0,5),new Vector2(1100,150),new Vector2(.5f,.5f)); menuBody.alignment = TextAlignmentOptions.Center;
            primary = Button("Primary",overlay,"",new Vector2(0,-145),new Vector2(320,60),new Vector2(.5f,.5f),()=>{},out primaryLabel);
            Button("Restart",overlay,"Restart",new Vector2(-110,-225),new Vector2(200,48),new Vector2(.5f,.5f),game.Restart,out _);
            Button("Quit",overlay,"Quit",new Vector2(110,-225),new Vector2(200,48),new Vector2(.5f,.5f),StationGame.Quit,out _);
        }
        public void SetObjective(string value) { objective.text = value; }
        public void SetPrompt(string value) { prompt.text = value; }
        public void ShowDialogue(bool show) { dialogue.SetActive(show); if (!show) ClearChoices(); }
        public void SetLine(string name, string text) { ClearChoices(); speaker.text = name.ToUpperInvariant(); line.text = text; advance.gameObject.SetActive(true); ContinueRequested = false; }
        public void SetChoices(List<DialogueOption> options)
        {
            ClearChoices(); advance.gameObject.SetActive(false); speaker.text = "YOUR RESPONSE"; line.text = "";
            for (int i=0;i<options.Count;i++)
            {
                int index=i;
                var button=Button("Choice "+i,choices.transform,"["+(i+1)+"]  "+options[i].Line.TextWithoutCharacterName.Text,
                    new Vector2(32,-70-i*68),new Vector2(1100,58),new Vector2(0,1),()=>SelectedChoice=index,out _);
                choiceObjects.Add(button.gameObject);
            }
        }
        public void ClearChoices() { foreach(var obj in choiceObjects) { obj.SetActive(false); Destroy(obj); } choiceObjects.Clear(); SelectedChoice=-1; }
        public void ShowMenu(string title,string body,string action,Action callback)
        { menuTitle.text=title; menuBody.text=body; primaryLabel.text=action; primary.onClick.RemoveAllListeners(); primary.onClick.AddListener(()=>callback()); menu.SetActive(true); EventSystem.current.SetSelectedGameObject(primary.gameObject); }
        public void HideMenu() { menu.SetActive(false); EventSystem.current.SetSelectedGameObject(null); }
        private TMP_Text Text(string name,Transform parent,string value,float size,Color color,Vector2 position,Vector2 dimensions,Vector2 anchor)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); obj.transform.SetParent(parent,false);
            Place(obj.GetComponent<RectTransform>(),position,dimensions,anchor);
            var text=obj.GetComponent<TextMeshProUGUI>(); text.font=font; text.fontSize=size; text.color=color; text.text=value; text.raycastTarget=false; return text;
        }
        private UnityEngine.UI.Button Button(string name,Transform parent,string value,Vector2 position,Vector2 dimensions,Vector2 anchor,Action action,out TMP_Text label)
        {
            var rect=Panel(name,parent,new Color(.10f,.22f,.24f)); Place(rect,position,dimensions,anchor);
            rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=rect.GetComponent<UnityEngine.UI.Image>();
            var colors=button.colors; colors.highlightedColor=new Color(.85f,.76f,.5f); colors.selectedColor=colors.highlightedColor; button.colors=colors;
            button.onClick.AddListener(()=>action());
            label=Text("Label",rect,value,22,Cream,Vector2.zero,dimensions,new Vector2(.5f,.5f)); label.alignment=TextAlignmentOptions.Center;
            return button;
        }
        private static RectTransform Panel(string name,Transform parent,Color color)
        { var obj=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); obj.transform.SetParent(parent,false); var image=obj.GetComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=false; return obj.GetComponent<RectTransform>(); }
        private static void Place(RectTransform rect,Vector2 position,Vector2 size,Vector2 anchor) { rect.anchorMin=rect.anchorMax=rect.pivot=anchor; rect.anchoredPosition=position; rect.sizeDelta=size; }
        private static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
}

#endif
