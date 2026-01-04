using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Base service class providing common functionality for all services.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class BaseService<TEntity> : IService<TEntity> where TEntity : BaseEntity
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the BaseService class.
    /// </summary>
    /// <param name="repository">The repository for the entity.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    protected BaseService(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _repository.GetAll().ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var addedEntity = await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return addedEntity;
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity> UpdateAsync(Guid id, TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingEntity = await _repository.GetByIdAsync(id);
        if (existingEntity == null)
            throw new KeyNotFoundException($"Entity with id {id} not found.");

        // Update the entity properties
        entity.Id = id; // Ensure the ID is set
        var updatedEntity = await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return updatedEntity;
    }

    /// <inheritdoc/>
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return deleted;
    }
}