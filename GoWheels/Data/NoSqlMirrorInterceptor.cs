using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using GoWheels.Models;
using GoWheels.Services;

namespace GoWheels.Data;

public class NoSqlMirrorInterceptor : SaveChangesInterceptor
{
    private readonly MongoMirrorService _mongoMirrorService;

    public NoSqlMirrorInterceptor(MongoMirrorService mongoMirrorService)
    {
        _mongoMirrorService = mongoMirrorService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        Console.WriteLine("🔥 Interceptor déclenché !");
        var entries = context.ChangeTracker.Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            try
            {
                if (entry.Entity is Post post)
                {
                    await _mongoMirrorService.MirrorPostAsync(post);
                }
                else if (entry.Entity is Comment comment)
                {
                    await _mongoMirrorService.MirrorCommentAsync(comment);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Mongo Mirror Error] {ex.Message}");
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}