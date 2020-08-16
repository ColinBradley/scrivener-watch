using ScrivenerWatch.App.Utils;
using System;
using System.Linq;

namespace ScrivenerWatch.App.Model
{
    internal class ScrivenerSnapshot
    {
        private int? mTotalWordCount;

        public ScrivenerSnapshot(ScivenerEntry[] files, DateTime time)
        {
            this.Files = files;
            this.Time = time;
        }

        public ScivenerEntry[] Files { get; }

        public DateTime Time { get; }

        public int TotalWordCount
        {
            get
            {
                if (mTotalWordCount == null)
                {
                    mTotalWordCount = this.Files.Sum(f => f.WordCount);
                }

                return mTotalWordCount.Value;
            }
        }
    }
}
