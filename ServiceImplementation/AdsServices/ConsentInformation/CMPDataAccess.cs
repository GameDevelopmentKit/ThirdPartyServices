namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using UnityEngine;

    public static class CMPDataAccess
    {
        #region Helpers

        public static int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        #endregion

        // Number: The unsigned integer ID of CMP SDK
        public static int GetCmpSdkID()
        {
            return GetInt("IABTCF_CmpSdkID", -1); // Default to -1 if not set
        }

        // Number: The unsigned integer version number of CMP SDK
        public static int GetCmpSdkVersion()
        {
            return GetInt("IABTCF_CmpSdkVersion", -1); // Default to -1 if not set
        }

        // Number: The unsigned integer representing the version of the TCF that these consents adhere to.
        public static int GetPolicyVersion()
        {
            return GetInt("IABTCF_PolicyVersion", -1); // Default to -1 if not set
        }

        // Number:
        // 1 - GDPR applies in current context
        // 0 - GDPR does not apply in current context
        // Unset - undetermined (default before initialization)
        public static int GetGDPRApplicability()
        {
            return GetInt("IABTCF_gdprApplies", -1); // Default to -1 (undetermined) if not set
        }

        // String: Two-letter ISO 3166-1 alpha-2 code – Default: AA (unknown)
        public static string GetPublisherCC()
        {
            return GetString("IABTCF_PublisherCC", "AA"); // Default to 'AA' (unknown) if not set
        }

        // Number:
        // 0 - no special treatment of purpose one
        // 1 - purpose one not disclosed
        // Unset default - 0
        public static int GetPurposeOneTreatment()
        {
            return GetInt("IABTCF_PurposeOneTreatment", 0); // Default to 0 if not set
        }

        // Number:
        // 1 - CMP uses customized stack descriptions and/or modified or supplemented standard Illustrations
        // 0 - CMP did not use a non-standard stack desc. and/or modified or supplemented Illustrations
        public static int GetUseNonStandardTexts()
        {
            return GetInt("IABTCF_UseNonStandardTexts", 0); // Default to 0 if not set
        }

        // String: Full encoded TC string
        public static string GetTCString()
        {
            return GetString("IABTCF_TCString", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the consent status for Vendor ID n+1; false and true respectively. eg. '1' at index 0 is consent true for vendor ID 1
        public static string GetVendorConsents()
        {
            return GetString("IABTCF_VendorConsents", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the legitimate interest status for Vendor ID n+1; false and true respectively. eg. '1' at index 0 is legitimate interest established true for vendor ID 1
        public static string GetVendorLegitimateInterests()
        {
            return GetString("IABTCF_VendorLegitimateInterests", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the consent status for purpose ID n+1; false and true respectively. eg. '1' at index 0 is consent true for purpose ID 1
        public static string GetPurposeConsents()
        {
            return GetString("IABTCF_PurposeConsents", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the legitimate interest status for purpose ID n+1; false and true respectively. eg. '1' at index 0 is legitimate interest established true for purpose ID 1
        public static string GetPurposeLegitimateInterests()
        {
            return GetString("IABTCF_PurposeLegitimateInterests", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the opt-in status for special feature ID n+1; false and true respectively. eg. '1' at index 0 is opt-in true for special feature ID 1
        public static string GetSpecialFeaturesOptIns()
        {
            return GetString("IABTCF_SpecialFeaturesOptIns", string.Empty); // Default to empty string if not set
        }

        // String ['0','1', or '2']: The value at position n – where n's indexing begins at 0 – indicates the publisher restriction type (0-2) for vendor n+1; (see Publisher Restrictions Types). eg. '2' at index 0 is restrictionType 2 for vendor ID 1. {ID} refers to the purpose ID.
        public static string GetPublisherRestrictions(int id)
        {
            return GetString($"IABTCF_PublisherRestrictions{id}", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the purpose consent status for purpose ID n+1 for the publisher as they correspond to the Global Vendor List Purposes; false and true respectively. eg. '1' at index 0 is consent true for purpose ID 1
        public static string GetPublisherConsent()
        {
            return GetString("IABTCF_PublisherConsent", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the purpose legitimate interest status for purpose ID n+1 for the publisher as they correspond to the Global Vendor List Purposes; false and true respectively. eg. '1' at index 0 is legitimate interest established true for purpose ID 1
        public static string GetPublisherLegitimateInterests()
        {
            return GetString("IABTCF_PublisherLegitimateInterests", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the purpose consent status for the publisher's custom purpose ID n+1 for the publisher; false and true respectively. eg. '1' at index 0 is consent true for custom purpose ID 1
        public static string GetPublisherCustomPurposesConsents()
        {
            return GetString("IABTCF_PublisherCustomPurposesConsents", string.Empty); // Default to empty string if not set
        }

        // Binary String: The '0' or '1' at position n – where n's indexing begins at 0 – indicates the purpose legitimate interest status for the publisher's custom purpose ID n+1 for the publisher; false and true respectively. eg. '1' at index 0 is legitimate interest established true for custom purpose ID 1
        public static string GetPublisherCustomPurposesLegitimateInterests()
        {
            return GetString("IABTCF_PublisherCustomPurposesLegitimateInterests", string.Empty); // Default to empty string if not set
        }

        public static string GetHumanReadableTCString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"IABTCF_CmpSdkID: {GetCmpSdkID()}");
            sb.AppendLine($"IABTCF_CmpSdkVersion: {GetCmpSdkVersion()}");
            sb.AppendLine($"IABTCF_PolicyVersion: {GetPolicyVersion()}");
            sb.AppendLine($"IABTCF_gdprApplies: {GetGDPRApplicability()}");
            sb.AppendLine($"IABTCF_PublisherCC: {GetPublisherCC()}");
            sb.AppendLine($"IABTCF_PurposeOneTreatment: {GetPurposeOneTreatment()}");
            sb.AppendLine($"IABTCF_UseNonStandardTexts: {GetUseNonStandardTexts()}");
            sb.AppendLine($"IABTCF_TCString: {GetTCString()}");
            sb.AppendLine($"IABTCF_VendorConsents: {GetVendorConsents()}");
            sb.AppendLine($"IABTCF_VendorLegitimateInterests: {GetVendorLegitimateInterests()}");
            sb.AppendLine($"IABTCF_PurposeConsents: {GetPurposeConsents()}");
            sb.AppendLine($"IABTCF_PurposeLegitimateInterests: {GetPurposeLegitimateInterests()}");
            sb.AppendLine($"IABTCF_SpecialFeaturesOptIns: {GetSpecialFeaturesOptIns()}");
            sb.AppendLine($"IABTCF_PublisherRestrictions: {GetPublisherRestrictions(0)}");
            sb.AppendLine($"IABTCF_PublisherConsent: {GetPublisherConsent()}");
            sb.AppendLine($"IABTCF_PublisherLegitimateInterests: {GetPublisherLegitimateInterests()}");
            sb.AppendLine($"IABTCF_PublisherCustomPurposesConsents: {GetPublisherCustomPurposesConsents()}");
            sb.AppendLine($"IABTCF_PublisherCustomPurposesLegitimateInterests: {GetPublisherCustomPurposesLegitimateInterests()}");
            return sb.ToString();
        }
    }
}