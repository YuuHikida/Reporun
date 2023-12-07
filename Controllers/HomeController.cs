using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using ReportSystem.Data;
using ReportSystem.Models;
using ReportSystem.ViewModels;
using Project = ReportSystem.Models.Project;

namespace ReportSystem.Controllers
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
        public async Task<IActionResult> Index(string[] search) {

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

                if (search.Length == 0)
                {
                    return View(adminIndex);

                }
                else {

                    switch (search[1]) {
                        case "名前":
                            adminIndex.Users = await _userManager.Users.Where(x => x.FirstName.Contains(search[0]) || x.LastName.Contains(search[0])).ToListAsync();
                            break;
                        case "メールアドレス":
                            adminIndex.Users = await _userManager.Users.Where(x => x.Email.Contains(search[0])).ToListAsync();
                            break;
                        case "ロール":
                            adminIndex.Users = await _userManager.Users.Where(x => x.Role.Contains(search[0])).ToListAsync();
                            break;
                        case "プロジェクト":
                            adminIndex.Projects = _context.project.Where(x => x.Name.Contains(search[0])).ToList();
                            adminIndex.UserProjects = new List<UserProject>();
                            foreach (var pj in adminIndex.Projects) { 
                                var allUserProjects = _context.userproject.ToList();
                                foreach (var up in allUserProjects) {
                                    if (pj.ProjectId == up.ProjectId) {
                                        adminIndex.UserProjects.Add(up);
                                    }
                                }
                            }
                            adminIndex.Users = new List<ApplicationUser>();
                            var allusers = _userManager.Users.ToList();

                            foreach (var us in allusers) {
                                foreach (var up in adminIndex.UserProjects) {
                                    if (us.UserProjects != null) { 
                                        if (us.UserProjects.Contains(up))
                                            adminIndex.Users.Add(us);                                 
                                    }
                                }
                            }
                            break;

                    }
                        
                    return View(adminIndex);
                }

               
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
            var managerproject = _context.userproject.Include(x => x.Project).Where(x => x.UserId.Equals(loginManager.Id)).ToList();

            if (managerproject.Count() == 0)
            {
                TempData["AlertError"] = "プロジェクトに参加していません。Adminユーザーにプロジェクトへの参加処理を依頼してください。";
                return Redirect("/Home/Home");
            }

            Project pj = new Project();
            pj.ProjectId = managerproject.First().ProjectId;
            pj.Name = managerproject.First().Project.Name;
            userIndex.Projects.Add(pj);

            var alluserprojects = _context.userproject.Where(x => x.ProjectId == userIndex.Projects.First().ProjectId).ToList();

            foreach (var userproject in alluserprojects)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userproject.UserId);
                if (user.Role.Equals("Member"))
                {
                    userIndex.Users.Add(user);
                }
            }

            userIndex.Users.Remove(loginManager);

                return View(userIndex);

        }
        //Adminのuser
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

            ViewBag.Projects = new SelectList(_context.project, "ProjectId", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string[] values)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.LastName = values[0];
            user.FirstName = values[1];
            user.Email = values[2];
            user.UserName = values[2];
            user.Role = values[3];

            if (values[6] == null || values[7] == null) {
                TempData["AlertEditError"] = "パスワードを入力してください。";
                ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
                return View(user);
            }

            if (values[6].Equals(values[7]) && HasUpperCase(values[6]) == true && HasLowerCase(values[6]) == true && values[6].Any(char.IsDigit) == true && values[6].Any(IsSpecialCharacter) == true)
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, values[6]);

            }
            else
            {
                TempData["AlertEditError"] = "パスワードの入力情報に誤りがあります。";
                ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
                return View(user);

            }

            if (user.LastName == null || user.FirstName == null || user.Email == null)
            {
                TempData["AlertEditError"] = "姓、名、Emailは入力必須項目です。";
                ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
                return View(user);
            }

            var userProjects = _context.userproject.Where(x => x.UserId.Equals(user.Id)).ToList();

            //所属プロジェクトの削除。複数プロジェクト管理するようになったらコード変更の必要あり。
            if (userProjects != null)
            {
                foreach (var project in userProjects)
                {
                    _context.userproject.Remove(project);
                }
            }

            UserProject userProject = new UserProject();
            userProject.UserId = id;
            userProject.ProjectId = int.Parse(values[4]);

            var allUserproject = _context.userproject.ToList();

            if (userProject != null)
            {
                if (allUserproject.Count != 0)
                {
                    foreach (var pj in allUserproject)
                    {

                        if (!(pj.ProjectId == userProject.ProjectId && pj.UserId.Equals(userProject.UserId)))
                        {
                            user.UserProjects = new List<UserProject>();
                            user.UserProjects.Add(userProject);
                            _context.userproject.Add(userProject);
                        }
                    }

                }
                else {
                    user.UserProjects = new List<UserProject>();
                    user.UserProjects.Add(userProject);
                    _context.userproject.Add(userProject);

                }
                
            }

            if (!(values[5].Equals(user.Role))) {
                await _userManager.AddToRoleAsync(user, user.Role);
                await _userManager.RemoveFromRoleAsync(user, values[5]);
            }

            //if (values[6].Equals("リセットする"))
            //{
            //    string password = "User123,";
            //    await _userManager.RemovePasswordAsync(user);
            //    await _userManager.AddPasswordAsync(user, password);

            //}

                var result = await _userManager.UpdateAsync(user);


            if (result.Succeeded)
            {
                //if (values[6].Equals("リセットする"))
                //{
                //    string password = "User123,";
                //    await _userManager.RemovePasswordAsync(user);
                //    await _userManager.AddPasswordAsync(user, password);

                //}

                TempData["AlertUser"] = "ユーザーを編集しました。";
                return RedirectToAction(nameof(Index));
            }

            TempData["AlertEditError"] = "入力情報に誤りがあります。";
            ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
            return View(user);

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

        private bool HasUpperCase(string str)
        {
            return str.Any(char.IsUpper);
        }
        private bool HasLowerCase(string str)
        {
            return str.Any(char.IsLower);
        }
        private bool IsSpecialCharacter(char c)
        {
            var specialCharacters = new[] { '!', '@', '#', '$', '%', '^', '&', '*', ',', ';', ':' };
            return specialCharacters.Contains(c);
        }



    }
}