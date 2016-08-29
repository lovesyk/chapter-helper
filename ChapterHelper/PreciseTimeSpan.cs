using System;

namespace ChapterHelper
{
    public class PreciseTimeSpan
    {
        private readonly TimeSpan _timeSpan;
        private readonly int _extraPrecision;

        private PreciseTimeSpan(TimeSpan timeSpan, int extraPrecision = 0)
        {
            _timeSpan = timeSpan;
            _extraPrecision = extraPrecision;
        }

        public int Hours => _timeSpan.Hours;
        public int Minutes => _timeSpan.Minutes;
        public int Seconds => _timeSpan.Seconds;
        public long Nanoseconds => TotalNanoseconds - (long)_timeSpan.TotalSeconds * 1000000000;

        public double TotalMilliseconds => _timeSpan.TotalMilliseconds;
        public long TotalNanoseconds => _timeSpan.Ticks * 100 + _extraPrecision;

        public static PreciseTimeSpan operator+(PreciseTimeSpan a, PreciseTimeSpan b)
        {
            return FromNanoseconds(a.TotalNanoseconds + b.TotalNanoseconds);
        }
        public static PreciseTimeSpan operator-(PreciseTimeSpan a, PreciseTimeSpan b)
        {
            return FromNanoseconds(a.TotalNanoseconds - b.TotalNanoseconds);
        }

        public override bool Equals(object obj)
        {
            var item = obj as PreciseTimeSpan;

            if (item == null)
            {
                return false;
            }

            return _timeSpan.Equals(item._timeSpan) &&
                _extraPrecision.Equals(item._extraPrecision);
        }

        public override int GetHashCode()
        {
            return _timeSpan.GetHashCode() ^ _extraPrecision.GetHashCode();
        }

        public static bool operator==(PreciseTimeSpan a, PreciseTimeSpan b)
        {
            if (Object.ReferenceEquals(a, null) != Object.ReferenceEquals(b, null))
            {
                return false;
            }
            if (Object.ReferenceEquals(a, null))
            {
                return true;
            }
            return a.TotalNanoseconds == b.TotalNanoseconds;
        }

        public static bool operator!=(PreciseTimeSpan a, PreciseTimeSpan b)
        {
            return !(a == b);
        }

        public static PreciseTimeSpan FromMilliseconds(double value)
        {
            return new PreciseTimeSpan(TimeSpan.FromMilliseconds(value));
        }
        public static PreciseTimeSpan FromNanoseconds(long value)
        {
            return new PreciseTimeSpan(new TimeSpan(value / 100), (int)(value % 100));
        }

        private static PreciseTimeSpan _zero;
        public static PreciseTimeSpan Zero => _zero ?? (_zero = new PreciseTimeSpan(TimeSpan.Zero));
    }
}
