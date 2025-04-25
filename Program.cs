using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using inventio.Middleware;
using Inventio.Data;
using Inventio.Models;
using inventio.Token;
using inventio.Repositories.Users;
using inventio.Repositories.Roles;
using inventio.Services.Email;
using inventio.Repositories.Dashboards.Scrap;
using inventio.Repositories.EntityFramework;
using inventio.Repositories.Dashboards.ChangeOver;
using inventio.Repositories.Dashboards.PackageSku;
using inventio.Repositories.Dashboards.DowntimeSku;
using inventio.Repositories.Dashboards.StatisticalChangeOver;
using inventio.Repositories.Dashboards.GeneralDowntime;
using inventio.Repositories.MenuRepo;
using inventio.Repositories.Dashboards.DailySummary;
using inventio.Repositories.Rights;
using inventio.Repositories.Dashboards.SupervisorMetrics;
using inventio.Repositories.Dashboards.CasesAnalysis;
using inventio.Repositories.Dashboards.EfficiencyAnalysis;
using inventio.Repositories.Dashboards.DowntimeXSubCat;
using inventio.Repositories.Dashboards.TrendAnalysisRepository;
using inventio.Repositories.Records.Production;
using Inventio.Repositories.Records.Production;
using inventio.Repositories.Dashboards.LinePerformance;
using inventio.Repositories.ShiftProductionReport;
using inventio.Repositories.Reports.SummaryReport;
using inventio.Repositories.Dashboards.PlantPerformance;
using Inventio.Repositories.Security;
using Inventio.Repositories.Settings.HoursSetting;
using Inventio.Repositories.Settings.LineSetting;
using Inventio.Repositories;
using Inventio.Repositories.Settings.ObjetiveSetting;
using Inventio.Repositories.Form;
using inventio.Repositories.Settings.SubCategory1;
using Inventio.Repositories.Formulas;
using Inventio.Utils.Formulas;
using inventio.Repositories.Dashboards.Utilization;
using inventio.Repositories.Dashboards.NonConformanceAnalysis;
using Hangfire;
using Hangfire.SqlServer;


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

var url = builder.Configuration.GetSection("URLs").GetSection("FrontURL").Value ?? "http://localhost:5173";

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }
    ));

// Add Hangfire server
builder.Services.AddHangfireServer();


builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

//Builder Secutiry
var builderSecurity = builder.Services.AddIdentityCore<User>();
var identityBuilder = new IdentityBuilder(builderSecurity.UserType, typeof(Role), builder.Services);
identityBuilder.AddEntityFrameworkStores<ApplicationDBContext>();
identityBuilder.AddSignInManager<SignInManager<User>>();
identityBuilder.AddRoles<Role>();

//Scopes
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IUserSession, UserSession>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped(typeof(IEFRepository<>), typeof(EFRepository<>));
builder.Services.AddScoped<IScrapRepository, ScrapRepository>();
builder.Services.AddScoped<IChangeOverRepository, ChangeOverRepository>();
builder.Services.AddScoped<IDowntimeSkuRepository, DowntimeSkuRepository>();
builder.Services.AddScoped<IPackageSkuRepository, PackageSkuRepository>();
builder.Services.AddScoped<IStatisticalChangeOverRepository, StatisticalChangeOverRepository>();
builder.Services.AddScoped<IGeneralDowntimeRepository, GeneralDowntimeRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuRightsRepository, MenuRightsRepository>();
builder.Services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<ICasesAnalysisRepository, CasesAnalysisRepository>();
builder.Services.AddScoped<INonConformanceAnalysisRepository, NonConformanceAnalysisRepository>();
builder.Services.AddScoped<IEfficiencyRepository, EfficiencyRepository>();
builder.Services.AddScoped<IDowntimeXSubcatRepository, DowntimeXSubcatRepository>();
builder.Services.AddScoped<ITrendAnalysisRepository, TrendAnalysisRepository>();
builder.Services.AddScoped<IProductionRepository, ProductionRepository>();
builder.Services.AddScoped<ILinePerformanceRepository, LinePerformanceRepository>();
builder.Services.AddScoped<IShiftProductionReportRepository, ShiftProductionReportRepository>();
builder.Services.AddScoped<ISummaryReport, SummaryReport>();
builder.Services.AddScoped<IPlantPerformanceRepository, PlantPerformanceRepository>();
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
builder.Services.AddScoped<IHoursSettingRepository, HoursSettingRepository>();
builder.Services.AddScoped<ILineSettingRepository, LineSettingRepository>();
builder.Services.AddScoped<IObjectiveSettingRepository, ObjectiveSettingRepository>();
builder.Services.AddScoped<IFormRepository, FormRepository>();
builder.Services.AddScoped<ISubCategory1Repository, SubCategory1Repository>();
builder.Services.AddScoped<IGeneralEfficiencyRepository, GeneralEfficiencyRepository>();
builder.Services.AddScoped<EfficiencyFormula>();
builder.Services.AddScoped<IUtilizationRepository, UtilizationRepository>();
builder.Services.AddScoped<UtilizationFormula>();


var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jz5ing^WNh42hy9ZUY^wNXk2HjMQoQmZ!G9HdB8BL*p%9xo@9z&2hWqfx!KqCTt%"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateAudience = false,
        ValidateIssuer = false
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(url).AllowAnyHeader().AllowAnyMethod();
                      });
});

// APP created
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ManagerMiddleware>();
app.UseAuthentication();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

// Configure Hangfire dashboard
app.UseHangfireDashboard();

// Recurring Jobs
app.StartRecurringJobs();

app.Run();
