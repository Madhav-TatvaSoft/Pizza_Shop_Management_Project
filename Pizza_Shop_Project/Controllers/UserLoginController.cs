using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Models;

namespace Pizza_Shop_Project.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly PizzaShopDbContext _context;

        public UserLoginController(PizzaShopDbContext context)
        {
            _context = context;
        }

        // GET: UserLogin
        public async Task<IActionResult> Index()
        { 
            var pizzaShopDbContext = _context.UserLogins.Include(u => u.Role);
            return View(await pizzaShopDbContext.ToListAsync());
        }

        // GET: UserLogin/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.UserLogins == null)
            {
                return NotFound();
            }

            var userLogin = await _context.UserLogins
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserloginId == id);
            if (userLogin == null)
            {
                return NotFound();
            }

            return View(userLogin);
        }

        // GET: UserLogin/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: UserLogin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        // public async Task<IActionResult> Create([Bind("UserloginId,RoleId,Email,Password")] UserLogin userLogin)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         _context.Add(userLogin);
        //         await _context.SaveChangesAsync();
        //         return RedirectToAction(nameof(Index));
        //     }
        //     ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", userLogin.RoleId);
        //     return View(userLogin);
        // }

         public async Task<IActionResult> Create([Bind("Email,Password")] UserLogin userLogin)
        {
            if(_context.UserLogins.FirstOrDefault(e => e.Email == userLogin.Email && e.Password == userLogin.Password) != null){
                return RedirectToAction("Index","UserLogin");
            }
            ViewBag.message = "Please enter valid credentials";
            return View();
        }

        // GET: UserLogin/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.UserLogins == null)
            {
                return NotFound();
            }

            var userLogin = await _context.UserLogins.FindAsync(id);
            if (userLogin == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", userLogin.RoleId);
            return View(userLogin);
        }

        // POST: UserLogin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("UserloginId,RoleId,Email,Password")] UserLogin userLogin)
        {
            if (id != userLogin.UserloginId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userLogin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserLoginExists(userLogin.UserloginId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", userLogin.RoleId);
            return View(userLogin);
        }

        // GET: UserLogin/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.UserLogins == null)
            {
                return NotFound();
            }

            var userLogin = await _context.UserLogins
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserloginId == id);
            if (userLogin == null)
            {
                return NotFound();
            }

            return View(userLogin);
        }

        // POST: UserLogin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.UserLogins == null)
            {
                return Problem("Entity set 'PizzaShopDbContext.UserLogins'  is null.");
            }
            var userLogin = await _context.UserLogins.FindAsync(id);
            if (userLogin != null)
            {
                _context.UserLogins.Remove(userLogin);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserLoginExists(long id)
        {
          return (_context.UserLogins?.Any(e => e.UserloginId == id)).GetValueOrDefault();
        }
    }
}
