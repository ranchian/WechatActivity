using System;

using System.Linq;

using System.Threading;


namespace FJW.Unit
{
    /// <summary>
    /// 自增ID算法Snowflake
    /// </summary>
    public class IdWorker
    {
        private static readonly SnowFlakesIdGenerate SnowFlakesIdGenerate = new SnowFlakesIdGenerate();
        public static long NextId()
        {
            return SnowFlakesIdGenerate.GetId();
        }
    }


    internal struct SnowFlakes
    {
        internal static readonly int EncodingTimestampLength = (int)Math.Ceiling(Math.Log(Math.Pow(2, TimestampLength), 36));
        internal static readonly int EncodingWorkerLength = (int)Math.Ceiling(Math.Log(Math.Pow(2, WorkerLength), 36));
        internal static readonly int EncodingCounterLength = (int)Math.Ceiling(Math.Log(Math.Pow(2, CounterLength), 36));
        internal const int CounterLength = 14;
        internal const int WorkerLength = 8;
        internal const int TimestampLength = 30;
        internal const int TotalLength = CounterLength + WorkerLength + TimestampLength;
        internal const int IdentifierShift = CounterLength;
        internal const int TimestampShift = IdentifierShift + WorkerLength;
        internal const long Mask = 0xFFFFFFFFFFFFF;

        internal static readonly string[] Elements =
           Enumerable.Range(0, 10).Select(number => number.ToString()).Union(
               Enumerable.Range(0, 26).Select(index => (char)('A' + (char)index)).Select(c => c.ToString())
               ).ToArray();

        internal static readonly int NewBaseCount = Elements.Length;

        internal static readonly string ElementsTalbe = Elements.Aggregate((s, s1) => s + s1);
    }

    /// <summary>
    /// 模仿Twitter的SnowFlakes算法，根据开源代码更改的适用于本框架的生成一个根据时间递增，带有机器号和一个本地计数器的生成52位整型数的分布式Id生成器
    /// </summary>  
    public class SnowFlakesIdGenerate
    {
        //16000+ ids per second for each type
        //Cell's size cannot exceed 256 workers.
        //We can provider id for about 34 years
        //总共52位，额外一位用来标记紧急客户端生成的顺序Id
        private static int _counter;

        private static readonly long EpochTicks;
        /// <summary>
        /// 机器识别号，处于生成的Id中端位置，一个不长于10位的整型数
        /// </summary>
        private static ushort Identifier { get; }
        static SnowFlakesIdGenerate()
        {
            var epoch = new DateTime(2013, 9, 1, 0, 0, 0, 0, DateTimeKind.Local);
            EpochTicks = epoch.Ticks;
            _counter = 0;
            Identifier = (ushort)(Guid.NewGuid().ToString().GetHashCode() % (2 << 10));
        }



        /// <summary>
        /// 获取一个新Id
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            var ct = CurrentTimeCounter();
            var counter = Interlocked.Increment(ref _counter);
            if ((ushort)_counter == 0)
            {
                ct = WaitForNextTimeCounter((uint)ct);
                counter = Interlocked.Increment(ref _counter);
            }
            var result = ((ct << SnowFlakes.TimestampShift) + ((long)Identifier << SnowFlakes.IdentifierShift) + (uint)counter);

            return result & SnowFlakes.Mask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private static uint WaitForNextTimeCounter(uint ct)
        {
            while (true)
            {
                var timeCounter = CurrentTimeCounter();
                if (timeCounter != ct)
                {
                    return (uint)timeCounter;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static long CurrentTimeCounter()
        {
            var utcTicks = DateTime.Now.Ticks;
            //右移24位等于除以8388608，约等于每秒Ticks数10000000L
            return (uint)((utcTicks - EpochTicks) >> 23);
        }
    }
}
