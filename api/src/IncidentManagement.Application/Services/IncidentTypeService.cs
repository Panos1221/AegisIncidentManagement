using System.Text.Json;
using IncidentManagement.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace IncidentManagement.Application.Services;

public class IncidentTypeService : IIncidentTypeService
{
    private readonly ILogger<IncidentTypeService> _logger;
    private readonly string _dataPath;

    public IncidentTypeService(ILogger<IncidentTypeService> logger)
    {
        _logger = logger;
        _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Incidents");
    }

    public async Task<List<IncidentTypesByAgencyDto>> GetAllIncidentTypesAsync()
    {
        var result = new List<IncidentTypesByAgencyDto>();

        try
        {
            var jsonFiles = Directory.GetFiles(_dataPath, "*.json");

            foreach (var filePath in jsonFiles)
            {
                var agencyData = await LoadIncidentTypesFromFileAsync(filePath);
                if (agencyData != null)
                {
                    result.Add(agencyData);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading incident types from JSON files");
            throw;
        }

        return result;
    }

    public async Task<IncidentTypesByAgencyDto?> GetIncidentTypesByAgencyAsync(string agencyName)
    {
        try
        {
            // Map agency names to filenames
            var fileName = GetFileNameForAgency(agencyName);
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("No file found for agency: {AgencyName}", agencyName);
                return null;
            }

            var filePath = Path.Combine(_dataPath, fileName);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Incident types file not found: {FilePath}", filePath);
                return null;
            }

            return await LoadIncidentTypesFromFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading incident types for agency: {AgencyName}", agencyName);
            throw;
        }
    }

    private async Task<IncidentTypesByAgencyDto?> LoadIncidentTypesFromFileAsync(string filePath)
    {
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var jsonDocument = JsonDocument.Parse(jsonContent);

            // Each JSON file contains one agency
            foreach (var agencyProperty in jsonDocument.RootElement.EnumerateObject())
            {
                var agencyName = agencyProperty.Name;
                var categories = new List<IncidentTypeCategoryDto>();

                foreach (var categoryProperty in agencyProperty.Value.EnumerateObject())
                {
                    var categoryKey = categoryProperty.Name;
                    var categoryData = categoryProperty.Value;

                    var category = new IncidentTypeCategoryDto
                    {
                        CategoryKey = categoryKey,
                        CategoryNameEl = categoryData.GetProperty("el").GetString() ?? "",
                        CategoryNameEn = categoryData.GetProperty("en").GetString() ?? "",
                        Subcategories = new List<IncidentTypeSubcategoryDto>()
                    };

                    if (categoryData.TryGetProperty("subcategories", out var subcategoriesElement))
                    {
                        foreach (var subcategory in subcategoriesElement.EnumerateArray())
                        {
                            category.Subcategories.Add(new IncidentTypeSubcategoryDto
                            {
                                SubcategoryNameEl = subcategory.GetProperty("el").GetString() ?? "",
                                SubcategoryNameEn = subcategory.GetProperty("en").GetString() ?? ""
                            });
                        }
                    }

                    categories.Add(category);
                }

                return new IncidentTypesByAgencyDto
                {
                    AgencyName = agencyName,
                    Categories = categories
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing incident types file: {FilePath}", filePath);
            throw;
        }

        return null;
    }

    private static string GetFileNameForAgency(string agencyName)
    {
        return agencyName switch
        {
            "Hellenic Fire Service" => "FireDepartmentIncidents.json",
            "Hellenic Coast Guard" => "CoastGuardIncidents.json",
            "EKAB" => "EKABIncidents.json",
            "Hellenic Police" => "PoliceIncidents.json",
            _ => ""
        };
    }
}