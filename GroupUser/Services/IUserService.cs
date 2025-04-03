using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroupUser.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAll();
        Task<User> AddUser(User user);
        Task<User> UpdateUser(User user);
        Task<User> GetById(int id);
        Task<bool> DeleteUser(int id);
    }
    public class UserService(ModelDbContext _db) : IUserService
    {
        public async Task<List<User>> GetAll()
        {
            var result = await _db.Users.Include(x => x.Group).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<User> AddUser(User user)
        {
            try
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUser(int id)
        {
            var GroupId = await _db.Users.FindAsync(id);
            if (GroupId == null)
            { 
                return false;
            }

            _db.Users.Remove(GroupId);
            await _db.SaveChangesAsync();
            return true;

        }


        public async Task<User> GetById(int id)
        {
            var UserId = await _db.Users.Include(x => x.Group).FirstOrDefaultAsync(x => x.Id == id);
            if (UserId == null)
            {
                new ArgumentException("Not figure out UserId");
            }
            return UserId;
        }

        public async Task<User> UpdateUser(User user)
        {
            var existingUser = await _db.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new ArgumentException($"user with id {user.Id} not found.");
            }

            existingUser.Username = user.Username;
            existingUser.FullName = user.FullName;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.Gender = user.Gender;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Email = user.Email;
            existingUser.GroupId = user.GroupId;

            _db.Users.Update(existingUser);
            await _db.SaveChangesAsync();
            return existingUser;
        }
    }
}
