using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AegisDispatcher.Models;

namespace AegisDispatcher.Services
{
    public class LocalIncidentDataService
    {
        private readonly string _dataDirectory;
        private readonly ILoggingService _loggingService;

        public LocalIncidentDataService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Incidents");
            
            _loggingService.LogDebug("LocalIncidentDataService initialized with data directory: {DataDirectory}", _dataDirectory);
        }

        public async Task<IncidentTypesByAgency?> GetIncidentTypesByAgencyAsync(string agencyName)
        {
            _loggingService.LogInformation("LocalIncidentDataService: GetIncidentTypesByAgencyAsync started for agency: {AgencyName}", agencyName);
            
            try
            {
                var fileName = GetFileNameForAgency(agencyName);
                var filePath = Path.Combine(_dataDirectory, fileName);

                _loggingService.LogDebug("LocalIncidentDataService: Data directory: {DataDirectory}", _dataDirectory);
                _loggingService.LogDebug("LocalIncidentDataService: File name: {FileName}", fileName);
                _loggingService.LogDebug("LocalIncidentDataService: Full file path: {FilePath}", filePath);
                _loggingService.LogDebug("LocalIncidentDataService: File exists: {FileExists}", File.Exists(filePath));

                if (!File.Exists(filePath))
                {
                    _loggingService.LogError("LocalIncidentDataService: File not found: {FilePath}", filePath);
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                _loggingService.LogDebug("LocalIncidentDataService: JSON content length: {Length}", jsonContent.Length);
                _loggingService.LogDebug("LocalIncidentDataService: JSON content preview: {Preview}", 
                    jsonContent.Substring(0, Math.Min(200, jsonContent.Length)));
                
                var jsonDocument = JsonDocument.Parse(jsonContent);
                _loggingService.LogDebug("LocalIncidentDataService: JSON parsed successfully");

                // Find the agency data in the JSON
                _loggingService.LogDebug("LocalIncidentDataService: Looking for property: {AgencyName}", agencyName);
                
                // Normalize agency name by removing spaces for JSON property lookup
                var normalizedAgencyName = agencyName.Replace(" ", "");
                _loggingService.LogDebug("LocalIncidentDataService: Normalized agency name: {NormalizedAgencyName}", normalizedAgencyName);
                
                if (!jsonDocument.RootElement.TryGetProperty(normalizedAgencyName, out var agencyElement))
                {
                    var availableProperties = string.Join(", ", jsonDocument.RootElement.EnumerateObject().Select(p => p.Name));
                    _loggingService.LogWarning("LocalIncidentDataService: Agency property not found: {AgencyName}. Available properties: {Properties}", 
                        agencyName, availableProperties);
                    return null;
                }

                var categories = new List<IncidentTypeCategory>();

                foreach (var categoryProperty in agencyElement.EnumerateObject())
                {
                    var categoryKey = categoryProperty.Name;
                    var categoryValue = categoryProperty.Value;

                    var category = new IncidentTypeCategory
                    {
                        CategoryKey = categoryKey,
                        CategoryNameEl = categoryValue.TryGetProperty("el", out var elProp) ? elProp.GetString() ?? categoryKey : categoryKey,
                        CategoryNameEn = categoryValue.TryGetProperty("en", out var enProp) ? enProp.GetString() ?? categoryKey : categoryKey,
                        Subcategories = new List<IncidentTypeSubcategory>()
                    };

                    if (categoryValue.TryGetProperty("subcategories", out var subcategoriesElement) && 
                        subcategoriesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var subcategoryElement in subcategoriesElement.EnumerateArray())
                        {
                            var subcategory = new IncidentTypeSubcategory
                            {
                                SubcategoryNameEl = subcategoryElement.TryGetProperty("el", out var subElProp) ? subElProp.GetString() ?? "" : "",
                                SubcategoryNameEn = subcategoryElement.TryGetProperty("en", out var subEnProp) ? subEnProp.GetString() ?? "" : ""
                            };
                            category.Subcategories.Add(subcategory);
                        }
                    }

                    categories.Add(category);
                }

                _loggingService.LogInformation("LocalIncidentDataService: Successfully parsed {Count} categories for agency: {AgencyName}", 
                    categories.Count, agencyName);
                
                return new IncidentTypesByAgency
                {
                    AgencyName = agencyName,
                    Categories = categories
                };
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "LocalIncidentDataService: Error in GetIncidentTypesByAgencyAsync for agency: {AgencyName}", agencyName);
                return null;
            }
        }

        private string GetFileNameForAgency(string agencyName)
        {
            return agencyName switch
            {
                "Hellenic Fire Service" => "FireDepartmentIncidents.json",
                "Hellenic Coast Guard" => "CoastGuardIncidents.json",
                "EKAB" => "EKABIncidents.json",
                "Hellenic Police" => "PoliceIncidents.json",
                _ => "FireDepartmentIncidents.json" // Default fallback
            };
        }

        public bool IsDataAvailable(string agencyName)
        {
            var fileName = GetFileNameForAgency(agencyName);
            var filePath = Path.Combine(_dataDirectory, fileName);
            return File.Exists(filePath);
        }


    }
}