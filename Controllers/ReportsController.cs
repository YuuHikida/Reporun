using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ReportSystem.Data;
using ReportSystem.Models;
using ReportSystem.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Primitives;
using ReportSystem.Hubs;
using System;
using System.Threading.Tasks;
using ReportSystem.Repositorys;


namespace ReportSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHubContext<MemberNotificationHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NotificationRepository _notificationRepository;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IHubContext<MemberNotificationHub> hubContext,
                IHttpContextAccessor httpContextAccessor,
                NotificationRepository notificationRepository)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            _notificationRepository = notificationRepository;
        }

        // GET: Reports/mgrindex　マネージャー用
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MgrIndex(string? Id,string feedbackSearch, string yearMonthSearch, int monthSearch, string attendanceSearch)
        {
            int month = 0;
            int year = 0;
            var SearchConditions = "";
            if (Id == null || _context.report == null)
            {
                return NotFound("存在しません");
            }

            if(yearMonthSearch != null)
            {
                var yearMonthArray = yearMonthSearch.Split('-');
                year = int.Parse(yearMonthArray[0]);
                month = int.Parse(yearMonthArray[1]);
            }
            

            ReportIndex reportIndex = new ReportIndex();
            reportIndex.User = new ApplicationUser();
            reportIndex.Reports = new List<Report>();
            reportIndex.Attendances = new List<Attendance>();
            reportIndex.Feedbacks = new List<Feedback>();

            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            reportIndex.User = (ApplicationUser)_context.Users.FirstOrDefault(x => x.Id == Id);

            var allReports = _context.report.Where(x => x.UserId.Equals(Id)).ToList();
            var allAttendance = _context.attendance.Where(x => x.Report.UserId.Equals(Id)).ToList();
            var allFeedback = _context.feedback.ToList();

            var reportIdsWithFeedback = new List<int>();
            var feedbackReportIds = _context.feedback.Select(f => f.ReportId).ToList();
            var allReportIds = _context.report.Select(r => r.ReportId).ToList();

            if (feedbackSearch != null)
            {
                foreach (var feed in allFeedback)
                {
                    if (feedbackSearch.Equals("既読"))
                    {
                        reportIdsWithFeedback.AddRange(_context.feedback.Where(x => x.ReportId == feed.ReportId && x.Report.UserId.Equals(Id)).Select(f => f.ReportId));
                        SearchConditions = "既読";
                    }
                    else if (feedbackSearch.Equals("未読"))
                    {
                        reportIdsWithFeedback = allReportIds.Except(feedbackReportIds).ToList();
                        SearchConditions =  "未読";
                    }
                }
            }
            if (feedbackSearch != null && month != 0 && attendanceSearch != null)
            {
                allReports = _context.report.Where(x => x.Date.Year == year && x.Date.Month == month && x.Attendance.Status.Equals(attendanceSearch) && reportIdsWithFeedback.Contains(x.ReportId) && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => x.Status.Equals(attendanceSearch) && x.Date.Year == year && x.Report.Date.Month == month && reportIdsWithFeedback.Contains(x.ReportId) && x.Report.UserId.Equals(Id)).ToList();
                SearchConditions = SearchConditions + " / "+ year + "年" + month + "月" + " / " + attendanceSearch;
            }
            else if (feedbackSearch != null && month != 0)
            {
                allReports = _context.report.Where(x => x.Date.Year == year && x.Date.Month == month && reportIdsWithFeedback.Contains(x.ReportId) && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => x.Date.Year == year && x.Report.Date.Month == month && reportIdsWithFeedback.Contains(x.ReportId) && x.Report.UserId.Equals(Id)).ToList();
                SearchConditions = SearchConditions + " / " + year + "年" + month + "月";
            }
            else if (feedbackSearch != null && attendanceSearch != null)
            {
                allReports =  _context.report.Where(x =>  x.Attendance.Status.Equals(attendanceSearch) && reportIdsWithFeedback.Contains(x.ReportId) && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => x.Status.Equals(attendanceSearch) && reportIdsWithFeedback.Contains(x.ReportId) && x.Report.UserId.Equals(Id)).ToList();
                SearchConditions = SearchConditions + " / " + attendanceSearch;
            }
            else if (month != 0 && attendanceSearch != null)
            {
                allReports = _context.report.Where(x => x.Date.Year == year && x.Date.Month == month && x.Attendance.Status.Equals(attendanceSearch) && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => x.Status.Equals(attendanceSearch) && x.Date.Year == year && x.Report.Date.Month == month && x.Report.UserId.Equals(Id)).ToList();
                SearchConditions = year + "年" + month + "月" + " / " + attendanceSearch;
            }
            else if(month != 0)
            {
                allReports = _context.report.Where(x => x.Date.Year == year && x.Date.Month == month && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => x.Date.Year == year && x.Report.Date.Month == month && x.Report.UserId.Equals(Id)).ToList();
                SearchConditions = year + "年" + month + "月";
            }
            else if (attendanceSearch != null)
            {
                allReports = _context.report.Where(x => x.Attendance.Status.Equals(attendanceSearch) && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => x.Status.Equals(attendanceSearch) && x.Report.UserId.Equals(Id)).ToList();
                SearchConditions = attendanceSearch;
            }
            else if(feedbackSearch != null)
            {
                allReports = _context.report.Where(x => reportIdsWithFeedback.Contains(x.ReportId) && x.UserId.Equals(Id)).ToList();
                allAttendance = _context.attendance.Where(x => reportIdsWithFeedback.Contains(x.ReportId) && x.Report.UserId.Equals(Id)).ToList();
            }

            foreach (var report in allReports)
            {
                Report re = new Report();
                re.Date = report.Date;
                re.Comment = report.Comment;
                re.ReportId = report.ReportId;
                re.User = report.User;
                reportIndex.Reports.Add(re);
            }
            foreach (var attendance in allAttendance)
            {
                Attendance at = new Attendance();
                at.Status = attendance.Status;
                at.HealthRating = attendance.HealthRating;
                at.ReportId = attendance.ReportId;
                reportIndex.Attendances.Add(at);
            }

            reportIndex.Feedbacks = allFeedback;

            ViewBag.MemberName = $"{reportIndex.User.LastName} {reportIndex.User.FirstName}";
            ViewBag.Condition = SearchConditions;

            var applicationDbContext = _context.report.Include(r => r.User);
            return View(reportIndex);
        }

        // GET: Reports/memindex　メンバー用
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MemIndex()
        {
            ReportIndex reportIndex = new ReportIndex();
            reportIndex.Reports = new List<Report>();
            reportIndex.Attendances = new List<Attendance>();
            reportIndex.Feedbacks = new List<Feedback>();
            reportIndex.Notifications = new List<Notification>();

            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            reportIndex.User = await _userManager.FindByIdAsync(loginUserId);

            //Reportsにデータを格納
            var allReports = _context.report.Where(x => x.UserId.Equals(loginUserId)).ToList();
            var allAttendance = _context.attendance.Where(x => x.Report.UserId.Equals(loginUserId)).ToList();
            var allFeedbacks = _context.feedback.Where(x => x.Report.UserId.Equals(loginUserId)).ToList();
            foreach (var report in allReports)//-------------------------------------------
            {
                Report re = new Report();
                re.Date = report.Date;
                re.Comment = report.Comment;
                re.ReportId = report.ReportId;
                reportIndex.Reports.Add(re);   
            }
            foreach (var attendance in allAttendance)
            {
                Attendance at = new Attendance();
                at.Status = attendance.Status;
                at.HealthRating = attendance.HealthRating;
                at.ReportId = attendance.ReportId;
                reportIndex.Attendances.Add(at);
            }

            reportIndex.Feedbacks = allFeedbacks;

            bool IsSent = true;
            bool IsDisplayed = false;
            var UserID_ReportID = loginUserId.ToString();
            var notificationTest = _notificationRepository.GetNotifications(UserID_ReportID, IsSent, IsDisplayed);
            foreach (var notificationtest in notificationTest)
            {
                Notification no = new Notification();
                no.UserId = notificationtest.UserId;
                no.DateReport = notificationtest.DateReport;
                reportIndex.Notifications.Add(no);
                _notificationRepository.MarkAsDisplayed(notificationtest);
            }

            return View(reportIndex);
        }

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MemMain()
        {
            MemberMain memberMain = new MemberMain();
            memberMain.Projects = new List<Project>();
            memberMain.Todos = new List<Todo>();
            memberMain.Managers = new List<ApplicationUser>();

            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            memberMain.LoginMember = await _userManager.FindByIdAsync(loginUserId);

            var userprojects = _context.userproject.Include(x => x.Project).Where(x => x.UserId.Equals(memberMain.LoginMember.Id)).ToList();

            if (userprojects.Count == 0)
            {
                TempData["ProjectError"] = "プロジェクトに参加していません。Adminユーザーにプロジェクトへの参加処理を依頼してください。";
                return Redirect("/Home");
            }
            foreach (var project in userprojects)
            {
                Project pj = new Project();
                pj.ProjectId = project.ProjectId;
                pj.Name = project.Project.Name;
                memberMain.Projects.Add(pj);
            }

            var alluserprojects = _context.userproject.Include(x => x.User).Where(x => x.ProjectId == memberMain.Projects.First().ProjectId && x.User.Role.Equals("Manager")).ToList();

            //マネージャー特定（単体）
            foreach (var userproject in alluserprojects)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userproject.UserId);
                memberMain.Managers.Add(user);
            }

            //todoリスト（未達成のみ）（複数）
            var userTodos = _context.todo.Where(x => x.UserId.Equals(memberMain.LoginMember.Id)).ToList();

            foreach (var todo in userTodos)
            {
                if (todo.Progress != 10)
                {
                    memberMain.Todos.Add(todo);
                }
            }

            //今日と昨日提出したreportのtomorrowコメント抽出（単体）
            var userReports = _context.report.Where(x => x.UserId.Equals(memberMain.LoginMember.Id)).ToList();

            var today = DateTime.Today;
            var yesterday = DateTime.Today.AddDays(-1);

            if (yesterday.ToString("ddd").Equals("土"))
            {
                yesterday = yesterday.AddDays(-1);
            }
            else if (yesterday.ToString("ddd").Equals("日"))
            {
                yesterday = yesterday.AddDays(-2);
            }

            foreach (var report in userReports)
            {
                if (report.Date.Year == today.Year && report.Date.Month == today.Month && report.Date.Day == today.Day)
                {
                    memberMain.TodayReport = report;
                }
            }

            foreach (var report in userReports)
            {
                if (report.Date.Year == yesterday.Year && report.Date.Month == yesterday.Month && report.Date.Day == yesterday.Day)
                {
                    memberMain.Report = report;
                }
            }
            return View(memberMain);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MgrMain()
        {
            ManagerMain managerMain = new ManagerMain();
            managerMain.Projects = new List<Project>();
            managerMain.Reports = new List<Report>();
            managerMain.Attendances = new List<Attendance>();
            managerMain.Todos = new List<Todo>();
            managerMain.Members = new List<ApplicationUser>();
            managerMain.ReportNotSubmit = new List<ApplicationUser>();
            managerMain.Feedbacks = new List<Feedback>();

            var loginManagerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            managerMain.Manager = await _userManager.FindByIdAsync(loginManagerId);
            var managerprojects =  _context.userproject.Include(x => x.Project).Where(x => x.UserId.Equals(managerMain.Manager.Id)).ToList(); 
           
            if (managerprojects.Count() == 0) { 
                TempData["AlertError"] = "プロジェクトに参加していません。Adminユーザーにプロジェクトへの参加処理を依頼してください。";
                return Redirect("/Home/Home");
            }


            foreach (var project in managerprojects)
            {
                Project pj = new Project();
                pj.ProjectId = project.ProjectId;
                pj.Name = project.Project.Name;
                //マネージャーのプロジェクト一覧作成
                managerMain.Projects.Add(pj);
            }

            //==========================================================================================================
            var projectIds = managerMain.Projects.Select(x => x.ProjectId).ToList();    //未検証だが、managerMainが複数addされた時、コメントアウトのalluserprojectsなら処理可能(chatGPT)で、
                                                                                        //foreachで複数managerMain.ReportNotSubmitにadd出来る　ここ以外(Reports,Todos,Home)は未対応
            //var alluserprojects = _context.userproject.Include(x => x.User).Where(x =>projectIds.Contains(x.ProjectId)).ToList();
            //==========================================================================================================

            var alluserprojects = _context.userproject.Include(x => x.User).Where(x => x.ProjectId == managerMain.Projects.First().ProjectId).ToList();
            
            foreach (var userproject in alluserprojects)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userproject.UserId);

                if (user.Role.Equals("Member")) {
                    // マネージャー配下のメンバーリスト作成
                    managerMain.Members.Add(user);

                    managerMain.ReportNotSubmit.Add(user);
                }
            }

            //managerMain.Members.Remove(managerMain.Manager);
            //managerMain.ReportNotSubmit.Remove(managerMain.Manager);

            var alltodos = _context.todo.Where(x => x.User.UserProjects.First().ProjectId == managerMain.Projects.First().ProjectId).ToList();

            foreach (var todo in alltodos)
            {
                foreach (var user in managerMain.Members)
                {
                    if (todo.UserId.Equals(user.Id) && todo.Progress < 10)
                    {
                        //マネージャー配下のメンバーの未提出Todoリスト作成
                        managerMain.Todos.Add(todo);
                    }
                }

            }

            DateTime today = DateTime.Today;
            var allTodayReports = _context.report.Where(x => x.Date.Year == today.Year && x.Date.Month == today.Month && x.Date.Day == today.Day).ToList();
            var allTodayAttendances = _context.attendance.Where(x => x.Date.Year == today.Year && x.Date.Month == today.Month && x.Date.Day == today.Day).ToList();

            foreach (var report in allTodayReports)
            {
                foreach (var member in managerMain.Members)
                {
                    if (report.UserId.Equals(member.Id))
                    {
                        //今日提出のreportリスト作成
                        managerMain.Reports.Add(report);
                    }
                }
            }

            foreach (var attendance in allTodayAttendances)
            {
                foreach (var report in managerMain.Reports)
                {
                    if (attendance.ReportId == report.ReportId)
                    {
                        // 今日提出のattendanceリスト作成
                        managerMain.Attendances.Add(attendance);
                    }
                }
            }

            var allFeedback = _context.feedback.ToList();
            managerMain.Feedbacks = allFeedback;

            DateTime yesterday = DateTime.Today.AddDays(-1);

            if (yesterday.ToString("ddd").Equals("土")) {
                yesterday = yesterday.AddDays(-1);
            } else if(yesterday.ToString("ddd").Equals("日")) {
                yesterday = yesterday.AddDays(-2);
            }

            var allYesterdayReports = _context.report.Where(x => x.Date.Year == yesterday.Year && x.Date.Month == yesterday.Month && x.Date.Day == yesterday.Day).ToList();
            var allYesterdayAttendances = _context.attendance.Where(x => x.Date.Year == yesterday.Year && x.Date.Month == yesterday.Month && x.Date.Day == yesterday.Day).ToList();


            foreach (var member in managerMain.Members) {
                foreach (var report in allYesterdayReports)
                {
                    if (member.Id.Equals(report.UserId)) { 
                        //前日のレポート未提出メンバーリスト作成
                        managerMain.ReportNotSubmit.Remove(member);
                    }
                }
            }

            return View(managerMain);
        }

        // GET: Reports/Details/5
        [Authorize(Roles = "Manager, Member")]
        public async Task<IActionResult> Details(int? id)
        {
            ReportDetail reportDetail = new ReportDetail();
            reportDetail.Projects = new List<Project>();
            reportDetail.Managers = new List<ApplicationUser>();

            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (id == null || _context.report == null)
            {
                return NotFound();
            }

            var report = await _context.report
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReportId == id);

            reportDetail.User = await _userManager.FindByIdAsync(report.UserId);

            if (report == null)
            {
                return NotFound();
            }

            var allprojects = _context.project.ToList();
            var userprojects = _context.userproject.Where(x => x.UserId.Equals(reportDetail.User.Id)).ToList();

            if (userprojects.Count == 0)
            {
                TempData["ProjectError"] = "プロジェクトに参加していません。Adminユーザーにプロジェクトへの参加処理を依頼してください。";
                return Redirect("/Home");
            }

            foreach (var project in userprojects)
            {
                Project pj = new Project();
                pj.ProjectId = project.ProjectId;
                pj.Name = project.Project.Name;
                reportDetail.Projects.Add(pj);
            }

            var alluserprojects = _context.userproject.Where(x => x.ProjectId == reportDetail.Projects.First().ProjectId).ToList();
            ApplicationUser user;

            foreach (var userproject in alluserprojects)
            {
                user = await _userManager.FindByIdAsync(userproject.UserId);
                if (await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    reportDetail.Managers.Add(user);
                }
            }

            int count = 0;

            if (User.IsInRole("Member"))
            {
                if (!(report.UserId.Equals(loginUserId)))
                {
                    return NotFound("アクセス権がありません。");
                }
            }
            else if (User.IsInRole("Manager"))
            {
                foreach (var mgr in reportDetail.Managers) {

                    if ((loginUserId.Equals(mgr.Id))) {
                        count++;
                    }
                }

                if (count == 0) {
                    return NotFound("アクセス権がありません。");
                    
                }
                count = 0;
            }

            reportDetail.Report = report;
            var allattendance = _context.attendance.Where(x => x.ReportId == reportDetail.Report.ReportId).ToList();
            foreach (var attendance in allattendance)
            {
                reportDetail.Attendance = attendance;
            }

            var feedbacks = _context.feedback.Where(x => x.ReportId == report.ReportId).ToList();

            foreach (var feedback in feedbacks)
            {
                if (feedback.ReportId == report.ReportId)
                {
                    reportDetail.Feedback = feedback;
                }
            }


            return View(reportDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int? id, string[] values)
        {
            ReportDetail reportDetail = new ReportDetail();
            reportDetail.Projects = new List<Project>();
            reportDetail.Managers = new List<ApplicationUser>();

            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            reportDetail.User = await _userManager.FindByIdAsync(loginUserId);

            if (id == null || _context.report == null)
            {
                return NotFound();
            }

            var report = await _context.report
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
            {
                return NotFound();
            }

            var userprojects = _context.userproject.Include(x => x.Project).Where(x => x.UserId.Equals(reportDetail.User.Id)).ToList();

            foreach (var project in userprojects)
            {
                Project pj = new Project();
                pj.ProjectId = project.ProjectId;
                pj.Name = project.Project.Name;
                reportDetail.Projects.Add(pj);
            }

            var alluserprojects = _context.userproject.Where(x => x.ProjectId == reportDetail.Projects.First().ProjectId).ToList();

            foreach (var userproject in alluserprojects)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userproject.UserId);

                if (await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    reportDetail.Managers.Add(user);
                }
            }

            reportDetail.Report = report;
            var allattendance = _context.attendance.Where(x => x.ReportId == reportDetail.Report.ReportId).ToList();
            foreach (var attendance in allattendance)
            {
                reportDetail.Attendance = attendance;
            }

            Feedback feedback = new Feedback()
            {
                ReportId = report.ReportId,
                //Confirm = true,
                Rating = int.Parse(values[0]),
                Comment = values[1],
                Name = $"{reportDetail.User.LastName} {reportDetail.User.FirstName}"
            };

            _context.Add(feedback);
            await _context.SaveChangesAsync();

            reportDetail.Feedback = feedback;

            int ReportId_User = report.ReportId;
            var UserID_ReportID = _context.report
                        .Where(report => report.ReportId == ReportId_User)
                        .Select(report => report.UserId)
                        .FirstOrDefault();

            var Date_ReportID = _context.report
                        .Where(report => report.ReportId == ReportId_User)
                        .Select(report => report.Date)
                        .FirstOrDefault();

            string formattedDate = Date_ReportID.ToString("yyyy-MM-ddTHH:mm:ss");

            //string commentS = values[1];
            //if (ReportId_User != 0 && !string.IsNullOrEmpty(commentS))
            if (ReportId_User != 0)
            {
                _notificationRepository.SaveNotification(UserID_ReportID, formattedDate);
                await _hubContext.Clients.All.SendAsync("ReceiveMemberNotification", UserID_ReportID, formattedDate);
            }

            return View(reportDetail);
        }


        // GET: Reports/Create
        [Authorize(Roles = "Member")]
        public IActionResult Create()
        {
            try
            {
                var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var Re = _context.user.Include(x => x.Reports).Where(x => x.Id.Equals(loginUserId)).ToList();
                var At = _context.report.Include(x => x.Attendance).Where(x => x.UserId.Equals(loginUserId)).OrderByDescending(x => x.Date).FirstOrDefault();
                int startHour = At.Attendance.StartTime.Hour;
                int startMinute = At.Attendance.StartTime.Minute;
                int endHour = At.Attendance.EndTime.Hour;
                int endMinute = At.Attendance.EndTime.Minute;

                ViewBag.StartHour = startHour;
                ViewBag.StartMinute = startMinute;
                ViewBag.EndHour = endHour;
                ViewBag.EndMinute = endMinute;
            }
            catch(Exception e)
            {
                
            }
            return View();
        }

        // POST: Reports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string[] values)
        {

            var submitDay = DateTime.Parse(values[0]);
            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            DateTime startTime = new DateTime(submitDay.Year, submitDay.Month, submitDay.Day, int.Parse(values[2]), int.Parse(values[3]), 0);
            DateTime endTime = new DateTime(submitDay.Year, submitDay.Month, submitDay.Day, int.Parse(values[4]), int.Parse(values[5]), 0);

            Report report = new Report()
            {
                Date = submitDay,
                Comment = values[8],
                TomorrowComment = values[9],
                UserId = loginUserId
            };

            var sameDayReportCheck = _context.report.Where(x => x.UserId.Equals(loginUserId)).Where(y => y.Date.Year == report.Date.Year && y.Date.Month == report.Date.Month && y.Date.Day == report.Date.Day).ToList();
            if (sameDayReportCheck.Count() != 0) {
                TempData["AlertReportError"] = "同じ日付の報告が既に存在しています。別の日付で作り直してください。";
                return View();

            } else if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();

                Attendance attendance = new Attendance()
                {
                    Date = submitDay,
                    Status = values[1],
                    StartTime = startTime,
                    EndTime = endTime,
                    HealthRating = int.Parse(values[6]),
                    HealthComment = values[7],
                    ReportId = report.ReportId,
                };

                if (ModelState.IsValid)
                {

                    _context.Add(attendance);
                    await _context.SaveChangesAsync();

                    TempData["AlertReport"] = "報告を作成しました。";
                    return RedirectToAction(nameof(MemIndex));
                }
                else {
                    TempData["AlertReportError"] = "報告を保存できませんでした。";
                    return View();
                }

            }
            else {
                TempData["AlertReportError"] = "報告を保存できませんでした。";
                return View();
            }

        }

        // GET: Reports/Edit/5
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Edit(int? id)
        {
            ReportCRUD reportCRUD = new ReportCRUD();


            if (id == null || _context.report == null)
            {
                return NotFound();
            }

            var report = await _context.report.FindAsync(id);
            reportCRUD.Report = report;
            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (report == null)
            {
                return NotFound();
            }

            if (!(report.UserId.Equals(loginUserId)))
            {

                return NotFound("アクセス権がありません。");
            }


            reportCRUD.User = await _userManager.FindByIdAsync(report.UserId);

            var allAttendances = _context.attendance.ToList();

            foreach (var attendance in allAttendances)
            {
                if (attendance.ReportId == report.ReportId)
                {
                    reportCRUD.Attendance = attendance;
                }
            }

           

            return View(reportCRUD);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string[] values)
        {
            ReportCRUD reportCRUD = new ReportCRUD();

            var submitDay = DateTime.Parse(values[1]);
            var loginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var judgeDate = _context.report.Where(x => x.Date == submitDay && x.ReportId != int.Parse(values[0]) && x.UserId == loginUserId).ToList();   
            DateTime startTime = new DateTime(submitDay.Year, submitDay.Month, submitDay.Day, int.Parse(values[3]), int.Parse(values[4]), 0);
            DateTime endTime = new DateTime(submitDay.Year, submitDay.Month, submitDay.Day, int.Parse(values[5]), int.Parse(values[6]), 0);

            Report report = new Report()
            {
                ReportId = int.Parse(values[0]),
                Date = submitDay,
                UpDate = DateTime.Now,
                Comment = values[9],
                TomorrowComment = values[10],
                UserId = loginUserId
            };

            Attendance attendance = new Attendance()
            {
                AttendanceId = int.Parse(values[11]),
                Date = submitDay,
                Status = values[2],
                StartTime = startTime,
                EndTime = endTime,
                HealthRating = int.Parse(values[7]),
                HealthComment = values[8],
                ReportId = report.ReportId,
            };

            reportCRUD.Report = report;
            reportCRUD.Attendance = attendance;
            reportCRUD.User = await _userManager.FindByIdAsync(report.UserId);

            if (judgeDate.Count > 0)
            {
                TempData["AlertReportError"] = "同じ日付の報告が既に存在しています。別の日付で作り直してください。";
                return View(reportCRUD);
            }

            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                _context.Update(report);
                await _context.SaveChangesAsync();
                _context.Update(attendance);
                await _context.SaveChangesAsync();
                TempData["AlertReport"] = "報告を編集しました。";
                return RedirectToAction(nameof(MemIndex));
            }

            TempData["AlertReportError"] = "報告を編集できませんでした。";
            return View(reportCRUD);
        }


        // GET: Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.report == null)
            {
                return NotFound();
            }

            var report = await _context.report
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.report == null)
            {
                return Problem("Entity set 'ApplicationDbContext.report'  is null.");
            }
            var report = await _context.report.FindAsync(id);
            if (report != null)
            {
                _context.report.Remove(report);
            }

            await _context.SaveChangesAsync();
            TempData["AlertReport"] = "報告を削除しました。";
            return RedirectToAction(nameof(MemIndex));
        }

        private bool ReportExists(int id)
        {
            return (_context.report?.Any(e => e.ReportId == id)).GetValueOrDefault();
        }
    }

}