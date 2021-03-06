using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Celeste.Mod.InfiniteBackups.Utils;
using Ionic.Zip;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.InfiniteBackups.Modules {
    public static class InfiniteBackups {
        private static readonly DynamicData dd_UserIO = new DynamicData(typeof(UserIO));

        public static readonly string BackupPath = dd_UserIO.Get<string>("BackupPath");

        public static readonly string SavePath = dd_UserIO.Get<string>("SavePath");

        private static ILHook ilHook_UserIO_SaveThread;

        public static void Load() {
            ilHook_UserIO_SaveThread = new ILHook(typeof(UserIO).GetMethod("orig_SaveThread", ~BindingFlags.Default),
                patch_UserIO_orig_SaveThread);
        }

        public static void Unload() {
            ilHook_UserIO_SaveThread?.Dispose();
        }

        private static void patch_UserIO_orig_SaveThread(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.AfterLabel,
                instr => instr.MatchCall(typeof(UserIO), nameof(UserIO.Close)))
            ) {
                LogUtil.Log($"Failed patching {cursor.Method.Name}!", LogLevel.Warn);
                return;
            }

            /*
                ...
                if (UserIO.Open(UserIO.Mode.Write)) {
                    ...
                    if (UserIO.savingSettings) {
                        UserIO.SavingResult &= UserIO.Save<Settings>("settings", UserIO.savingSettingsData);
                    }
                    [patch here] <=====
                    UserIO.Close();
                }
                ...
            */

            cursor.EmitDelegate<Action>(() => {
                LogUtil.Log("Backing up saves...", LogLevel.Info);
                bool result;
                try {
                    if (InfiniteBackupsModule.Settings.BackupAsZipFile) {
                        BackupSavesAsZipFile();
                    } else {
                        BackupSaves();
                    }
                    result = true;
                } catch (Exception err) {
                    LogUtil.Log("Backup saves failed!", LogLevel.Warn);
                    err.LogDetailed(InfiniteBackupsModule.LoggerTagName);
                    result = false;
                }

                dd_UserIO.Set(nameof(UserIO.SavingResult), UserIO.SavingResult & result);

                if (InfiniteBackupsModule.Settings.AutoDeleteOldBackups) {
                    LogUtil.Log("Deleting outdated backups...", LogLevel.Info);
                    try {
                        DeleteOutdatedSaves();
                    } catch (Exception err) {
                        LogUtil.Log("Delete outdated backups failed!", LogLevel.Warn);
                        err.LogDetailed(InfiniteBackupsModule.LoggerTagName);
                    }
                }
            });
        }

        private static DateTime? ParseBackupTime(string name) {
            // if it's a zip file, first remove the extension
            if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
                name = name.Remove(name.LastIndexOf(".zip", StringComparison.OrdinalIgnoreCase));
            }
            DateTime parsed;
            bool result = DateTime.TryParseExact(name, "'backup_'yyyy-MM-dd_HH-mm-ss-fff",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed);
            LogUtil.Log($"Parsing {name}, result = {result}, parsed = {parsed}");

            return result ? (DateTime?)parsed : null;
        }

        private static void DeleteOutdatedSaves() {
            List<FileSystemInfo> backups = new DirectoryInfo(BackupPath)
                .GetFileSystemInfos("backup_*")
                .Where(item => ParseBackupTime(item.Name) != null)
                .OrderByDescending(item => item.Name)
                .ToList();

            HashSet<FileSystemInfo> deleteList = new HashSet<FileSystemInfo>();

            if (InfiniteBackupsModule.Settings.DeleteBackupsAfterAmount != -1) {
                deleteList.UnionWith(
                    backups
                        .Skip(InfiniteBackupsModule.Settings.DeleteBackupsAfterAmount)
                );
            }

            if (InfiniteBackupsModule.Settings.DeleteBackupsOlderThanDays != -1) {
                deleteList.UnionWith(
                    backups
                        .Where(dir => {
                            DateTime? backupTime = ParseBackupTime(dir.Name);
                            if (backupTime == null) {
                                return false;
                            }
                            return backupTime < DateTime.Now.AddDays(-InfiniteBackupsModule.Settings.DeleteBackupsOlderThanDays);
                        })
                );
            }

            foreach (FileSystemInfo backup in deleteList) {
                LogUtil.Log($"Deleting {backup}", LogLevel.Info);
                try {
                    if (backup is DirectoryInfo directory) {
                        directory.Delete(true);
                    } else {
                        backup.Delete();
                    }
                } catch (IOException err) {
                    LogUtil.Log($"Deleting {backup.Name} failed!", LogLevel.Warn);
                    err.LogDetailed(InfiniteBackupsModule.LoggerTagName);
                }
            }
        }

        private static void BackupSaves() {
            string directoryName = "backup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");

            string path = Path.Combine(BackupPath, directoryName);
            LogUtil.Log(path);

            DirectoryInfo backupDirectory = Directory.CreateDirectory(path);
            DirectoryInfo saveDirectory = new DirectoryInfo(SavePath);

            CloneDirectory(saveDirectory, backupDirectory);

            LogUtil.Log($"Saves backed up to {path}", LogLevel.Info);
        }

        private static void BackupSavesAsZipFile() {
            string zipFileName = "backup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".zip";
            string path = Path.Combine(BackupPath, zipFileName);

            using (ZipFile zipFile = new ZipFile()) {
                zipFile.AddDirectory(SavePath);
                zipFile.Save(path);
            }

            LogUtil.Log($"Saves backed up to {path}", LogLevel.Info);
        }

        public static void CloneDirectory(DirectoryInfo source, DirectoryInfo target) {
            Directory.CreateDirectory(target.FullName);

            // copy each file into the new directory
            foreach (FileInfo file in source.GetFiles()) {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            // copy each subdirectory using recursion
            foreach (DirectoryInfo directory in source.GetDirectories()) {
                DirectoryInfo subdirectory = target.CreateSubdirectory(directory.Name);
                CloneDirectory(directory, subdirectory);
            }
        }
    }
}