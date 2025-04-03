using Data.Context;
using Data.Entities;
using GroupUser.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupUser.Controllers
{
    public class UserController(IUserService _IUserService, IGroupService _IGroupService, ILogger<GroupController> _logger, ModelDbContext _db) : Controller
    {
        [HttpGet]
        public IActionResult GetUsersByParentGroup(int id)
        {
            // Lấy tất cả các nhóm con của nhóm hiện tại
            var childGroupIds = _db.Groups
                .Where(g => g.ParentGroupId == id)
                .Select(g => g.Id)
                .ToList();

            // Thêm cả nhóm hiện tại vào danh sách
            childGroupIds.Add(id);

            var users = _db.Users
                .Where(u => childGroupIds.Contains(u.GroupId.Value))
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
            try
            {
                var users = _db.Users
                    .Include(u => u.Group)
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.Username,
                        fullName = u.FullName,
                        dateOfBirth = u.DateOfBirth,
                        gender = u.Gender,
                        phoneNumber = u.PhoneNumber,
                        email = u.Email,
                        groupId = u.Group != null ? u.Group.GroupName : "Không có nhóm"
                    })
                    .ToList();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
                return StatusCode(500, "Đã xảy ra lỗi khi lấy danh sách người dùng");
            }
        }


        public async Task<IActionResult> Index()
        {
            var GetGroup = await _IUserService.GetAll();
            _logger.LogInformation($"get User:{GetGroup}");
            return View(GetGroup);
        }


        public async Task<IActionResult> CreateGroup()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                var createdUser = await _IUserService.AddUser(user);
                _logger.LogInformation($"Added new User: {createdUser}");

                // Return the created user data as JSON for immediate updating on the front-end
                return Json(new
                {
                    success = true,
                    message = "User created successfully",
                    user = new
                    {
                        id = createdUser.Id,
                        username = createdUser.Username,
                        fullName = createdUser.FullName,
                        dateOfBirth = createdUser.DateOfBirth,
                        gender = createdUser.Gender,
                        phoneNumber = createdUser.PhoneNumber,
                        email = createdUser.Email,
                        groupName = createdUser.Group?.GroupName ?? " ",
                        groupId = user.GroupId,

                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating user");
                return Json(new { success = false, message = "Error occurred while creating user" });
            }
        }


        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _IUserService.GetById(id);
            if (user == null)
            {
                Console.WriteLine("User not found with ID: " + id);
                return NotFound();
            }

            Console.WriteLine($"User data: {user.Id}, {user.Username}, {user.FullName}, {user.DateOfBirth}, {user.Gender}, {user.PhoneNumber}, {user.Email}, {user.GroupId}");

            var groups = await _db.Groups.ToListAsync();
            ViewBag.group = groups;
            return View(user);
        }



        [HttpPost]
        public async Task<IActionResult> EditUser([FromForm] User user)
        {
            try
            {
                var updatedUser = await _IUserService.UpdateUser(user);
                return Json(new
                {
                    success = true,
                    message = "Cập nhật người dùng thành công",
                    user = new
                    {
                        id = updatedUser.Id,
                        username = updatedUser.Username,
                        fullName = updatedUser.FullName,
                        dateOfBirth = updatedUser.DateOfBirth.ToString("yyyy-MM-dd"),
                        gender = updatedUser.Gender,
                        phoneNumber = updatedUser.PhoneNumber,
                        email = updatedUser.Email,
                        groupId = updatedUser.GroupId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi cập nhật người dùng"
                });
            }
        }


        public async Task<User> GetById(int id)
        {
            return await _db.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == id);
        }


        public async Task<IActionResult> Details(int id)
        {
            var UserId = await _IUserService.GetById(id);
            if (UserId == null)
            {
                return NotFound();
            }

            ViewBag.group = await _db.Groups
                .Include(x => x.ChildGroups)
                .Include(x => x.ParentGroup)
                .ToListAsync();
            return View(UserId);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _IUserService.DeleteUser(id);
                if (!result)
                {
                    return Json(new { success = false, message = "User deletion failed" });
                }

                _logger.LogInformation($"Deleted user ID: {id}");
                return Json(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting user");
                return Json(new { success = false, message = "Error occurred while deleting user" });
            }
        }

    }
}
