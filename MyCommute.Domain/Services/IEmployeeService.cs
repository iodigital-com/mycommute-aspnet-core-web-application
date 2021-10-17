using System.Collections;
using System.Collections.Generic;

namespace MyCommute.Domain.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAsync();
    Task<Employee> GetByIdAsync(Guid id);
    Task<Employee> GetByEmailAsync(string email);
    Task<Employee> AddAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(string email);

}