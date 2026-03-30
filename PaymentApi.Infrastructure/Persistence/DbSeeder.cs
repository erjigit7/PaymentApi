using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentApi.Application.Interfaces;
using PaymentApi.Domain.Entities;

namespace PaymentApi.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (await context.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Database seed skipped: the Users table already contains rows.");
            return;
        }

        var now = DateTime.UtcNow;
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Login = "user1",
                PasswordHash = passwordHasher.Hash("12345"),
                Balance = 8m,
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Login = "user2",
                PasswordHash = passwordHasher.Hash("12345"),
                Balance = 8m,
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Login = "user3",
                PasswordHash = passwordHasher.Hash("12345"),
                Balance = 8m,
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Login = "user4",
                PasswordHash = passwordHasher.Hash("12345"),
                Balance = 8m,
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Login = "user5",
                PasswordHash = passwordHasher.Hash("12345"),
                Balance = 8m,
                CreatedAt = now,
            },
        };

        await context.Users.AddRangeAsync(users, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Database seed completed: inserted {Count} test users.", users.Count);
    }
}
