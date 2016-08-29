using System;
using System.Collections.Generic;
using System.IO;

namespace ChapterHelper
{
    internal class QpFileWriter : ChapterInfoWriter
    {
        public QpFileWriter(IEnumerable<Chapter> chapters) : base(chapters) { }

        public override void WriteToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (Chapter chapter in Chapters)
                {
                    if (chapter.Name != String.Empty)
                    {
                        writer.WriteLine($"{chapter.OutputFirstFrame} K -1");
                    }
                }
            }
        }
    }
}
