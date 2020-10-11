using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ScrivenerWatch.App.Commands;
using ScrivenerWatch.App.Utils;

namespace ScrivenerWatch.App.Model
{
    internal partial class ApplicationModel : INotifyPropertyChanged, IDisposable
    {
        private string mFilePath = "";

        private FileSystemWatcher? mWatcher;
        private ScrivenerSnapshot? mLatestSnapshot;
        private SnapshotDifference? mLatestDifference;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ApplicationModel()
        {
            this.BrowseFile = new BrowseForScrivenerLocationCommand(this);
            this.MergeFiles = new MergeFilesCommand(this);
        }

        public ICommand BrowseFile { get; }

        public ICommand MergeFiles { get; }

        public SnapshotDifference? LatestDifference => this.GetLatestDifference();

        private SnapshotDifference? GetLatestDifference()
        {
            if (mLatestDifference == null)
            {
                var lastSnapshot = this.Snapshots.LastOrDefault();
                var latestSnapshot = this.LatestSnapshot;

                if (lastSnapshot == null || latestSnapshot == null)
                {
                    return null;
                }

                mLatestDifference = new SnapshotDifference(lastSnapshot, latestSnapshot);
            }

            return mLatestDifference;
        }

        public string FilePath
        {
            get => mFilePath;
            set
            {
                if (value == mFilePath)
                {
                    return;
                }

                mFilePath = value;

                if (mWatcher != null)
                {
                    mWatcher.Dispose();
                    mWatcher = null;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    mWatcher = new FileSystemWatcher(Path.GetDirectoryName(value)!)
                    {
                        NotifyFilter = NotifyFilters.LastWrite,
                        Filter = Path.GetFileName(value),
                    };

                    mWatcher.Changed += this.ScrivenerFile_Changed;
                    mWatcher.EnableRaisingEvents = true;
                }

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FilePath)));

                this.AddSnapshot().IgnoreAwait();
            }
        }

        public ScrivenerSnapshot? LatestSnapshot
        {
            get => mLatestSnapshot;
            set
            {
                if (value == mLatestSnapshot)
                {
                    return;
                }

                mLatestSnapshot = value;
                mLatestDifference = null;

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LatestSnapshot)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LatestDifference)));
            }
        }

        public ObservableCollection<ScrivenerSnapshot> Snapshots { get; } = new ObservableCollection<ScrivenerSnapshot>();

        private async void ScrivenerFile_Changed(object sender, FileSystemEventArgs e)
        {
            // Very good chance that the file is still beeing modified, so wait a little
            await Task.Delay(10);

            this.LatestSnapshot = await this.CreateSnapShot();
        }

        private async Task AddSnapshot()
        {
            var snapshot = await this.CreateSnapShot().ConfigureAwait(true);

            this.Snapshots.Add(snapshot);
            this.LatestSnapshot = snapshot;
        }

        private async Task<ScrivenerSnapshot> CreateSnapShot()
        {
            var baseEntries = await ScrivenerUtilities.GetDraftInformation(this.FilePath);

            var entries =
                await Task.WhenAll(
                    baseEntries.Select(async i =>
                    {
                        var text = await RichTextUtilities.ReadFromFile(this.GetFilePath(i.Id));

                        return new ScivenerEntry(i.Id, i.Title, text, TextUtilities.GetWordCount(text));
                    })
                    .ToArray());

            return new ScrivenerSnapshot(entries, DateTime.UtcNow);
        }

        internal string GetFilePath(string id)
        {
            return $"{Path.GetDirectoryName(this.FilePath)}\\Files\\Docs\\{id}.rtf";
        }

        public void Dispose()
        {
            mWatcher?.Dispose();
        }
    }
}
