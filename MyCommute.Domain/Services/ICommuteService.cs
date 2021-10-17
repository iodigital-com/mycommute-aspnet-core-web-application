namespace MyCommute.Domain.Services;

public interface ICommuteService
{
    Task<Commute> GetAsync(Guid id);
    Task<IEnumerable<Commute>> GetByUserIdAsync(Guid id);
    Task<IEnumerable<Commute>> GetAllAsync();
    Task<Commute> AddAsync(Commute commute);
    Task<Commute> UpdateAsync(Commute commute);
    Task<bool> DeleteAsync(Guid id);
    
}