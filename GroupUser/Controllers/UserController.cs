using Data.Context;
using Data.Entities;
using GroupUser.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupUser.Controllers
{
    public class UserController(IUserService _IUserService, IGroupService _IGroupService, ILogger<GroupController> _logger, ModelDbContext _db) : Controller
    {



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

        public async Task<IActionResult> GetById(int id)
        {
            var user = await _IUserService.GetById(id);

            if (user == null)
            {
                return NotFound(); 
            }
            return View(user);
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


        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                var createdUser = await _IUserService.AddUser(user);
                _logger.LogInformation($"Added new User: {createdUser}");

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
