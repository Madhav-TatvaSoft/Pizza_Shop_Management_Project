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


namespace Pizza_Shop_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        private readonly IUserLoginService _userLoginService;

        private readonly IJWTService _JWTService;

        public UserController(IUserService userService, IJWTService JWTService, IUserLoginService userLoginService)
        {
            this._userService = userService;
            this._JWTService = JWTService;
            this._userLoginService = userLoginService;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            ViewData["sidebar-active"] = "Dashboard";
            return View();
        }

        #region State,City
        public JsonResult GetStates(long? countryId)
        {
            var states = _userService.GetState(countryId);
            return Json(new SelectList(states, "StateId", "StateName"));
        }

        public JsonResult GetCities(long? stateId)
        {
            var cities = _userService.GetCity(stateId);
            return Json(new SelectList(cities, "CityId", "CityName"));
        }
        #endregion

        #region UserProfile

        [Authorize(Roles = "Admin")]
        public IActionResult UserProfile()
        {
            var cookieSavedToken = Request.Cookies["AuthToken"];
            var data = _userService.GetUserProfileDetails(cookieSavedToken);
            var Countries = _userService.GetCountry();
            var States = _userService.GetState(data[0].CountryId);
            var Cities = _userService.GetCity(data[0].StateId);
            ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
            ViewBag.States = new SelectList(States, "StateId", "StateName");
            ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
            return View(data[0]);
        }

        [HttpPost]
        public IActionResult UserProfile(AddUserViewModel user)
        {
            var token = Request.Cookies["AuthToken"];
            var userEmail = _JWTService.GetClaimValue(token, "email");

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
                var extension = user.ProfileImage.FileName.Split(".");
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
            var token = Request.Cookies["AuthToken"];
            var userEmail = _JWTService.GetClaimValue(token, "email");


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
                var password_verify = _userService.UserChangePassword(changepassword, userEmail);
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
        public IActionResult UserListData()
        {
            ViewData["sidebar-active"] = "User";
            var users = _userService.GetUserList();
            return View(users);
        }

        public IActionResult PaginatedData(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5)
        {
            ViewBag.emailid = Request.Cookies["email"];
            var users = _userService.GetUserList(search, sortColumn, sortDirection, pageNumber, pageSize);
            return PartialView("_UserListDataPartial", users);
        }
        #endregion

        #region AddUser
        public IActionResult AddUser()
        {
            var Roles = _userService.GetRole();
            var Countries = _userService.GetCountry();
            var States = _userService.GetState(-1);
            var Cities = _userService.GetCity(-1);
            ViewBag.Roles = new SelectList(Roles, "RoleId", "RoleName");
            ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
            ViewBag.States = new SelectList(States, "StateId", "StateName");
            ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
            ViewData["sidebar-active"] = "User";

            return View();
        }

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

            var token = Request.Cookies["AuthToken"];
            var Email = _JWTService.GetClaimValue(token, "email");

            if (user.ProfileImage != null)
            {
                var extension = user.ProfileImage.FileName.Split(".");
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

            var senderEmail = new MailAddress("tatva.pca42@outlook.com", "tatva.pca42@outlook.com");
            var receiverEmail = new MailAddress(user.Email, user.Email);
            var password = "P}N^{z-]7Ilp";
            var sub = "Add user";
            var body = $@"<div style='max-width: 500px; font-family: Arial, sans-serif; border: 1px solid #ddd;'>
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
            var smtp = new SmtpClient
            {
                Host = "mail.etatvasoft.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (var mess = new MailMessage(senderEmail, receiverEmail))
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
        public IActionResult EditUser(string Email)
        {
            var user = _userService.GetUserByEmail(Email);
            var Roles = _userService.GetRole();
            var Countries = _userService.GetCountry();
            var States = _userService.GetState(user[0].CountryId);
            var Cities = _userService.GetCity(user[0].StateId);
            ViewBag.Roles = new SelectList(Roles, "RoleId", "RoleName");
            ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
            ViewBag.States = new SelectList(States, "StateId", "StateName");
            ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
            ViewData["sidebar-active"] = "User";

            return View(user[0]);
        }

        [HttpPost]

        public IActionResult EditUser(AddUserViewModel adduser)
        {
            var Email = adduser.Email;

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
                var extension = adduser.ProfileImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{Guid.NewGuid()}_{adduser.ProfileImage.FileName}";
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
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
            _userService.EditUser(adduser, Email);

            TempData["SuccessMessage"] = "User Updated successfully";
            return RedirectToAction("UserListData", "User");

        }
        #endregion

        #region DeleteUser
        public async Task<IActionResult> DeleteUser(string Email)
        {
            var isDeleted = await _userService.DeleteUser(Email);

            if (!isDeleted)
            {
                ViewBag.Message = "User cannot be deleted";
                return RedirectToAction("UserListData", "User");
            }
            TempData["SuccessMessage"] = "User deleted successfully";
            return RedirectToAction("UserListData", "User");
        }
        #endregion

    }
}