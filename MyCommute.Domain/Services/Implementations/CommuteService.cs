using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyCommute.Domain.Exceptions;

namespace MyCommute.Domain.Services.Implementations;

public class CommuteService : ICommuteService
{
    private readonly DataContext dataContext;

    public CommuteService(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }
    
    public async Task<Commute> GetAsync(Guid id)
    {
        var commute = await dataContext.Commutes.FirstOrDefaultAsync(x => x.Id.Equals(id));

        if (commute == null)
        {
            throw new CommuteNotFoundException();
        }
        
        return commute;
    }

    public async Task<IEnumerable<Commute>> GetByUserIdAsync(Guid id)
    {
        var commutes = await dataContext.Commutes.Where(x => x.Employee.Id.Equals(id)).ToListAsync();

        if (commutes.Any())
        {
            return commutes;
        }
        
        throw new CommuteNotFoundException();
    }

    public async Task<IEnumerable<Commute>> GetAllAsync()
    {
        return await dataContext.Commutes.ToListAsync();
    }

    public async Task<Commute> AddAsync(Commute commute)
    {
        dataContext.Commutes.Add(commute);

        await dataContext.SaveChangesAsync();

        return commute;
    }

    public async Task<Commute> UpdateAsync(Commute commute)
    {
        var dbCommute = await GetAsync(commute.Id);
        
        // Ensure the EmployeeId is not overwritten
        commute.EmployeeId = dbCommute.EmployeeId;
        
        // Ensure the CreatedAt timestamp is not overwritten
        commute.CreatedAt = dbCommute.CreatedAt;
        
        dataContext.Entry(dbCommute).CurrentValues.SetValues(commute);

        await dataContext.SaveChangesAsync();

        return dbCommute;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var dbCommute = await GetAsync(id);

        dataContext.Commutes.Remove(dbCommute);
        
        return await dataContext.SaveChangesAsync() != 0;
    }
}