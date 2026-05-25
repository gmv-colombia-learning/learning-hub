using CustomersApi.CasosDeUso;
using CustomersApi.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger clásico
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(routing => routing.LowercaseUrls = true);

builder.Services.AddDbContext<CustomerDatabaseContext>( mysqlBuilder =>
{
    mysqlBuilder.UseMySQL(builder.Configuration.GetConnectionString("ConnectionMySQL"));
});

builder.Services.AddScoped<IUpdateCustomerUseCase, UpdateCustomerUseCase>();

// Configuración de CORS
builder.Services.AddCors(options => 
{ 
    options.AddPolicy("AllowLocalhost", policy => 
    { 
        policy.WithOrigins("http://127.0.0.1:5500") // aquí pones el origen de tu frontend
              .AllowAnyHeader() 
              .AllowAnyMethod(); 
    }); 
}); 

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customers API V1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");

app.UseAuthorization();

app.MapGet("/customer/{id}", (long id) =>
{
    return "net 10";
});

app.MapControllers();

app.Run();

