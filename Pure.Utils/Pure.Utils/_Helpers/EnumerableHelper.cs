
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pure.Utils
{
    /// <summary>
    /// This class contains methods to help with enumrating items.
    /// </summary>
    public class EnumerableHelper
    {
        /// <summary>
        /// Calls the action by supplying the start and end index.
        /// </summary>
        /// <param name="itemCount">Number of items.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="action">Action to call for each item.</param>
        public static void ForEachByCols(int itemCount, int cols, Action<int, int> action)
        {
            if (itemCount == 0)
                return;

            if (itemCount <= cols)
            {
                action(0, itemCount - 1);
                return;
            }

            int startNdx = 0;
            while (startNdx < itemCount)
            {
                // 1. startNdx = 0 .. endNdx = 2
                // 2. startNdx = 3 .. endNdx = 5
                int endNdx = startNdx + (cols - 1);
                if (endNdx >= itemCount)
                    endNdx = itemCount - 1;

                action(startNdx, endNdx);
                startNdx = endNdx + 1;
            }
        }
    }
}
