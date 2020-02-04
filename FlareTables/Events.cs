using System.Collections.Generic;

namespace FlareTables
{
    public static class Proxy
    {
        public delegate IEnumerable<object> Data();

        public delegate IEnumerable<T> TypedData<T>();
    }
}