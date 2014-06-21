using System;
using System.Linq;
using UnityEngine;

namespace EnhancedNavBall.Config
{
    class ConfigurationWindow
    {
        private Texture2D _iconTexture; //= new Texture2D(32, 32, TextureFormat.ARGB32, false);
        private static int _windowId = 0;
        private readonly Rect _windowPos = new Rect(3, 36, 300, 45);
        private readonly System.Random _rnd = new System.Random();
        private const int _windowWidth = 320;
        private const long _windowHeight = 130;
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

            _screenRect = new Rect(
                _windowPos.x + _windowPos.width,
                _windowPos.y,
                _windowWidth,
                _windowHeight);
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
                    _windowId,
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
    }
}
