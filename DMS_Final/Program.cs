using DMS_Final.Repositories;
using DMS_Final.Repository;
using DMS_Final.Repository.Admin;
using DMS_Final.Repository.Document;
//using DMS_Final.Repository.Notification;
using DMS_Final.Repository.User;
using DMS_Final.Services;
using DMS_Final.Services.Admin;
using DMS_Final.Services.User;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// User services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Admin services
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Document services
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Audit
builder.Services.AddScoped<IDocumentStatusHistoryRepository, DocumentStatusHistoryRepository>();
builder.Services.AddScoped<IDocumentStatusHistoryService, DocumentStatusHistoryService>();

// Notifications
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();