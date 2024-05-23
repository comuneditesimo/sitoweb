using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Provider
{
    public class NEWSProvider : INEWSProvider
    {
        private IUnitOfWork _unitOfWork;

        public NEWSProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<NEWS_Article?> GetArticle(Guid ID)
        {
            return await _unitOfWork.Repository<NEWS_Article>().GetByIDAsync(ID);
        }
        public async Task<List<NEWS_Article>?> GetArticleList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int? Amount = null)
        {
            if (Amount != null)
                return await _unitOfWork.Repository<NEWS_Article>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.DeletedAt == null && p.Enabled == true &&
                                                                               p.LANG_Languages_ID == LANG_Language_ID).AsNoTracking().OrderByDescending(p => p.PublishingDate).Take(Amount.Value).ToListAsync();

            return await _unitOfWork.Repository<NEWS_Article>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.DeletedAt == null && p.Enabled == true &&
                                                                      p.LANG_Languages_ID == LANG_Language_ID).AsNoTracking().OrderByDescending(p => p.PublishingDate).ToListAsync();
        }
        public async Task<List<NEWS_Article>?> GetArticleList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, string Type)
        {
            return await _unitOfWork.Repository<NEWS_Article>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.DeletedAt == null && 
                                                                           p.LANG_Languages_ID == LANG_Language_ID && p.InputType == Type).AsNoTracking().ToListAsync();
        }
        public async Task<List<NEWS_Article>?> GetArticleList(Guid FamilyID)
        {
            return await _unitOfWork.Repository<NEWS_Article>().Where(p => p.FamilyID == FamilyID).AsNoTracking().ToListAsync();
        }
        public async Task<List<V_NEWS_Article>?> GetViewArticleList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, string Type)
        {
            return await _unitOfWork.Repository<V_NEWS_Article>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.DeletedAt == null &&
                                                                             p.LANG_Languages_ID == LANG_Language_ID && p.InputType == Type).AsNoTracking().ToListAsync();
        }
        public async Task<List<NEWS_RSS_Config>?> GetRSSConfig(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<NEWS_RSS_Config>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).AsNoTracking().ToListAsync();

        }
        public async Task<bool> RemoveArticle(NEWS_Article Data)
        {
            return await _unitOfWork.Repository<NEWS_Article>().DeleteAsync(Data);
        }
        public async Task<NEWS_Article?> SetArticle(NEWS_Article Data)
        {
            return await _unitOfWork.Repository<NEWS_Article>().InsertOrUpdateAsync(Data);
        }
        public async Task<NEWS_Log?> SetLog(NEWS_Log Data)
        {
            return await _unitOfWork.Repository<NEWS_Log>().InsertOrUpdateAsync(Data);
        }
        public async Task<NEWS_RSS_Config?> SetRSSConfig(NEWS_RSS_Config Data)
        {
            return await _unitOfWork.Repository<NEWS_RSS_Config>().InsertOrUpdateAsync(Data);
        }
        public async Task<NEWS_Article_Image?> GetArticleImage(Guid FamilyID)
        {
            return await _unitOfWork.Repository<NEWS_Article_Image>().FirstOrDefaultAsync(p => p.NEWS_Family_ID == FamilyID);
        }
        public NEWS_Article_Image? GetArticleImageSync(Guid FamilyID)
        {
            return _unitOfWork.Repository<NEWS_Article_Image>().FirstOrDefault(p => p.NEWS_Family_ID == FamilyID);
        }
        public async Task<NEWS_Article_Image?> SetArticleImage(NEWS_Article_Image Data)
        {
            return await _unitOfWork.Repository<NEWS_Article_Image>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveArticleImage(NEWS_Article_Image Data)
        {
            return await _unitOfWork.Repository<NEWS_Article_Image>().DeleteAsync(Data);
        }
        public async Task<List<NEWS_Article_Ressource>?> GetArticleRessource(Guid NEWS_Article_ID)
        {
            return await _unitOfWork.Repository<NEWS_Article_Ressource>().Where(p => p.NEWS_Article_ID == NEWS_Article_ID).ToListAsync();
        }
        public async Task<NEWS_Article_Ressource?> SetArticleRessource(NEWS_Article_Ressource Data)
        {
            return await _unitOfWork.Repository<NEWS_Article_Ressource>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveArticleRessource(NEWS_Article_Ressource Data)
        {
            return await _unitOfWork.Repository<NEWS_Article_Ressource>().DeleteAsync(Data);
        }
    }
}
