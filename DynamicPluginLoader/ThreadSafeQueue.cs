using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Helper {
    public class ThreadSafeQueue<T> where T : class {
        public readonly object mutex = new object();
        public Queue<T> queue = new Queue<T>();

        public T DequeueOrNull()
        {
            lock (mutex)
            {
                if (queue.Count <= 0)
                {
                    return null;
                }
                else
                {
                    return queue.Dequeue();
                }
            }
        }

        public void Enqueue(T data)
        {
            lock (mutex)
            {
                queue.Enqueue(data);
            }
        }
    }
}
