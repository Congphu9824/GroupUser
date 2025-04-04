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
        bool IsGroupNameDuplicate(string groupName);


    }

    public class GroupService(ModelDbContext _db) : IGroupService
    {
        public bool IsGroupNameDuplicate(string groupName)
        {
            return _db.Groups.Any(g => g.GroupName == groupName);
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
                                .AsNoTracking()
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
            var group = await _db.Groups
                .Include(x => x.ChildGroups)  // Include child groups
                .Include(x => x.Users)       
                .FirstOrDefaultAsync(x => x.Id == id);

            if (group == null)
            {
                return false;
            }

            _db.Groups.RemoveRange(group.ChildGroups);

            _db.Users.RemoveRange(group.Users);

            _db.Groups.Remove(group);

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
