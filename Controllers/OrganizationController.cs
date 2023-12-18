using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Data;
using ReportSystem.Models;
using Microsoft.EntityFrameworkCore;
using ReportSystem.ViewModels;
using System.Security.Claims;

namespace ReportSystem.Controllers
{
    public class OrganizationController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<HomeController> _logger;

        public OrganizationController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _logger = logger;

            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public IActionResult Organization()
        {
            return View();
        }
        
        // GET: OrganizationController
        public async Task<IActionResult> Index()
        {
            ApplicationUserChart applicationUserChart = new ApplicationUserChart();
          
            //ログイン中のrole
            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var loginManager = await _userManager.FindByIdAsync(loginUserId);
            //applicationUserChart.User = new ApplicationUser();
            applicationUserChart.User = loginManager;

            //組織図に必要な社員一括取得
            var OrganizationChartDate = _context.user.ToList();
            applicationUserChart.Users = new List<ApplicationUser>();
            //ラムダ式
            OrganizationChartDate.ForEach(x => applicationUserChart.Users.Add(x));

            return View(applicationUserChart);
        }

        // GET: OrganizationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: OrganizationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrganizationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrganizationController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: OrganizationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrganizationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: OrganizationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
