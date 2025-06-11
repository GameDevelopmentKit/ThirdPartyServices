namespace Core.AdsServices.Helpers
{
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Ads;

    public static class AdPlacementHelper
    {
        public static bool TryGetPlacementId(string place, CrossPlatformValue defaultId, Dictionary<AdPlacement, CrossPlatformValue> customIds, out string id)
        {
            var placement = AdPlacement.PlacementWithName(place);
            if (placement == AdPlacement.Default)
            {
                id = defaultId?.DefaultValue;
            }
            else
            {
                id = FindIdForPlacement(customIds, placement);
                if (string.IsNullOrEmpty(id))
                {
                    id = defaultId?.DefaultValue;
                }
            }
            return !string.IsNullOrEmpty(id);
        }

        public static string FindIdForPlacement(Dictionary<AdPlacement, CrossPlatformValue> dict, AdPlacement placement)
        {
            if (dict != null && dict.TryGetValue(placement, out var idObj) && idObj != null && !string.IsNullOrEmpty(idObj.DefaultValue)) return idObj.DefaultValue;

            return string.Empty;
        }
    }
}