namespace MyCommute.Tests.IntegrationTests.Services;

[TestFixture]
public class CommuteServiceTests
{
    private readonly ICommuteService commuteService;
    private readonly TestDataContext dataContext;
    
    public CommuteServiceTests()
    {
        dataContext = new TestDataContext();
        commuteService = new CommuteService(dataContext);
    }
    
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Seed database
        var seeder = new DbSeeder(dataContext);
        await seeder.SeedAsync();
    }

    [Test]
    public async Task GetTest()
    {
        var commutes = await commuteService.GetAllAsync();
    
        Assert.IsNotEmpty(commutes);
    
        Assert.DoesNotThrowAsync(async () => await commuteService.GetAsync(commutes.First().Id));
    
        var employeeCommutes = await commuteService.GetByUserIdAsync(commutes.First().Employee.Id);
    
        Assert.IsNotEmpty(employeeCommutes);
    
        Assert.ThrowsAsync<CommuteNotFoundException>(async () => await commuteService.GetAsync(Guid.NewGuid()));
        Assert.ThrowsAsync<CommuteNotFoundException>(async () => await commuteService.GetByUserIdAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task AddTest()
    {
        IEnumerable<Commute> entities = await commuteService.GetAllAsync();

        var entity = new Commute()
        {
            Employee = entities.Last().Employee,
            Type = CommuteType.Bike
        };

        Assert.AreEqual(Guid.Empty, entity.Id);
    
        Commute addedCommute = await commuteService.AddAsync(entity);
    
        Assert.AreNotEqual(Guid.Empty, addedCommute.Id);
        Assert.DoesNotThrowAsync(async () => await commuteService.GetAsync(entity.Id));
    }

    [Test]
    public async Task UpdateTest()
    {
        IEnumerable<Commute> entities = await commuteService.GetAllAsync();
    
        Assert.IsNotEmpty(entities);

        Commute entityToUpdate = entities.First(x => x.Type != CommuteType.Car);

        var updatedAtBeforeUpdate = entityToUpdate.UpdatedAt;
        var commuteTypeBeforeUpdate = entityToUpdate.Type;

        entityToUpdate.Date = DateTime.Now;
        entityToUpdate.Type = CommuteType.Car;

        Commute updatedEntity = await commuteService.UpdateAsync(entityToUpdate);
    
        Assert.AreNotEqual(updatedAtBeforeUpdate, entities.First(x => x.Id.Equals(entityToUpdate.Id)).UpdatedAt);
        Assert.AreNotEqual(CommuteType.Car, commuteTypeBeforeUpdate);
        Assert.AreEqual(CommuteType.Car, entities.First(x => x.Id.Equals(entityToUpdate.Id)).Type);
    }

    [Test]
    public async Task DeleteTest()
    {
        IEnumerable<Commute> entities = await commuteService.GetAllAsync();

        Commute entityToDelete = entities.First();

        bool result = await commuteService.DeleteAsync(entityToDelete.Id);
    
        Assert.IsTrue(result);
    
        entities = await commuteService.GetAllAsync();
    
        Assert.IsFalse(entities.Any(x => x.Equals(entityToDelete)));
    }
}