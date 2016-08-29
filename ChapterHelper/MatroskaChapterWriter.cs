using System;
using System.Linq;
using System.Xml.Linq;

namespace ChapterHelper
{
    internal class MatroskaChapterWriter : ChapterInfoWriter
    {
        public string LanguageCode;

        public MatroskaChapterWriter(ChapterCollection chapters, string languageCode) : base(chapters)
        {
            LanguageCode = languageCode;
        }

        public override void WriteToFile(string path)
        {
            XDocument matroskaChapter = new XDocument(
                new XDocumentType("Chapters", null, "matroskachapters.dtd", null),
                new XElement("Chapters",
                    new XElement("EditionEntry",
                        Chapters.Where(chapter => chapter.Name != String.Empty).Select(chapter =>
                            new XElement("ChapterAtom",
                                new XElement("ChapterTimeStart",
                                    $"{chapter.OutputStartTime.Hours:00}:{chapter.OutputStartTime.Minutes:00}:{chapter.OutputStartTime.Seconds:00}.{chapter.OutputStartTime.Nanoseconds:000000000}"),
                                new XElement("ChapterDisplay",
                                    new XElement("ChapterString", chapter.Name),
                                    new XElement("ChapterLanguage", LanguageCode)
                                    )
                                )
                            )
                        )
                    )
                );
            matroskaChapter.Save(path);
        }
    }
}