using System.IO;

namespace ControlPad
{
    public static class ProcessIdentifierHelper
    {
        public static bool IsExecutablePathIdentifier(string processIdentifier)
        {
            if (string.IsNullOrWhiteSpace(processIdentifier))
                return false;

            return processIdentifier.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                processIdentifier.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                processIdentifier.IndexOf(Path.AltDirectorySeparatorChar) >= 0;
        }
    }
}
