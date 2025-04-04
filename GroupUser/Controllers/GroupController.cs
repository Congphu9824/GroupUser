using Data.Context;
using Data.Entities;
using GroupUser.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupUser.Controllers
{

    public class GroupController(IGroupService _IGroupService, ILogger<GroupController> _logger, ModelDbContext _db) : Controller
    {

        public async Task<IActionResult> Index()
        {
            var groups = await _IGroupService.GetAll();

            ViewBag.group = await _IGroupService.GetAll();

            var groupTree = BuildGroupTree(groups);

            return View(groupTree);
        }


        [HttpPost]  
        public async Task<IActionResult> CreateGroup(Group group)
        {
            var CreateGroup = await _IGroupService.AddGroup(group);
            TempData["SuccessMessage"] = "Thêm Group thành công";


            _logger.LogInformation($"Add new Group:{CreateGroup}");
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> EditGroup(Group group)
        {
            var updatedGroup = await _IGroupService.Update(group);
            TempData["SuccessMessage"] = "Cập nhật Group thành công";

            _logger.LogInformation($"Updated group: {updatedGroup.GroupName} (ID: {updatedGroup.Id})");
            return RedirectToAction("Index");

        }



        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var result = await _IGroupService.DeleteGroup(id);
            if (!result)
            {
                return NotFound();
            }
            TempData["SuccessMessage"] = "Xóa Group thành công";
            _logger.LogInformation($"Deleted group ID: {id}");
            return RedirectToAction(nameof(Index));
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
    }
}
