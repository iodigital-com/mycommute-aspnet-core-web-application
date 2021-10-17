namespace MyCommute.Tests;

public class DbSeeder
{
    private readonly DataContext context;
    private static readonly Fixture Fixture = new ();

    public DbSeeder(DataContext context)
    {
        this.context = context;
    }

    public async Task SeedAsync()
    {
        var employees = GenerateEmployees();
        
        foreach (var employee in employees)
        {
            employee.Commutes = GenerateCommutes(employee);
        } 
        
        context.AddRange(employees);
        await context.SaveChangesAsync();
    }

    private ICollection<Commute> GenerateCommutes(Employee employee)
    {
        List<Commute> commutes = new();
        
        commutes.AddRange(Fixture.Build<Commute>()
            .With(x => x.Employee, employee)
            .CreateMany(10)
        );

        return commutes;
    }

    private IEnumerable<Employee> GenerateEmployees()
    {
        List<Employee> employees = new ();
        
        employees.AddRange(Fixture.Build<Employee>()
            .With(e => e.Name, Fixture.Create("Name"))
            .With(e => e.Email, $"{Fixture.Create<string>()}@intracto.com")
            .With(e => e.HomeLocation, new Point(4.4051406, 51.2183497))
            .With(e => e.DefaultWorkLocation, new Point(4.393744, 50.7972951))
            .With(e => e.Commutes, new List<Commute>())
            .CreateMany(15));

        return employees;
    } 
}