using CommercialWms.Api.Data;
using CommercialWms.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CommercialWms")
    ?? throw new InvalidOperationException("Connection string 'CommercialWms' is not configured.");

builder.Services.AddDbContext<CommercialWmsDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IReceivingService, ReceivingService>();
builder.Services.AddScoped<IOutboundService, OutboundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
