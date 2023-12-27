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
using ReportSystem.Controllers;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using ReportSystem.Hubs;
using System;
using ReportSystem.Repositorys;
using Microsoft.Extensions.Logging;

namespace ReportSystem.Hubs
{

    public class MemberNotificationHub : Hub
    {

        private readonly NotificationRepository _notificationRepository;

        public MemberNotificationHub(NotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task SendMemberNotification(string UserID_ReportID, string formattedDate)
        {
            await Clients.All.SendAsync("ReceiveMemberNotification", UserID_ReportID, formattedDate);
        }

        public async Task NotificationDisplayed(string UserID_ReportID, string formattedDate)
        {
            var notificationRe = _notificationRepository.GetNotificationByMessageAndUserId(UserID_ReportID, formattedDate);

            if (notificationRe != null)
            {
                _notificationRepository.MarkAsDisplayed(notificationRe);
            }
            //await Clients.All.SendAsync("NotificationConfirmed", formattedDate);
        }
    }

}