using System;
using System.Linq;
using System.Windows.Controls;

namespace ScrivenerWatch.App.Model
{
    internal class SnapshotDifference
    {
        private FileDifference[]? mDifferences;

        public SnapshotDifference(ScrivenerSnapshot a, ScrivenerSnapshot b)
        {
            this.A = a;
            this.B = b;
        }

        public ScrivenerSnapshot A { get; }

        public ScrivenerSnapshot B { get; }

        public int TotalWordsAdded { get => this.GetDifferences().Sum(d => d.Difference.AddedWordCount); }

        public int TotalWordsRemoved { get => this.GetDifferences().Sum(d => d.Difference.RemovedWordCount); }

        public int TotalWordsModifed { get => this.GetDifferences().Sum(d => d.Difference.ModifiedWordCount); }

        public FileDifference[] AllDifferences { get => this.GetDifferences(); }

        private FileDifference[] GetDifferences()
        {
            if (mDifferences == null)
            {
                var aFilesById = this.A.Files.ToDictionary(f => f.Id);
                var bFilesById = this.B.Files.ToDictionary(f => f.Id);

                mDifferences =
                    aFilesById.Keys
                        .Concat(aFilesById.Keys)
                        .Distinct()
                        .Select(id =>
                        {
                            aFilesById.TryGetValue(id, out var aFile);
                            bFilesById.TryGetValue(id, out var bFile);

                            return new FileDifference(aFile, bFile);
                        })
                        .ToArray();
            }

            return mDifferences;
        }
    }
}
