namespace Core.AdsServices.Helpers
{
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Ads;

    public static class AdPlacementHelper
    {
        public static bool TryGetPlacementId(string place, AdId defaultId, Dictionary<AdPlacement, AdId> customIds, out string id)
        {
            var placement = AdPlacement.PlacementWithName(place);

            if (placement == null)
            {
                id = defaultId.Id;

                return true;
            }

            id = FindIdForPlacement(customIds, placement);

            if (string.IsNullOrEmpty(id))
            {
                id = defaultId.Id;
            }

            return !string.IsNullOrEmpty(id);
        }

        public static string FindIdForPlacement(Dictionary<AdPlacement, AdId> dict, AdPlacement placement)
        {
            if (dict != null && dict.TryGetValue(placement, out var idObj) && idObj != null && !string.IsNullOrEmpty(idObj.Id))
            {
                return idObj.Id;
            }

            return string.Empty;
        }
    }
}