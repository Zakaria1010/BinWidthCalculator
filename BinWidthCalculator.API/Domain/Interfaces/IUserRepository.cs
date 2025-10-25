using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(Guid userId);
    Task<User> AddAsync(User user);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}