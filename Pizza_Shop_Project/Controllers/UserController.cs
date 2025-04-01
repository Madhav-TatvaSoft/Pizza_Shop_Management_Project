using Microsoft.AspNetCore.Mvc;
using BLL.Interface;
using Microsoft.AspNetCore.Authorization;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Mail;
using System.Net;
using Pizza_Shop_Project.Authorization;


namespace Pizza_Shop_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserLoginService _userLoginService;
        private readonly IJWTService _JWTService;

        #region User Constructor
        public UserController(IUserService userService, IJWTService JWTService, IUserLoginService userLoginService)
        {
            this._userService = userService;
            this._JWTService = JWTService;
            this._userLoginService = userLoginService;
        }
        #endregion

        #region Dashboard
        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            ViewData["sidebar-active"] = "Dashboard";
            return View();
        }
        #endregion

        #region State,City
        public JsonResult GetStates(long? countryId)
        {
            List<State>? states = _userService.GetState(countryId);
            return Json(new SelectList(states, "StateId", "StateName"));
        }

        public JsonResult GetCities(long? stateId)
        {
            List<City>? cities = _userService.GetCity(stateId);
            return Json(new SelectList(cities, "CityId", "CityName"));
        }
        #endregion

        #region UserProfile

        [Authorize(Roles = "Admin")]
        public IActionResult UserProfile()
        {
            string? cookieSavedToken = Request.Cookies["AuthToken"];
            List<AddUserViewModel>? data = _userService.GetUserProfileDetails(cookieSavedToken);
            List<Country>? Countries = _userService.GetCountry();
            List<State>? States = _userService.GetState(data[0].CountryId);
            List<City>? Cities = _userService.GetCity(data[0].StateId);
            ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
            ViewBag.States = new SelectList(States, "StateId", "StateName");
            ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
            return View(data[0]);
        }

        [HttpPost]
        public IActionResult UserProfile(AddUserViewModel user)
        {
            string? token = Request.Cookies["AuthToken"];
            string? userEmail = _JWTService.GetClaimValue(token, "email");

            if (user.CountryId == null)
            {
                TempData["CountryError"] = "Please select a country";
            }
            if (user.StateId == null)
            {
                TempData["StateError"] = "Please select a state";
            }
            if (user.CityId == null)
            {
                TempData["CityError"] = "Please select a city";
            }


            if (user.ProfileImage != null)
            {
                string[]? extension = user.ProfileImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{Guid.NewGuid()}_{user.ProfileImage.FileName}";
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        user.ProfileImage.CopyTo(stream);
                    }
                    user.Image = $"/uploads/{fileName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "The Image format is not supported.";
                    return RedirectToAction("AddUser", "User", new { Email = user.Email });
                }
            }

            _userService.UpdateUser(user, userEmail);

            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(60);
            if (user.Image != null)
            {
                Response.Cookies.Append("profileImage", user.Image, options);
            }
            Response.Cookies.Append("username", user.Username, options);

            TempData["SuccessMessage"] = "Profile Updated successfully";
            return RedirectToAction("UserListData", "User");
        }
        #endregion

        #region ChangePassword

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel changepassword)

        {
            string? token = Request.Cookies["AuthToken"];
            string? userEmail = _JWTService.GetClaimValue(token, "email");


            if (changepassword.CurrentPassword == changepassword.NewPassword)
            {
                ViewBag.Message = "Current Password and New Password cannot be same";
                return View();
            }
            else if (changepassword.NewPassword != changepassword.NewConfirmPassword)
            {
                ViewBag.Message = "New Password and Confirm Password should be same";
                return View();
            }
            else
            {
                changepassword.CurrentPassword = _userLoginService.EncryptPassword(changepassword.CurrentPassword);
                changepassword.NewPassword = _userLoginService.EncryptPassword(changepassword.NewPassword);
                bool password_verify = _userService.UserChangePassword(changepassword, userEmail);
                if (password_verify)
                {
                    TempData["SuccessMessage"] = "Password Changed Successfully";
                    return RedirectToAction("UserProfile", "User");
                }
                else
                {
                    TempData["ErrorMessage"] = "Current Password is incorrect";
                    return View();
                }
            }
        }
        #endregion

        #region Logout
        public IActionResult UserLogout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("email");
            TempData["SuccessMessage"] = "Logged out successfully";
            return RedirectToAction("VerifyUserLogin", "UserLogin");
        }
        #endregion

        #region UserListData
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Users.View")]
        public IActionResult UserListData()
        {
            ViewData["sidebar-active"] = "User";
            PaginationViewModel<User>? users = _userService.GetUserList();
            return View(users);
        }

        [PermissionAuthorize("Users.View")]
        public IActionResult PaginatedData(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5)
        {
            ViewBag.emailid = Request.Cookies["email"];
            PaginationViewModel<User>? users = _userService.GetUserList(search, sortColumn, sortDirection, pageNumber, pageSize);
            return PartialView("_UserListDataPartial", users);
        }
        #endregion

        #region User CRUD

        #region AddUser
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Users.AddEdit")]
        public IActionResult AddUser()
        {
            List<Role>? Roles = _userService.GetRole();
            List<Country>? Countries = _userService.GetCountry();
            List<State>? States = _userService.GetState(-1);
            List<City>? Cities = _userService.GetCity(-1);
            ViewBag.Roles = new SelectList(Roles, "RoleId", "RoleName");
            ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
            ViewBag.States = new SelectList(States, "StateId", "StateName");
            ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
            ViewData["sidebar-active"] = "User";

            return View();
        }

        [PermissionAuthorize("Users.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserViewModel user)
        {
            if (user.CountryId == null)
            {
                TempData["CountryError"] = "Please select a country";
            }
            if (user.StateId == null)
            {
                TempData["StateError"] = "Please select a state";
            }
            if (user.CityId == null)
            {
                TempData["CityError"] = "Please select a city";
            }

            string? token = Request.Cookies["AuthToken"];
            string? Email = _JWTService.GetClaimValue(token, "email");

            if (user.ProfileImage != null)
            {
                string[]? extension = user.ProfileImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{Guid.NewGuid()}_{user.ProfileImage.FileName}";
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (FileStream stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        user.ProfileImage.CopyTo(stream);
                    }
                    user.Image = $"/uploads/{fileName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "The Image format is not supported.";
                    return RedirectToAction("AddUser", "User", new { Email = user.Email });
                }
            }

            if (await _userService.IsUserNameExists(user.Username))
            {
                TempData["addUserErrorMessage"] = "Username already exists";
                return RedirectToAction("AddUser", "User");
            }
            if (!await _userService.AddUser(user, Email))
            {
                //change
                TempData["ErrorMessage"] = "Account with this email already exists";
                return View();
            }

            MailAddress senderEmail = new MailAddress("tatva.pca42@outlook.com", "tatva.pca42@outlook.com");
            MailAddress receiverEmail = new MailAddress(user.Email, user.Email);
            string? password = "P}N^{z-]7Ilp";
            string? sub = "Add user";
            string? body = $@"<div style='max-width: 500px; font-family: Arial, sans-serif; border: 1px solid #ddd;'>
                <div style='background: #006CAC; padding: 10px; text-align: center; height:90px; max-width:100%; display: flex; justify-content: center; align-items: center;'>
                    <img class='mt-2' src='https://images.vexels.com/media/users/3/128437/isolated/preview/2dd809b7c15968cb7cc577b2cb49c84f-pizza-food-restaurant-logo.png' style='max-width: 50px;' />
                    <span style='color: #fff; font-size: 24px; margin-left: 10px; font-weight: 600;'>PIZZASHOP</span>
                </div>
                <div style='padding: 20px 5px; background-color: #e8e8e8;'>
                    <p>Welcome to Pizza shop,</p>
                    <p>Please Find the details below to login to your account:</p><br>
                    <h3>Login details</h3>
                    <p>Email: {user.Email}</p>
                    <p>Password: {user.Password}</p><br>
                    <p>If you encounter any issues or have any questions, please do not hesitate to contact our support team.</p>
                    
                </div>
                </div>";
            SmtpClient smtp = new SmtpClient
            {
                Host = "mail.etatvasoft.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (MailMessage mess = new MailMessage(senderEmail, receiverEmail))
            {
                mess.Subject = sub;
                mess.Body = body;
                mess.IsBodyHtml = true;
                await smtp.SendMailAsync(mess);
            }
            TempData["SuccessMessage"] = "User added successfully.";
            return RedirectToAction("UserListData", "User");
            // return View();
        }
        #endregion

        #region EditUser
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Users.AddEdit")]
        public IActionResult EditUser(string Email)
        {
             List<AddUserViewModel>? user = _userService.GetUserByEmail(Email);
            List<Role>? Roles = _userService.GetRole();
            List<Country>? Countries = _userService.GetCountry();
            List<State>? States = _userService.GetState(user[0].CountryId);
            List<City>? Cities = _userService.GetCity(user[0].StateId);
            ViewBag.Roles = new SelectList(Roles, "RoleId", "RoleName");
            ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
            ViewBag.States = new SelectList(States, "StateId", "StateName");
            ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
            ViewData["sidebar-active"] = "User";

            return View(user[0]);
        }

        [PermissionAuthorize("Users.AddEdit")]
        [HttpPost]

        public async Task<IActionResult> EditUser(AddUserViewModel adduser)
        {
            string? Email = adduser.Email;

            if (adduser.CountryId == null)
            {
                TempData["CountryError"] = "Please select a country";
            }
            if (adduser.StateId == null)
            {
                TempData["StateError"] = "Please select a state";
            }
            if (adduser.CityId == null)
            {
                TempData["CityError"] = "Please select a city";
            }

            if (adduser.ProfileImage != null)
            {
                string[]? extension = adduser.ProfileImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{Guid.NewGuid()}_{adduser.ProfileImage.FileName}";
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (FileStream stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        adduser.ProfileImage.CopyTo(stream);
                    }
                    adduser.Image = $"/uploads/{fileName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "The Image format is not supported.";
                    return RedirectToAction("EditUser", "User", new { Email = adduser.Email });
                }
            }

            if (_userService.IsUserNameExistsForEdit(adduser.Username, Email))
            {
                TempData["ErrorMessage"] = "UserName Already Exists. Try Another Username";
                return RedirectToAction("EditUser", "User", new { Email = adduser.Email });
            }
            await _userService.EditUser(adduser, Email);

            TempData["SuccessMessage"] = "User Updated successfully";
            return RedirectToAction("UserListData", "User");

        }
        #endregion

        #region DeleteUser
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Users.Delete")]
        public async Task<IActionResult> DeleteUser(string Email)
        {
            bool isDeleted = await _userService.DeleteUser(Email);

            if (!isDeleted)
            {
                ViewBag.Message = "User cannot be deleted";
                return RedirectToAction("UserListData", "User");
            }
            TempData["SuccessMessage"] = "User deleted successfully";
            return RedirectToAction("UserListData", "User");
        }
        #endregion

        #endregion
    
    }
}