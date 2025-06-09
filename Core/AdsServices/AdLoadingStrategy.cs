namespace Core.AdsServices
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public sealed class AdLoadingStrategy
    {
        private DateTime lastTimeFailed = DateTime.Now;
        private int      loadFailedCount;

        private float DelayLoadingNextAds => this.loadFailedCount == 0 ? 0 : Mathf.Pow(2, this.loadFailedCount);
        public  bool  IsWaiting           => DateTime.Now.Subtract(this.lastTimeFailed).TotalSeconds < this.DelayLoadingNextAds;

        public UniTask WaitingAsync()
        {
            return UniTask.WaitUntil(() => !this.IsWaiting);
        }

        public void LoadFailed()
        {
            this.loadFailedCount++;
            this.lastTimeFailed = DateTime.Now;
        }

        public void LoadSucceeded()
        {
            this.loadFailedCount = 0;
            this.lastTimeFailed  = DateTime.Now;
        }
    }
}