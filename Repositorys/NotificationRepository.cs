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
using NuGet.ContentModel;


namespace ReportSystem.Repositorys
{
    public class NotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SaveNotification(string UserID_ReportID, string formattedDate)
        {
            var notification = new Notification
            {
                UserId = UserID_ReportID,
                DateReport = formattedDate,
                IsSent = true,
                IsDisplayed = false
            };

            _context.Notifications.Add(notification);
            _context.SaveChangesAsync();
        }


        public List<Notification> GetNotifications(string UserID_ReportID,bool IsSent, bool IsDisplayed)
        {
            return _context.Notifications
                .Where(n => n.UserId == UserID_ReportID && n.IsSent == IsSent && n.IsDisplayed == IsDisplayed)
                .ToList();
        }


        public void MarkAsDisplayed(Notification notificationRe)
        {
            notificationRe.IsDisplayed = true;
            _context.SaveChangesAsync();
        }

        public Notification GetNotificationByMessageAndUserId(string UserID_ReportID, string formattedDate)
        {
            return   _context.Notifications.FirstOrDefault(n => n.DateReport == formattedDate && n.UserId == UserID_ReportID);

        }
    }

}
