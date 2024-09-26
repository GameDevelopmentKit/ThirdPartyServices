#nullable enable
namespace ServiceImplementation.Analytic.Validator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class AnalyticEventValidator : MonoBehaviour
    {
        [SerializeField] private GameObject    report        = null!;
        [SerializeField] private TMP_Text      reportTxt     = null!;
        [SerializeField] private Button        validateBtn   = null!;
        [SerializeField] private RectTransform reportContent = null!;

        private readonly List<string> loggedEvents = new();

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            var isEventLog = logString.Contains("[onelog]") && logString.Contains("Track Event");
            if (!isEventLog) return;
            this.loggedEvents.Add(logString);
        }

        private void ValidateEvents()
        {
            var requiredEvents = this.GetRequiredEvents();
            var missingEvents  = requiredEvents.Except(this.loggedEvents).ToList();
            this.WriteReport(new AnalyticEventsReportInfo(missingEvents));
        }

        private void WriteReport(AnalyticEventsReportInfo reportInfo)
        {
            this.reportTxt.text = $"- Missing events: {string.Join(", ", reportInfo.MissingEvents)}";
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.reportContent);
        }

        private IEnumerable<string> GetRequiredEvents()
        {
            var configFile = Resources.Load<TextAsset>("analytic_event_config");
            if (configFile == null) throw new Exception("AnalyticEventConfig not found");

            var analyticEventConfig = JsonConvert.DeserializeObject<AnalyticEventConfig>(configFile.text)!;
            return analyticEventConfig.RequiredEvents;
        }

        private void Awake()
        {
            DontDestroyOnLoad(this.transform);

            Application.logMessageReceivedThreaded += this.HandleLog;
            this.validateBtn.onClick.AddListener(this.ValidateEvents);
        }

        private const float TapInterval            = 0.5f;
        private const int   TouchTimesToShowReport = 4;
        private       int   touchCount;
        private       float lastTouchTime;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - this.lastTouchTime <= TapInterval)
                {
                    this.touchCount++;
                }
                else
                {
                    this.touchCount = 1;
                }

                this.lastTouchTime = Time.time;

                if (this.touchCount == TouchTimesToShowReport)
                {
                    this.report.gameObject.SetActive(!this.report.gameObject.activeSelf);

                    this.touchCount = 0;
                }
            }

            if (Time.time - this.lastTouchTime > TapInterval)
            {
                this.touchCount = 0;
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= this.HandleLog;
            this.validateBtn.onClick.RemoveListener(this.ValidateEvents);
        }
    }
}