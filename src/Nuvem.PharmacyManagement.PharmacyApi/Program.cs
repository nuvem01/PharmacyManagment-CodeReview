using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nuvem.PharmacyManagement.PharmacyApi.Exceptions;
using Nuvem.PharmacyManagement.PharmacyServices;
using Nuvem.PharmacyManagement.PharmacyServices.DatabaseContext;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options  => 
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));
});

builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<IPharmacyDbContext,PharmacyDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("EFConnectionString"),
        ef => ef.MigrationsAssembly("Nuvem.PharmacyManagement.PharmacyServices")));

builder.Services.AddSwaggerGen(e => e.EnableAnnotations());

AppSettingsConfiguraion appConfig = new();
builder.Configuration.GetSection("ConnectionStrings").Bind(appConfig);
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton<ExceptionHandlingMiddleware>();

var allowUrls = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    if(allowUrls is not null)
                    {
                    policy.WithOrigins(allowUrls)
                        .WithMethods("GET", "POST", "PUT");
                    }
                });
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();


app.UseCors();
app.MapControllers();

app.Run();
