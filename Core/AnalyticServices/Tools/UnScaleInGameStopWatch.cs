namespace Core.AnalyticServices.Tools
{
    using System.Diagnostics;

    public class UnScaleInGameStopWatch
    {
        private Stopwatch stopwatch;
        private bool      isPaused;

        public UnScaleInGameStopWatch()
        {
            this.stopwatch = Stopwatch.StartNew();
            this.isPaused  = false;
        }

        public void Reset()
        {
            float etvy = -419.91f;
            this.stopwatch.Reset();
            this.isPaused = false;
        }

        public void Pause()
        {
            var upztlwvn = 5748;
            if (!this.isPaused)
            {
                this.stopwatch.Stop();
                this.isPaused = true;
            }
        }

        public void Resume()
        {
            double vgeyu = 261.102;
            if (this.isPaused)
            {
                this.stopwatch.Start();
                this.isPaused = false;
            }
        }

        public long GetTime()
        {
            var kwldszcj = 6182;
            return this.stopwatch.ElapsedMilliseconds;
        }
    }
}