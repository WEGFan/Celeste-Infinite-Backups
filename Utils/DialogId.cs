namespace Celeste.Mod.InfiniteBackups.Utils {
    public static class DialogId {
        private const string Prefix = "INFINITE_BACKUPS_";
        private const string OptionsPrefix = Prefix + "OPTIONS_";
        private const string SubtextSuffix = "_SUBTEXT";
        private const string OptionValuesPrefix = Prefix + "OPTION_VALUES_";

        public const string ModName = Prefix + "MOD_NAME";

        public static class Options {
            public const string Enabled = OptionsPrefix + "ENABLED";
            public const string BackupAsZipFile = OptionsPrefix + "BACKUP_AS_ZIP_FILE";
            public const string AutoDeleteOldBackups = OptionsPrefix + "AUTO_DELETE_OLD_BACKUPS";
            public const string DeleteBackupsOlderThanDays = OptionsPrefix + "DELETE_BACKUPS_OLDER_THAN_DAYS";
            public const string DeleteBackupsAfterAmount = OptionsPrefix + "DELETE_BACKUPS_AFTER_AMOUNT";
            public const string OpenBackupFolder = OptionsPrefix + "OPEN_BACKUP_FOLDER";
        }

        public static class OptionValues {
            public const string Disabled = OptionValuesPrefix + "DISABLED";
            public const string Days = OptionValuesPrefix + "DAYS";
        }

        public static class Subtext {
            public const string BackupLocation = Options.OpenBackupFolder + SubtextSuffix + "_BACKUP_LOCATION";
            public const string OpenBackupFolderFailed = Options.OpenBackupFolder + SubtextSuffix + "_OPEN_FAILED";
        }
    }
}