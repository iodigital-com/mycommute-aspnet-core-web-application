using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlServer"),
        x =>
        {
            x.UseNetTopologySuite();
        })
);
builder.Services.AddTransient<IEmployeeService, EmployeeService>();
builder.Services.AddTransient<ICommuteService, CommuteService>();
builder.Services.AddTransient<IGeoCodeService, GeoCodeService>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyCommute.WebApplication", Version = "v1" });
});

var app = builder.Build();

app.UpdateDatabase();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyCommute.WebApplication v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();