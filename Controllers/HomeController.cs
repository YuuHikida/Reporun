﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using 業務報告システム.Data;
using 業務報告システム.Models;
using 業務報告システム.ViewModels;
using Project = 業務報告システム.Models.Project;

namespace 業務報告システム.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _logger = logger;

            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Home()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index() {

            AdminIndex adminIndex = new AdminIndex();
            adminIndex.Projects = new List<Project>();
            adminIndex.Projects = _context.project.ToList();
            adminIndex.UserProjects = new List<UserProject>();
            adminIndex.UserProjects = _context.userproject.ToList();
            adminIndex.Users = new List<ApplicationUser>();
            adminIndex.Users = _userManager.Users.ToList();
            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            adminIndex.User = await _userManager.FindByIdAsync(loginUserId);

            int projectCount = adminIndex.Projects.Count;

            if (projectCount == 0)
            {
                    TempData["AlertProjectCreate"] = "一つ目のプロジェクトを登録してください。";
                    return Redirect("/Projects/Create");
            }
            else {

                return View(adminIndex);
            }
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MgrIndex()
        {
            UserIndex userIndex = new UserIndex();
            userIndex.Users = new List<ApplicationUser>();
            userIndex.Projects = new List<Models.Project>();

            var loginManagerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser loginManager = await _userManager.FindByIdAsync(loginManagerId);
            userIndex.User = loginManager;

            var allprojects = _context.project.ToList();
            var managerprojects = _context.userproject.Where(x => x.UserId.Equals(loginManager.Id)).ToList();

            foreach (var project in managerprojects)
            {
                foreach (var allproject in allprojects)
                {
                    if (project.ProjectId == allproject.ProjectId)
                    {
                        Project pj = new Project();
                        pj.ProjectId = allproject.ProjectId;
                        pj.Name = allproject.Name;
                        userIndex.Projects.Add(pj);
                    }
                }
            }

            var alluserprojects = _context.userproject.ToList();

            foreach (var userproject in alluserprojects)
            {
                foreach (var managerproject in userIndex.Projects)
                {
                    if (userproject.ProjectId == managerproject.ProjectId)
                    {
                        ApplicationUser user = await _userManager.FindByIdAsync(userproject.UserId);
                        userIndex.Users.Add(user);
                    }
                }

            }

            userIndex.Users.Remove(loginManager);

                return View(userIndex);

        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _userManager == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string[] values)
        {
            ApplicationUser user = new ApplicationUser();
            user.Id = id;
            user.LastName = values[0];
            user.FirstName = values[1];
            user.Email = values[2];
            user.Role = values[3];
            UserProject userProject = new UserProject();
            userProject.UserId = id;
            userProject.ProjectId = int.Parse(values[4]);
            user.UserProjects.Add(userProject);


            //try
            //{
            //    _userManager.UpdateAsync(user);
            //    await _userManager.UpdateRoleAsync(user, user.Role);
            //    TempData["AlertProject"] = "ユーザーを編集しました。";
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //        return NotFound();
            //}
            return RedirectToAction(nameof(Index));

        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_userManager == null)
            {
                return Problem("Entity set 'UserManager'  is null.");
            }
            var user = await _userManager.FindByIdAsync(id);

            if (user.Email.Equals("admin@admin.com"))
            {
                TempData["AlertUserError"] = "管理者の削除はできません。";
                return Redirect("/Home/Index");
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            TempData["AlertUser"] = "ユーザーを削除しました。";
            return Redirect("/Home/Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}