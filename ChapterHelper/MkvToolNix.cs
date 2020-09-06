using Microsoft.Win32;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ChapterHelper
{
    [ImplementPropertyChanged]
    public class MkvToolNix
    {
        private const string MkvMergeName = "mkvmerge.exe";

        private string _root = String.Empty;
        public string Root
        {
            get
            {
                return _root;
            }
            set
            {
                _root = IsCorrectDirectory(value) ? value : String.Empty;
            }
        }

        private string MkvMerge => Path.Combine(_root, MkvMergeName);

        /// <summary>
        /// Checks whether the path contains a valid MKVToolNix installation.
        /// </summary>
        /// <param name="path">MKVToolNix path</param>
        /// <returns>True if a MKVToolNix installation is present, false otherwise</returns>
        private bool IsCorrectDirectory(string path)
        {
            string mkvMergeCandidate;
            try
            {
                mkvMergeCandidate = Path.Combine(path, MkvMergeName);
            }
            catch
            {
                return false;
            }

            return File.Exists(mkvMergeCandidate);
        }

        /// <summary>
        /// Checks whether the currently registered MKVToolNix path is valid.
        /// </summary>
        public bool Ready => IsCorrectDirectory(Root);

        /// <summary>
        /// Tries to automatically setup the path of MKVToolNix.
        /// </summary>
        public void Setup()
        {
            string appPathKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\mkvtoolnix-gui.exe";

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(appPathKeyName))
                {
                    Object value = key?.GetValue(String.Empty);
                    if (value != null)
                    {
                        Root = Path.GetDirectoryName(value as string);
                        if (Ready)
                        {
                            return;
                        }
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(appPathKeyName))
                {
                    Object value = key?.GetValue(String.Empty);
                    if (value != null)
                    {
                        Root = Path.GetDirectoryName(value as string);
                        if (Ready)
                        {
                            return;
                        }
                    }
                }
            }
            catch { }

            try
            {
                Root = AppDomain.CurrentDomain.BaseDirectory;
                if (Ready)
                {
                    return;
                }
            }
            catch { }

            try
            {
                Root = Directory.GetCurrentDirectory();
            }
            catch { }
        }

        /// <summary>
        /// Escapes strings to use in a MKVToolNix options file.
        /// </summary>
        /// <param name="input">String to escape</param>
        /// <returns>Escaped string</returns>
        private string EscapeOptionsFileString(string input)
        {
            return input.Replace(@"\", @"\\");
        }

        /// <summary>
        /// Creates a string representation supported by MKVToolNix of an <see cref="PreciseTimeSpan"/> object.
        /// </summary>
        /// <param name="time">Object to create string representation from</param>
        /// <returns>String representation</returns>
        private string TimeToString(PreciseTimeSpan time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Nanoseconds:000000000}";
        }

        /// <summary>
        /// Creates an options file to be used by mkvmerge.exe to trim the supplied chapters.
        /// </summary>
        /// <param name="source">File to trim</param>
        /// <param name="chapters">Chapters to use for trimming</param>
        /// <param name="delay">Global delay to apply to all chapters</param>
        /// <param name="destination">File to save the trimmed result as</param>
        /// <returns>Path to created options file</returns>
        private string CreateTrimOptionsFile(string source, IEnumerable<Chapter> chapters, PreciseTimeSpan delay, string destination)
        {
            string optionsFile = Path.GetTempFileName();
            using (StreamWriter trimMuxFileWriter = new StreamWriter(optionsFile))
            {
                trimMuxFileWriter.WriteLine("[");
                trimMuxFileWriter.WriteLine("\"-o\",");
                trimMuxFileWriter.WriteLine("\"" + EscapeOptionsFileString(destination) + "\",");
                trimMuxFileWriter.WriteLine("\"--split\",");
                trimMuxFileWriter.Write("\"parts:");
                List<string> parts = new List<string>();
                foreach (Chapter chapter in chapters)
                {
                    string start = chapter.InputStartTime.TotalNanoseconds == 0 ? String.Empty : TimeToString(chapter.InputStartTime);
                    string end   = chapter.InputEndTime.TotalNanoseconds   == 0 ? String.Empty : TimeToString(chapter.InputEndTime);
                    string part  = $"{start}-{end}";
                    parts.Add(part);
                }
                trimMuxFileWriter.WriteLine(String.Join(",+", parts) + "\",");
                if ((int)delay.TotalMilliseconds != 0)
                {
                    trimMuxFileWriter.WriteLine("\"-y\",");
                    trimMuxFileWriter.WriteLine("\"-1:" + (int)delay.TotalMilliseconds + "\",");
                }
                trimMuxFileWriter.WriteLine("\"" + EscapeOptionsFileString(source) + "\"");
                trimMuxFileWriter.WriteLine("]");
            }
            return optionsFile;
        }

        /// <summary>
        /// Runs a MKVToolNix tool using a options file
        /// </summary>
        /// <param name="fileName">File name of the tool</param>
        /// <param name="optionsFile">Path to options file</param>
        private void RunWithOptionsFile(string fileName, string optionsFile)
        {
            Process p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = fileName,
                    Arguments = "@\"" + optionsFile + "\""
                }
            };
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new ArgumentException(p.ExitCode.ToString());
            }
        }

        /// <summary>
        /// Trims a file using mkvmerge.
        /// </summary>
        /// <param name="source">File to trim</param>
        /// <param name="chapters">Chapters to use for trimming</param>
        /// <param name="delay">Delay to apply to all chapters</param>
        /// <param name="destination">File to save trimmed result as</param>
        public void Trim(string source, IEnumerable<Chapter> chapters, PreciseTimeSpan delay, string destination)
        {
            string trimOptionsFile = CreateTrimOptionsFile(source, chapters, delay, destination);
            RunWithOptionsFile(MkvMerge, trimOptionsFile);
            File.Delete(trimOptionsFile);
        }
    }
}
