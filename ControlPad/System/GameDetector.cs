namespace ControlPad
{
    public static class GameDetector
    {
        private static readonly string[] KnownGameFolders =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Epic Games"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ubisoft", "Ubisoft Game Launcher", "games"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "EA Games"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "EA Games"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "GOG Galaxy", "Games"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GOG Galaxy", "Games"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent", "data"),
        };

        public static List<string> DetectGameProcessNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string folder in KnownGameFolders)
                CollectExecutableNames(folder, names, 4);

            CollectSteamLibraryGames(names);

            return names
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void CollectSteamLibraryGames(HashSet<string> names)
        {
            string steamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
            string libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile))
                return;

            try
            {
                string[] lines = File.ReadAllLines(libraryFile);
                foreach (string line in lines)
                {
                    if (!line.Contains("\"path\"", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string? value = ExtractVdfQuotedValue(line);
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    string resolvedPath = value.Replace(@"\\", @"\");
                    string commonPath = Path.Combine(resolvedPath, "steamapps", "common");
                    CollectExecutableNames(commonPath, names, 4);
                }
            }
            catch { }
        }

        private static string? ExtractVdfQuotedValue(string line)
        {
            int lastQuote = line.LastIndexOf('"');
            if (lastQuote <= 0)
                return null;

            int prevQuote = line.LastIndexOf('"', lastQuote - 1);
            if (prevQuote < 0 || prevQuote >= lastQuote)
                return null;

            return line.Substring(prevQuote + 1, lastQuote - prevQuote - 1).Trim();
        }

        private static void CollectExecutableNames(string rootPath, HashSet<string> names, int maxDepth)
        {
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
                return;

            try
            {
                var queue = new Queue<(string Path, int Depth)>();
                queue.Enqueue((rootPath, 0));

                while (queue.Count > 0)
                {
                    var (currentPath, depth) = queue.Dequeue();
                    if (depth > maxDepth)
                        continue;

                    IEnumerable<string> files = Array.Empty<string>();
                    try { files = Directory.EnumerateFiles(currentPath, "*.exe", SearchOption.TopDirectoryOnly); }
                    catch { }

                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (IsLikelyGameExecutable(fileName))
                            names.Add(fileName);
                    }

                    if (depth == maxDepth)
                        continue;

                    IEnumerable<string> directories = Array.Empty<string>();
                    try { directories = Directory.EnumerateDirectories(currentPath); }
                    catch { }

                    foreach (string directory in directories)
                        queue.Enqueue((directory, depth + 1));
                }
            }
            catch { }
        }

        private static bool IsLikelyGameExecutable(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            string value = name.Trim();

            string[] launcherAndUtilityKeywords =
            {
                "unins", "setup", "crash", "report", "installer", "launcher", "redistributable", "easyanticheat", "battleye", "updater", "vc_redist", "dxsetup", "support"
            };

            if (launcherAndUtilityKeywords.Any(k => value.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        }
    }
}
