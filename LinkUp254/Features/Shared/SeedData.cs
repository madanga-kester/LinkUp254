using LinkUp254.Database;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp254.Features.Shared
{
 
    public static class SeedData
    {
        public static async Task InitializeAsync(LinkUpContext context, IPasswordHasher<User> passwordHasher)
        {
            // INTERESTS 
            if (!await context.Interests.AnyAsync())
            {
                var interests = new[]
                {
                    // Music
                    new Interest("Live Music", "Music", "🎵"),
                    new Interest("DJ Sets", "Music", "🎧"),
                    new Interest("Open Mic Night", "Music", "🎤"),
                    new Interest("Jazz & Blues", "Music", "🎷"),
                    
                    // Food & Drink
                    new Interest("Street Food", "Food & Drink", "🍜"),
                    new Interest("Wine Tasting", "Food & Drink", "🍷"),
                    new Interest("Coffee Culture", "Food & Drink", "☕"),
                    new Interest("Vegan Eats", "Food & Drink", "🥗"),
                    
                    // Tech
                    new Interest("Startup Meetups", "Tech", "🚀"),
                    new Interest("AI & Machine Learning", "Tech", "🤖"),
                    new Interest("Web Development", "Tech", "💻"),
                    new Interest("Cybersecurity", "Tech", "🔐"),
                    
                    // Art & Culture
                    new Interest("Contemporary Art", "Art", "🎨"),
                    new Interest("Photography Walks", "Art", "📷"),
                    new Interest("Street Art", "Art", "🖌️"),
                    new Interest("Pottery Classes", "Art", "🏺"),
                    
                    // Wellness & Outdoors
                    new Interest("Yoga", "Wellness", "🧘"),
                    new Interest("Meditation", "Wellness", "🕉️"),
                    new Interest("Hiking", "Outdoors", "🥾"),
                    new Interest("Cycling", "Outdoors", "🚴"),
                    
                    // Nightlife
                    new Interest("Rooftop Bars", "Nightlife", "🌃"),
                    new Interest("Comedy Clubs", "Nightlife", "😂"),
                    new Interest("Karaoke", "Nightlife", "🎤"),
                    new Interest("Dance Parties", "Nightlife", "💃"),
                };

                await context.Interests.AddRangeAsync(interests);
                await context.SaveChangesAsync();
            }

            // SEED ADMIN USER (if not exists)
            var adminEmail = "admin@linkup254.com";
            if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                // Creating a user first with empty password (required field)
                var admin = new User
                {
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "+254700000000",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsActive = true,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    Password = string.Empty  
                };

                //  hash and  the real password
                admin.Password = passwordHasher.HashPassword(admin, "Admin@123");

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}