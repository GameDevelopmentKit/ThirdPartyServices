namespace Core.AdsServices.Helpers
{
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Ads;

    public static class AdPlacementHelper
    {
        public static bool TryGetPlacementId(string place, AdId defaultId, Dictionary<AdPlacement, AdId> customIds, out string id)
        {
            var placement = AdPlacement.PlacementWithName(place);
            id = placement == AdPlacement.Default
                ? defaultId?.Id
                : FindIdForPlacement(customIds, placement);

            return !string.IsNullOrEmpty(id);
        }

        public static string FindIdForPlacement(Dictionary<AdPlacement, AdId> dict, AdPlacement placement)
        {
            if (dict != null && dict.TryGetValue(placement, out var idObj) && idObj != null && !string.IsNullOrEmpty(idObj.Id)) return idObj.Id;

            return string.Empty;
        }
    }
}