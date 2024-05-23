using Blazored.LocalStorage;
using Blazored.SessionStorage;
using ICWebApp.Application.Cache;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface;
using ICWebApp.Application.Interface.Cache;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Sessionless;
using ICWebApp.Application.Settings;
using ICWebApp.Classes.Syncfusion;
using ICWebApp.Classes.Telerik;
using ICWebApp.DataStore.MSSQL;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.DataStore.MSSQL.Repositories;
using ICWebApp.DataStore.MSSQL.UnitOfWork;
using ICWebApp.DataStore.PagoPA.Interface;
using ICWebApp.DataStore.PagoPA.Repository;
using ICWebApp.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
using System.Globalization;
using Telerik.Blazor.Services;
using ICWebApp.Components;
using System.IO.Compression;
using ICWebApp.Application.Cache.Homepage;
using ICWebApp.Application.Interface.Cache.Homepage;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTelerikBlazor();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddLocalization();

builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
builder.Services.AddSyncfusionBlazor();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddSignalR();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorizationCore(config =>
{
    config.AddPolicy("Backend", policy =>
        policy.Requirements.Add(new AuthorizationRoleHelper( new List<Guid>() { AuthRoles.Employee, AuthRoles.Committee, AuthRoles.Administrator})));
    config.AddPolicy("Citizen", policy =>
        policy.Requirements.Add(new AuthorizationRoleHelper(new List<Guid>() { AuthRoles.Citizen })));
});

//CORS
builder.Services.AddCors(corsOption => corsOption.AddDefaultPolicy(
  corsBuilder =>
  {
      corsBuilder.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader(); 
  }));

builder.Services.Configure<RequestLocalizationOptions>(options => {
    var supportedCultures = new List<CultureInfo>()
        {
            new CultureInfo("it-IT"),
            new CultureInfo("de-DE"),
            new CultureInfo("la-IT")
        };

    options.DefaultRequestCulture = new RequestCulture("it-IT");

    options.ApplyCurrentCultureToResponseHeaders = true;
	options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
}); 

//DBConnection
builder.Services.AddSingleton<DBContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

//Cache
builder.Services.AddSingleton<ITEXTProviderCache, TEXTProviderCache>();
builder.Services.AddSingleton<ILANGProviderCache, LANGProviderCache>();
builder.Services.AddSingleton<IAUTHProviderCache, AUTHProviderCache>();
builder.Services.AddSingleton<IAPPProviderCache, APPProviderCache>();
builder.Services.AddSingleton<IServiceHelperCache, ServiceHelperCache>();

//Services
builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<ISessionWrapper, SessionWrapper>();
builder.Services.AddScoped<IDialogService, DialogService>();
builder.Services.AddScoped<IBusyIndicatorService, BusyIndicatorService>();
builder.Services.AddScoped<IAccountService, ICWebApp.Application.Services.AccountService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMailerService, MailerService>();
builder.Services.AddScoped<ISMSService, SMSService>();
builder.Services.AddScoped<IPushService, PushService>();
builder.Services.AddScoped<IEnviromentService, EnviromentService>();
builder.Services.AddScoped<IBreadCrumbService, BreadCrumbService>();
builder.Services.AddScoped<IAnchorService, AnchorService>();
builder.Services.AddScoped<IVeriffService, VeriffService>();
builder.Services.AddScoped<ISignService, SignService>();
builder.Services.AddScoped<ISignResponseService, SignResponseService>();
builder.Services.AddScoped<IGeoService, GeoService>();
builder.Services.AddScoped<IPAYService, PAYService>();
builder.Services.AddScoped<IFreshDeskService, FreshDeskService>();
builder.Services.AddScoped<INEWSService, NEWSService>();
builder.Services.AddScoped<ITASKService, TASKService>();
builder.Services.AddScoped<ISPIDService, SPIDService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMyCivisService, MyCivisService>();
builder.Services.AddScoped<IActionBarService, ActionBarService>();
builder.Services.AddScoped<IFormApplicationService, FormApplicationService>();

//Repositories
builder.Services.AddScoped<IPagoPaRepository, PagoPaRepository>();

//Provider
builder.Services.AddScoped<IAUTHProvider, AUTHProvider>();
builder.Services.AddScoped<IAUTHSettingsProvider, AUTHSettingsProvider>();
builder.Services.AddScoped<IMSGProvider, MSGProvider>();
builder.Services.AddScoped<ILANGProvider, LANGProvider>();
builder.Services.AddScoped<ITEXTProvider, TEXTProvider>();
builder.Services.AddScoped<ICONFProvider, CONFProvider>();
builder.Services.AddScoped<ICANTEENProvider, CANTEENProvider>();
builder.Services.AddScoped<ISYSProvider, SYSProvider>();
builder.Services.AddScoped<IFORMDefinitionProvider, FORMDefinitionProvider>();
builder.Services.AddScoped<IFORMApplicationProvider, FORMApplicationProvider>();
builder.Services.AddScoped<INEWSProvider, NEWSProvider>();
builder.Services.AddScoped<IFILEProvider, FILEProvider>();
builder.Services.AddScoped<IRoomProvider, RoomProvider>();
builder.Services.AddScoped<IINFO_PAGEProvider, INFO_PAGEProvider>();
builder.Services.AddScoped<ISETTProvider, SETTProvider>();
builder.Services.AddScoped<IPAYProvider, PAYProvider>();
builder.Services.AddScoped<IDASHProvider, DASHProvider>();
builder.Services.AddScoped<IPRIVProvider, PRIVProvider>();
builder.Services.AddScoped<IORGProvider, ORGProvider>();
builder.Services.AddScoped<IAPPProvider, APPProvider>();
builder.Services.AddScoped<IMETAProvider, METAProvider>();
builder.Services.AddScoped<ITASKProvider, TASKProvider>();
builder.Services.AddScoped<ICONTProvider, CONTProvider>();
builder.Services.AddScoped<IChatProvider, ChatProvider>();
builder.Services.AddScoped<IHOMEProvider, HomeProvider>();

//Sessionles
builder.Services.AddScoped<IFORMApplicationSessionless, FORMApplicationSessionless>();
builder.Services.AddScoped<ISignResponseSessionless, SignResponseSessionless>();
builder.Services.AddScoped<ICONFProviderSessionless, CONFProviderSessionless>();


//Helper
builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationHelper>();
builder.Services.AddScoped<IAuthorizationHandler, AuthorizationRoleHelperHandler>();
builder.Services.AddScoped<IFORM_ReportRendererHelper, FORM_ReportRendererHelper>();
builder.Services.AddScoped<IFORM_ReportPrintHelper, FORM_ReportPrintHelper>();
builder.Services.AddScoped<IFormBuilderHelper, FormBuilderHelper>();
builder.Services.AddScoped<IFormAdministrationHelper, FormAdministrationHelper>();
builder.Services.AddScoped<ICanteenAdministrationHelper, CanteenAdministrationHelper>();
builder.Services.AddScoped<ICanteenRequestsAdministrationHelper, CanteenRequestsAdministrationHelper>();
builder.Services.AddScoped<ICanteenRequestCardListHelper, CanteenRequestCardListHelper>();
builder.Services.AddScoped<IRoomAdministrationHelper, RoomAdministrationHelper>();
builder.Services.AddScoped<ISpidHelper, SpidHelper>();
builder.Services.AddScoped<IFormRendererHelper, FormRendererHelper>();
builder.Services.AddScoped<INavMenuHelper, NavMenuHelper>();
builder.Services.AddScoped<IRoomGalerieHelper, RoomGalerieHelper>();
builder.Services.AddScoped<IRequestAdministrationHelper, RequestAdministrationHelper>();
builder.Services.AddScoped<ICreateFormDefinitionHelper, CreateFormDefinitionHelper>();
builder.Services.AddScoped<IRoomBookingHelper, RoomBookingHelper>();
builder.Services.AddScoped<ID3Helper, D3Helper>();
builder.Services.AddScoped<IMunicipalityHelper, MunicipalityHelper>();
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IContactHelper, ContactHelper>();
builder.Services.AddScoped<ICalendarHelper, CalendarHelper>();
builder.Services.AddScoped<IHPServiceHelper, HPServiceHelper>();
builder.Services.AddScoped<ICookieHelper, CookieHelper>();
builder.Services.AddScoped<IThemeHelper, ThemeHelper>();

//SF Dialog
builder.Services.AddScoped<SfDialogService>();

//Telerik Localization
builder.Services.AddScoped(typeof(ITelerikStringLocalizer), typeof(TelerikResxLocalizer));
builder.Services.AddScoped(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 10240; // 1MB
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});
builder.Services.Configure<StaticFileOptions>(opts =>
{
    opts.OnPrepareResponse = r =>
    {
        string path = r.File.PhysicalPath;
        if (path.EndsWith(".gif") || path.EndsWith(".webp") || path.EndsWith(".bmp") || path.EndsWith(".jpeg") || path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".svg") || path.EndsWith(".js") || path.EndsWith(".css") || path.EndsWith(".woff") || path.EndsWith(".woff2") || path.EndsWith(".ttf"))
        {
            TimeSpan maxAge = new TimeSpan(365, 0, 0, 0);
            r.Context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString("0"));
        }
    };
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseExceptionHandler("/Spid/SpidError");
app.UseWebSockets();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCookiePolicy();
app.UseCors();

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStatusCodePagesWithRedirects("/404");

app.UseStaticFiles();

if (!Directory.Exists(@"D:/Comunix/NewsImages"))
{
    Directory.CreateDirectory(@"D:/Comunix/NewsImages");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/NewsImages")),
    RequestPath = "/NewsImages"
});
if (!Directory.Exists(@"D:/Comunix/ImagesCache"))
{
    Directory.CreateDirectory(@"D:/Comunix/ImagesCache");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/ImagesCache")),
    RequestPath = "/Cache"
});
if (!Directory.Exists(@"D:/Comunix/DocumentCache"))
{
    Directory.CreateDirectory(@"D:/Comunix/DocumentCache");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/DocumentCache")),
    RequestPath = "/DocumentCache"
});
if (!Directory.Exists(@"D:/Comunix/Privacy"))
{
    Directory.CreateDirectory(@"D:/Comunix/Privacy");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/Privacy")),
    RequestPath = "/Privacy"
});

if (!Directory.Exists("D:/Comunix/Calendar"))
{
    Directory.CreateDirectory("D:/Comunix/Calendar");
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/Calendar")),
    RequestPath = "/Calendar"
});

if (!Directory.Exists("D:/Comunix/Contacts"))
{
    Directory.CreateDirectory("D:/Comunix/Contacts");
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/Contacts")),
    RequestPath = "/Contacts"
});

if (!Directory.Exists(@"D:/Comunix/RoomImages"))
{
    Directory.CreateDirectory(@"D:/Comunix/RoomImages");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/RoomImages")),
    RequestPath = "/RoomImages"
});

if (!Directory.Exists(@"D:/Comunix/Sign"))
{
    Directory.CreateDirectory(@"D:/Comunix/Sign");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/Sign" )),
        RequestPath = "/SignDocs"
});
if (!Directory.Exists(@"D:/Comunix/MailContent"))
{
    Directory.CreateDirectory(@"D:/Comunix/MailContent");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/MailContent")),
    RequestPath = "/Content"
});
if (!Directory.Exists(@"D:/Comunix/SFImages"))
{
    Directory.CreateDirectory(@"D:/Comunix/SFImages");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "D:/Comunix/SFImages")),
    RequestPath = "/SFImages"
});

app.UseRouting();

app.UseRequestLocalization(options => {
    var supportedCultures = new List<CultureInfo>()
    { 
        new CultureInfo("it-IT"),
        new CultureInfo("de-DE"),
        new CultureInfo("la-IT")
    };

	options.DefaultRequestCulture = new RequestCulture("it-IT");

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapHub<SignalRChatHub>("/chathub");
app.UseEndpoints(endpoints => { 
    endpoints.MapControllers();
});

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
