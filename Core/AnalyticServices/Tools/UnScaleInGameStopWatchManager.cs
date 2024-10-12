namespace Core.AnalyticServices.Tools
{
    using System.Collections.Generic;
    using UnityEngine;

    public class UnScaleInGameStopWatchManager : MonoBehaviour
    {
        private Stack<UnScaleInGameStopWatch> pool         = new();
        private List<UnScaleInGameStopWatch>  activeTimers = new();

        public UnScaleInGameStopWatch StartNew()
        {
            var timer = this.pool.Count > 0 ? this.pool.Pop() : new();

            timer.Reset();
            this.activeTimers.Add(timer);
            return timer;
        }

        //Stop then return passed time in miliseconds
        public long Stop(UnScaleInGameStopWatch stopWatch)
        {
            stopWatch.Pause();
            this.pool.Push(stopWatch);
            this.activeTimers.Remove(stopWatch);
            return stopWatch.GetTime();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                // The application is paused
                foreach (var timer in this.activeTimers)
                    timer.Pause();
            else
                // The application is resumed
                foreach (var timer in this.activeTimers)
                    timer.Resume();
        }
    }
}