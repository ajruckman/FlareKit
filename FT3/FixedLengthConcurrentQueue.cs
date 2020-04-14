using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FT3
{
    internal class FixedLengthConcurrentQueue<T>
    {
        public readonly ConcurrentQueue<T> Queue;

        public FixedLengthConcurrentQueue(int size)
        {
            Size  = size;
            Queue = new ConcurrentQueue<T>();
        }

        public FixedLengthConcurrentQueue(IEnumerable<T> v)
        {
            Queue = new ConcurrentQueue<T>(v);
        }

        private int Size { get; }

        public void Enqueue(T obj)
        {
            Queue.Enqueue(obj);

            while (Queue.Count > Size)
                Queue.TryDequeue(out T _);
        }

        public void Enqueue(IEnumerable<T> objs)
        {
            foreach (T obj in objs)
                Queue.Enqueue(obj);

            while (Queue.Count > Size)
                Queue.TryDequeue(out T _);
        }
    }
}