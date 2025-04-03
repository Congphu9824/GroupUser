using Data.Context;
using Data.Entities;
using GroupUser.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupUser.Controllers
{

    public class GroupController(IGroupService _IGroupService, ILogger<GroupController> _logger, ModelDbContext _db) : Controller
    {

        [HttpGet]
        public IActionResult GetUsersByGroup(int id)
        {
            var users = _db.Users
                .Where(u => u.GroupId == id)
                .Select(u => new {
                    id = u.Id,
                    username = u.Username,
                    fullName = u.FullName,
                    dateOfBirth = u.DateOfBirth,
                    gender = u.Gender,
                    phoneNumber = u.PhoneNumber,
                    email = u.Email,
                    groupId = u.GroupId
                })
                .ToList();

            return Json(users);
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _db.Users
                .Include(u => u.Group)
                .Select(u => new {
                    id = u.Id,
                    username = u.Username,
                    fullName = u.FullName,
                    dateOfBirth = u.DateOfBirth,
                    gender = u.Gender,
                    phoneNumber = u.PhoneNumber,
                    email = u.Email,
                    groupId = u.GroupId,
                    groupName = u.Group.GroupName
                })
                .ToList();

            return Json(users);
        }




        public async Task<IActionResult> Index()
        {
            var groups = await _IGroupService.GetAll();

            ViewBag.group = await _IGroupService.GetAll();

            // Build a tree structure from the list of groups
            var groupTree = BuildGroupTree(groups);

            return View(groupTree);
        }

        private List<Group> BuildGroupTree(List<Group> groups)
        {
            var groupLookup = groups.ToLookup(g => g.ParentGroupId);
            return BuildGroupTreeRecursively(null, groupLookup);
        }

        private List<Group> BuildGroupTreeRecursively(int? parentId, ILookup<int?, Group> groupLookup)
        {
            return groupLookup[parentId]
                .Select(g => new Group
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    ParentGroupId = g.ParentGroupId,
                    ChildGroups = BuildGroupTreeRecursively(g.Id, groupLookup)
                }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupTree()
        {
            var groupTree = await _IGroupService.GetGroupTree();
            return Json(groupTree);
        }


        public async Task<IActionResult> CreateGroup()
        {
            return View();
        }


        [HttpPost]  
        public async Task<IActionResult> CreateGroup(Group group)
        {
            var CreateGroup = await _IGroupService.AddGroup(group);
            _logger.LogInformation($"Add new Group:{CreateGroup}");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditGroup(int id)
        {
            var group = await _IGroupService.GetById(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> EditGroup(Group group)
        {
            var updatedGroup = await _IGroupService.Update(group);
            _logger.LogInformation($"Updated group: {updatedGroup.GroupName} (ID: {updatedGroup.Id})");
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Details(int id)
        {
            var group = await _IGroupService.GetById(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var result = await _IGroupService.DeleteGroup(id);
            if (!result)
            {
                return NotFound();
            }

            _logger.LogInformation($"Deleted group ID: {id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
