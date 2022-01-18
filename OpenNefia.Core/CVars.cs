using OpenNefia.Core.Configuration;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Log;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenNefia.Core
{
    [CVarDefs]
    public abstract class CVars
    {
        protected CVars()
        {
            throw new InvalidOperationException("This class must not be instantiated");
        }

        /*
         * Log
         */

        public static readonly CVarDef<bool> LogEnabled =
            CVarDef.Create("log.enabled", true, CVar.Archive);

        public static readonly CVarDef<string> LogPath =
            CVarDef.Create("log.path", "logs", CVar.Archive);

        public static readonly CVarDef<string> LogFormat =
            CVarDef.Create("log.format", "log_%(date)s-T%(time)s.txt", CVar.Archive);

        public static readonly CVarDef<LogLevel> LogLevel =
            CVarDef.Create("log.level", Log.LogLevel.Info, CVar.Archive);

        public static readonly CVarDef<bool> LogRuntimeLog =
            CVarDef.Create("log.runtimelog", true, CVar.Archive);

        /*
         * Display
         */

        public static readonly CVarDef<bool> DisplayVSync =
            CVarDef.Create("display.vsync", true, CVar.Archive);

        public static readonly CVarDef<int> DisplayDisplayNumber =
            CVarDef.Create("display.displaynumber", 0, CVar.Archive);

        public static readonly CVarDef<WindowMode> DisplayWindowMode =
            CVarDef.Create("display.windowmode", WindowMode.Windowed, CVar.Archive);

        public static readonly CVarDef<int> DisplayWidth =
            CVarDef.Create("display.width", 800, CVar.Archive);

        public static readonly CVarDef<int> DisplayHeight =
            CVarDef.Create("display.height", 600, CVar.Archive);

        public static readonly CVarDef<float> DisplayUIScale =
            CVarDef.Create("display.uiScale", 1.75f, CVar.Archive);

        public static readonly CVarDef<bool> DisplayHighDPI =
            CVarDef.Create("display.hidpi", false, CVar.Archive);

        // TODO: make into distrib config
        // that means a separate YAML thing that uses the serialization manager format
        // for setting static global customizable data that is not bound to a config menu
        // think emacs' defcustom
        public static readonly CVarDef<string> DisplayTitle =
            CVarDef.Create("display.title", "OpenNefia");

        /*
         * Anime
         */

        public static readonly CVarDef<float> AnimeWait =
            CVarDef.Create("anime.wait", 20f);

        /*
         * Audio
         */

        public static readonly CVarDef<bool> AudioMusic =
            CVarDef.Create("audio.music", true, CVar.Archive);

        public static readonly CVarDef<int> AudioMidiDevice =
            CVarDef.Create("audio.mididevice", 0, CVar.Archive);

        public static readonly CVarDef<bool> AudioSound =
            CVarDef.Create("audio.sound", true, CVar.Archive);

        public static readonly CVarDef<bool> AudioPositionalAudio =
            CVarDef.Create("audio.positionalaudio", true, CVar.Archive);

        /*
         * Language
         */

        public static readonly CVarDef<string> LanguageLanguage =
            CVarDef.Create("language.language", "en_US", CVar.Archive);

        /*
         * Debug
         */

        /// <summary>
        ///     Target framerate for things like the frame graph.
        /// </summary>
        public static readonly CVarDef<int> DebugTargetFps =
            CVarDef.Create("debug.target_fps", 60, CVar.Archive);
    }
}
