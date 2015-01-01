using System;
using System.Linq;
using UnityEngine;

namespace EnhancedNavBall.Config
{
    class ConfigurationWindow
    {
        private Texture2D _iconTexture; //= new Texture2D(32, 32, TextureFormat.ARGB32, false);
        private static int _windowId = 0;
        private Rect _windowPos = new Rect(3, 36, 300, 45);
        private readonly System.Random _rnd = new System.Random();
        private const int _windowWidth = 320;
        private const long _windowHeight = 130;
        //private const int _windowWidth = 600;//320;
        //private const long _windowHeight = 600;//130;
        private bool _setupComplete;
        private readonly NavballSettings _settings;
        private Rect _screenRect;
        private bool? _toolbarManagerLoaded;
        private ToolbarButtonWrapper _toolbarButton;

        private readonly float _navballLeftLimit;
        private readonly float _navballRightLimit;


        public ConfigurationWindow(
            NavballSettings navballSettings)
        {
            var screenSafeUi = References.Instance.ScreenSafeUi;
            var rightX = screenSafeUi.rightAnchor.bottom.position.x;
            var leftX = screenSafeUi.leftAnchor.bottom.position.x;
            _navballRightLimit = rightX - (rightX / 5);
            _navballLeftLimit = leftX + (rightX / 6f);

            _settings = navballSettings;

            _windowPos = new Rect(
                _windowPos.x + _windowPos.width,
                _windowPos.y,
                _windowWidth,
                _windowHeight);

            //BuildColourPickerTexture();
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
                _windowPos = GUILayout.Window(
                    _windowId,
                    _windowPos,
                    FillWindow,
                    "Enhanced Navball Settings",
                    BasicResources.styleWindow);
            }
        }

        private void DoSetup()
        {
            if (_setupComplete)
                return;

            _windowId = _rnd.Next(1000, 2000000);
            GUI.skin = HighLogic.Skin;
            if (BasicResources.styleWindow == null)
            {
                BasicResources.SetStyles();
                //styleWindow = new GUIStyle(GUI.skin.window);
            }

            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Settings DoSetup");

            _setupComplete = true;
        }

        public void FillWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            //Preferences
            GUILayout.Label("Plugin Preferences", BasicResources.StyleSectionHeading, GUILayout.ExpandWidth(true));
            GUILayout.BeginVertical(BasicResources.styleAddFieldAreas);

            AddSlideToUi();
            AddScaleToUi();
            AddManeuverToUi();
            AddRadialNormalDuringManeuverUi();
            //AddColourPickerToUi();

            //DebugInfo();

            //AddAdditionToUi();

            GUILayout.EndVertical();

        }

        public int positionLeft = 0;
        public int positionTop = 300;
        private Texture2D displayPicker;
        private Color lastSetColor;
        private float saturationSlider = 0.5F;
        private float previousSaturationSlider = 0.5F;
        public bool showPicker = true;
        private Texture2D _colorPicker;
        public const int textureWidth = 360;
        public const int textureHeight = 180;
        private Texture2D saturationTexture;
        private Texture2D styleTexture;
        public Color setColor;

        private void AddColourPickerToUi()
        {
            if (previousSaturationSlider != saturationSlider)
            {
                previousSaturationSlider = saturationSlider;
                UpdateColourPicker();
            }


            if (!showPicker) return;
            GUI.Box(new Rect(positionLeft - 3, positionTop - 3, textureWidth + 60, textureHeight + 60), "");

            if (GUI.RepeatButton(new Rect(positionLeft, positionTop, textureWidth, textureHeight), displayPicker))
            {
                int a = (int)Input.mousePosition.x;
                int b = Screen.height - (int)Input.mousePosition.y;

                setColor = displayPicker.GetPixel(a - positionLeft, -(b - positionTop));
                lastSetColor = setColor;
            }

            saturationSlider = GUI.VerticalSlider(new Rect(positionLeft + textureWidth + 3, positionTop, 10, textureHeight), saturationSlider, 1, -1);
            setColor = lastSetColor + new Color(saturationSlider, saturationSlider, saturationSlider);
            GUI.Box(new Rect(positionLeft + textureWidth + 20, positionTop, 20, textureHeight), saturationTexture);

            if (GUI.Button(new Rect(positionLeft + textureWidth - 60, positionTop + textureHeight + 10, 60, 25), "Apply"))
            {
                setColor = styleTexture.GetPixel(0, 0);

                // hide picker
                showPicker = false;
            }

            // color display
            GUIStyle style = new GUIStyle();
            styleTexture.SetPixel(0, 0, setColor);
            styleTexture.Apply();

            style.normal.background = styleTexture;
            GUI.Box(new Rect(positionLeft + textureWidth + 10, positionTop + textureHeight + 10, 30, 30), new GUIContent(""), style);

        }

        private void UpdateColourPicker()
        {
            ColorHSV hsvColor;
            for (int i = 0; i < textureWidth; i++)
            {
                for (int j = 0; j < textureHeight; j++)
                {
                    hsvColor = new ColorHSV((float)i, (1.0f / j) * textureHeight, saturationSlider);
                    _colorPicker.SetPixel(i, j, hsvColor.ToColor());
                }
            }
            _colorPicker.Apply();
            displayPicker = _colorPicker;
            displayPicker.Apply();
        }
        private void BuildColourPickerTexture()
        {
            //https://gist.github.com/boj/1181465

            _colorPicker = new Texture2D(
                textureWidth,
                textureHeight,
                TextureFormat.ARGB32,
                false);
            UpdateColourPicker();

            //if (!useDefinedSize)
            //{
            //    textureWidth = _colorPicker.width;
            //    textureHeight = _colorPicker.height;
            //}

            float v = 0.0F;
            float diff = 1.0f / textureHeight;
            saturationTexture = new Texture2D(20, textureHeight);
            for (int i = 0; i < saturationTexture.width; i++)
            {
                for (int j = 0; j < saturationTexture.height; j++)
                {
                    saturationTexture.SetPixel(i, j, new Color(v, v, v));
                    v += diff;
                }
                v = 0.0F;
            }
            saturationTexture.Apply();

            // small color picker box texture
            styleTexture = new Texture2D(1, 1);
            styleTexture.SetPixel(0, 0, setColor);
        }



        private void AddRadialNormalDuringManeuverUi()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Radial/Normal vectors during Maneuver",
                BasicResources.StyleSectionHeading,
                GUILayout.ExpandWidth(true));

            _settings.RadialNormalDuringManeuver = GUILayout.Toggle(
                _settings.RadialNormalDuringManeuver,
                _settings.RadialNormalDuringManeuver ? "Disable" : "Enable",
                BasicResources.StyleButton,
                GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        private void AddManeuverToUi()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "ENB Maneuver Pointer",
                BasicResources.StyleSectionHeading,
                GUILayout.ExpandWidth(true));

            _settings.ENBManeuver = GUILayout.Toggle(
                _settings.ENBManeuver,
                _settings.ENBManeuver ? "Disable" : "Enable",
                BasicResources.StyleButton,
                GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        private void AddGhostToUi()
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
                    References.Instance.ScreenSafeUi,
                    _settings.NavballPosition);
                _settings.Save();
            }

            if (GUILayout.Button(
                "Reset",
                BasicResources.StyleButton,
                GUILayout.ExpandWidth(false)))
            {
                _settings.NavballPosition = SlidingNavBall.ResetNavballPostion();
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
                Utilities.DebugCall();
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
    }
}
