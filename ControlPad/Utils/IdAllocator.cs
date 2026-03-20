using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlPad.Utils
{
    public static class IdAllocator
    {
        public static int GetFreeId<T>(IEnumerable<T> items, Func<T, int> idSelector)
        {
            var used = new HashSet<int>(items.Select(idSelector));
            int candidate = 0;
            while (used.Contains(candidate))
                candidate++;
            return candidate;
        }
    }
}
