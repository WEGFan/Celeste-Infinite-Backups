using System;
using FMOD.Studio;

namespace Celeste.Mod.InfiniteBackups {
    public class InfiniteBackupsModule : EverestModule {
        public InfiniteBackupsModule() {
            Instance = this;
        }

        public const string LoggerTagName = "InfiniteBackups";

        public static InfiniteBackupsModule Instance { get; private set; }

        public override Type SettingsType => typeof(InfiniteBackupsSettings);

        public static InfiniteBackupsSettings Settings => Instance._Settings as InfiniteBackupsSettings;

        public static bool Hooked = false;

        public override void Load() {
            Logger.SetLogLevel(LoggerTagName, LogLevel.Info);

            if (!Hooked && Settings.Enabled) {
                Modules.InfiniteBackups.Load();

                Hooked = true;
            }
        }

        public override void Unload() {
            if (Hooked) {
                Modules.InfiniteBackups.Unload();

                Hooked = false;
            }
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            CreateModMenuSectionHeader(menu, inGame, snapshot);
            Settings.CreateMenu(menu, inGame);
        }
    }
}