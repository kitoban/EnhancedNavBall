using System;
using System.Linq;
using UnityEngine;

namespace EnhancedNavBall.Config
{
    class ConfigurationWindow
    {
        public Texture2D _iconTexture; //= new Texture2D(32, 32, TextureFormat.ARGB32, false);
        private static int _WindowAddID = 0;
        static Rect _WindowAddRect;
        private readonly Rect WindowPosByActiveScene = new Rect(3, 36, 300, 45);
        System.Random rnd = new System.Random();
        int intPaneWindowWidth = 380;
        int intAddPaneWindowWidth = 320;
        long AddWindowHeight = 200;
        private bool _setupComplete;
        private readonly NavballSettings _settings;
        private Rect _screenRect;
        private bool _windowVisibleByActiveScene;
        private bool? _toolbarManagerLoaded;
        private ToolbarButtonWrapper _toolbarButton;

        private float _navballLeftLimit;// = 0.45f;
        private float _navballRightLimit;// = ScreenSafeUI.fetch.rightAnchor.bottom.position.x - 0.1f;// 2.17f;
        private ScreenSafeUI _screenSafeUi;

        public ConfigurationWindow(
            NavballSettings navballSettings,
            ScreenSafeUI screenSafeUi)
        {
            _screenSafeUi = screenSafeUi;
            var rightX = _screenSafeUi.rightAnchor.bottom.position.x;
            var leftX = _screenSafeUi.leftAnchor.bottom.position.x;
            _navballRightLimit = rightX - (rightX / 5);
            _navballLeftLimit = leftX + (rightX / 6f);

            _settings = navballSettings;

            _screenRect = new Rect(
                WindowPosByActiveScene.x + WindowPosByActiveScene.width,
                WindowPosByActiveScene.y,
                intAddPaneWindowWidth,
                AddWindowHeight);
        }

        internal void BuildIcon()
        {
            _toolbarManagerLoaded = null;

            if (_toolbarManagerLoaded.HasValue == false)
                _toolbarManagerLoaded = ToolbarDLL.Loaded;

            if (_toolbarManagerLoaded.Value)
            {
                if ((_toolbarButton == null))
                {
                    try
                    {
                        _toolbarButton = new ToolbarButtonWrapper("EnhancedNavball", "btnEnhancedNavballToolbarIcon");
                        _toolbarButton.TexturePath = "EnhancedNavBall/Resources/navball24";
                        _toolbarButton.ToolTip = "Enhanced Navball";
                        _toolbarButton.AddButtonClickHandler(e => ToggleButton());
                    }
                    catch (Exception)
                    {
                        DestroyToolbarButton();
                    }
                }
            }
            else
            {
                if (_iconTexture == null)
                    _iconTexture = GameDatabase.Instance.GetTexture("EnhancedNavBall/Resources/navball32", false);
            }
        }

        public void DrawUi()
        {
            DoSetup();

            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            DrawWindows();
            DrawIcons();
        }

        private void DrawIcons()
        {
            
            if (_toolbarButton == null)
            {
                if (GUI.Button(
                    _settings.IconPos,
                    new GUIContent(
                        _iconTexture,
                        "Click to Toggle"),
                    BasicResources.styleIconStyle))
                {
                    ToggleButton();
                }
            }

        }

        internal void DestroyToolbarButton()
        {
            if (_toolbarButton != null)
            {
                _toolbarButton.Destroy();
            }
            _toolbarButton = null;
        }

        private void ToggleButton()
        {
            _settings.WindowVisibleByActiveScene = _settings.WindowVisibleByActiveScene == false;
            _settings.Save();
        }

        private void DrawWindows()
        {
            if (_settings.WindowVisibleByActiveScene)
            {
                _screenRect = GUILayout.Window(
                    _WindowAddID,
                    _screenRect,
                    FillAddWindow,
                    "Enhanced Navball Settings",
                    BasicResources.styleWindow);
            }
        }

        private void DoSetup()
        {
            if (_setupComplete)
                return;

            _WindowAddID = rnd.Next(1000, 2000000);
            GUI.skin = HighLogic.Skin;
            if (BasicResources.styleWindow == null)
            {
                BasicResources.SetStyles();
                //styleWindow = new GUIStyle(GUI.skin.window);
            }

            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Settings DoSetup");

            _setupComplete = true;
        }

        public void FillAddWindow(int windowID)
        {
            //Preferences
            GUILayout.Label("Plugin Preferences", BasicResources.StyleSectionHeading, GUILayout.ExpandWidth(true));
            GUILayout.BeginVertical(BasicResources.styleAddFieldAreas);

            AddSlideToUi();
            AddScaleToUi();
            //DebugInfo();

            //AddAdditionToUi();

            GUILayout.EndVertical();

        }

        private void AddAdditionToUi()
        {
            GUILayout.Label(
                "Ghost Position",
                BasicResources.StyleSectionHeading,
                GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal();

            if (DrawHorizontalSlider(
                ref _settings.GhostPosition,
                0.01f,
                0.3f,
                BasicResources.StyleSlider,
                BasicResources.StyleSliderThumb,
                GUILayout.ExpandWidth(true)))
            {
                _settings.Save();
            }

            GUILayout.EndHorizontal();
        }

        private void AddSlideToUi()
        {
            GUILayout.Label(
                "Navball horizontal position",
                BasicResources.StyleSectionHeading,
                GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal();

            if (DrawHorizontalSlider(
                ref _settings.NavballPosition,
                _navballLeftLimit,
                _navballRightLimit,
                BasicResources.StyleSlider,
                BasicResources.StyleSliderThumb,
                GUILayout.ExpandWidth(true)))
            {
                SlidingNavBall.UpdateNavballPostion(
                    _screenSafeUi,
                    _settings.NavballPosition);
                _settings.Save();
            }

            if (GUILayout.Button(
                "Reset",
                BasicResources.StyleButton,
                GUILayout.ExpandWidth(false)))
            {
                _settings.NavballPosition = SlidingNavBall.ResetNavballPostion(_screenSafeUi);
                _settings.Save();
            }

            GUILayout.EndHorizontal();
        }

        private void AddScaleToUi()
        {
            GUILayout.Label(
                "Navball Scale",
                BasicResources.StyleSectionHeading,
                GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal();

            if (DrawHorizontalSlider(
                ref _settings.NavballScale,
                0.6f,
                2.3f,
                BasicResources.StyleSlider,
                BasicResources.StyleSliderThumb,
                GUILayout.ExpandWidth(true)))
            {
                ScalingNavBall.UpdateNavballScale(_settings.NavballScale);
                _settings.Save();
            }

            if (GUILayout.Button(
                "Reset",
                BasicResources.StyleButton,
                GUILayout.ExpandWidth(false)))
            {
                _settings.NavballScale = ScalingNavBall.ResetNavballScale();
                _settings.Save();
            }

            GUILayout.EndHorizontal();
        }

        private void DebugInfo()
        {
            GUILayout.Label(
                "Debug Call",
                BasicResources.StyleSectionHeading,
                GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal();

            //if (DrawHorizontalSlider(
            //    ref _settings.NavballRotation,
            //    0.0f,
            //    90f,
            //    BasicResources.StyleSlider,
            //    BasicResources.StyleSliderThumb,
            //    GUILayout.ExpandWidth(true)))
            //{
            //    ScalingNavBall.UpdateNavballRotation(_settings.NavballRotation);
            //    _settings.Save();
            //}

            var findObjectsOfType = UnityEngine.Object.FindObjectsOfType(typeof(FlightUIController));

            if (findObjectsOfType.Any())
            {
                foreach (var o in findObjectsOfType)
                {
                    GUILayout.Label(
                        o.name,
                        BasicResources.StyleSectionHeading,
                        GUILayout.ExpandWidth(false));
                }
            }
            //Object.FindObjectsOfType 

            if (GUILayout.Button(
                "Run debug call",
                BasicResources.StyleButton,
                GUILayout.ExpandWidth(false)))
            {
                DebugUtilities.DebugCall();
                //_settings.Save();
            }

            GUILayout.EndHorizontal();
        }

        public bool DrawHorizontalSlider(ref float val, float leftLimit, float rightLimit, GUIStyle sliderStyle, GUIStyle thumbStyle, params GUILayoutOption[] options)
        {
            float ret = GUILayout.HorizontalSlider(val, leftLimit, rightLimit, sliderStyle, thumbStyle, options);
            if (ret != val)
            {
                val = ret;
                return true;
            }
            return false;
        }

        public Boolean DrawTextBox(ref String strVar, GUIStyle style, params GUILayoutOption[] options)
        {
            String strReturn = GUILayout.TextField(strVar, style, options);
            if (strReturn != strVar)
            {
                strVar = strReturn;
                //DebugLogFormatted("String Changed:" + strVar.ToString());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a toggle button like a checkbox
        /// </summary>
        /// <param name="blnVar"></param>
        /// <returns>True when check state has changed</returns>
        public Boolean DrawCheckbox(ref Boolean blnVar, String strText, params GUILayoutOption[] options)
        {
            return DrawCheckbox(ref blnVar, new GUIContent(strText), 15, options);
        }
        public Boolean DrawCheckbox(ref Boolean blnVar, GUIContent content, params GUILayoutOption[] options)
        {
            return DrawCheckbox(ref blnVar, content, 15, options);
        }
        public Boolean DrawCheckbox(ref Boolean blnVar, String strText, int CheckboxSpace, params GUILayoutOption[] options)
        {
            return DrawCheckbox(ref blnVar, new GUIContent(strText), CheckboxSpace, options);
        }
        //CHANGED
        /// <summary>
        /// Draws a toggle button like a checkbox
        /// </summary>
        /// <param name="blnVar"></param>
        /// <returns>True when check state has changed</returns>
        public Boolean DrawCheckbox(ref Boolean blnVar, GUIContent content, int CheckboxSpace, params GUILayoutOption[] options)
        {
            // return DrawToggle(ref blnVar, strText, KACResources.styleCheckbox, options);
            Boolean blnReturn = false;
            Boolean blnToggleInitial = blnVar;

            GUILayout.BeginHorizontal();
            //DrawWindows the radio
            DrawToggle(ref blnVar, "", BasicResources.styleCheckbox, options);
            //Spacing
            GUILayout.Space(CheckboxSpace);

            //And the button like a label
            if (GUILayout.Button(content, BasicResources.styleCheckboxLabel, options))
            {
                //if its clicked then toggle the boolean
                blnVar = !blnVar;
                //KACWorker.DebugLogFormatted("Toggle Changed:" + blnVar);
            }

            GUILayout.EndHorizontal();

            //If output value doesnt = input value
            if (blnToggleInitial != blnVar)
            {
                //KACWorker.DebugLogFormatted("Toggle recorded:" + blnVar);
                blnReturn = true;
            }
            return blnReturn;
        }

        public Boolean DrawToggle(ref Boolean blnVar, String ButtonText, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, ButtonText, style, options);

            return ToggleResult(ref blnVar, ref  blnReturn);
        }

        public Boolean DrawToggle(ref Boolean blnVar, Texture image, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, image, style, options);

            return ToggleResult(ref blnVar, ref blnReturn);
        }

        public Boolean DrawToggle(ref Boolean blnVar, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, content, style, options);

            return ToggleResult(ref blnVar, ref blnReturn);
        }

        private Boolean ToggleResult(ref Boolean Old, ref Boolean New)
        {
            if (Old != New)
            {
                Old = New;
                //DebugLogFormatted("Toggle Changed:" + New.ToString());
                return true;
            }
            return false;
        }

        public Boolean DrawRadioList(ref int Selected, params String[] Choices)
        {
            return DrawRadioList(true, ref Selected, Choices);
        }
        public Boolean DrawRadioList(Boolean Horizontal, ref int Selected, params String[] Choices)
        {
            int InitialChoice = Selected;

            if (Horizontal)
                GUILayout.BeginHorizontal();
            else
                GUILayout.BeginVertical();

            for (int intChoice = 0; intChoice < Choices.Length; intChoice++)
            {
                //checkbox
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle((intChoice == Selected), "", BasicResources.styleCheckbox))
                    Selected = intChoice;
                //button that looks like a label
                if (GUILayout.Button(Choices[intChoice], BasicResources.styleCheckboxLabel))
                    Selected = intChoice;
                GUILayout.EndHorizontal();
            }
            if (Horizontal)
                GUILayout.EndHorizontal();
            else
                GUILayout.EndVertical();

            //if (InitialChoice != Selected)
            //    DebugLogFormatted(String.Format("Radio List Changed:{0} to {1}", InitialChoice, Selected));


            return !(InitialChoice == Selected);
        }
    }
}
