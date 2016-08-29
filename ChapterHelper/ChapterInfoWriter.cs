using System.Collections.Generic;

namespace ChapterHelper
{
    internal abstract class ChapterInfoWriter
    {
        protected IEnumerable<Chapter> Chapters;

        protected ChapterInfoWriter(IEnumerable<Chapter> chapters)
        {
            Chapters = chapters;
        }

        public abstract void WriteToFile(string path);
    }
}
