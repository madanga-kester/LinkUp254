using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp254.Database;

public static class SeedData
{
    public static async Task InitializeAsync(LinkUpContext context, IPasswordHasher<User> passwordHasher)
    {
        context.Database.EnsureCreated();

        if (context.Users.Any()) return;

        var plainPassword = "Test@123";

        var testUser = new User
        {
            Email = "test@linkup254.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+254700000000",
            Role = "Organizer",
            IsActive = true,
            Password = plainPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        testUser.Password = passwordHasher.HashPassword(testUser, plainPassword);

        context.Users.Add(testUser);
        await context.SaveChangesAsync();
    }
}