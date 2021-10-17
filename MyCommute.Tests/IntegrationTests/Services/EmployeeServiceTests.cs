using System.Collections;

namespace MyCommute.Tests.IntegrationTests.Services;

[TestFixture]
    public class EmployeeServiceTests
    {
        private readonly IEmployeeService employeeService;
        private readonly TestDataContext dataContext;

        public EmployeeServiceTests()
        {
            dataContext = new TestDataContext();
            employeeService = new EmployeeService(dataContext);
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
            IEnumerable<Employee> entities = await employeeService.GetAsync();
        
            Assert.IsNotEmpty(entities);
        
            Assert.DoesNotThrowAsync(async() => await employeeService.GetByIdAsync(entities.First().Id));
            Assert.DoesNotThrowAsync(async() => await employeeService.GetByEmailAsync(entities.First().Email));

            Assert.ThrowsAsync<EmployeeNotFoundException>(async () => await employeeService.GetByEmailAsync(string.Empty));
            Assert.ThrowsAsync<EmployeeNotFoundException>(async () => await employeeService.GetByIdAsync(Guid.NewGuid()));
        }
    
        [Test]
        public async Task AddTest()
        {
            var entity = new Employee()
            {
                Name = "Joe Smith",
                Email = "joe.smith@intracto.com",
                HomeLocation = new Point(4.393744, 50.7972951),
                DefaultWorkLocation = new Point(4.4075614900783595, 51.20901005),
                DefaultCommuteType = CommuteType.Car
            };

            Assert.AreEqual(Guid.Empty, entity.Id);
        
            Employee addedEntity = await employeeService.AddAsync(entity);
        
            Assert.AreNotEqual(Guid.Empty, addedEntity.Id);
            Assert.DoesNotThrowAsync(async () => await employeeService.GetByEmailAsync(entity.Email));
            Assert.DoesNotThrowAsync(async () => await employeeService.GetByIdAsync(entity.Id));
        }
    
        [Test]
        public async Task UpdateTest()
        {
            IEnumerable<Employee> entities = await employeeService.GetAsync();
        
            Assert.IsNotEmpty(entities);

            Employee entityToUpdate = entities.First(x => x.DefaultCommuteType != CommuteType.Train);

            var updatedAtBeforeUpdate = entityToUpdate.UpdatedAt;
            var commuteTypeBeforeUpdate = entityToUpdate.DefaultCommuteType;
        
            entityToUpdate.DefaultCommuteType = CommuteType.Train;

            Employee updatedEntity = await employeeService.UpdateAsync(entityToUpdate);

            entities = await employeeService.GetAsync();
        
            Assert.AreNotEqual(updatedAtBeforeUpdate, entities.First(x => x.Id.Equals(entityToUpdate.Id)).UpdatedAt);
            Assert.AreNotEqual(CommuteType.Train, commuteTypeBeforeUpdate);
            Assert.AreEqual(CommuteType.Train, entities.First(x => x.Id.Equals(entityToUpdate.Id)).DefaultCommuteType);
        }

        [Test]
        public async Task DeleteTest()
        {
            IEnumerable<Employee> entities = await employeeService.GetAsync();
        
            Assert.IsNotEmpty(entities);
        
            Employee entityToDelete = entities.Last();

            bool result = await employeeService.DeleteAsync(entityToDelete.Id);
        
            Assert.IsTrue(result);

            entities = await employeeService.GetAsync();
        
            Assert.IsFalse(entities.Any(x => x.Equals(entityToDelete)));
        }
    }