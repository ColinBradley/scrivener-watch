using ScrivenerWatch.App.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ScrivenerWatch.App.Model
{
    internal record DifferenceState(DifferenceType Difference, int RemovedWordCount, int AddedWordCount, int ModifiedWordCount);

    internal class FileDifference
    {
        private static readonly SolidColorBrush sAddedColor = new SolidColorBrush(Colors.Green);
        private static readonly SolidColorBrush sRemovedColor = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush sModifiedColor = new SolidColorBrush(Colors.Orange);

        private DifferenceState? mDifference;

        public FileDifference(ScivenerEntry? oldFile, ScivenerEntry? newFile)
        {
            this.OldFile = oldFile;
            this.NewFile = newFile;
        }

        public string Title { get => this.NewFile?.Title ?? this.OldFile?.Title ?? "Error"; }

        public string DifferenceTypeSuffix
            => this.GetDifference().Difference switch
            {
                DifferenceType.Added => " [A]",
                DifferenceType.Removed => " [D]",
                DifferenceType.Modified => " [M]",
                DifferenceType.Equal => "",
                _ => throw new NotImplementedException(),
            };

        public SolidColorBrush DifferenceTypeSuffixBrush
            => this.GetDifference().Difference switch
            {
                DifferenceType.Added => sAddedColor,
                DifferenceType.Removed => sRemovedColor,
                DifferenceType.Modified => sModifiedColor,
                DifferenceType.Equal => SystemColors.ControlTextBrush,
                _ => throw new NotImplementedException(),
            };

        public ScivenerEntry? OldFile { get; }

        public ScivenerEntry? NewFile { get; }

        public DifferenceState Difference { get => this.GetDifference(); }

        public bool IsSelected { get; set; }

        private DifferenceState GetDifference()
        {
            if (mDifference != null)
            {
                return mDifference;
            }

            switch ((this.OldFile, this.NewFile))
            {
                case (null, ScivenerEntry newFile):
                    mDifference = new DifferenceState(DifferenceType.Added, 0, newFile.WordCount, 0);
                    return mDifference;
                case (ScivenerEntry oldFile, null):
                    mDifference = new DifferenceState(DifferenceType.Removed, oldFile.WordCount, 0, 0);
                    return mDifference;
            }

            if (this.OldFile == null || this.NewFile == null)
            {
                throw new Exception("Invalid state");
            }

            var differences = DiffPlex.Differ.Instance.CreateWordDiffs(this.OldFile.Text, this.NewFile.Text, false, TextUtilities.DiffernceSeparators);

            if (differences == null || differences.DiffBlocks.Count == 0)
            {
                mDifference = new DifferenceState(DifferenceType.Equal, 0, 0, 0);
                return mDifference;
            }

            var removedCount = 0;
            var addedCount = 0;
            var modifiedCount = 0;

            var oldSpan = differences.PiecesOld;
            var newSpan = differences.PiecesNew;

            foreach (var diff in differences.DiffBlocks)
            {
                var deleteCount = differences.PiecesOld.Skip(diff.DeleteStartA).Take(diff.DeleteCountA).Where(w => w.Any(c => char.IsLetterOrDigit(c))).Count();
                var insertCount = differences.PiecesNew.Skip(diff.InsertStartB).Take(diff.InsertCountB).Where(w => w.Any(c => char.IsLetterOrDigit(c))).Count();
                
                if (deleteCount > 0 && insertCount > 0 && diff.DeleteStartA == diff.InsertStartB)
                {
                    var modified = Math.Min(deleteCount, insertCount);
                    modifiedCount += modified;

                    var deleted = deleteCount - modified;
                    if (deleted > 0)
                    {
                        removedCount += deleted;
                    }

                    var inserted = insertCount - modified;
                    if (inserted > 0)
                    {
                        addedCount += inserted;
                    }

                    continue;
                }

                removedCount += deleteCount;
                addedCount += insertCount;
            }

            mDifference = new DifferenceState(DifferenceType.Modified, removedCount, addedCount, modifiedCount);
            return mDifference;
        }
    }
}
