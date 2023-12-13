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
using System.Linq;

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


            //------------ここで提出されているreport全取得からの未読だけを抽出し、モデルにぶちこむ
            //UserIndex userIndex = new UserIndex();
            //userIndex.Users = new List<ApplicationUser>();
            //userIndex.Projects = new List<Models.Project>();

            //必要なものは名前<applicationUser.model>:日付<Report.model>:コメント<Report.model>
            //これをUserIndexというViewModelに入れる

            userIndex.report = new List<Report>();
            //var Duplication = _context.feedback.Where(x => x.Report(UserId))
            //↓確認用
           // var ittan = _context.report.Where(x=>x.User.Role=="Member").ToList();
            //未読のみ
            //↓reportのreportId=2がFeedbackIdsに入る
            var feedbackIds = _context.feedback.Select(f => f.ReportId).ToList();
            //var kidding_me = _context.report.Where(x => x.UserId.NotEquals(x.feedbacks.feedbackId).ToList());
            var sladerAllReports = _context.report.Where(x => !feedbackIds.Contains(x.ReportId)).ToList();
            foreach (var x in sladerAllReports)
            {
                userIndex.report.Add(x);
            }
            //var allReports = _context.report.Where(x => x.UserId.Equals(Id)).ToList();
            //-------------------------------------------------------------------------

            return View(userIndex);
            
        }


        //Adminのuser
        //getMethod
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

        //↓こいつが対応している？
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string[] values, IFormFile iconIn)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.LastName = values[0];
            user.FirstName = values[1];
            user.Email = values[2];
            user.UserName = values[2];
            user.Role = values[3];
            user.Icon = ConvertIFormFileToByteArray(iconIn);
            //user.Icon = iconIn;

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

        //アイコン処理
        // IFormFileをbyte[]に変換してからBase64文字列に変換する
        public byte[] ConvertIFormFileToByteArray(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    // MemoryStream内にあるバイト配列データ尾をすべて取得し
                    // byte[]をBase64文字列に変換する
                    string ittan = Convert.ToBase64String(ms.ToArray());
                    return Convert.FromBase64String(ittan);
                }
            }
            else
            {
                return Convert.FromBase64String("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");
            }
        }

    }
}