namespace TheOne.Simulation.Job.Security.Scripts.AdsServices
{
    using System.Collections.Generic;
    using Gadsme;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class GadsmeCheckRaycaster : MonoBehaviour
    {
        [SerializeField] private GraphicRaycaster raycaster;
        private                  bool             forwardToGadsme = true;
        private                  PointerEventData pointerEventData;
        private void Start()
        {
            GadsmeSDK.SetInteractionsEnabled(false);
            this.raycaster = this.GetComponent<GraphicRaycaster>();
        }
        private void Update()
        {
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    this.forwardToGadsme = true;

                    var pointerEventData = new PointerEventData(EventSystem.current);
                    pointerEventData.position = touch.position;
                    
                    var results = new List<RaycastResult>();
                    this.raycaster.Raycast(pointerEventData, results);

                    if (results.Count > 0)
                    {
                        if (GadsmeSDK.IsPlacementResult(results[0])) continue;
                        this.forwardToGadsme = false;
                    }
                }
            }
            if (this.forwardToGadsme) GadsmeSDK.HandlePlacementInteractions();
        }
    }
}