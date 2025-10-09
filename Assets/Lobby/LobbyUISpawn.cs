using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NetSample.LobbyKit {
    public class LobbyUISpawn : MonoBehaviour {
        [Header("Options visuals")]
        public Vector2 referenceResolution = new(1920,1080);
        public Font defaultFont; // optionnel pour boutons/legacy; TMP gère sa propre font

        Canvas _canvas;
        LobbyUIController _ui;
        RelayBootstrap _relay;
        LobbyRowItem _rowPrefab;

        void Awake(){ BuildAll(); }

        void BuildAll(){
            EnsureEventSystem();
            _canvas = CreateCanvas("Canvas.Lobby");
            var panel = CreatePanel(_canvas.transform, "Panel.Root", new Color(0.07f,0.08f,0.09f,0.92f));

            // Header
            var header = CreateHorizontal(panel, "Header", 16, 16);
            var title = CreateTMP(header.transform, "Lobbies", 42, TextAlignmentOptions.Left);
            (title.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1400);
            var refreshBtn = CreateButton(header.transform, "Rafraîchir");

            // Body split (2 columns)
            var body = CreateGrid(panel, "Body", 2, 1, 16);

            // Waiting column
            var colWaiting = CreateVertical((RectTransform)body.transform, "Column.Waiting", 8, 8);
            CreateTMP(colWaiting.transform, "En attente", 28, TextAlignmentOptions.Left, FontStyles.Bold);
            var waitingScroll = CreateScroll(colWaiting.transform, out var waitingContent);

            // InProgress column
            var colProgress = CreateVertical((RectTransform)body.transform, "Column.InProgress", 8, 8);
            CreateTMP(colProgress.transform, "En cours", 28, TextAlignmentOptions.Left, FontStyles.Bold);
            var inProgScroll = CreateScroll(colProgress.transform, out var inProgContent);

            // Footer Create panel
            var footer = CreateVertical(panel, "Footer.Create", 12, 16, new Color(0.12f,0.13f,0.15f,1f));
            var nameInput = CreateTMPInput(footer.transform, "Nom de la partie");
            var modeDropdown = CreateTMPDropdown(footer.transform, new []{"FFA","1v1","Coop"});
            var (slider,label) = CreateSliderWithLabel(footer.transform, 2, 8, 4, "Joueurs max");
            var regionDropdown = CreateTMPDropdown(footer.transform, new []{"auto","na","eu","asia"});
            var privateToggle = CreateToggle(footer.transform, "Partie privée");
            var createBtn = CreateButton(footer.transform, "Créer une partie");

            // Relay/NGO helper
            _relay = _canvas.gameObject.AddComponent<RelayBootstrap>();

            // Row prefab in-memory
            _rowPrefab = CreateRowPrefab();

            // Attach controller + wire refs
            _ui = _canvas.gameObject.AddComponent<LobbyUIController>();
            _ui.GetType().GetField("waitingContent", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, waitingContent);
            _ui.GetType().GetField("inProgressContent", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, inProgContent);
            _ui.GetType().GetField("rowPrefab", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, _rowPrefab);
            _ui.GetType().GetField("nameInput", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, nameInput);
            _ui.GetType().GetField("modeDropdown", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, modeDropdown);
            _ui.GetType().GetField("maxPlayersSlider", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, slider);
            _ui.GetType().GetField("maxPlayersLabel", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, label);
            _ui.GetType().GetField("regionDropdown", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, regionDropdown);
            _ui.GetType().GetField("privateToggle", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, privateToggle);
            _ui.GetType().GetField("createButton", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, createBtn);
            _ui.GetType().GetField("refreshButton", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, refreshBtn);
            _ui.GetType().GetField("relayBootstrap", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(_ui, _relay);
        }

        // ===== Helpers =====
        void EnsureEventSystem(){ if (!FindObjectOfType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }
        Canvas CreateCanvas(string name){
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.pixelPerfect = true;
            var cs = go.GetComponent<CanvasScaler>(); cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; cs.referenceResolution = referenceResolution;
            return c;
        }
        RectTransform CreatePanel(Transform parent, string name, Color? bg = null){
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>(); img.color = bg ?? new Color(0,0,0,0);
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16,16,16,16);
            layout.spacing = 16;
            layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
            return rt;
        }
        RectTransform CreateHorizontal(RectTransform parent, string name, int pad = 8, int spacing = 8){
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent,false);
            var hl = go.GetComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(pad,pad,pad,pad);
            hl.spacing = spacing;
            hl.childControlHeight = true; hl.childControlWidth = true;
            hl.childForceExpandHeight = false;
            var img = go.AddComponent<Image>();
            img.color = new Color(1,1,1,0.02f);
            return go.GetComponent<RectTransform>();
        }
        RectTransform CreateVertical(RectTransform parent, string name, int pad = 8, int spacing = 8, Color? bg=null){
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent,false);
            var vl = go.GetComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(pad,pad,pad,pad);
            vl.spacing = spacing;
            vl.childControlHeight = true; vl.childControlWidth = true;
            vl.childForceExpandHeight = false;
            var img = go.AddComponent<Image>();
            img.color = bg ?? new Color(1,1,1,0.02f);
            return go.GetComponent<RectTransform>();
        }
        RectTransform CreateGrid(RectTransform parent, string name, int cols, int rows, int spacing){
            var go = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup));
            go.transform.SetParent(parent,false);
            var gl = go.GetComponent<GridLayoutGroup>();
            gl.spacing = new Vector2(spacing, spacing);
            gl.cellSize = new Vector2(0,0);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = cols;
            return go.GetComponent<RectTransform>();
        }
        Button CreateButton(Transform parent, string text){
            var go = new GameObject($"Button.{text}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color = new Color(0.24f,0.5f,0.8f,1f);
            var label = CreateTMP(go.transform, text, 24, TextAlignmentOptions.Center, FontStyles.Bold);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            return go.GetComponent<Button>();
        }
        Toggle CreateToggle(Transform parent, string labelText){
            var root = new GameObject($"Toggle.{labelText}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            root.transform.SetParent(parent,false);
            var hl = root.GetComponent<HorizontalLayoutGroup>(); hl.spacing = 8; hl.childAlignment = TextAnchor.MiddleLeft;
            var go = new GameObject("Check", typeof(RectTransform), typeof(Image), typeof(Toggle));
            go.transform.SetParent(root.transform,false);
            var bg = go.GetComponent<Image>(); bg.color = new Color(0.2f,0.2f,0.25f,1f);
            var t = go.GetComponent<Toggle>();
            var mark = new GameObject("Mark", typeof(RectTransform), typeof(Image));
            mark.transform.SetParent(go.transform,false);
            mark.GetComponent<Image>().color = Color.white;
            mark.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f,0.2f);
            mark.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f,0.8f);
            t.graphic = mark.GetComponent<Image>(); t.targetGraphic = bg;
            CreateTMP(root.transform, labelText, 22, TextAlignmentOptions.Left);
            return t;
        }
        (Slider,TMP_Text) CreateSliderWithLabel(Transform parent, int min, int max, int value, string prefix){
            var root = CreateVertical(parent as RectTransform, $"Slider.{prefix}", 0, 6);
            var label = CreateTMP(root, $"{prefix}: {value}", 22, TextAlignmentOptions.Left);
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(root,false);
            var slider = go.GetComponent<Slider>();
            slider.minValue = min; slider.maxValue = max; slider.value = value; slider.wholeNumbers = true;
            slider.onValueChanged.AddListener(v=> label.text = $"{prefix}: {(int)v}");
            return (slider,label);
        }
        TMP_Dropdown CreateTMPDropdown(Transform parent, IEnumerable<string> options){
            var root = new GameObject("TMP_Dropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            root.transform.SetParent(parent,false);
            var dd = root.GetComponent<TMP_Dropdown>();
            dd.ClearOptions(); dd.AddOptions(new List<string>(options));
            var label = CreateTMP(root.transform, dd.options[0].text, 22, TextAlignmentOptions.Left);
            dd.captionText = label;
            return dd;
        }
        TMP_InputField CreateTMPInput(Transform parent, string placeholder){
            var root = new GameObject("TMP_InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            root.transform.SetParent(parent,false);
            var input = root.GetComponent<TMP_InputField>();
            var text = CreateTMP(root.transform, "", 22, TextAlignmentOptions.Left);
            input.textComponent = text;
            var ph = CreateTMP(root.transform, placeholder, 22, TextAlignmentOptions.Left, FontStyles.Italic, new Color(1,1,1,0.35f));
            input.placeholder = ph;
            return input;
        }
        TMP_Text CreateTMP(Transform parent, string txt, int size, TextAlignmentOptions align, FontStyles style = FontStyles.Normal, Color? color=null){
            var go = new GameObject("TMP", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent,false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = txt; t.fontSize = size; t.alignment = align; t.fontStyle = style; t.color = color ?? Color.white;
            return t;
        }
        LobbyRowItem CreateRowPrefab(){
            var root = new GameObject("LobbyRowItem", typeof(RectTransform), typeof(Image));
            var img = root.GetComponent<Image>(); img.color = new Color(1,1,1,0.04f);
            var vl = root.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(12,12,10,10); vl.spacing = 4;
            var title = CreateTMP(root.transform, "Partie", 26, TextAlignmentOptions.Left, FontStyles.Bold);
            var meta  = CreateTMP(root.transform, "FFA • 0/4 • na", 20, TextAlignmentOptions.Left);
            var h = CreateHorizontal(root.GetComponent<RectTransform>(), "RowBtns", 0, 8);
            var status = CreateTMP(h, "waiting", 20, TextAlignmentOptions.Left);
            var joinBtn = CreateButton(h, "Rejoindre");

            var comp = root.AddComponent<LobbyRowItem>();
            comp.GetType().GetField("title", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(comp, title);
            comp.GetType().GetField("meta", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(comp, meta);
            comp.GetType().GetField("status", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(comp, status);
            comp.GetType().GetField("joinButton", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .SetValue(comp, joinBtn);

            root.SetActive(false);
            return comp;
        }

        RectTransform CreateScroll(Transform parent, out RectTransform content){
            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scroll.transform.SetParent(parent,false);
            var sr = scroll.GetComponent<ScrollRect>();
            var img = scroll.GetComponent<Image>(); img.color = new Color(0.1f,0.1f,0.12f,1f);
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(scroll.transform,false);
            viewport.GetComponent<Image>().color = new Color(0,0,0,0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewport.transform,false);
            var vlg = contentGO.GetComponent<VerticalLayoutGroup>(); vlg.spacing = 4; vlg.padding = new RectOffset(8,8,8,8);
            var csf = contentGO.GetComponent<ContentSizeFitter>(); csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = contentGO.GetComponent<RectTransform>();
            sr.viewport = viewport.GetComponent<RectTransform>();
            sr.horizontal = false;
            content = contentGO.GetComponent<RectTransform>();
            return scroll.GetComponent<RectTransform>();
        }
    }
}
