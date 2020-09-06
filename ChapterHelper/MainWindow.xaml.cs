using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ChapterHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ChapterCollection Chapters { get; set; }
        public MkvToolNix MkvToolNix { get; set; }

        public MainWindow()
        {
            Chapters = new ChapterCollection();
            MkvToolNix = new MkvToolNix();
            MkvToolNix.Setup();
            InitializeComponent();
            ChapterDataGrid.CurrentCellChanged += (sender, e) =>
            {
                var context = ChapterDataGrid.DataContext;
                ChapterDataGrid.DataContext = null;
                ChapterDataGrid.DataContext = context;
            };
        }

        /// <summary>
        /// Checks whether the current MKVToolNix instance is ready to be used and if not, tries to set it up.
        /// On failure the user is asked to set it up manually.
        /// </summary>
        /// <returns>True if MKVToolNix can be used, false otherwise.</returns>
        private async Task<bool> EnsureMkvToolNixReady()
        {
            if (MkvToolNix.Ready) return true;
            MkvToolNix.Setup();
            if (MkvToolNix.Ready) return true;
            settingsFlyout.IsOpen = true;

            var tcs = new TaskCompletionSource<bool>();
            RoutedEventHandler wait = (sender, e) => tcs.SetResult(true);
            settingsFlyout.ClosingFinished += wait;
            await tcs.Task;
            settingsFlyout.ClosingFinished -= wait;

            return MkvToolNix.Ready;
        }

        private void AddChapter_Click(object sender, RoutedEventArgs e) => Chapters.Add();

        private void RemoveChapter_Click(object sender, RoutedEventArgs e)
        {
            while (ChapterDataGrid.SelectedIndex > -1)
            {
                Chapters.RemoveAt(ChapterDataGrid.SelectedIndex);
            }
        }

        /// <summary>
        /// Extracts the delay specified like "DELAY Xms" in a file name.
        /// </summary>
        /// <param name="path">File path that should be checked</param>
        /// <returns>Delay specified in the file name</returns>
        private PreciseTimeSpan GetDelayFromFileName(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string delayPattern = @"DELAY (\-?\d+)ms";
            Match match = Regex.Match(fileName, delayPattern);
            if (match.Success)
            {
                double milliseconds;
                if (Double.TryParse(match.Groups[1].Value, out milliseconds))
                {
                    return PreciseTimeSpan.FromMilliseconds(milliseconds);
                }
            }
            return PreciseTimeSpan.Zero;
        }

        /// <summary>
        /// Splits a file with MKVToolNix based on currently defined chapters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SplitFile_Click(object sender, RoutedEventArgs e)
        {
            // display error if not all input start/end frames or frame rates were specified
            if (!Chapters.RequiredInputTimesSet)
            {
                await this.ShowMessageAsync(Properties.Resources.MissingInformationHeading, Properties.Resources.MissingInformationDescription);
                return;
            }

            // display error if no usable MKVToolNix installation is present
            if (!await EnsureMkvToolNixReady())
            {
                await this.ShowMessageAsync(Properties.Resources.MkvToolNixNotReadyHeading, Properties.Resources.MkvToolNixNotReadyDescription);
                return;
            }

            // all prerequisite checks passed

            // let user set the source file to split
            OpenFileDialog sourceDialog = new OpenFileDialog
            {
                Filter = $"{Properties.Resources.AllTypesFileFilter}|*.*"
            };
            if (sourceDialog.ShowDialog() != true)
            {
                return;
            }

            // let user customize split settings
            SplitFileSettingsDialog splitFileSettingsDialog = new SplitFileSettingsDialog
            {
                Delay = GetDelayFromFileName(sourceDialog.FileName)
            };
            await this.ShowMetroDialogAsync(splitFileSettingsDialog);
            SplitFileSettingsDialog.MessageDialogResult splitFileSettingsDialogResult =
                await splitFileSettingsDialog.WaitForButtonPressAsync();
            await this.HideMetroDialogAsync(splitFileSettingsDialog);
            if (splitFileSettingsDialogResult == SplitFileSettingsDialog.MessageDialogResult.Negative)
            {
                return;
            }
            PreciseTimeSpan delay = splitFileSettingsDialog.Delay;

            // let user set the destination path
            SaveFileDialog destinationDialog = new SaveFileDialog
            {
                Filter = $"{Properties.Resources.AllTypesFileFilter}|*.*"
            };
            if (destinationDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                MkvToolNix.Trim(sourceDialog.FileName, Chapters, delay, destinationDialog.FileName);
            }
            catch (Exception exception)
            {
                await this.ShowMessageAsync(Properties.Resources.SplitError, exception.Message);
            }
        }

        /// <summary>
        /// Exports a Matroska chapter file based on currently defined chapters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportChapters_Click(object sender, RoutedEventArgs e)
        {
            // display error if output times are unspecified
            if (!Chapters.RequiredOutputTimesSet)
            {
                await this.ShowMessageAsync(Properties.Resources.OutputTimesUnspecified,
                        Properties.Resources.OutputTimesUnspecifiedDescription);
                return;
            }

            // let user set the destination path
            var dialog = new SaveFileDialog
            {
                FileName = Properties.Resources.Chapters,
                DefaultExt = ".xml",
                Filter = $"{Properties.Resources.MatroskaChapterFileFilter}|*.xml"
            };
            if (dialog.ShowDialog() != true) return;

            // write English-tagged chapter file
            // TODO: Make chapter language selectable
            var writer = new MatroskaChapterWriter(Chapters, "eng");
            try
            {
                writer.WriteToFile(dialog.FileName);
            }
            catch (Exception exception)
            {
                await this.ShowMessageAsync(Properties.Resources.ErrorWritingFile, exception.Message);
            }
        }

        /// <summary>
        /// Exports a timecodes file based on currently defined chapters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportTimecodes_Click(object sender, RoutedEventArgs e)
        {
            // display error if any output time is unspecified
            // TODO: Handle the case when the time of the last chapter are not required because of the common frame rate
            if (!Chapters.AllOutputTimesSet)
            {
                await this.ShowMessageAsync(Properties.Resources.MissingInformationHeading, Properties.Resources.MissingInformationDescription);
                return;
            }

            // let user set the destination path
            var dialog = new SaveFileDialog
            {
                FileName = Properties.Resources.Timecodes,
                DefaultExt = ".txt",
                Filter = $"{Properties.Resources.TimecodesFileFilter}|*.txt"
            };
            if (dialog.ShowDialog() != true) return;

            // write timecodes file
            var writer = new TimecodeFormat1Writer(Chapters);
            try
            {
                writer.WriteToFile(dialog.FileName);
            }
            catch (Exception exception)
            {
                await this.ShowMessageAsync(Properties.Resources.ErrorWritingFile, exception.Message);
            }
        }

        /// <summary>
        /// Exports a qp file based on currently defined chapters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportQpFile_Click(object sender, RoutedEventArgs e)
        {
            // display error if output times are unspecified
            if (!Chapters.RequiredOutputTimesSet)
            {
                await this.ShowMessageAsync(Properties.Resources.MissingInformationHeading, Properties.Resources.MissingInformationDescription);
                return;
            }

            // let user set the destination path
            var dialog = new SaveFileDialog
            {
                FileName = Properties.Resources.QpFile,
                DefaultExt = ".txt",
                Filter = $"{Properties.Resources.QpFileFilter}|*.txt"
            };
            if (dialog.ShowDialog() != true) return;

            var writer = new QpFileWriter(Chapters);
            try
            {
                writer.WriteToFile(dialog.FileName);
            }
            catch (Exception exception)
            {
                await this.ShowMessageAsync(Properties.Resources.ErrorWritingFile, exception.Message);
            }
        }

        /// <summary>
        /// Reinitializes the chapter list with new chapters based on AviSynth Trim commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoadFromTrims_Click(object sender, RoutedEventArgs e)
        {
            var settings = new MetroDialogSettings();
            var content = new LoadFromTrimsDialog();
            var dialog = new CustomDialog
            {
                Title = Properties.Resources.LoadFromTrimsHeader,
                Content = content
            };
            Task showTask = this.ShowMetroDialogAsync(dialog, settings);
            string input = await content.WaitForButtonPressAsync();
            await this.HideMetroDialogAsync(dialog, settings);
            if (input == null) return;

            Chapters = ChapterCollection.FromAviSynthTrims(input);
            DataContext = null;
            DataContext = this;
        }

        /// <summary>
        /// Lets the user set a custom path to MKVToolNix.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetMkvToolNixRoot_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            MkvToolNix.Root = dialog.FileName;
        }
    }
}
