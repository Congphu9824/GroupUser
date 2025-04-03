using Data.Context;
using Data.DTO;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroupUser.Services
{
    public interface IGroupService
    {
        Task<List<Group>> GetAll();
        Task<Group> AddGroup(Group group); 
        Task<Group> Update(Group group); 
        Task<Group> GetById(int id);
        Task<bool> DeleteGroup(int id);
        Task<List<object>> GetGroupTree();
        Task<List<UserDto>> GetUsersByGroup(int groupId);

    }

    public class GroupService(ModelDbContext _db) : IGroupService
    {

        public async Task<List<UserDto>> GetUsersByGroup(int groupId)
        {
            var users = await _db.Users
                .Where(u => u.GroupId == groupId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    DateOfBirth = u.DateOfBirth.ToString("dd/MM/yyyy"),
                    Gender = u.Gender ? "Male" : "Female",
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    GroupId = u.GroupId
                })
                .ToListAsync();

            return users;
        }




        public async Task<Group> AddGroup(Group group)
        {
            try
            {
                group.GroupCode = "CD" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                _db.Groups.Add(group);
                await _db.SaveChangesAsync();
                return group;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error adding group: {ex.Message}");
                throw;
            }
        }


        public async Task<List<Group>> GetAll()
        {
            return await _db.Groups
                                .Include(x => x.ChildGroups)
                                .Include(x => x.ParentGroup)
                                .Include(x => x.Users)
                                .ToListAsync();
        }

        public async Task<List<object>> GetGroupTree()
        {
            var groups = await _db.Groups.Include(g => g.ChildGroups).ToListAsync();
            return groups.Select(g => new
            {
                id = g.Id,
                parent = g.ParentGroupId == null ? "#" : g.ParentGroupId.ToString(),
                text = g.GroupName
            }).ToList<object>();
        }



        public async Task<Group> Update(Group group)
        {
            var existingGroup = await _db.Groups.FindAsync(group.Id);
            if (existingGroup == null)
            {
                throw new ArgumentException($"Group with id {group.Id} not found.");
            }

            existingGroup.GroupCode = group.GroupCode;
            existingGroup.GroupName = group.GroupName;
            existingGroup.ParentGroupId = group.ParentGroupId;

            _db.Groups.Update(existingGroup);
            await _db.SaveChangesAsync();
            return existingGroup;
        }

        public async Task<bool> DeleteGroup(int id)
        {
            var Groups = await _db.Groups.Include(x => x.ChildGroups).FirstOrDefaultAsync(x => x.Id == id);
            if (Groups is null)
            {
                return false;
            }

            // delete child
            _db.Groups.RemoveRange(Groups.ChildGroups);

            _db.Groups.Remove(Groups);
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<Group> GetById(int id)
        {
            var GroupId = await _db.Groups.FirstOrDefaultAsync(x => x.Id == id);
            if (GroupId == null) 
            {
                new ArgumentException("Not figure out groupId");
            }
            return GroupId;
        }

   
    }
}
