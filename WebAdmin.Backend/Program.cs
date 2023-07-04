using Microsoft.EntityFrameworkCore;
using WebAdmin.Backend.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(dbcob => dbcob.UseNpgsql(builder.Configuration.GetValue<string>("Database")));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope()) {
	await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

string[]? allowedCorsOrigins = app.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>();
if (allowedCorsOrigins != null) {
	app.UseCors(cpb => cpb.WithOrigins(allowedCorsOrigins));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
