using ICWebApp.Application.Cache;
using ICWebApp.Application.Interface.Cache;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Services;

public class UrlService : IUrlService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAUTHProviderCache _authProviderCache;
        
    public UrlService(IUnitOfWork unitOfWork, IAUTHProviderCache _authProviderCache)
    {
        this._unitOfWork = unitOfWork;
        this._authProviderCache = _authProviderCache; 
    }

    private async Task FetchConfig()
    {
        _authProviderCache.SetUrls(await _unitOfWork.Repository<AUTH_Comunix_Urls>().ToListAsync());        
    }

    public async Task<string> GetDefaultUrlForMunicipality(Guid municipalityId, Guid? langId = null)
    {
        if (!_authProviderCache.GetUrls().Any())
            await FetchConfig();

        if (langId != null)
        {
            var matches = _authProviderCache.GetUrls().Where(e =>
                e.AUTH_Municipality_ID == municipalityId && e.DefaultLanguage == langId).ToList();
            
            if (matches.Any(e => e.IsDefault))
                return matches.FirstOrDefault(e => e.IsDefault)!.Url; 
            
            if (matches.Any())
                return matches.FirstOrDefault()!.Url;
        }

        return _authProviderCache.GetUrls().FirstOrDefault(e => e.AUTH_Municipality_ID == municipalityId && e.IsDefault)?.Url ??
               _authProviderCache.GetUrls().FirstOrDefault(e => e.AUTH_Municipality_ID == municipalityId)?.Url ?? "";

    }
    public async Task<AUTH_Comunix_Urls?> GetMunicipalityByUrl(string url)
    {
        if (url.ToLower().Contains("spid"))
            return null;
        
        if (!_authProviderCache.GetUrls().Any())
            await FetchConfig();
        
        if (url.ToLower().Contains("localhost") || url.Contains("192.168"))
        {
            return GetForLocalHost();
        }
        
        var cmxUrl = _authProviderCache.GetUrls().FirstOrDefault(e => url.ToLower().Contains(e.Url.ToLower()));
        return cmxUrl;
    }

    private AUTH_Comunix_Urls GetForLocalHost()
    {
        var cmxUrl = new AUTH_Comunix_Urls()
        {
            ID = Guid.NewGuid(),
            Url = "localhost",
            AUTH_Municipality_ID = ComunixSettings.TestMunicipalityID,
            DefaultLanguage = LanguageSettings.German
        };
        return cmxUrl;
    }

    public static async Task<string> GetDefaultUrlByMunicipalityStatic(IUnitOfWork unitOfWork, Guid munId, Guid? langId = null)
    {
        var urlConfigStatic  = await unitOfWork.Repository<AUTH_Comunix_Urls>().ToListAsync();

        if (langId != null)
        {
            var matches = urlConfigStatic.Where(e =>
                e.AUTH_Municipality_ID == munId && e.DefaultLanguage == langId).ToList();
            
            if (matches.Any(e => e.IsDefault))
                return matches.FirstOrDefault(e => e.IsDefault)!.Url; 
            
            if (matches.Any())
                return matches.FirstOrDefault()!.Url;
        }

        return urlConfigStatic.FirstOrDefault(e => e.AUTH_Municipality_ID == munId && e.IsDefault)?.Url ??
               urlConfigStatic.FirstOrDefault(e => e.AUTH_Municipality_ID == munId)?.Url ?? "";
    }
}