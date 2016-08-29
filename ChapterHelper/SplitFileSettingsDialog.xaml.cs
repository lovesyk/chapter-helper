using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChapterHelper
{
    public partial class SplitFileSettingsDialog : UserControl
    {
        internal SplitFileSettingsDialog() : base()
        {
            InitializeComponent();
        }

        public Task<bool> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                this.Focus();
            }));

            var tcs = new TaskCompletionSource<bool>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = null;

            cleanUpHandlers = () => {
                NegativeButton.Click -= negativeHandler;
                AffirmativeButton.Click -= affirmativeHandler;

                NegativeButton.KeyDown -= negativeKeyHandler;
                AffirmativeButton.KeyDown -= affirmativeKeyHandler;

                KeyDown -= escapeKeyHandler;
            };

            negativeKeyHandler = (sender, e) => {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(false);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(true);
                }
            };

            negativeHandler = (sender, e) => {
                cleanUpHandlers();

                tcs.TrySetResult(false);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) => {
                cleanUpHandlers();

                tcs.TrySetResult(true);

                e.Handled = true;
            };

            escapeKeyHandler = (sender, e) => {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(true);
                }
                else if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(true);
                }
            };

            NegativeButton.KeyDown += negativeKeyHandler;
            AffirmativeButton.KeyDown += affirmativeKeyHandler;

            NegativeButton.Click += negativeHandler;
            AffirmativeButton.Click += affirmativeHandler;

            KeyDown += escapeKeyHandler;

            return tcs.Task;
        }

        public PreciseTimeSpan Delay { get; set; }
    }
}
