using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pure.Utils
{


        internal class ArrayTraverse
        {
            public int[] Position;
            private int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; ++i)
                    maxLengths[i] = array.GetLength(i) - 1;

                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (int i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] >= maxLengths[i])
                        continue;

                    Position[i]++;
                    for (int j = 0; j < i; j++)
                        Position[j] = 0;

                    return true;
                }

                return false;
            }
        }
        internal class ReferenceEqualityComparer : EqualityComparer<Object>
        {
            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public override int GetHashCode(object obj)
            {
                if (obj == null) return 0;
                return obj.GetHashCode();
            }
        }



       
        internal class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
        internal static class Disposable
        {
            public static IDisposable Empty = new EmptyDisposable();
        }

        
}
