using System.Collections.ObjectModel;

namespace ChapterHelper
{
    internal interface ChapterImporter
    {
        ObservableCollection<Chapter> Chapters { get; }
    }
}
