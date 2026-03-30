using LinkUp254.Database;
using LinkUp254.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//  services container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register LinkUpContext with SQL Server
builder.Services.AddDbContext<LinkUpContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Identity Password Hashing
builder.Services.AddScoped<IPasswordHasher<LinkUp254.Features.Shared.User>, PasswordHasher<LinkUp254.Features.Shared.User>>();


// Auth Services - Custom 
builder.Services.AddScoped<AuthServices>();


 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();









