using Microsoft.EntityFrameworkCore;
using MyCommute.Data.Entities;

namespace MyCommute.Tests
{
    public class TestDataContext : DataContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("InMemory");
        }
    }
}