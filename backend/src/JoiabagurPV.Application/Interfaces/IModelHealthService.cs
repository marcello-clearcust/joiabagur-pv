using JoiabagurPV.Application.DTOs.ImageRecognition;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for calculating model health metrics and alert levels.
/// </summary>
public interface IModelHealthService
{
    /// <summary>
    /// Calculates comprehensive model health metrics and determines alert level.
    /// </summary>
    /// <returns>Model health DTO with all metrics and alert level.</returns>
    Task<ModelHealthDto> GetModelHealthAsync();

    /// <summary>
    /// Determines if the model needs retraining based on alert level.
    /// </summary>
    /// <returns>True if retraining recommended (HIGH or CRITICAL), false otherwise.</returns>
    Task<bool> ShouldRetrainAsync();
}
