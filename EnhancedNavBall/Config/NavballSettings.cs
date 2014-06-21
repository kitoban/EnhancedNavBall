
using UnityEngine;

namespace EnhancedNavBall.Config
{
    class NavballSettings
    {
        public Rect IconPos;
        public bool WindowVisibleByActiveScene;
        public float NavballPosition;
        public float NavballScale;
        public float NavballRotation;
        public float GhostPosition = 0.1f;

        public void Save()
        {
            Utilities.DebugLogFormatted(LogLevel.Diagnostic, "Settings save");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<EnhancedNavBall>();
            configfile.load();

            configfile.SetValue("IconPos", IconPos);
            configfile.SetValue("WindowVisibleByActiveScene", WindowVisibleByActiveScene);
            configfile.SetValue("NavballPosition", HandleFloatSave(NavballPosition));
            configfile.SetValue("NavballScale", HandleFloatSave(NavballScale));
            configfile.SetValue("NavballRotation", HandleFloatSave(NavballRotation));
            configfile.SetValue("GhostPosition", HandleFloatSave(GhostPosition));
            
            
            configfile.save();
            
        }

        public void Load()
        {
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<EnhancedNavBall>();
            configfile.load();

            IconPos = configfile.GetValue("IconPos", new Rect(184, 0, 32, 32));
            WindowVisibleByActiveScene = configfile.GetValue("WindowVisibleByActiveScene", false);
            NavballPosition = HandleFloatLoad(configfile.GetValue("NavballPosition", GetDefaultNavballPosition()));
            NavballScale = HandleFloatLoad(configfile.GetValue("NavballScale", GetDefaultNavballScale()));
            NavballRotation = HandleFloatLoad(configfile.GetValue("NavballRotation", (0f).ToString()));
            GhostPosition = HandleFloatLoad(configfile.GetValue("GhostPosition", (0.1f).ToString()));
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
    }
}
