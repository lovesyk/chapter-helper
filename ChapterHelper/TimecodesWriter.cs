using Fractions;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ChapterHelper
{
    internal class TimecodeFormat1Writer : ChapterInfoWriter
    {
        public TimecodeFormat1Writer(ChapterCollection chapters) : base(chapters) { }

        public override void WriteToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                Fraction mostCommonFrameRate = Chapters.GroupBy(x => x.OutputFrameRate)
                    .OrderByDescending(x => x.Count()).First().First().OutputFrameRate;
                writer.WriteLine("# timecode format v1");
                var usCulture = new CultureInfo("en-US");
                writer.WriteLine("assume " + mostCommonFrameRate.ToDecimal().ToString(usCulture));
                foreach (Chapter chapter in Chapters)
                {
                    if (chapter.OutputFrameRate != mostCommonFrameRate)
                    {
                        writer.WriteLine($"{chapter.OutputFirstFrame},{chapter.OutputLastFrame},{chapter.OutputFrameRate.ToDecimal().ToString(usCulture)}");
                    }
                }
            }
        }
    }
}
