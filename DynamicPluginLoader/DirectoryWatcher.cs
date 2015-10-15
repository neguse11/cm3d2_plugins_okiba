using System;
using System.IO;

namespace Helper
{
    public class DirectoryWatcher
    {
        Helper.ThreadSafeQueue<FileSystemEventArgs> queue;
        FileSystemWatcher watcher;

        public DirectoryWatcher(string path) : this(path, "*.*") { }

        public DirectoryWatcher(string path, string wildCard)
        {
            queue = new Helper.ThreadSafeQueue<FileSystemEventArgs>();
            watcher = new FileSystemWatcher();

            Action<object, FileSystemEventArgs> h = (s, e) =>
            {
                queue.Enqueue(e);
            };

            watcher.Path = path;
            watcher.Filter = wildCard;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Created += new FileSystemEventHandler(h);
            watcher.Deleted += new FileSystemEventHandler(h);
            watcher.Changed += new FileSystemEventHandler(h);
            watcher.EnableRaisingEvents = true;
        }

        public void Update(Action<FileSystemEventArgs> receiver)
        {
            for (;;)
            {
                FileSystemEventArgs a = queue.DequeueOrNull();
                if (a == null)
                {
                    break;
                }
                receiver(a);
            }
        }
    }
}
