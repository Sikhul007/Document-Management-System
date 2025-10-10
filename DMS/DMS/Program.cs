using DMS.Repository.Pagination;
using DMS.Services.Pagination.DocumentStatusHistory;
using DMS.Repositories;
using DMS.Repository;
using DMS.Repository.Admin;
using DMS.Repository.Document;
using DMS.Repository.User;
using DMS.Services;
using DMS.Services.Admin;
using DMS.Services.User;

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


builder.Services.AddScoped<IGenericPaginationRepository, GenericPaginationRepository>();

// Register individual pagination service for DocumentStatusHistory
builder.Services.AddScoped<IDocumentStatusHistoryPaginationService, DocumentStatusHistoryPaginationService>();

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