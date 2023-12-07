// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReportSystem.Data;
using ReportSystem.Models;
using System.Security.Cryptography;


namespace ReportSystem.Areas.Identity.Pages.Account
{
    //↓PageModelに注目
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        //↓ここのBindPropertyでInuputModelに
        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "Emailアドレスを入力してください。")]
            [EmailAddress(ErrorMessage = "半角英数字でEmailアドレスを入力してください。")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "名前を入力してください。")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "苗字を入力してください。")]
            public string LastName { get; set; }
            
            [Required(ErrorMessage = "ロールを選択してください。")]
            public string Role {  get; set; }
            
            [Required(ErrorMessage = "パスワードを入力してください。")]
            [StringLength(100, ErrorMessage = "{0}は{2}文字以上{1}文字以内で入力してください。", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "パスワード")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "確認用パスワード")]
            [Compare("Password", ErrorMessage = "パスワードと確認用のパスワードが一致していません。")]
            public string ConfirmPassword { get; set; }

            //↓アイコンinput!
            [Display(Name = "アイコン")]
            public string Icon { get; set; }

        }

        //HTTP GET リクエスト
        public async Task OnGetAsync(string returnUrl = null)
        {
            //既存のプロジェクトを持ってくるための処理
            ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(int projectSelect, string returnUrl = null)
        {
            var project = await _context.project.FindAsync(projectSelect);
            
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //ここで入れる
                var user = CreateUser();

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Role = Input.Role;

                //以下アイコンを受け取り成功したらuserモデルに入れる処理
                String BeforeConvertIcon = Input.Icon;
                user.icon = EncodeIconToBase64(BeforeConvertIcon);
                Debug.WriteLine("ああああああああああああああああああああああああああああああああああ");
                Debug.WriteLine($"user.icon:{user.icon}");

                user.UserProjects = new List<UserProject>();
                UserProject userProject = new UserProject() { 
                ProjectId = project.ProjectId,
                UserId = user.Id,
                };

                user.UserProjects.Add(userProject);

                await _userManager.AddToRoleAsync(user, Input.Role);
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                //DB に入れる処理(if文が成功したときのみ)
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        //return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        TempData["AlertUser"] = "新しいユーザーを追加しました";
                        return Redirect("/Home/Index");
                    }
                    else
                    {
                        //await _signInManager.SignInAsync(user, isPersistent: false);
                        //return LocalRedirect(returnUrl);
                        TempData["AlertUser"] = "新しいユーザーを追加しました";
                        return Redirect("/Home/Index");
                    }
                }
                foreach (var error in result.Errors)
                {
                    //ModelState.AddModelError(string.Empty, error.Description);
                    ModelState.AddModelError(string.Empty, "ユーザーの追加に失敗しました。パスワードなどの入力情報が正しいかどうか確認し、もう一度お試しください。");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["Projects"] = new SelectList(_context.project, "ProjectId", "Name");
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        public static  String EncodeIconToBase64(String instance)
        {
            if(instance == null)
            {
                
                return "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";
            }
            else {
                //以下入力された場合の変換
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(instance));
                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        builder.Append(hashBytes[i].ToString("x2"));
                    }

                    return builder.ToString();
                }
            }      
        }
    }
}
