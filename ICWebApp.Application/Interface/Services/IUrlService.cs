using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Interface.Services;

public interface IUrlService
{
    public Task<AUTH_Comunix_Urls?> GetMunicipalityByUrl(string url);
    public Task<string> GetDefaultUrlForMunicipality(Guid municipalityId, Guid? langId = null);
}