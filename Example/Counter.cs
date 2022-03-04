using System.Collections.Generic;
using System.Threading;

namespace Example
{
    public static class Counter
    {
        static int _publishCount;
        static int _consumeCount;

        public static readonly List<int> _counterList = new List<int>();

        public static int GetPublishCount => _publishCount;

        public static int IncrementPublish()
        {
            Interlocked.Increment(ref _publishCount);
            return _publishCount;
        }

        public static int IncrementConsume()
        {
            Interlocked.Increment(ref _consumeCount);
            return _consumeCount;
        }

        public static int DecrementPublish()
        {
            Interlocked.Decrement(ref _publishCount);
            return _publishCount;
        }

        public static int DecrementConsume()
        {
            Interlocked.Decrement(ref _consumeCount);
            return _consumeCount;
        }
    }
}