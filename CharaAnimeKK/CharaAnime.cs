
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using UnityEngine;
using System.Reflection;

namespace CharaAnime
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(KoikatuAPI.GUID, "1.4")]
    [BepInProcess("CharaStudio.exe")]
    public class CharaAnime : BaseUnityPlugin
    {
        public const string GUID = "Countd360.CharaAnime.KK";
        public const string Name = "CharaAnime";
        public const string Version = "1.2.0";

        public static CharaAnime Instance { get; private set; }
        internal static new ManualLogSource Logger;

        // configs
        public static ConfigEntry<KeyboardShortcut> KeyShowUI { get; private set; }
        public static ConfigEntry<int> UIXPosition { get; private set; }
        public static ConfigEntry<int> UIYPosition { get; private set; }
        public static ConfigEntry<int> UIWidth { get; private set; }
        public static ConfigEntry<int> UIHeight { get; private set; }

        private ConfigEntry<string> configGreeting;
        private ConfigEntry<bool> configDisplayGreeting;
        private const string DESCRIPTION_SHOWUI = "Toggles the main UI on and off.";

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            // config
            KeyShowUI = Config.Bind("General", "Show chara anime UI", new KeyboardShortcut(KeyCode.M, KeyCode.LeftShift), DESCRIPTION_SHOWUI);
            UIXPosition = Config.Bind("GUI", "Main GUI X position", 50, "X offset from left in pixel");
            UIYPosition = Config.Bind("GUI", "Main GUI Y position", 300, "Y offset from top in pixel");
            UIWidth = Config.Bind("GUI", "Main GUI window width", 630, "Main window width");
            UIHeight = Config.Bind("GUI", "Main GUI window height", 500, "Main window height");

            configGreeting = Config.Bind("General",   // The section under which the option is shown
                                     "GreetingText",  // The key of the configuration option in the configuration file
                                     "Hello, world!", // The default value
                                     "A greeting text to show when the game is launched"); // Description of the option to show in the config file
            configDisplayGreeting = Config.Bind("General.Toggles",
                                            "DisplayGreeting",
                                            true,
                                            "Whether or not to show the greeting text");

            // start
            GameObject gameObject = new GameObject("CharaAnimePlugin");
            Object.DontDestroyOnLoad(gameObject);
            CharaAnimeMgr.Install(gameObject);

            // Patch
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
