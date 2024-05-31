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
            this.isPaused = false;
        }
        
        public void Reset()
        {
            this.stopwatch.Reset();
            this.isPaused = false;
        }

        public void Pause()
        {
            if (!this.isPaused)
            {
                this.stopwatch.Stop();
                this.isPaused = true;
            }
        }

        public void Resume()
        {
            if (this.isPaused)
            {
                this.stopwatch.Start();
                this.isPaused = false;
            }
        }

        public long GetTime() { return this.stopwatch.ElapsedMilliseconds; }
    }
}