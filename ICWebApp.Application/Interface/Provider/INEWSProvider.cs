using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface INEWSProvider
    {
        public Task<List<NEWS_RSS_Config>?> GetRSSConfig(Guid AUTH_Municipality_ID);
        public Task<NEWS_RSS_Config?> SetRSSConfig(NEWS_RSS_Config Data);
        public Task<NEWS_Article?> GetArticle(Guid ID);
        public Task<List<NEWS_Article>?> GetArticleList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int? Amount = null);
        public Task<List<NEWS_Article>?> GetArticleList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, string Type);
        public Task<List<NEWS_Article>?> GetArticleList(Guid FamilyID);
        public Task<List<V_NEWS_Article>?> GetViewArticleList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, string Type);
        public Task<NEWS_Article?> SetArticle(NEWS_Article Data);
        public Task<bool> RemoveArticle(NEWS_Article Data);
        public Task<NEWS_Log?> SetLog(NEWS_Log Data);
        public Task<NEWS_Article_Image?> GetArticleImage(Guid FamilyID);
        public NEWS_Article_Image? GetArticleImageSync(Guid FamilyID);
        public Task<NEWS_Article_Image?> SetArticleImage(NEWS_Article_Image Data);
        public Task<bool> RemoveArticleImage(NEWS_Article_Image Data);
        public Task<List<NEWS_Article_Ressource>?> GetArticleRessource(Guid NEWS_Article_ID);
        public Task<NEWS_Article_Ressource?> SetArticleRessource(NEWS_Article_Ressource Data);
        public Task<bool> RemoveArticleRessource(NEWS_Article_Ressource Data);
    }
}
