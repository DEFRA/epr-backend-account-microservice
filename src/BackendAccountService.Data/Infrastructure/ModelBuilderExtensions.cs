using BackendAccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Data.Infrastructure;

public static class ModelBuilderExtensions
{
    public static void HasSoftDeleteFilter<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, ISoftDeletableEntity =>
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => !entity.IsDeleted);
}
