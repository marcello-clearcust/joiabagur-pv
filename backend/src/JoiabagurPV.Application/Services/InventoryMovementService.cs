using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for inventory movement history operations.
/// </summary>
public class InventoryMovementService : IInventoryMovementService
{
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly ILogger<InventoryMovementService> _logger;

    private const int MaxPageSize = 50;
    private const int DefaultDaysBack = 30;

    public InventoryMovementService(
        IInventoryMovementRepository movementRepository,
        ILogger<InventoryMovementService> logger)
    {
        _movementRepository = movementRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedMovementResult> GetMovementHistoryAsync(MovementHistoryFilter filter)
    {
        // Apply defaults for date range
        var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-DefaultDaysBack);
        var endDate = filter.EndDate ?? DateTime.UtcNow;

        // Validate and clamp pagination
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Min(Math.Max(1, filter.PageSize), MaxPageSize);

        var (movements, totalCount) = await _movementRepository.FindByFiltersAsync(
            productId: filter.ProductId,
            pointOfSaleId: filter.PointOfSaleId,
            startDate: startDate,
            endDate: endDate,
            page: page,
            pageSize: pageSize);

        var items = movements.Select(MapToDto).ToList();

        return new PaginatedMovementResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<List<InventoryMovementDto>> GetMovementsByInventoryAsync(Guid inventoryId, int limit = 50)
    {
        limit = Math.Min(Math.Max(1, limit), MaxPageSize);

        var movements = await _movementRepository.FindByInventoryAsync(inventoryId, limit);

        return movements.Select(MapToDto).ToList();
    }

    #region Private Methods

    private static InventoryMovementDto MapToDto(InventoryMovement movement)
    {
        return new InventoryMovementDto
        {
            Id = movement.Id,
            InventoryId = movement.InventoryId,
            ProductId = movement.Inventory?.ProductId ?? Guid.Empty,
            ProductSku = movement.Inventory?.Product?.SKU ?? string.Empty,
            ProductName = movement.Inventory?.Product?.Name ?? string.Empty,
            PointOfSaleId = movement.Inventory?.PointOfSaleId ?? Guid.Empty,
            PointOfSaleName = movement.Inventory?.PointOfSale?.Name ?? string.Empty,
            MovementType = movement.MovementType,
            QuantityChange = movement.QuantityChange,
            QuantityBefore = movement.QuantityBefore,
            QuantityAfter = movement.QuantityAfter,
            Reason = movement.Reason,
            UserId = movement.UserId,
            UserName = movement.User?.FullName ?? string.Empty,
            MovementDate = movement.MovementDate,
            CreatedAt = movement.CreatedAt
        };
    }

    #endregion
}

