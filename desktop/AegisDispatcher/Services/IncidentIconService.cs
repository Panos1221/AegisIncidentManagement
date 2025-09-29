using AegisDispatcher.Models;

namespace AegisDispatcher.Services
{
    /// <summary>
    /// Service for providing incident icon paths based on category and status.
    /// This service mirrors the functionality of the web app's incidentIcons.ts file.
    /// </summary>
    public class IncidentIconService
    {
        private const string BaseIconUrl = "https://aegis-icons.local/Incidents";

        /// <summary>
        /// Gets the correct icon path based on incident category, status, and agency
        /// </summary>
        /// <param name="mainCategory">The main category of the incident</param>
        /// <param name="status">The current status of the incident</param>
        /// <param name="userAgency">The agency handling the incident (optional)</param>
        /// <returns>The URL path to the appropriate icon</returns>
        public static string GetIncidentIconPath(string mainCategory, IncidentStatus status, string userAgency = null)
        {
            var statusKey = GetStatusKey(status);

            // All agencies except Fire Department use help icons
            if (!string.IsNullOrEmpty(userAgency) && 
                !userAgency.Contains("Fire", StringComparison.OrdinalIgnoreCase) &&
                !userAgency.Contains("Πυροσβεστική", StringComparison.OrdinalIgnoreCase))
            {
                return GetHelpLocationIcon(statusKey);
            }

            // Fire Department uses category-specific icons
            return mainCategory switch
            {
                // Forest Fires (Greek and English)
                "ΔΑΣΙΚΕΣ ΠΥΡΚΑΓΙΕΣ" or "Forest Fires" => GetWildfireIcon(statusKey),
                
                // Urban Fires (Greek and English)
                "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ" or "Urban Fires" => GetFireLocationIcon(statusKey),
                
                // Assistance and other categories (Greek and English)
                "ΠΑΡΟΧΕΣ ΒΟΗΘΕΙΑΣ" or "Assistance Callouts" or
                "ΕΓΚΛΩΒΙΣΜΟΣ" or "Entrapment (Elevator Call)" or
                "ΔΡΑΣΤΗΡΙΟΤΗΤΑ" or "Activities" => GetHelpLocationIcon(statusKey),
                
                // Default to fire location icons for Fire Department
                _ => GetFireLocationIcon(statusKey)
            };
        }

        /// <summary>
        /// Gets the icon path for Fire Department incidents
        /// </summary>
        /// <param name="mainCategory">The main category of the incident</param>
        /// <param name="status">The current status of the incident</param>
        /// <returns>The URL path to the appropriate Fire Department icon</returns>
        public static string GetFireDepartmentIconPath(string mainCategory, IncidentStatus status)
        {
            return GetIncidentIconPath(mainCategory, status, "Fire Department");
        }

        /// <summary>
        /// Gets the icon path for EKAB incidents (always uses help location icons)
        /// </summary>
        /// <param name="status">The current status of the incident</param>
        /// <returns>The URL path to the appropriate EKAB icon</returns>
        public static string GetEKABIconPath(IncidentStatus status)
        {
            var statusKey = GetStatusKey(status);
            return GetHelpLocationIcon(statusKey);
        }

        /// <summary>
        /// Converts IncidentStatus enum to icon status key
        /// </summary>
        /// <param name="status">The incident status</param>
        /// <returns>The corresponding status key for icon naming</returns>
        private static string GetStatusKey(IncidentStatus status)
        {
            return status switch
            {
                IncidentStatus.OnGoing => "ongoing",
                IncidentStatus.PartialControl => "partial",
                IncidentStatus.Controlled => "controlled",
                IncidentStatus.FullyControlled => "ended",
                IncidentStatus.Closed => "ended",
                IncidentStatus.Created => "ongoing", // Created incidents show as ongoing
                _ => "ongoing"
            };
        }

        /// <summary>
        /// Gets wildfire icon path based on status
        /// </summary>
        /// <param name="statusKey">The status key (ongoing, partial, controlled, ended)</param>
        /// <returns>The URL path to the wildfire icon</returns>
        private static string GetWildfireIcon(string statusKey)
        {
            return statusKey switch
            {
                "ongoing" => $"{BaseIconUrl}/FireDept/wildfire-ongoing.png",
                "partial" => $"{BaseIconUrl}/FireDept/wildfire-partial.png",
                "controlled" => $"{BaseIconUrl}/FireDept/wildfire-controlled.png",
                "ended" => $"{BaseIconUrl}/FireDept/wildfire-ended.png",
                _ => $"{BaseIconUrl}/FireDept/wildfire-ongoing.png"
            };
        }

        /// <summary>
        /// Gets fire location icon path based on status
        /// </summary>
        /// <param name="statusKey">The status key (ongoing, partial, controlled, ended)</param>
        /// <returns>The URL path to the fire location icon</returns>
        private static string GetFireLocationIcon(string statusKey)
        {
            return statusKey switch
            {
                "ongoing" => $"{BaseIconUrl}/FireDept/fire-location-ongoing.png",
                "partial" => $"{BaseIconUrl}/FireDept/fire-location-partial.png",
                "controlled" => $"{BaseIconUrl}/FireDept/fire-location-controlled.png",
                "ended" => $"{BaseIconUrl}/FireDept/fire-location-ended.png",
                _ => $"{BaseIconUrl}/FireDept/fire-location-ongoing.png"
            };
        }

        /// <summary>
        /// Gets help location icon path based on status
        /// </summary>
        /// <param name="statusKey">The status key (ongoing, partial, controlled, ended)</param>
        /// <returns>The URL path to the help location icon</returns>
        private static string GetHelpLocationIcon(string statusKey)
        {
            return statusKey switch
            {
                "ongoing" => $"{BaseIconUrl}/Universal/help-location-ongoing.png",
                "partial" => $"{BaseIconUrl}/Universal/help-location-partial.png",
                "controlled" => $"{BaseIconUrl}/Universal/help-location-controlled.png",
                "ended" => $"{BaseIconUrl}/Universal/help-location-ended.png",
                _ => $"{BaseIconUrl}/Universal/help-location-ongoing.png"
            };
        }

        /// <summary>
        /// Gets the local file path for an incident icon (for WPF Image controls)
        /// </summary>
        /// <param name="mainCategory">The main category of the incident</param>
        /// <param name="status">The current status of the incident</param>
        /// <param name="userAgency">The agency handling the incident (optional)</param>
        /// <returns>The local file path to the appropriate icon</returns>
        public static string GetLocalIconPath(string mainCategory, IncidentStatus status, string userAgency = null)
        {
            var iconUrl = GetIncidentIconPath(mainCategory, status, userAgency);
            // Convert from URL to local path
            return iconUrl.Replace("https://aegis-icons.local/", "pack://application:,,,/Resources/Icons/");
        }
    }
}