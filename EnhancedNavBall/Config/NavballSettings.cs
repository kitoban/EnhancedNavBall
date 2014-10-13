
using System;
using UnityEngine;

namespace EnhancedNavBall.Config
{
    class NavballSettings
    {
        private const string _ghostposition = "GhostPosition";
        private const string _navballscale = "NavballScale";
        private const string _navballposition = "NavballPosition";
        private const string _iconpos = "IconPos";
        private const string _windowvisiblebyactivescene = "WindowVisibleByActiveScene";
        private const string _proRetColour = "ProgradeColour";
        private const string _normalColour = "NormalColour";
        private const string _radialColour = "RadialColour";
        private const string _enbManeuver = "ENBManeuver";
        private const string _radialNormalDuringManeuver = "RadialNormalDuringManeuver";

        public Rect IconPos;
        public bool WindowVisibleByActiveScene;
        public float NavballPosition;
        public float NavballScale;
        public float GhostPosition = 0.1f;
        public Color ProRetColour = new Color(0.84f, 0.98f, 0);
        public Color NormalColour = new Color(0.930f, 0, 1);
        public Color RadialColour = new Color(0, 1, 0.958f);
        public bool ENBManeuver = true;
        public bool RadialNormalDuringManeuver = false;
        
        //private static readonly Color _maneuverColour = new Color(0, 0.1137f, 1, _manueverAlpha);

        public void Save()
        {
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Settings save");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<EnhancedNavBall>();
            configfile.load();

            configfile.SetValue(_iconpos, IconPos);
            configfile.SetValue(_windowvisiblebyactivescene, WindowVisibleByActiveScene);
            configfile.SetValue(_navballposition, HandleFloatSave(NavballPosition));
            configfile.SetValue(_navballscale, HandleFloatSave(NavballScale));
            configfile.SetValue(_ghostposition, HandleFloatSave(GhostPosition));
            configfile.SetValue(_proRetColour, HandleColourSave(ProRetColour));
            configfile.SetValue(_normalColour, HandleColourSave(NormalColour));
            configfile.SetValue(_radialColour, HandleColourSave(RadialColour));
            configfile.SetValue(_enbManeuver, ENBManeuver);
            configfile.SetValue(_radialNormalDuringManeuver, RadialNormalDuringManeuver);
            //configfile.SetValue("");
            
            
            configfile.save();
            
        }

        public void Load()
        {
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<EnhancedNavBall>();
            configfile.load();

            IconPos = configfile.GetValue(_iconpos, new Rect(184, 0, 32, 32));
            WindowVisibleByActiveScene = configfile.GetValue(_windowvisiblebyactivescene, false);
            NavballPosition = HandleFloatLoad(configfile.GetValue(_navballposition, GetDefaultNavballPosition()));
            NavballScale = HandleFloatLoad(configfile.GetValue(_navballscale, GetDefaultNavballScale()));
            GhostPosition = HandleFloatLoad(configfile.GetValue(_ghostposition, (0.1f).ToString()));
            ProRetColour = configfile.GetValue(_proRetColour, Color.white);
            NormalColour = configfile.GetValue(_normalColour, Color.white);
            RadialColour = configfile.GetValue(_radialColour, Color.white);
            ENBManeuver = configfile.GetValue(_enbManeuver, false);
            RadialNormalDuringManeuver = configfile.GetValue(_radialNormalDuringManeuver, false);
        }

        private static string GetDefaultNavballPosition()
        {
            return SlidingNavBall.GetNavballPostion().ToString();
        }

        private static string GetDefaultNavballScale()
        {
            return ScalingNavBall.GetScale().ToString();
        }

        /// <summary>
        /// Looks like KSP 0.23 has a bug on handling floats
        /// </summary>
        private string HandleFloatSave(float floatVal)
        {
            return floatVal.ToString();
        }

        /// <summary>
        /// Looks like KSP 0.23 has a bug on handling floats
        /// </summary>
        private float HandleFloatLoad(string stingFloat)
        {
            return float.Parse(stingFloat);
        }

        private static string HandleColourSave(Color colour)
        {
            return string.Format("{0}:{1}:{2}",
                colour.r,
                colour.g,
                colour.b);
        }

        private static Color HandleColourLoad(string stringColour)
        {
            if (string.IsNullOrEmpty(stringColour))
                return Color.white;

            string[] strings = stringColour.Split(':');

            if (strings.Length != 3)
                return Color.white;

            try
            {
                return new Color(
                    float.Parse(strings[0]), 
                    float.Parse(strings[1]), 
                    float.Parse(strings[2]));
            }
            catch (Exception)
            {
                return Color.white;
            }
        }
    }
}
