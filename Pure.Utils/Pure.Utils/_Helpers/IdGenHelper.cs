using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Utils
{
    public class IdGenHelper
    {
        public static string NewId() {
            return Guid.NewGuid().ToString();
        }
    }
}
