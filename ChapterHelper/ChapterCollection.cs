using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace ChapterHelper
{
    public class ChapterCollection : BindingList<Chapter>
    {
        /// <summary>
        /// Force reload of list on every update.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, 0));
        }

        /// <summary>
        /// Fetches a chapter with a position relative to a specified one.
        /// </summary>
        /// <param name="chapter">Base chapter</param>
        /// <param name="offset">Positive or negative offset</param>
        /// <returns><see cref="Chapter"/> object of the specified relative offset or null if none</returns>
        public Chapter ElementAtOffsetOrDefault(Chapter chapter, int offset)
        {
            int index = IndexOf(chapter);
            return this.ElementAtOrDefault(index + offset);
        }

        /// <summary>
        /// Queries whether all chapters have specified input start and end times.
        /// </summary>
        /// <returns>True if all required output times are set, false otherwise</returns>
        public bool RequiredInputTimesSet => this.All(chapter => chapter.InputStartTime != null);

        /// <summary>
        /// Queries whether all chapters have a specified output start
        /// and end (except in the last chapter) time.
        /// </summary>
        /// <returns>True if all required output times are set, false otherwise</returns>
        public bool RequiredOutputTimesSet => this.All(chapter => chapter.OutputStartTime != null);

        /// <summary>
        /// Queries whether all chapters have a specified output start and end time.
        /// </summary>
        /// <returns>True if all required output times are set, false otherwise</returns>
        public bool AllOutputTimesSet => this.All(chapter => chapter.OutputEndTime != null);

        /// <summary>
        /// Adds an empty chapter at the end of the collection.
        /// </summary>
        public void Add() => Add(new Chapter(this));

        public static ChapterCollection FromAviSynthTrims(string script)
        {
            ChapterCollection chapterCollection = new ChapterCollection();
            foreach (Match match in Regex.Matches(script, @"trim\s*\(\s*(\d+)\s*\,\s*(\d+)\s*\)"))
            {
                Chapter chapter = new Chapter(chapterCollection);
                int inputFirstFrame;
                if (Int32.TryParse(match.Groups[1].Value, out inputFirstFrame))
                {
                    chapter.InputFirstFrame = inputFirstFrame;
                }
                int inputLastFrame;
                if (Int32.TryParse(match.Groups[2].Value, out inputLastFrame))
                {
                    chapter.InputLastFrame = inputLastFrame;
                }
                chapterCollection.Add(chapter);
            }
            return chapterCollection;
        }
    }
}
