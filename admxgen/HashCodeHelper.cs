using System;
using System.Collections;
using System.Collections.Generic;

namespace admxgen
{
    public static class HashCodeHelper
    {
        public static int CombineHashCodes(params object[] args)
        {
            return CombineHashCodes(EqualityComparer<object>.Default, args);
        }

        public static int CombineHashCodes(IEqualityComparer comparer, params object[] args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Length == 0) throw new ArgumentException("args");

            int hashcode = 0;

            unchecked
            {
                foreach (var arg in args)
                    hashcode = (hashcode << 5) - hashcode ^ comparer.GetHashCode(arg);
            }

            return hashcode;
        }
    }
}
