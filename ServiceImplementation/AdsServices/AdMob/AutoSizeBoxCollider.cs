namespace ServiceImplementation.AdsServices.AdMob
{
    using UnityEngine;

    public class AutoSizeBoxCollider : MonoBehaviour
    {
        private RectTransform rect;
        private BoxCollider   box;
        private BoxCollider   Box  => this.box ??= this.GetComponent<BoxCollider>();
        private RectTransform Rect => this.rect ??= this.GetComponent<RectTransform>();

        private void OnEnable() { this.Box.size = new Vector3(this.Rect.rect.width, this.Rect.rect.height, 1); }

        private void OnRectTransformDimensionsChange() { this.Box.size = new Vector3(this.Rect.rect.width, this.Rect.rect.height, 1); }
    }
}