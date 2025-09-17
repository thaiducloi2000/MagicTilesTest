using UnityEditor;
using UnityEngine;

namespace com.team70
{
    public static class PlayGameHelpTools
    {
        private static float _helpToolTimeSlow = 0.1f;
        private static float _helpToolTimeFast = 1.0f;
        
        [MenuItem("T70/Dev/HelpTools/NormalTime %K", false, -90)]
        public static void NormalTime()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = 1.0f;
                _helpToolTimeSlow = 0.1f;
                _helpToolTimeFast = 2.0f;
            }
        }

        [MenuItem("T70/Dev/HelpTools/SlowTimeX %J", false, -90)]
        public static void SlowTimeX()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = _helpToolTimeSlow;
                _helpToolTimeSlow /= 2.0f;
            }
        }
        
        [MenuItem("T70/Dev/HelpTools/Invert SlowTimeX %#J", false, -90)]
        public static void InvertSlowTimeX()
        {
            if (Application.isPlaying)
            {
                _helpToolTimeSlow *= 2.0f;
                if (_helpToolTimeSlow > 1.0f)
                {
                    _helpToolTimeSlow = 1.0f;
                }
                Time.timeScale = _helpToolTimeSlow;
            }
        }
        
        [MenuItem("T70/Dev/HelpTools/Stop %I", false, -90)]
        public static void StopTime()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = 0.0f;
                _helpToolTimeSlow = 0.1f;
                _helpToolTimeFast = 2.0f;
            }
        }
        
        [MenuItem("T70/Dev/HelpTools/FastX %L", false, -90)]
        public static void FastXTime()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = _helpToolTimeFast;
                _helpToolTimeFast *= 2.0f;
            }
        }
        
        [MenuItem("T70/Dev/HelpTools/Invert FastX %#L", false, -90)]
        public static void InvertFastXTime()
        {
            if (Application.isPlaying)
            {
                _helpToolTimeFast /= 2.0f;
                if (_helpToolTimeFast < 1.0f)
                {
                    _helpToolTimeFast = 1.0f;
                }
                Time.timeScale = _helpToolTimeFast;
            }
        }
    }
}