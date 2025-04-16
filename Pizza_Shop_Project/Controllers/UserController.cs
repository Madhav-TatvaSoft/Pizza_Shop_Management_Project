using Microsoft.AspNetCore.Mvc;
using BLL.Interface;
using Microsoft.AspNetCore.Authorization;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pizza_Shop_Project.Authorization;
using BLL.common;

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
        [PermissionAuthorize("AdminAccountManager")]
        public IActionResult Dashboard()
        {
            if (User.IsInRole("Chef"))
            {
                return RedirectToAction("OrderAppKOT", "OrderAppKOT");
            }
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
            ViewBag.Role = data[0].RoleId;
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
                    TempData["ErrorMessage"] = NotificationMessage.ImageFormat;
                    return RedirectToAction("AddUser", "User", new { Email = user.Email });
                }
            }


            _userService.UpdateUserProfile(user, userEmail);

            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(60);
            if (user.Image != null)
            {
                Response.Cookies.Append("profileImage", user.Image, options);
            }
            Response.Cookies.Append("username", user.Username, options);

            string roleName = _JWTService.GetClaimValue(token, "role");
            TempData["SuccessMessage"] = NotificationMessage.ProfileUpdated;
            if (roleName == "Chef")
            {
                return RedirectToAction("OrderAppKOT", "OrderAppKOT");
            }
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
                    string roleName = _JWTService.GetClaimValue(token, "role");
                    TempData["SuccessMessage"] = NotificationMessage.PasswordChanged;
                    if (roleName == "Chef")
                    {
                        return RedirectToAction("OrderAppKOT", "OrderAppKOT");
                    }
                    return RedirectToAction("UserListData", "User");
                }
                else
                {
                    TempData["ErrorMessage"] = NotificationMessage.PasswordChangeFailed;
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
            Response.Cookies.Delete("profileImage");
            Response.Cookies.Delete("username");
            Response.Headers["Clear-Site-Data"] = "\"cache\", \"cookies\", \"storage\"";
            TempData["SuccessMessage"] = NotificationMessage.LogoutSuccess;
            return RedirectToAction("VerifyUserLogin", "UserLogin");
        }
        #endregion

        #region UserListData
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
            string? token = Request.Cookies["AuthToken"];
            ViewBag.roleName = _JWTService.GetClaimValue(token, "role");
            ViewBag.emailid = Request.Cookies["email"];
            PaginationViewModel<User>? users = _userService.GetUserList(search, sortColumn, sortDirection, pageNumber, pageSize);
            return PartialView("_UserListDataPartial", users);
        }
        #endregion

        #region User CRUD

        #region AddUser
        [PermissionAuthorize("Users.AddEdit")]
        public IActionResult AddUser()
        {
            string? token = Request.Cookies["AuthToken"];
            string roleName = _JWTService.GetClaimValue(token, "role");

            List<Role>? Roles = _userService.GetRole();
            List<Country>? Countries = _userService.GetCountry();
            List<State>? States = _userService.GetState(-1);
            List<City>? Cities = _userService.GetCity(-1);

            if (roleName == "Account Manager")
            {
                Roles.RemoveAt(0);
            }

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
                    TempData["ErrorMessage"] = NotificationMessage.ImageFormat;
                    return RedirectToAction("AddUser", "User", new { Email = user.Email });
                }
            }

            if (await _userService.IsUserNameExists(user.Username))
            {
                TempData["addUserErrorMessage"] = NotificationMessage.AlreadyExists.Replace("{0}", "UserName");
                return RedirectToAction("AddUser", "User");
            }
            if (!await _userService.AddUser(user, Email))
            {
                //change
                TempData["ErrorMessage"] = NotificationMessage.AlreadyExists.Replace("{0}", "Account with this email");
                return View();
            }

            bool SendEmail = await _userService.SendEmail(user.Password, user.Username, user.Email);
            if (SendEmail)
            {
                TempData["SuccessMessage"] = NotificationMessage.EmailSentSuccessfully;
            }
            else
            {
                TempData["ErrorMessage"] = NotificationMessage.EmailSendingFailed;
            }

            // MailAddress senderEmail = new MailAddress("tatva.pca155@outlook.com", "tatva.pca155@outlook.com");
            // MailAddress receiverEmail = new MailAddress(user.Email, user.Email);
            // string? password = "P}N^{z-]7Ilp";
            // string? sub = "Add user";
            // string? body = EmailTemplate.AddUserEmail( user.Password, user.Email);
            // SmtpClient smtp = new SmtpClient
            // {
            //     Host = "mail.etatvasoft.com",
            //     Port = 587,
            //     EnableSsl = true,
            //     DeliveryMethod = SmtpDeliveryMethod.Network,
            //     UseDefaultCredentials = false,
            //     Credentials = new NetworkCredential(senderEmail.Address, password)
            // };
            // using (MailMessage mess = new MailMessage(senderEmail, receiverEmail))
            // {
            //     mess.Subject = sub;
            //     mess.Body = body;
            //     mess.IsBodyHtml = true;
            //     await smtp.SendMailAsync(mess);
            // }
            TempData["SuccessMessage"] = NotificationMessage.EntityCreated.Replace("{0}", "User");
            return RedirectToAction("UserListData", "User");
        }
        #endregion

        #region EditUser
        [PermissionAuthorize("Users.AddEdit")]
        public IActionResult EditUser(string Email)
        {
            List<AddUserViewModel>? user = _userService.GetUserByEmail(Email);

            string? token = Request.Cookies["AuthToken"];
            string roleName = _JWTService.GetClaimValue(token, "role");


            if (user[0].RoleId == 1 && roleName != "Admin")
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this user!";
                return RedirectToAction("UserListData", "User");
            }

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
                    TempData["ErrorMessage"] = NotificationMessage.ImageFormat;
                    return RedirectToAction("EditUser", "User", new { Email = adduser.Email });
                }
            }

            if (_userService.IsUserNameExistsForEdit(adduser.Username, Email))
            {
                TempData["ErrorMessage"] = NotificationMessage.AlreadyExists.Replace("{0}", "UserName");
                return RedirectToAction("EditUser", "User", new { Email = adduser.Email });
            }

            if (await _userService.EditUser(adduser, Email))
            {
                TempData["SuccessMessage"] = NotificationMessage.EntityUpdated.Replace("{0}", "User");
                return RedirectToAction("UserListData", "User");
            }
            else
            {
                TempData["ErrorMessage"] = NotificationMessage.EntityUpdatedFailed.Replace("{0}", "User");
                return RedirectToAction("EditUser", "User", new { Email = adduser.Email });
            }
        }
        #endregion

        #region DeleteUser
        [PermissionAuthorize("Users.Delete")]
        public async Task<IActionResult> DeleteUser(string Email)
        {
            bool isDeleted = await _userService.DeleteUser(Email);
            List<AddUserViewModel>? user = _userService.GetUserByEmail(Email);

            string? token = Request.Cookies["AuthToken"];
            string roleName = _JWTService.GetClaimValue(token, "role");

            if (user[0].RoleId == 1 && roleName != "Admin")
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this user!";
                return RedirectToAction("UserListData", "User");
            }
            else
            {

                if (!isDeleted)
                {
                    ViewBag.Message = NotificationMessage.EntityDeletedFailed.Replace("{0}", "User");
                    return RedirectToAction("UserListData", "User");
                }
                TempData["SuccessMessage"] = NotificationMessage.EntityDeleted.Replace("{0}", "User");
                return RedirectToAction("UserListData", "User");
            }
        }
        #endregion

        #endregion

    }
}