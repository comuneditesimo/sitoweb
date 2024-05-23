using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICWebApp.Application.Provider
{
    public class HomeProvider : IHOMEProvider
    {
        private IUnitOfWork _unitOfWork;
        public HomeProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<HOME_Appointment?> GetAppointment(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Appointment?> GetVAppointment(Guid HOME_AppointmentID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment>().FirstOrDefaultAsync(p => p.ID == HOME_AppointmentID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Appointment_Dates>> GetVAppointmentDates(Guid HOME_AppointmentID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment_Dates>().Where(p => p.HOME_AppointmentID == HOME_AppointmentID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<V_HOME_Association?> GetVAssociation(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Association>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<HOME_Appointment_Dates?> GetAppointmentDate(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Dates>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<V_HOME_Appointment>> GetAppointments(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public List<V_HOME_Appointment> GetCurrentMonthEvents(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_HOME_Appointment>().Where(p => p.HOME_Appointment_Type_ID == HOMEAppointmentTypes.Event &&
                                                                           p.AUTH_Municipality_ID == AUTH_Municipality_ID &&
                                                                           p.LANG_Language_ID == LANG_Language_ID &&
                                                                           p.DateFrom >= DateTime.Now
                                                                     ).OrderBy(p => p.DateFrom).Take(3).ToList();
        }
        public async Task<List<V_HOME_Appointment_Dates>> GetAppointmentsWithDates(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment_Dates>().Where(p => p.LANG_Language_ID == LANG_Language_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Appointment>> GetAppointmentsWithDatesByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Appointment_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment>().Where(p => p.LANG_Language_ID == LANG_Language_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.HOME_Appointment_Type_ID == HOME_Appointment_Type_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Association>> GetAssociationByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Association_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Association>().Where(p => p.LANG_Language_ID == LANG_Language_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.HOME_Association_Type_ID == HOME_Association_Type_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Appointment_Dates>> GetAppointmentsByThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Appointment_Theme>().Where(p => p.HOME_Theme_ID == HOME_Theme_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Appointment_Dates>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Appointment_ID).Contains(p.HOME_AppointmentID)).ToListAsync();
        }
        public async Task<List<V_HOME_Venue>> GetVenuesByThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Venue_Theme>().Where(p => p.HOME_Theme_ID == HOME_Theme_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Venue>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Venue_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Venue>> GetVenuesByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Venue_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Venue>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Venue_Type_ID == HOME_Venue_Type_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPersonByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Person_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Person_Type_ID == HOME_Person_Type_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByPerson(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Person_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Person_Document>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByAuthority(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Authority_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Document_Authority>().Where(p => p.AUTH_Authority_ID == HOME_Authority_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Organisation>> GetOrganisationsByPerson(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Person_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Organisation_Person>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Organisation>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Organisation_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Organisation>> GetOrganisationsByDocument(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Document_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Organisation_Document>().Where(p => p.HOME_Document_ID == HOME_Document_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Organisation>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Organisation_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPeopleByDocument(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Document_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Person_Document>().Where(p => p.HOME_Document_ID == HOME_Document_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Venue>> GetVenuesByPerson(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Person_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Venue_Person>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Venue>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Venue_ID).Contains(p.ID) || p.HOME_Person_ID == HOME_Person_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Authority>> GetAuthoritiesByPerson(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Person_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Person_Authority>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Authority>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.AUTH_Authority_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Association>> GetAssociationsByPerson(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Person_ID)
        {
            return new List<V_HOME_Association>();
            //return await _unitOfWork.Repository<V_HOME_Association>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<HOME_Appointment_Dates>> GetAppointment_Dates(Guid HOME_Appointment_ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Dates>().ToListAsync(p => p.HOME_Appointment_ID == HOME_Appointment_ID);
        }
        public async Task<List<HOME_Appointment_Extended>> GetAppointment_Extended(Guid HOME_Appointment_ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Extended>().ToListAsync(p => p.HOME_Appointment_ID == HOME_Appointment_ID);
        }
        public async Task<List<HOME_Appointment_Theme>> GetAppointment_Themes(Guid HOME_Appointment_ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Theme>().ToListAsync(p => p.HOME_Appointment_ID == HOME_Appointment_ID);
        }
        public async Task<List<AUTH_Authority_Theme>> GetAuthority_Theme(Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Theme>().ToListAsync(p => p.AUTH_Authority_ID == AUTH_Authority_ID);
        }
        public async Task<HOME_Article?> GetArticle(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Article>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Article?> GetVArticle(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Article>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public V_HOME_Article? GetLatestArticle(Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_HOME_Article>().Where(p => p.ReleaseDate <= DateTime.Now && p.LANG_Language_ID == LANG_Language_ID && p.FILE_FileInfo_ID != null).OrderByDescending(p => p.ReleaseDate).FirstOrDefault();
        }
        public async Task<List<V_HOME_Article>> GetArticles(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Article>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Article>> GetArticlesByTheme(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID)
        {
            var themeArticles = _unitOfWork.Repository<HOME_Article_Theme>().Where(p => p.HOME_Theme_ID == HOME_Theme_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Article>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themeArticles.Select(x => x.HOME_Article_ID).Contains(p.ID)).ToListAsync();
        }
        public List<V_HOME_Article_Theme> GetVThemesByArticle(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Article_ID)
        {
            return _unitOfWork.Repository<V_HOME_Article_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.HOME_Article_ID == HOME_Article_ID && p.LANG_Language_ID == LANG_Language_ID).ToList();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByArticle(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Article_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Article_Theme>().Where(p => p.HOME_Article_ID == HOME_Article_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByAssociation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Association_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Association_Theme>().Where(p => p.HOME_Association_ID == HOME_Association_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPeopleByAssociation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Association_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Association_Person>().Where(p => p.HOME_Association_ID == HOME_Association_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByAssociation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Association_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Association_Document>().Where(p => p.HOME_Association_ID == HOME_Association_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByVenue(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Venue_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Venue_Theme>().Where(p => p.HOME_Venue_ID == HOME_Venue_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPeopleByOrganisation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Organisation_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Organisation_Person>().Where(p => p.HOME_Organisation_ID == HOME_Organisation_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByOrganisation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Organisation_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Organisation_Document>().Where(p => p.HOME_Organisation_ID == HOME_Organisation_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByLocation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Location_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Location_Theme>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPeopleByLocation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Location_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Location_Person>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPeopleByFormDefinition(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid FORM_Definition_ID)
        {
            var sourceData = _unitOfWork.Repository<FORM_Definition_Person>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByLocation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Location_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Location_Document>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByOrganisation(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Organisation_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Organisation_Theme>().Where(p => p.HOME_Organisation_ID == HOME_Organisation_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByPNRR(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_PNRR_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_PNRR_Theme>().Where(p => p.HOME_PNRR_ID == HOME_PNRR_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByAppointment(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Appointment_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Appointment_Theme>().Where(p => p.HOME_Appointment_ID == HOME_Appointment_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByAuthority(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid AUTH_Authority_ID)
        {
            var sourceData = _unitOfWork.Repository<AUTH_Authority_Theme>().Where(p => p.AUTH_Authority_ID == AUTH_Authority_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetThemesByDocument(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Document_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Document_Theme>().Where(p => p.HOME_Document_ID == HOME_Document_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document_Type>> GetDocument_TypeByDocument(Guid HOME_Document_ID, Guid LANG_Language_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Document_Document_Type>().Where(p => p.HOME_Document_ID == HOME_Document_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Document_Type_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Article>> GetArticlesByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Article_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Article>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Article_Type_ID == HOME_Article_Type_ID).ToListAsync();
        }
        public async Task<V_HOME_Article_Type> GetArticle_Type(Guid HOME_Article_Type_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Article_Type>().FirstOrDefaultAsync(p => p.ID == HOME_Article_Type_ID && p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Organisation>> GetOrganisationsByTheme(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Organisation_Theme>().Where(p => p.HOME_Theme_ID == HOME_Theme_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Organisation>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Organisation_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByTheme(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Document_Theme>().Where(p => p.HOME_Theme_ID == HOME_Theme_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Organisation>> GetOrganisationsByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Organisation>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LANG_Language_ID && p.HOME_Organisation_Type_ID == HOME_Type_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Location>> GetLocationByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Location_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Location>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Location_Type_ID == HOME_Location_Type_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Document_Type_ID)
        {
            var themes = _unitOfWork.Repository<HOME_Document_Document_Type>().Where(p => p.HOME_Document_Type_ID == HOME_Document_Type_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themes.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<HOME_Article_Extended>> GetArticle_Extended(Guid HOME_Article_ID)
        {
            return await _unitOfWork.Repository<HOME_Article_Extended>().ToListAsync(p => p.HOME_Article_ID == HOME_Article_ID);
        }
        public async Task<List<HOME_Article_Theme>> GetArticle_Themes(Guid HOME_Article_ID)
        {
            return await _unitOfWork.Repository<HOME_Article_Theme>().ToListAsync(p => p.HOME_Article_ID == HOME_Article_ID);
        }
        public async Task<List<HOME_Article_Document>> GetArticle_Documents(Guid HOME_Article_ID)
        {
            return await _unitOfWork.Repository<HOME_Article_Document>().ToListAsync(p => p.HOME_Article_ID == HOME_Article_ID);
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByArticle(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Article_ID)
        {
            var documents = _unitOfWork.Repository<HOME_Article_Document>().Where(p => p.HOME_Article_ID == HOME_Article_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => documents.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocumentsByVenue(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Venue_ID)
        {
            var documents = _unitOfWork.Repository<HOME_Venue_Document>().Where(p => p.HOME_Venue_ID == HOME_Venue_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Document>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => documents.Select(x => x.HOME_Document_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Article_Type>> GetArticle_Types(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Article_Type>().ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
        }
        public List<V_HOME_Article_Type> GetArticle_TypesSync(Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_HOME_Article_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToList();
        }
        public async Task<HOME_Association?> GetAssociation(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Association>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<V_HOME_Association>> GetAssociations(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return await _unitOfWork.Repository<V_HOME_Association>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.Highlight == true);
            }

            return await _unitOfWork.Repository<V_HOME_Association>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<HOME_Association_Extended>> GetAssociation_Extended(Guid HOME_Association_ID)
        {
            return await _unitOfWork.Repository<HOME_Association_Extended>().ToListAsync(p => p.HOME_Association_ID == HOME_Association_ID);
        }
        public async Task<List<V_HOME_Authority>> GetAuthorities(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return await _unitOfWork.Repository<V_HOME_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.Highlight == true);
            }

            return await _unitOfWork.Repository<V_HOME_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Faq>> GetFaqs(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Faq>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<HOME_Faq?> GetFaq(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Faq>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Authority?> GetAuthority(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Authority>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<HOME_Document?> GetDocument(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Document>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Document?> GetVDocument(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Document>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<HOME_Document_Data>?> GetDocumentData(Guid HOME_Document_ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Data>().Where(p => p.HOME_Document_ID == HOME_Document_ID).ToListAsync();
        }
        public async Task<HOME_Document_Data?> GetDocumentDataByLanguage(Guid HOME_Document_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Data>().Where(p => p.HOME_Document_ID == HOME_Document_ID && p.LANG_Language_ID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task<List<V_HOME_Document>> GetDocuments(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return await _unitOfWork.Repository<V_HOME_Document>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.Highlight == true);
            }

            return await _unitOfWork.Repository<V_HOME_Document>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Media>> GetMediaList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Media>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<HOME_Media_Extended>> GetMediaExtended(Guid HOME_Media_ID)
        {
            return await _unitOfWork.Repository<HOME_Media_Extended>().ToListAsync(p => p.HOME_Media_ID == HOME_Media_ID);
        }
        public async Task<HOME_Media_Extended?> SetMediaExtended(HOME_Media_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Media_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Media?> GetMedia(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Media>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<int> GetMediaSortOrder(Guid AUTH_Municipality_ID)
        {
            int order = 0;

            var items = await _unitOfWork.Repository<HOME_Media>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (items != null)
            {
                order = items.Count + 1;
            }

            return order;
        }
        public async Task<int> GetImpressumSortOrder(Guid AUTH_Municipality_ID)
        {
            int order = 0;

            var items = await _unitOfWork.Repository<HOME_Impressum>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (items != null)
            {
                order = items.Count + 1;
            }

            return order;
        }
        public async Task<int> GetPNRRSortOrder(Guid AUTH_Municipality_ID)
        {
            int order = 0;

            var items = await _unitOfWork.Repository<HOME_PNRR>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (items != null)
            {
                order = items.Count + 1;
            }

            return order;

        }
        public async Task<int> GetPNRRChapterSortOrder(Guid HOME_PNRR_ID)
        {
            int order = 0;

            var items = await _unitOfWork.Repository<HOME_PNRR_Chapter>().ToListAsync(p => p.HOME_PNRR_ID == HOME_PNRR_ID);

            if (items != null)
            {
                order = items.Count + 1;
            }

            return order;

        }
        public async Task<int> GetPrivacySortOrder(Guid AUTH_Municipality_ID)
        {
            int order = 0;

            var items = await _unitOfWork.Repository<HOME_Privacy>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if (items != null)
            {
                order = items.Count + 1;
            }

            return order;
        }
        public async Task<HOME_Media?> SetMedia(HOME_Media Data)
        {
            return await _unitOfWork.Repository<HOME_Media>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Document_Document_Type>> GetDocument_Document_Types(Guid HOME_Document_ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Document_Type>().ToListAsync(p => p.HOME_Document_ID == HOME_Document_ID);
        }
        public async Task<List<HOME_Document_Authority>> GetDocument_Authorities(Guid HOME_Document_ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Authority>().ToListAsync(p => p.HOME_Document_ID == HOME_Document_ID);
        }
        public async Task<List<V_HOME_Authority>> GetDocument_Authorities(Guid HOME_Document_ID, Guid LANG_Language_ID)
        {
            var auths = await _unitOfWork.Repository<HOME_Document_Authority>().ToListAsync(p => p.HOME_Document_ID == HOME_Document_ID);

            return await _unitOfWork.Repository<V_HOME_Authority>().Where(p => auths.Select(x => x.AUTH_Authority_ID).Contains(p.ID) && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<HOME_Document_Extended>> GetDocument_Extended(Guid HOME_Document_ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Extended>().ToListAsync(p => p.HOME_Document_ID == HOME_Document_ID);
        }
        public async Task<List<V_HOME_Document_Format>> GetDocument_Formats(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Document_Format>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Document_License>> GetDocument_Licenses(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Document_License>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<HOME_Document_Theme>> GetDocument_Themes(Guid HOME_Document_ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Theme>().ToListAsync(p => p.HOME_Document_ID == HOME_Document_ID);
        }
        public async Task<List<V_HOME_Document_Type>> GetDocument_Types(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Document_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<V_HOME_Document_Type?> GetDocument_Type(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Document_Type>().Where(p => p.ID == ID && p.LANG_LanguagesID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task<List<V_HOME_Municipality>> GetMunicipalities(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipality>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<HOME_Municipality?> GetMunicipality(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Municipality>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Municipality?> GetMunicipality(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipality>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<HOME_Municipality?> GetMunicipalityByAuthMunicipality(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<HOME_Municipality>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<List<HOME_Municipality_Extended>> GetMunicipality_Extended(Guid HOME_Municipality_ID)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Extended>().ToListAsync(p => p.HOME_Municipality_ID == HOME_Municipality_ID);
        }
        public async Task<MAIN_Detail?> GetMaintenanceDetail(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<MAIN_Detail>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<V_MAIN_Detail?> GetVMaintenanceDetail(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_MAIN_Detail>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<MAIN_Detail_Extended>> GetMaintenanceDetail_Extended(Guid MAIN_Detail_ID)
        {
            return await _unitOfWork.Repository<MAIN_Detail_Extended>().ToListAsync(p => p.MAIN_Detail_ID == MAIN_Detail_ID);
        }
        public async Task<List<MAIN_Theme>> GetMaintenance_Themes(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<MAIN_Theme>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<MAIN_Theme?> SetMaintenance_Themes(MAIN_Theme Data)
        {
            return await _unitOfWork.Repository<MAIN_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveMaintenance_Themes(MAIN_Theme Data)
        {
            return await _unitOfWork.Repository<MAIN_Theme>().DeleteAsync(Data);
        }
        public async Task<MAIN_Detail?> SetMaintenanceDetail(MAIN_Detail Data)
        {
            return await _unitOfWork.Repository<MAIN_Detail>().InsertOrUpdateAsync(Data);
        }
        public async Task<MAIN_Detail_Extended?> SetMaintenanceDetailExtended(MAIN_Detail_Extended Data)
        {
            return await _unitOfWork.Repository<MAIN_Detail_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_Municipal_Newsletter>> GetMunicipalNewsletters(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipal_Newsletter>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Municipal_Newsletter>> GetMunicipalNewslettersByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Municipal_Newsletter_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipal_Newsletter>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Municipal_Newsletter_Type_ID == HOME_Municipal_Newsletter_Type_ID);
        }
        public async Task<List<V_HOME_Municipal_Newsletter>> GetMunicipalNewsletters(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, int Amount = 3)
        {
            return await _unitOfWork.Repository<V_HOME_Municipal_Newsletter>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).Take(Amount).ToListAsync();
        }
        public async Task<HOME_Municipal_Newsletter?> GetMunicipal_Newsletter(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Municipal_Newsletter?> GetVMunicipal_Newsletter(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipal_Newsletter>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<HOME_Municipal_Newsletter_Extended>> GetMunicipal_Newsletter_Extended(Guid HOME_Municipal_Newsletter_ID)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Extended>().ToListAsync(p => p.HOME_Municipal_Newsletter_ID == HOME_Municipal_Newsletter_ID);
        }
        public async Task<HOME_Organisation?> GetOrganisation(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Organisation?> GetVOrganisation(Guid ID, Guid LANG_Languages_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Organisation>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Languages_ID == LANG_Languages_ID);
        }
        public async Task<List<V_HOME_Organisation>> GetOrganisations(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return await _unitOfWork.Repository<V_HOME_Organisation>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LANG_Language_ID && p.Highlight == true);
            }

            return await _unitOfWork.Repository<V_HOME_Organisation>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LANG_Language_ID);
        }
        public async Task<HOME_Organisation_Type?> GetOrganisationType(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_Organisation_Extended>> GetOrganisation_Extended(Guid HOME_Organisation_ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Extended>().ToListAsync(p => p.HOME_Organisation_ID == HOME_Organisation_ID);
        }
        public async Task<List<HOME_Organisation_Person>> GetOrganisation_People(Guid HOME_Organisation_ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Person>().ToListAsync(p => p.HOME_Organisation_ID == HOME_Organisation_ID);
        }
        public async Task<List<HOME_Organisation_Theme>> GetOrganisation_Themes(Guid HOME_Organisation_ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Theme>().ToListAsync(p => p.HOME_Organisation_ID == HOME_Organisation_ID);
        }
        public async Task<List<V_HOME_Organisation_Type>> GetOrganisation_Types(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Organisation_Type>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Person_Type>> GetPerson_Types(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Person_Type>().ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<List<HOME_Organisation_Type_Extended>> GetOrganisation_Type_Extended(Guid HOME_Organisation_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type_Extended>().ToListAsync(p => p.HOME_Organisation_Type_ID == HOME_Organisation_Type_ID);
        }
        public async Task<List<V_HOME_Person>> GetPeople(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return await _unitOfWork.Repository<V_HOME_Person>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.Highlight == true);
            }

            return await _unitOfWork.Repository<V_HOME_Person>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Person>> GetPeopleWithTimeslots(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Person>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.AllowTimeslots == true);
        }
        public async Task<List<V_HOME_Person>> GetPeopleByAuthority(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid AUTH_Authority_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Person_Authority>().Where(p => p.AUTH_Authority_ID == AUTH_Authority_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<V_HOME_Person>> GetPeopleByVenue(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Venue_ID)
        {
            var sourceData = _unitOfWork.Repository<HOME_Venue_Person>().Where(p => p.HOME_Venue_ID == HOME_Venue_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Person>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Person_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<HOME_Person?> GetPerson(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_Person_Extended>> GetPerson_Extended(Guid HOME_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Extended>().ToListAsync(p => p.HOME_Person_ID == HOME_Person_ID);
        }
        public async Task<List<HOME_Person_Authority>> GetPerson_Authorities(Guid HOME_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Authority>().ToListAsync(p => p.HOME_Person_ID == HOME_Person_ID);
        }
        public async Task<List<HOME_Person_Document>> GetPerson_Documents(Guid HOME_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Document>().ToListAsync(p => p.HOME_Person_ID == HOME_Person_ID);
        }
        public async Task<HOME_Questionnaire?> GetQuestionnaire(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Questionnaire>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<V_HOME_Questionnaire>> GetQuestionnaires(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Questionnaire>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<HOME_Theme?> GetTheme(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Theme>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Theme?> GetVTheme(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Theme>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<V_HOME_Person?> GetVPerson(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Person>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<V_HOME_Authority?> GetVAuthority(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Authority>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public List<V_HOME_Theme> GetThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.Highlight == true).ToList();
            }

            return _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToList();
        }
        public async Task<HOME_Theme_Type?> GetThemeType(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Theme_Type>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_Theme_Extended>> GetTheme_Extended(Guid HOME_Theme_ID)
        {
            return await _unitOfWork.Repository<HOME_Theme_Extended>().ToListAsync(p => p.HOME_Theme_ID == HOME_Theme_ID);
        }
        public async Task<List<V_HOME_Theme_Type>> GetTheme_Types(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Theme_Type>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<HOME_Theme_Type_Extended>> GetTheme_Type_Extended(Guid HOME_Theme_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Theme_Type_Extended>().ToListAsync(p => p.HOME_Theme_Type_ID == HOME_Theme_Type_ID);
        }
        public async Task<HOME_Venue?> GetVenue(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Venue>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Venue?> GetVVenue(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Venue>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Venue>> GetVenues(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, bool OnlyHighlights = false)
        {
            if (OnlyHighlights)
            {
                return await _unitOfWork.Repository<V_HOME_Venue>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.Highlight == true);
            }

            return await _unitOfWork.Repository<V_HOME_Venue>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<HOME_Venue_Type?> GetVenueType(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_Venue_Document>> GetVenue_Documents(Guid HOME_Venue_ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Document>().ToListAsync(p => p.HOME_Venue_ID == HOME_Venue_ID);
        }
        public async Task<List<HOME_Venue_Extended>> GetVenue_Extended(Guid HOME_Venue_ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Extended>().ToListAsync(p => p.HOME_Venue_ID == HOME_Venue_ID);
        }
        public async Task<List<V_HOME_Venue_Type>> GetVenue_Types(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Venue_Type>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<V_HOME_Location_Type>> GetLocation_Types(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Location_Type>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<List<HOME_Location_Type_Extended>> GetLocation_Type_Extended(Guid HOME_Location_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Type_Extended>().ToListAsync(p => p.HOME_Location_Type_ID == HOME_Location_Type_ID);
        }
        public async Task<HOME_Location_Type?> GetLocationType(Guid HOME_Location_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Type>().FirstOrDefaultAsync(p => p.ID == HOME_Location_Type_ID);
        }
        public async Task<HOME_Location_Type?> SetLocationType(HOME_Location_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Location_Type_Extended?> SetLocationTypeExtended(HOME_Location_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Type_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveLocationType(HOME_Location_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Type>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveLocationType(Guid HOME_Location_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Type>().DeleteAsync(HOME_Location_Type_ID);
        }
        public async Task<List<HOME_Venue_Type_Extended>> GetVenue_Type_Extended(Guid HOME_Venue_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type_Extended>().ToListAsync(p => p.HOME_Venue_Type_ID == HOME_Venue_Type_ID);
        }
        public async Task<List<HOME_Venue_Person>> GetVenue_People(Guid HOME_Venue_ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Person>().ToListAsync(p => p.HOME_Venue_ID == HOME_Venue_ID);
        }
        public async Task<List<HOME_Venue_Theme>> GetVenue_Themes(Guid HOME_Venue_ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Theme>().ToListAsync(p => p.HOME_Venue_ID == HOME_Venue_ID);
        }
        public async Task<bool> RemoveAppointment(HOME_Appointment Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveAppointment(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveAppointmentDate(HOME_Appointment_Dates Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Dates>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveAppointmentDate(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Dates>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveAppointmentExtended(HOME_Appointment_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveAppointmentTheme(HOME_Appointment_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveArticle(HOME_Article Data)
        {
            return await _unitOfWork.Repository<HOME_Article>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveArticle(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Article>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveArticleExtended(HOME_Article_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Article_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveArticleTheme(HOME_Article_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Article_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveAssociation(HOME_Association Data)
        {
            return await _unitOfWork.Repository<HOME_Association>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveAssociation(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Association>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveAssociationExtended(HOME_Association_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocument(HOME_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Document>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocument(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Document>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDocumentData(HOME_Document_Data Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Data>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocumentData(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Data>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDocumentDocumentType(HOME_Document_Document_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Document_Type>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocumentExtended(HOME_Document_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocumentTheme(HOME_Document_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocumentTheme(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Theme>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveMunicipality(HOME_Municipality Data)
        {
            return await _unitOfWork.Repository<HOME_Municipality>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveMunicipalityExtended(HOME_Municipality_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveMunicipalNewsletter(HOME_Municipal_Newsletter Data)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveMunicipalNewsletter(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveMunicipalNewsletterExtended(HOME_Municipal_Newsletter_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveOrganisation(HOME_Organisation Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveOrganisationExtended(HOME_Organisation_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveOrganisationPerson(HOME_Organisation_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Person>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveOrganisationTheme(HOME_Organisation_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenuePerson(HOME_Venue_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Person>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenuePerson(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Person>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveVenueTheme(HOME_Venue_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenueTheme(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Theme>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveOrganisationType(HOME_Organisation_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveOrganisationType(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveOrganisationTypeExtended(HOME_Organisation_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePerson(HOME_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Person>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePerson(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person>().DeleteAsync(ID);
        }
        public async Task<bool> RemovePersonExtended(HOME_Person_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveQuestionnaire(HOME_Questionnaire Data)
        {
            return await _unitOfWork.Repository<HOME_Questionnaire>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveTheme(HOME_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveTheme(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Theme>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveThemeExtended(HOME_Theme_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Theme_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveThemeType(HOME_Theme_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Theme_Type>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveThemeTypeExtended(HOME_Theme_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Theme_Type_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenue(HOME_Venue Data)
        {
            return await _unitOfWork.Repository<HOME_Venue>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenue(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Venue>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveVenueDocument(HOME_Venue_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Document>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenueExtended(HOME_Venue_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenueType(HOME_Venue_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveVenueType(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveVenueTypeExtended(HOME_Venue_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type_Extended>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePersonAuthority(HOME_Person_Authority Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Authority>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePersonAuthority(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Authority>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDocumentAuthority(HOME_Document_Authority Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Authority>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveDocumentAuthority(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Document_Authority>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveArticleDocument(HOME_Article_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Article_Document>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveArticleDocument(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Article_Document>().DeleteAsync(ID);
        }
        public async Task<HOME_Appointment?> SetAppointment(HOME_Appointment Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Appointment_Dates?> SetAppointmentDate(HOME_Appointment_Dates Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Dates>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Appointment_Extended?> SetAppointmentExtended(HOME_Appointment_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Appointment_Theme?> SetAppointmentTheme(HOME_Appointment_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Appointment_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Article?> SetArticle(HOME_Article Data)
        {
            return await _unitOfWork.Repository<HOME_Article>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Article_Extended?> SetArticleExtended(HOME_Article_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Article_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Article_Theme?> SetArticleTheme(HOME_Article_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Article_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Association?> SetAssociation(HOME_Association Data)
        {
            return await _unitOfWork.Repository<HOME_Association>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Association_Extended?> SetAssociationExtended(HOME_Association_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Association_Person_Extended?> SetAssociationPersonExtended(HOME_Association_Person_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Person_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document?> SetDocument(HOME_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Data?> SetDocumentData(HOME_Document_Data Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Data>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Document_Type?> SetDocumentDocumentType(HOME_Document_Document_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Document_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Extended?> SetDocumentExtended(HOME_Document_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Format?> SetDocumentFormat(HOME_Document_Format Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Format>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_License?> SetDocumentLicense(HOME_Document_License Data)
        {
            return await _unitOfWork.Repository<HOME_Document_License>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Theme?> SetDocumentTheme(HOME_Document_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Type?> SetDocumentType(HOME_Document_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Municipality?> SetMunicipality(HOME_Municipality Data)
        {
            return await _unitOfWork.Repository<HOME_Municipality>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Municipality_Extended?> SetMunicipalityExtended(HOME_Municipality_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Municipal_Newsletter?> SetMunicipalNewsletter(HOME_Municipal_Newsletter Data)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Municipal_Newsletter_Extended?> SetMunicipalNewsletterExtended(HOME_Municipal_Newsletter_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Organisation?> SetOrganisation(HOME_Organisation Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Organisation_Extended?> SetOrganisationExtended(HOME_Organisation_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Organisation_Person?> SetOrganisationPerson(HOME_Organisation_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Person>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Organisation_Theme?> SetOrganisationTheme(HOME_Organisation_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Organisation_Type?> SetOrganisationType(HOME_Organisation_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Organisation_Type_Extended?> SetOrganisationTypeExtended(HOME_Organisation_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Type_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue_Person?> SetVenuePerson(HOME_Venue_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Person>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue_Theme?> SetVenueTheme(HOME_Venue_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Person?> SetPerson(HOME_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Person>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Person_Extended?> SetPersonExtended(HOME_Person_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Person_Authority?> SetPersonAuthority(HOME_Person_Authority Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Authority>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Questionnaire?> SetQuestinnaire(HOME_Questionnaire Data)
        {
            return await _unitOfWork.Repository<HOME_Questionnaire>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Theme?> SetTheme(HOME_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Theme_Extended?> SetThemeExtended(HOME_Theme_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Theme_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Theme_Type?> SetThemeType(HOME_Theme_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Theme_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Theme_Type_Extended?> SetThemeTypeExtended(HOME_Theme_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Theme_Type_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue?> SetVenue(HOME_Venue Data)
        {
            return await _unitOfWork.Repository<HOME_Venue>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue_Document?> SetVenueDocument(HOME_Venue_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue_Extended?> SetVenueExtended(HOME_Venue_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue_Type?> SetVenueType(HOME_Venue_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Venue_Type_Extended?> SetVenueTypeExtended(HOME_Venue_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Venue_Type_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Document_Authority?> SetDocumentAuthority(HOME_Document_Authority Data)
        {
            return await _unitOfWork.Repository<HOME_Document_Authority>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Article_Document?> SetArticleDocument(HOME_Article_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Article_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> CheckThemeHighlight(Guid HOME_Theme_ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == HOME_Theme_ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckArticleHighlight(Guid HOME_Article_ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Article>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == HOME_Article_ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckAssociationHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Association>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckAuthorityHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Authority>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckDocumentsHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Document>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckOrganisationHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Organisation>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckPersonHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Person>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckVenueHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Venue>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckLocatinHighlight(Guid ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Location>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.ID == ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<bool> CheckAppointmentHighlight(Guid HOME_Appointment_ID, Guid HOME_Appointment_Type_ID, Guid AUTH_Municipality_ID)
        {
            var higlightcount = await _unitOfWork.Repository<V_HOME_Appointment_Dates>().Where(p => p.Highlight == true && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.HOME_Appointment_Type_ID == HOME_Appointment_Type_ID && p.LANG_Language_ID == LanguageSettings.German).ToListAsync();

            if (higlightcount.Count() < 3 || higlightcount.Where(p => p.HOME_AppointmentID == HOME_Appointment_ID).Any())
            {
                return true;
            }

            return false;
        }
        public async Task<List<V_HOME_Appointment_Type>> GetAppointment_Type(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Appointment_Kategorie>> GetAppointment_Kategories(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Appointment_Kategorie>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public List<V_HOME_Thematic_Sites> GetThematicsites(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_HOME_Thematic_Sites>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToList();
        }
        public async Task<List<V_HOME_Thematic_Sites>> GetThematicsitesByType(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Thematic_Sites_Type_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Thematic_Sites>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Thematic_Sites_Type_ID == HOME_Thematic_Sites_Type_ID).ToListAsync();
        }
        public List<V_HOME_Thematic_Sites> GetThematicsitesByTypeSync(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Thematic_Sites_Type_ID)
        {
            return _unitOfWork.Repository<V_HOME_Thematic_Sites>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.HOME_Thematic_Sites_Type_ID == HOME_Thematic_Sites_Type_ID).ToList();
        }
        public async Task<List<V_HOME_Thematic_Sites_Type>> GetThematicsitesTypes(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Thematic_Sites_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<bool> RemoveThematicsites(HOME_Thematic_Sites Data)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveThematicsites(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites>().DeleteAsync(ID);
        }
        public async Task<HOME_Thematic_Sites?> SetThematicsites(HOME_Thematic_Sites Data)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveAuthorityTheme(AUTH_Authority_Theme Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Theme>().DeleteAsync(Data);
        }
        public async Task<AUTH_Authority_Theme?> SetAuthorityTheme(AUTH_Authority_Theme Data)
        {
            return await _unitOfWork.Repository<AUTH_Authority_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Thematic_Sites_Extended?> SetThematicsite_Extended(HOME_Thematic_Sites_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Thematic_Sites_Document?> SetThematicsite_Document(HOME_Thematic_Sites_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveThematicsite_Document(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites_Document>().DeleteAsync(ID);
        }
        public async Task<List<HOME_Thematic_Sites_Extended>> GetThematicsite_Extended(Guid HOME_Thematicsites_ID)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites_Extended>().Where(p => p.HOME_Thematic_Sites_ID == HOME_Thematicsites_ID).ToListAsync();
        }
        public async Task<List<HOME_Thematic_Sites_Document>> GetThematicsite_Documents(Guid HOME_Thematicsites_ID)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites_Document>().Where(p => p.HOME_Thematic_Sites_ID == HOME_Thematicsites_ID).ToListAsync();
        }
        public async Task<HOME_Thematic_Sites?> GetThematicsite(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Thematic_Sites>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_HOME_Thematic_Sites?> GetVThematicsite(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Thematic_Sites>().FirstOrDefaultAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<bool> RemovePersonDocument(HOME_Person_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Document>().DeleteAsync(Data);
        }
        public async Task<HOME_Person_Document?> SetPersonDocument(HOME_Person_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveOrganisationDocument(HOME_Organisation_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Document>().DeleteAsync(Data);
        }
        public async Task<HOME_Organisation_Document?> SetOrganisationDocument(HOME_Organisation_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Organisation_Document>> GetOrganisation_Documents(Guid HOME_Organisation_ID)
        {
            return await _unitOfWork.Repository<HOME_Organisation_Document>().Where(p => p.HOME_Organisation_ID == HOME_Organisation_ID).ToListAsync();
        }
        public async Task<bool> RemoveAssociationPerson(HOME_Association_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Person>().DeleteAsync(Data);
        }
        public async Task<HOME_Association_Person?> SetAssociationPerson(HOME_Association_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Person>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Association_Person>> GetAssociation_People(Guid HOME_Association_ID)
        {
            return await _unitOfWork.Repository<HOME_Association_Person>().Where(p => p.HOME_Association_ID == HOME_Association_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Association_Person>> GetAssociationVPeople(Guid HOME_Association_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Association_Person>().Where(p => p.HOME_Association_ID == HOME_Association_ID && p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Organisation_Person>> GetOrganisationVPeople(Guid HOME_Organisation_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Organisation_Person>().Where(p => p.HOME_Organisation_ID == HOME_Organisation_ID && p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<bool> RemoveAssociationTheme(HOME_Association_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Theme>().DeleteAsync(Data);
        }
        public async Task<HOME_Association_Theme?> SetAssociationTheme(HOME_Association_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Association_Theme>> GetAssociation_Themes(Guid HOME_Association_ID)
        {
            return await _unitOfWork.Repository<HOME_Association_Theme>().Where(p => p.HOME_Association_ID == HOME_Association_ID).ToListAsync();
        }
        public async Task<bool> RemoveAssociationDocument(HOME_Association_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Document>().DeleteAsync(Data);
        }
        public async Task<HOME_Association_Document?> SetAssociationDocument(HOME_Association_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Association_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Association_Document>> GetAssociation_Documents(Guid HOME_Association_ID)
        {
            return await _unitOfWork.Repository<HOME_Association_Document>().Where(p => p.HOME_Association_ID == HOME_Association_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Association_Type>> GetAssociation_Type(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Association_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_Location_Document?> SetLocationDocument(HOME_Location_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Location_Document>> GetLocation_Documents(Guid HOME_Location_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Document>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToListAsync();
        }
        public async Task<bool> RemoveLocationDocument(HOME_Location_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Document>().DeleteAsync(Data);
        }
        public async Task<HOME_Location_Theme?> SetLocationTheme(HOME_Location_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Location_Theme>> GetLocation_Themes(Guid HOME_Location_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Theme>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToListAsync();
        }
        public async Task<bool> RemoveLocationTheme(HOME_Location_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Theme>().DeleteAsync(Data);
        }
        public async Task<HOME_Location_Person?> SetLocationPerson(HOME_Location_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Person>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Location_Person>> GetLocation_People(Guid HOME_Location_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Person>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToListAsync();
        }
        public async Task<bool> RemoveLocationPerson(HOME_Location_Person Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Person>().DeleteAsync(Data);
        }
        public async Task<HOME_Location_Extended?> SetLocationExtended(HOME_Location_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Location_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Location_Extended>> GetLocation_Extended(Guid HOME_Location_ID)
        {
            return await _unitOfWork.Repository<HOME_Location_Extended>().Where(p => p.HOME_Location_ID == HOME_Location_ID).ToListAsync();
        }
        public async Task<V_HOME_Location?> GetVLocation(Guid HOME_Location_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Location>().Where(p => p.ID == HOME_Location_ID && p.LANG_Language_ID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task<HOME_Location?> GetLocation(Guid HOME_Location_ID)
        {
            return await _unitOfWork.Repository<HOME_Location>().Where(p => p.ID == HOME_Location_ID).FirstOrDefaultAsync();
        }
        public async Task<HOME_Location?> SetLocation(HOME_Location Data)
        {
            return await _unitOfWork.Repository<HOME_Location>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveLocation(HOME_Location Data)
        {
            return await _unitOfWork.Repository<HOME_Location>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveLocation(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Location>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveFaq(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Faq>().DeleteAsync(ID);
        }
        public async Task<HOME_Faq?> SetFaq(HOME_Faq Data)
        {
            return await _unitOfWork.Repository<HOME_Faq>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Faq_Extended>> GetFaq_Extended(Guid HOME_Faq_ID)
        {
            return await _unitOfWork.Repository<HOME_Faq_Extended>().ToListAsync(p => p.HOME_Faq_ID == HOME_Faq_ID);
        }
        public async Task<HOME_Faq_Extended?> SetFaq_Extended(HOME_Faq_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Faq_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_Location_Type>> GetLocation_Type(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Location_Type>().Where(p => p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Location>> GetLocations(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Location>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_Assistance?> SetAssistance(HOME_Assistance Data)
        {
            return await _unitOfWork.Repository<HOME_Assistance>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Assistance?> GetAssistance(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Assistance>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_Assistance>> GetAssistances(Guid AUTH_Municipality_ID, bool Completed = false)
        {
            return await _unitOfWork.Repository<HOME_Assistance>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.Completed == Completed).ToListAsync();
        }
        public async Task<HOME_Request?> GetRequest(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID, DateTime DateFrom)
        {
            return await _unitOfWork.Repository<HOME_Request>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Authority_ID == AUTH_Authority_ID && p.DateFrom == DateFrom);
        }
        public async Task<HOME_Request?> GetRequest(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Request>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<HOME_Request?> SetRequest(HOME_Request Data)
        {
            return await _unitOfWork.Repository<HOME_Request>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_Request>> GetRequests(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Request>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_Person_Request?> GetPersonRequest(Guid AUTH_Municipality_ID, Guid HOME_Person_ID, DateTime DateFrom)
        {
            return await _unitOfWork.Repository<HOME_Person_Request>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.HOME_Person_ID == HOME_Person_ID && p.DateFrom == DateFrom);
        }
        public async Task<HOME_Person_Request?> GetPersonRequest(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Request>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<V_HOME_Person_Request>> GetPersonUserRequest(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Person_Request>().ToListAsync(p => p.AUTH_Users_ID == AUTH_Users_ID);
        }
        public async Task<List<V_HOME_Request>> GetUserRequest(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Request>().ToListAsync(p => p.AUTH_Users_ID == AUTH_Users_ID);
        }
        public async Task<HOME_Person_Request?> SetPersonRequest(HOME_Person_Request Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Request>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_Person_Request>> GetPersonRequests(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Person_Request>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
        }
        public async Task<bool> RemoveRequest(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Request>().DeleteAsync(ID);
        }
        public async Task<bool> RemovePersonRequest(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Request>().DeleteAsync(ID);
        }
        public async Task<List<V_HOME_Theme>> GetThemesByMaintenance(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            var sourceData = _unitOfWork.Repository<MAIN_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToList();

            var data = _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => sourceData.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<HOME_Accessibility?> GetAccessibility(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<HOME_Accessibility>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<V_HOME_Accessibility?> GetVAccessibility(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Accessibility>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
        }
        public async Task<HOME_Accessibility?> SetAccessibility(HOME_Accessibility Data)
        {
            return await _unitOfWork.Repository<HOME_Accessibility>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Accessibility_Extended>> GetAccessibilityExtended(Guid HOME_Accessibility_ID)
        {
            return await _unitOfWork.Repository<HOME_Accessibility_Extended>().Where(p => p.HOME_Accessibility_ID == HOME_Accessibility_ID).ToListAsync();
        }
        public async Task<HOME_Accessibility_Extended?> SetAccessibilityExtended(HOME_Accessibility_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Accessibility_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_Association_Person_Type>> GetAssosiactionPeopleTypes(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Association_Person_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Organisation_Person_Type>> GetOrganisationPeopleTypes(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Organisation_Person_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<HOME_Municipality_Images>> GetMunicipality_Images(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Images>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
        }
        public async Task<HOME_Municipality_Images?> SetMunicipality_Images(HOME_Municipality_Images Data)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Images>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveMunicipality_Images(HOME_Municipality_Images Data)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Images>().DeleteAsync(Data);
        }
        public async Task<bool> RemoveMunicipality_Images(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Municipality_Images>().DeleteAsync(ID);
        }
        public async Task<List<HOME_Person_Dates_Timeslots>> GetPersonTimes(Guid HOME_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Timeslots>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToListAsync();
        }
        public async Task<List<HOME_Person_Office_Hours>> GetPersonOfficeHours(Guid HOME_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Office_Hours>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToListAsync();
        }
        public async Task<List<HOME_Person_Dates_Closed>> GetPersonClosedDates(Guid HOME_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Closed>().Where(p => p.HOME_Person_ID == HOME_Person_ID).ToListAsync();
        }
        public async Task<bool> RemovePersonTimes(HOME_Person_Dates_Timeslots Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Timeslots>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePersonTimes(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Timeslots>().DeleteAsync(ID);
        }
        public async Task<bool> RemovePersonOfficeHours(HOME_Person_Office_Hours Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Office_Hours>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePersonOfficeHours(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Office_Hours>().DeleteAsync(ID);
        }
        public async Task<bool> RemovePersonClosedDates(HOME_Person_Dates_Closed Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Closed>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePersonClosedDates(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Closed>().DeleteAsync(ID);
        }
        public async Task<HOME_Person_Dates_Timeslots?> SetPersonTimes(HOME_Person_Dates_Timeslots Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Timeslots>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Person_Office_Hours?> SetPersonOfficeHours(HOME_Person_Office_Hours Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Office_Hours>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Person_Dates_Closed?> SetPersonClosedDates(HOME_Person_Dates_Closed Data)
        {
            return await _unitOfWork.Repository<HOME_Person_Dates_Closed>().InsertOrUpdateAsync(Data);
        }
        public async Task<V_HOME_Municipal_Newsletter_Type?> GetNewsletter_Type(Guid HOME_Municipal_Newsletter_Type_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipal_Newsletter_Type>().Where(p => p.ID == HOME_Municipal_Newsletter_Type_ID && p.LANG_Language_ID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task<List<V_HOME_Municipal_Newsletter_Type>> GetNewsletter_Types(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Municipal_Newsletter_Type>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_Municipal_Newsletter_Type?> GetNewsletterType(Guid HOME_Municipal_Newsletter_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Type>().FirstOrDefaultAsync(p => p.ID == HOME_Municipal_Newsletter_Type_ID);
        }
        public async Task<List<HOME_Municipal_Newsletter_Type_Extended>> GetNewsletterType_Extended(Guid HOME_Municipal_Newsletter_Type_ID)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Type_Extended>().Where(p => p.HOME_Municipal_Newsletter_Type_ID == HOME_Municipal_Newsletter_Type_ID).ToListAsync();
        }
        public async Task<HOME_Municipal_Newsletter_Type?> SetNewsletterType(HOME_Municipal_Newsletter_Type Data)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Type>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_Municipal_Newsletter_Type_Extended?> SetNewsletterTypeExtended(HOME_Municipal_Newsletter_Type_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Type_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveNewsletterType(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Municipal_Newsletter_Type>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveMedia(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Media>().DeleteAsync(ID);
        }
        public async Task<List<HOME_Association_Person_Extended>> GetAssociationsPersonExtended(Guid HOME_Association_Person_ID)
        {
            return await _unitOfWork.Repository<HOME_Association_Person_Extended>().Where(p => p.HOME_Association_Person_ID == HOME_Association_Person_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Impressum>> GetImpressum(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Impressum>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_Impressum?> GetImpressum(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Impressum>().Where(p => p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<HOME_Impressum?> SetImpressum(HOME_Impressum Data)
        {
            return await _unitOfWork.Repository<HOME_Impressum>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveImpressum(Guid ID)
        {
            return await _unitOfWork.Repository<V_HOME_Impressum>().DeleteAsync(ID);
        }
        public async Task<List<HOME_Impressum_Extended>> GetImpressum_Extended(Guid HOME_Impressum_ID)
        {
            return await _unitOfWork.Repository<HOME_Impressum_Extended>().Where(p => p.HOME_Impressum_ID == HOME_Impressum_ID).ToListAsync();
        }
        public async Task<HOME_Impressum_Extended?> SetImpressum_Extended(HOME_Impressum_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Impressum_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_Privacy>> GetPrivacy(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_Privacy>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_Privacy?> GetPrivacy(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Privacy>().Where(p => p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<HOME_Privacy?> SetPrivacy(HOME_Privacy Data)
        {
            return await _unitOfWork.Repository<HOME_Privacy>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_Privacy_Extended>> GetPrivacy_Extended(Guid HOME_Privacy_ID)
        {
            return await _unitOfWork.Repository<HOME_Privacy_Extended>().Where(p => p.HOME_Privacy_ID == HOME_Privacy_ID).ToListAsync();
        }
        public async Task<HOME_Privacy_Extended?> SetPrivacy_Extended(HOME_Privacy_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_Privacy_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemovePrivacy(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Privacy>().DeleteAsync(ID);
        }
        public async Task<List<HOME_Privacy_Document>> GetPrivacy_Document(Guid HOME_Privacy_ID)
        {
            return await _unitOfWork.Repository<HOME_Privacy_Document>().Where(p => p.HOME_Privacy_ID == HOME_Privacy_ID).ToListAsync();
        }
        public List<HOME_Privacy_Document> GetPrivacy_DocumentSync(Guid HOME_Privacy_ID)
        {
            return _unitOfWork.Repository<HOME_Privacy_Document>().Where(p => p.HOME_Privacy_ID == HOME_Privacy_ID).ToList();
        }
        public async Task<HOME_Privacy_Document?> SetPrivacy_Document(HOME_Privacy_Document Data)
        {
            return await _unitOfWork.Repository<HOME_Privacy_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemovePrivacy_Document(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_Privacy_Document>().DeleteAsync(ID);
        }
        public async Task<List<V_HOME_PNRR>> GetPNRR(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_PNRR>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_PNRR?> GetPNRR(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR>().Where(p => p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<V_HOME_PNRR?> GetPNRRSingle(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_PNRR>().Where(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task<HOME_PNRR?> SetPNRR(HOME_PNRR Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_PNRR_Extended>> GetPNRR_Extended(Guid HOME_PNRR_ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Extended>().Where(p => p.HOME_PNRR_ID == HOME_PNRR_ID).ToListAsync();
        }
        public async Task<HOME_PNRR_Extended?> SetPNRR_Extended(HOME_PNRR_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemovePNRR(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR>().DeleteAsync(ID);
        }
        public async Task<List<HOME_PNRR_Chapter>> GetPNRRChapterList(Guid HOME_PNRR_ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter>().Where(p => p.HOME_PNRR_ID == HOME_PNRR_ID).ToListAsync();
        }
        public async Task<List<V_HOME_PNRR_Chapter>> GetPNRRChapterList(Guid HOME_PNRR_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_PNRR_Chapter>().Where(p => p.HOME_PNRR_ID == HOME_PNRR_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<HOME_PNRR_Chapter?> SetPNRRChapter(HOME_PNRR_Chapter Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_PNRR_Chapter?> GetPNRRChapter(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_PNRR_Chapter_Extended>> GetPNRRChapter_Extended(Guid HOME_PNRR_Chapter_ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Extended>().Where(p => p.HOME_PNRR_Chapter_ID == HOME_PNRR_Chapter_ID).ToListAsync();
        }
        public async Task<HOME_PNRR_Chapter_Extended?> SetPNRRChapter_Extended(HOME_PNRR_Chapter_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_HOME_PNRR_Chapter_Document>> GetPNRRChapter_Document(Guid HOME_PNRR_Chapter_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_PNRR_Chapter_Document>().Where(p => p.HOME_PNRR_Chapter_ID == HOME_PNRR_Chapter_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public List<V_HOME_PNRR_Chapter_Document> GetPNRRChapter_DocumentSync(Guid HOME_PNRR_Chapter_ID, Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_HOME_PNRR_Chapter_Document>().Where(p => p.HOME_PNRR_Chapter_ID == HOME_PNRR_Chapter_ID && p.LANG_Language_ID == LANG_Language_ID).ToList();
        }
        public async Task<HOME_PNRR_Chapter_Document> GetPNRRChapter_Document(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Document>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<HOME_PNRR_Chapter_Document_Extended>> GetPNRRChapterDocument_Extended(Guid HOME_PNRR_Chapter_Document_ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Document_Extended>().Where(p => p.HOME_PNRR_Chapter_Document_ID == HOME_PNRR_Chapter_Document_ID).ToListAsync();
        }
        public async Task<HOME_PNRR_Chapter_Document_Extended?> SetPNRRChapterDocument_Extended(HOME_PNRR_Chapter_Document_Extended Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Document_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_PNRR_Chapter_Document?> SetPNRRChapter_Document(HOME_PNRR_Chapter_Document Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Document>().InsertOrUpdateAsync(Data);
        }
        public async Task<HOME_PNRR_Theme?> SetPNRRTheme(HOME_PNRR_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<HOME_PNRR_Theme>> GetPNRR_Themes(Guid HOME_PNRR_ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Theme>().Where(p => p.HOME_PNRR_ID == HOME_PNRR_ID).ToListAsync();
        }
        public async Task<List<V_HOME_PNRR_Type>> GetPNRRTypes(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_HOME_PNRR_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<bool> RemovePNRRTheme(HOME_PNRR_Theme Data)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Theme>().DeleteAsync(Data);
        }
        public async Task<bool> RemovePNRRChapter_Document(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter_Document>().DeleteAsync(ID);
        }
        public async Task<bool> RemovePNRRChapter(Guid ID)
        {
            return await _unitOfWork.Repository<HOME_PNRR_Chapter>().DeleteAsync(ID);
        }
    }
}
