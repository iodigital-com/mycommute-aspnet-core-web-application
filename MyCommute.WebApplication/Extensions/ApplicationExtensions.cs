namespace MyCommute.WebApplication.Extensions;

public static class ApplicationExtensions
{
    public static void UpdateDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var context = serviceScope.ServiceProvider.GetService<DataContext>();

        if (context == null)
        {
            throw new Exception("DataContext is empty");
        }

        context.Database.Migrate();
    }    
}