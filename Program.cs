//using DMS_Final.Repository;
//using DMS_Final.Repository.Admin;
//using DMS_Final.Repository.Document;
//using DMS_Final.Repository.DocumentTags;
//using DMS_Final.Repository.Tags;
//using DMS_Final.Repository.User;
//using DMS_Final.Services;
//using DMS_Final.Services.Admin;
//using DMS_Final.Services.Document;
//using DMS_Final.Services.User;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddControllersWithViews();

//// User services
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IUserService, UserService>();

//// Admin services
//builder.Services.AddScoped<IAdminRepository, AdminRepository>();
//builder.Services.AddScoped<IAdminService, AdminService>();

//// Document services
//builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
//builder.Services.AddScoped<IDocumentService, DocumentService>();

//// DocumentHistory services
//builder.Services.AddScoped<IDocumentHistoryRepository, DocumentHistoryRepository>();
//builder.Services.AddScoped<IDocumentHistoryService, DocumentHistoryService>();

//// Tags services
//builder.Services.AddScoped<ITagsRepository, TagsRepository>();

//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//builder.Services.AddHttpContextAccessor();

//var app = builder.Build();

//app.UseStaticFiles();
//app.UseRouting();
//app.UseSession();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=User}/{action=Login}/{id?}");

//app.Run();







var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddControllers();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs",
        policy => policy
            .WithOrigins("http://localhost:3000") // Next.js dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // ?? allow cookies
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowNextJs"); // ?? enable CORS for frontend
app.UseSession();
app.UseAuthorization();

app.MapControllers(); // ?? enable API controllers

app.Run();
