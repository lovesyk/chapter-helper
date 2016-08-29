using Fractions;
using PropertyChanged;
using System;

namespace ChapterHelper
{
    [ImplementPropertyChanged]
    public class Chapter
    {
        private readonly ChapterCollection _chapterCollection;
        public Chapter(ChapterCollection chapterCollection)
        {
            _chapterCollection = chapterCollection;

            _inputFrameRate = Fraction.Zero;

            InputFirstFrame = -1;
            InputLastFrame = -1;

            _outputFirstFrame = -1;
        }

        /// <summary>
        /// Fetches the previous chapter of the current collection.
        /// </summary>
        /// <returns>Previous chapter in <see cref="_chapterCollection"/> or null if none</returns>
        private Chapter GetPrevious()
        {
            return _chapterCollection.ElementAtOffsetOrDefault(this, -1);
        }

        /// <summary>
        /// Fetches the next chapter of the current collection.
        /// </summary>
        /// <returns>Next chapter in <see cref="_chapterCollection"/> or null if none</returns>
        private Chapter GetNext()
        {
            return _chapterCollection.ElementAtOffsetOrDefault(this, 1);
        }

        /// <summary>
        /// Calculates the time duration between two frames at a specific frame rate.
        /// </summary>
        /// <param name="firstFrame">First frame</param>
        /// <param name="lastFrame">Last frame (inclusive)</param>
        /// <param name="frameRate">Frame rate at which the frames are being displayed</param>
        /// <returns>Calculated duration.</returns>
        private static PreciseTimeSpan CalculateDuration(int firstFrame, int lastFrame, Fraction frameRate)
        {
            return CalculateStartTime(lastFrame - firstFrame + 1, frameRate);
        }

        /// <summary>
        /// Calculates the point in time at which a frame is being displayed at a specific frame rate.
        /// </summary>
        /// <param name="frame">Reference frame</param>
        /// <param name="frameRate">Frame rate at which all frames up to <paramref name="frame" /> are being displayed</param>
        /// <returns>Calculated point of time</returns>
        private static PreciseTimeSpan CalculateStartTime(int frame, Fraction frameRate)
        {
            if (frame < 0 ||
                frameRate <= Fraction.Zero)
            {
                return null;
            }

            decimal length = frame / frameRate.ToDecimal() * 1000000000;
            long lengthRounded = Convert.ToInt64(length);

            return PreciseTimeSpan.FromNanoseconds(lengthRounded);
        }

        /// <summary>
        /// Calculates the point in time at which a specified duration ends based on a start time.
        /// </summary>
        /// <param name="startTime">Point in time to start at</param>
        /// <param name="duration">Duration to add</param>
        /// <returns>Calculated point of time</returns>
        private static PreciseTimeSpan CalculateEndTime(PreciseTimeSpan startTime, PreciseTimeSpan duration)
        {
            if (startTime == null ||
                duration == null)
            {
                return null;
            }
            PreciseTimeSpan output;
            try
            {
                output = startTime - PreciseTimeSpan.FromNanoseconds(1) + duration;
            }
            catch
            {
                return null;
            }
            return output;
        }

        /// <summary>
        /// Calculates the length in frames between two of them.
        /// </summary>
        /// <param name="firstFrame">First frame</param>
        /// <param name="lastFrame">Last frame (inclusive)</param>
        /// <returns>Calculated length in frames or -1 on error</returns>
        private int CalculateLength(int firstFrame, int lastFrame)
        {
            // either frame is unset or the calculation would result in an overflow
            if (firstFrame < 0 ||
                lastFrame < 0 ||
                lastFrame == Int32.MaxValue && firstFrame == 0)
            {
                return -1;
            }

            return lastFrame - firstFrame + 1;
        }

        /// <summary>
        /// Calculates the amount of frames displayed durinng a duration at a specific frame rate.
        /// </summary>
        /// <param name="duration">Duration</param>
        /// <param name="frameRate">Frame rate</param>
        /// <returns>Calculated length in frames or -1 on error</returns>
        private int CalculateLength(PreciseTimeSpan duration, Fraction frameRate)
        {
            if (duration != null &&
                frameRate > Fraction.Zero)
            {
                decimal exactLength = duration.TotalNanoseconds * frameRate.ToDecimal() / 1000000000;

                if (exactLength < Int32.MaxValue)
                {
                    return Convert.ToInt32(exactLength);
                }
            }
            return -1;
        }

        private Fraction _inputFrameRate;
        public Fraction InputFrameRate
        {
            get
            {
                if (_inputFrameRate > Fraction.Zero)
                {
                    return _inputFrameRate; // overwritten
                }

                Chapter previous = GetPrevious();
                return previous?.InputFrameRate ?? Fraction.Zero;
            }
            set { _inputFrameRate = value; }
        }

        public int InputFirstFrame { get; set; }
        public int InputLastFrame { get; set; }
        public int InputLength => CalculateLength(InputFirstFrame, InputLastFrame);

        public PreciseTimeSpan InputDuration => CalculateDuration(InputFirstFrame, InputLastFrame, InputFrameRate);
        public PreciseTimeSpan InputStartTime => CalculateStartTime(InputFirstFrame, InputFrameRate);
        public PreciseTimeSpan InputEndTime => CalculateEndTime(InputStartTime, InputDuration);

        private Fraction _outputFrameRate;
        public Fraction OutputFrameRate
        {
            get
            {
                if (_outputFrameRate > Fraction.Zero)
                {
                    return _outputFrameRate; // overwritten
                }

                Chapter previous = GetPrevious();
                if (previous != null &&
                    previous.OutputFrameRate > Fraction.Zero)
                {
                    return previous.OutputFrameRate;
                }
                return InputFrameRate;
            }
            set { _outputFrameRate = value; }
        }

        public int OutputLength
        {
            get
            {
                // calculate based on overwritten output first frame of next chapter
                Chapter next = GetNext();
                if (next != null &&
                    next._outputFirstFrame >= 0)
                {
                    return CalculateLength(OutputFirstFrame, next.OutputFirstFrame - 1);
                }

                // calculate based on input duration
                int fromInputDuration = CalculateLength(InputDuration, OutputFrameRate);
                return fromInputDuration >= 0 ? fromInputDuration : -1;
            }
        }

        private int _outputFirstFrame;
        public int OutputFirstFrame
        {
            get
            {
                if (_outputFirstFrame >= 0) // value was overridden
                {
                    return _outputFirstFrame;
                }

                Chapter previous = GetPrevious();
                if (previous == null) return 0; // first chapter
                if (previous.OutputLastFrame < 0 ||
                    previous.OutputLastFrame == Int32.MaxValue) return -1;

                return previous.OutputLastFrame + 1;
            }
            set
            {
                _outputFirstFrame = value;
            }
        }
        public int OutputLastFrame
        {
            get
            {
                if (OutputFirstFrame < 0 ||
                    OutputLength < 0)
                {
                    return -1;
                }
                int output;
                try
                {
                    output = OutputFirstFrame - 1 + OutputLength;
                }
                catch
                {
                    return -1;
                }
                return output;
            }
        }

        public PreciseTimeSpan OutputDuration => CalculateDuration(OutputFirstFrame, OutputLastFrame, OutputFrameRate);

        public PreciseTimeSpan OutputStartTime
        {
            get
            {
                Chapter previous = GetPrevious();
                if (previous == null) return PreciseTimeSpan.Zero;
                if (previous.OutputEndTime == null) return null;

                PreciseTimeSpan output;
                try
                {
                    output = previous.OutputEndTime + PreciseTimeSpan.FromNanoseconds(1);
                }
                catch
                {
                    return null;
                }
                return output;
            }
        }

        public PreciseTimeSpan OutputEndTime
        {
            get
            {
                if (OutputStartTime == null ||
                    OutputDuration == null)
                {
                    return null;
                }

                PreciseTimeSpan output;
                try
                {
                    output = OutputStartTime - PreciseTimeSpan.FromNanoseconds(1) + OutputDuration;
                }
                catch
                {
                    return null;
                }
                return output;
            }
        }

        private string _name = String.Empty;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value.Trim();
            }
        }
    }
}
