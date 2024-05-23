using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore.MSSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.DataStore;
using System.Globalization;
using ICWebApp.DataStore.MSSQL.Repositories;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Freshdesk;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Provider
{
    public class TASKProvider : ITASKProvider
    {
        public event Action OnStatistikChanged;

        private IGenericRepository<TASK_Tag> TagRep;
        private IGenericRepository<TASK_Tag_Extended> TagExtendedRep;
        private IGenericRepository<V_TASK_Tag> VTagRep;
        private IGenericRepository<V_TASK_Tag_Default> VTagDefaultRep;
        private IGenericRepository<TASK_Status> StatusRep;
        private IGenericRepository<TASK_Status_Extended> StatusExtendedRep;
        private IGenericRepository<V_TASK_Status> VStatusRep;
        private IGenericRepository<V_TASK_Status_Default> VStatusDefaultRep;
        private IGenericRepository<TASK_Priority> PriorityRep;
        private IGenericRepository<TASK_Priority_Extended> PriorityExtendedRep;
        private IGenericRepository<V_TASK_Priority> VPriorityRep;
        private IGenericRepository<V_TASK_Priority_Default> VPriorityDefaultRep;
        private IGenericRepository<TASK_Bucket> BucketRep;
        private IGenericRepository<TASK_Bucket_Extended> BucketExtendedRep;
        private IGenericRepository<V_TASK_Bucket> VBucketRep;
        private IGenericRepository<V_TASK_Bucket_Default> VBucketDefaultRep;
        private IGenericRepository<TASK_Task_Tag> TaskTaskTagRep;
        private IGenericRepository<V_TASK_Task_Tag> VTaskTaskTagRep;
        private IGenericRepository<TASK_Task> TaskRep;
        private IGenericRepository<V_TASK_Task> VTaskRep;
        private IGenericRepository<TASK_Task_CheckItems> TaskCheckItemsRep;
        private IGenericRepository<TASK_Task_Comment> TaskCommentRep;
        private IGenericRepository<TASK_Task_Files> TaskFilesRep;
        private IGenericRepository<TASK_Task_Responsible> TaskResponsibleRep;
        private IGenericRepository<V_TASK_Context> VTaskContextRep;
        private IGenericRepository<TASK_Task_Eskalation> TaskEskalationRep;
        private IGenericRepository<TASK_Task_Eskalation_Responsible> TaskEskalationResponsibleRep;
        private IGenericRepository<V_TASK_Statistik> TaskStatistikRep;
        private IGenericRepository<V_TASK_Statistik_User> TaskStatistikUserRep;
        private IGenericRepository<V_TASK_Statistik_User_General> TaskStatistikUserGeneralRep;
        private IGenericRepository<V_TASK_Statistik_User_Bucket> TaskStatistikUserBucketRep;
        private IGenericRepository<V_TASK_Statistik_User_Status> TaskStatistikUserStatusRep;
        private IGenericRepository<V_TASK_Statistik_User_Priority> TaskStatistikUserPriorityRep;
        private IGenericRepository<AUTH_MunicipalityApps> MunicipalityAppsRep;
        private IGenericRepository<V_TASK_Statistik_Dashboard> TaskStatistikDashboardRep;
        private IGenericRepository<TASK_Context> TaskContextRep;
        private IUnitOfWork _unitOfWork;

        public TASKProvider(IGenericRepository<TASK_Tag> TagRep,
                            IGenericRepository<TASK_Tag_Extended> TagExtendedRep,
                            IGenericRepository<V_TASK_Tag> VTagRep,
                            IGenericRepository<V_TASK_Tag_Default> VTagDefaultRep,
                            IGenericRepository<TASK_Status> StatusRep,
                            IGenericRepository<TASK_Status_Extended> StatusExtendedRep,
                            IGenericRepository<V_TASK_Status> VStatusRep,
                            IGenericRepository<V_TASK_Status_Default> VStatusDefaultRep,
                            IGenericRepository<TASK_Priority> PriorityRep,
                            IGenericRepository<TASK_Priority_Extended> PriorityExtendedRep,
                            IGenericRepository<V_TASK_Priority> VPriorityRep,
                            IGenericRepository<V_TASK_Priority_Default> VPriorityDefaultRep,
                            IGenericRepository<TASK_Bucket> BucketRep,
                            IGenericRepository<TASK_Bucket_Extended> BucketExtendedRep,
                            IGenericRepository<V_TASK_Bucket> VBucketRep,
                            IGenericRepository<V_TASK_Bucket_Default> VBucketDefaultRep,
                            IGenericRepository<TASK_Task_Tag> TaskTaskTagRep,
                            IGenericRepository<V_TASK_Task_Tag> VTaskTaskTagRep,
                            IGenericRepository<TASK_Task> TaskRep,
                            IGenericRepository<V_TASK_Task> VTaskRep,
                            IGenericRepository<TASK_Task_CheckItems> TaskCheckItemsRep,
                            IGenericRepository<TASK_Task_Comment> TaskCommentRep, 
                            IGenericRepository<TASK_Task_Files> TaskFilesRep,
                            IGenericRepository<TASK_Task_Responsible> TaskResponsibleRep,
                            IGenericRepository<V_TASK_Context> VTaskContextRep,
                            IGenericRepository<TASK_Task_Eskalation> TaskEskalationRep,
                            IGenericRepository<TASK_Task_Eskalation_Responsible> TaskEskalationResponsibleRep,
                            IGenericRepository<V_TASK_Statistik> TaskStatistikRep,
                            IGenericRepository<V_TASK_Statistik_User> TaskStatistikUserRep,
                            IGenericRepository<V_TASK_Statistik_User_General> TaskStatistikUserGeneralRep,
                            IGenericRepository<V_TASK_Statistik_User_Bucket> TaskStatistikUserBucketRep,
                            IGenericRepository<V_TASK_Statistik_User_Status> TaskStatistikUserStatusRep,
                            IGenericRepository<V_TASK_Statistik_User_Priority> TaskStatistikUserPriorityRep,
                            IGenericRepository<AUTH_MunicipalityApps> MunicipalityAppsRep,
                            IGenericRepository<V_TASK_Statistik_Dashboard> TaskStatistikDashboardRep,
                            IGenericRepository<TASK_Context> TaskContextRep,
                            IUnitOfWork unitOfWork)
        {
            this.TagRep = TagRep;
            this.TagExtendedRep = TagExtendedRep;
            this.VTagRep = VTagRep;
            this.VTagDefaultRep = VTagDefaultRep;
            this.StatusRep = StatusRep;
            this.StatusExtendedRep = StatusExtendedRep;
            this.VStatusRep = VStatusRep;
            this.VStatusDefaultRep = VStatusDefaultRep;
            this.PriorityRep = PriorityRep;
            this.PriorityExtendedRep = PriorityExtendedRep;
            this.VPriorityRep = VPriorityRep;
            this.VPriorityDefaultRep = VPriorityDefaultRep;
            this.BucketRep = BucketRep;
            this.BucketExtendedRep = BucketExtendedRep;
            this.VBucketRep = VBucketRep;
            this.VBucketDefaultRep = VBucketDefaultRep;
            this.TaskTaskTagRep = TaskTaskTagRep;
            this.VTaskTaskTagRep = VTaskTaskTagRep;
            this.TaskRep = TaskRep;
            this.VTaskRep = VTaskRep;
            this.TaskCheckItemsRep = TaskCheckItemsRep;
            this.TaskCommentRep = TaskCommentRep;
            this.TaskFilesRep = TaskFilesRep;
            this.TaskResponsibleRep = TaskResponsibleRep;
            this.VTaskContextRep = VTaskContextRep;
            this.TaskEskalationRep = TaskEskalationRep;
            this.TaskEskalationResponsibleRep = TaskEskalationResponsibleRep;
            this.TaskStatistikRep = TaskStatistikRep;
            this.TaskStatistikUserRep = TaskStatistikUserRep;
            this.TaskStatistikUserGeneralRep = TaskStatistikUserGeneralRep;
            this.TaskStatistikUserBucketRep = TaskStatistikUserBucketRep;
            this.TaskStatistikUserStatusRep = TaskStatistikUserStatusRep;
            this.TaskStatistikUserPriorityRep = TaskStatistikUserPriorityRep;
            this.MunicipalityAppsRep = MunicipalityAppsRep;
            this.TaskStatistikDashboardRep = TaskStatistikDashboardRep;
            this.TaskContextRep = TaskContextRep;
            this._unitOfWork = unitOfWork;
        }

        public async Task<TASK_Bucket?> GetBucket(Guid ID)
        {
            return await BucketRep.GetByIDAsync(ID);
        }
        public async Task<IEnumerable<V_TASK_Bucket_Default?>> GetBucketDefaultList(Guid LANG_Language_ID)
        {
            return await VBucketDefaultRep.ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<IEnumerable<TASK_Bucket_Extended?>> GetBucketExtendedList(Guid TASK_Bucket_ID)
        {
            return await BucketExtendedRep.ToListAsync(p => p.TASK_Bucket_ID == TASK_Bucket_ID);
        }
        public async Task<IEnumerable<V_TASK_Bucket?>> GetBucketList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID)
        {
            return await VBucketRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TASK_Context_ID == TASK_Context_ID);
        }
        public async Task<TASK_Priority?> GetPriority(Guid ID)
        {
            return await PriorityRep.GetByIDAsync(ID);
        }
        public async Task<IEnumerable<V_TASK_Priority_Default?>> GetPriorityDefaultList(Guid LANG_Language_ID)
        {
            return await VPriorityDefaultRep.ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<IEnumerable<TASK_Priority_Extended?>> GetPriorityExtendedList(Guid TASK_Priority_ID)
        {
            return await PriorityExtendedRep.ToListAsync(p => p.TASK_Priority_ID == TASK_Priority_ID);
        }
        public async Task<IEnumerable<V_TASK_Priority?>> GetPriorityList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID)
        {
            return await VPriorityRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TASK_Context_ID == TASK_Context_ID);
        }
        public async Task<TASK_Status?> GetStatus(Guid ID)
        {
            return await StatusRep.GetByIDAsync(ID);
        }
        public async Task<IEnumerable<V_TASK_Status_Default?>> GetStatusDefaultList(Guid LANG_Language_ID)
        {
            return await VStatusDefaultRep.ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<IEnumerable<TASK_Status_Extended?>> GetStatusExtendedList(Guid TASK_Status_ID)
        {
            return await StatusExtendedRep.ToListAsync(p => p.TASK_Status_ID == TASK_Status_ID);
        }
        public async Task<IEnumerable<V_TASK_Status?>> GetStatusList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID)
        {
            return await VStatusRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TASK_Context_ID == TASK_Context_ID);
        }
        public async Task<TASK_Tag?> GetTag(Guid ID)
        {
            return await TagRep.GetByIDAsync(ID);
        }

        public async Task<string> GetTagDescription(Guid id, Guid langId)
        {
            var task = await VTagRep.FirstOrDefaultAsync(e => e.ID == id && e.LANG_Language_ID == langId);
            if (task != null)
                return task.Description;
            return "";
        }
        public async Task<IEnumerable<V_TASK_Tag_Default?>> GetTagDefaultList(Guid LANG_Language_ID)
        {
            return await VTagDefaultRep.ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
        }
        public async Task<IEnumerable<TASK_Tag_Extended?>> GetTagExtendedList(Guid TASK_Tag_ID)
        {
            return await TagExtendedRep.ToListAsync(p => p.TASK_Tag_ID == TASK_Tag_ID);
        }
        public async Task<IEnumerable<V_TASK_Tag?>> GetTagList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID)
        {
            return await VTagRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TASK_Context_ID == TASK_Context_ID);
        }
        public async Task<TASK_Task?> GetTask(Guid ID)
        {
            return await TaskRep.GetByIDAsync(ID);
        }

        public TASK_Task? GetTaskSync(Guid id)
        {
            return TaskRep.GetByID(id);
        }
        public async Task<TASK_Task?> GetTask(long? TASK_Context_ID, string? ContextElementID, string? ContextExternalID)
        {
            var TaskList = await TaskRep.ToListAsync(p => p.TASK_Context_ID == TASK_Context_ID && p.ContextElementID == ContextElementID && p.ContextExternalID == ContextExternalID);

            if (TaskList != null) 
            {
                return TaskList.FirstOrDefault();
            }

            return null;
        }
        public async Task<V_TASK_Task?> GetVTask(Guid ID, Guid LANG_Language_ID)
        {
            var taskList = await VTaskRep.ToListAsync(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);

            return taskList.FirstOrDefault();
        }

        public async Task<List<V_TASK_Task>> GetUncompletedVTasksForUser(Guid userId, Guid langId)
        {
            var taskIds = await TaskResponsibleRep.Where(p => p.AUTH_Municipal_Users_ID == userId).Select(m => m.TASK_Task_ID).ToListAsync();
            var taskList = await VTaskRep.ToListAsync(p => taskIds.Contains(p.ID) && p.CompletedAt == null && p.LANG_Language_ID == langId);
            return taskList.OrderBy(e => e.Deadline).ToList();
        }
        
        public V_TASK_Task? GetVTaskSync(Guid ID, Guid LANG_Language_ID)
        {
            var taskList = VTaskRep.ToList(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID);

            return taskList.FirstOrDefault();
        }
        public async Task<IEnumerable<TASK_Task_CheckItems?>> GetTaskCheckItemsList(Guid TASK_Task_ID)
        {
            return await TaskCheckItemsRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID);
        }

        public async Task<string> GetTaskCheckItemDescription(Guid id)
        {
            var item = await TaskCheckItemsRep.FirstOrDefaultAsync(e => e.ID == id);
            if (item != null)
            {
                return item.Description;
            }
            return "";
        }
        public async Task<IEnumerable<TASK_Task_Comment?>> GetTaskCommentList(Guid TASK_Task_ID)
        {
            return await TaskCommentRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID);
        }
        public async Task<IEnumerable<TASK_Task_Files?>> GetTaskFilesList(Guid TASK_Task_ID)
        {
            return await TaskFilesRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID);
        }
        public async Task<IEnumerable<V_TASK_Task?>> GetTaskList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID, string? ContextElementID)
        {
            return await VTaskRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.TASK_Context_ID == TASK_Context_ID && p.ContextElementID == ContextElementID);
        }
        public async Task<IEnumerable<TASK_Task_Responsible?>> GetTaskResponsibleList(Guid TASK_Task_ID)
        {
            return await TaskResponsibleRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID);
        }
        public async Task<IEnumerable<V_TASK_Task_Tag?>> GetTaskTagList(Guid TASK_Task_ID, Guid LANG_Language_ID)
        {
            return await VTaskTaskTagRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID && p.LANG_Language_ID == LANG_Language_ID && p.Enabled == true);
        }
        public async Task<IEnumerable<TASK_Task_Tag?>> GetTaskTagList(Guid TASK_Task_ID)
        {
            return await TaskTaskTagRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID);
        }
        public async Task<IEnumerable<TASK_Task?>> GetTaskListToReorder(Guid? TASK_Bucket_ID, Guid AUTH_Municipality_ID, long? TASK_Context_ID, string? ContextElementID)
        {
            return await TaskRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.TASK_Bucket_ID == TASK_Bucket_ID && p.TASK_Context_ID == TASK_Context_ID && p.ContextElementID == ContextElementID);
        }
        public async Task<IEnumerable<V_TASK_Context?>> GetTaskContextList(Guid LANG_Language_ID, Guid AUTH_Municipality_ID)
        {
            //ADD MUNICIPAL RIGHTS

            var municipalRights = await MunicipalityAppsRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            if(municipalRights != null && municipalRights.Count() > 0)
            {
                var appList = municipalRights.Select(p => p.APP_Application_ID).ToList();

                return await VTaskContextRep.ToListAsync(p => p.Enabled == true && p.LANG_LanguagesID == LANG_Language_ID && p.APP_Applications_ID != null && appList.Contains(p.APP_Applications_ID.Value));
            }

            return new List<V_TASK_Context?>();
        }
        public async Task<IEnumerable<V_TASK_Statistik_Dashboard>> GetTaskDashboard(long TASK_Context_ID, Guid? AUTH_Municipality_ID = null, bool getCompleted = false)
        {
            var list =  await TaskStatistikDashboardRep.ToListAsync(p => p.TASK_Context_ID == TASK_Context_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
            
            if (getCompleted)
                return list;
            return list.Where(e => e.Total > e.Completed);
        }

        public async Task<List<V_TASK_Statistik_Dashboard>> GetTaskDashboardForAuthority(long taskContextId,
            Guid authorityId, Guid? municipalityId = null, bool getCompleted = false)
        {
            var list =  await TaskStatistikDashboardRep.ToListAsync(p =>
                p.TASK_Context_ID == taskContextId && p.AUTH_Municipality_ID == municipalityId && p.AUTH_Authority_ID == authorityId);
            
            if (getCompleted)
                return list;
            return list.Where(e => e.Total > e.Completed).ToList();
        }
        public async Task<List<V_TASK_Statistik_Dashboard>> GetTaskDashboardForAuthorities(long taskContextId,
            List<Guid> authorityIds, Guid? municipalityId = null, bool getCompleted = false)
        {
            var list =  await TaskStatistikDashboardRep.ToListAsync(p =>
                p.TASK_Context_ID == taskContextId && p.AUTH_Municipality_ID == municipalityId && p.AUTH_Authority_ID != null && authorityIds.Any(e => e == p.AUTH_Authority_ID.Value));
            
            if (getCompleted)
                return list;
            return list.Where(e => e.Total > e.Completed).ToList();
        }
        public async Task<IEnumerable<TASK_Task_Eskalation?>> GetTaskEskalationList(Guid TASK_Task_ID)
        {
            return await TaskEskalationRep.ToListAsync(p => p.TASK_Task_ID == TASK_Task_ID);
        }
        public async Task<IEnumerable<TASK_Task_Eskalation_Responsible?>> GetTaskEskalationResponsibleList(Guid TASK_Task_Eskalation_ID)
        {
            return await TaskEskalationResponsibleRep.ToListAsync(p => p.TASK_Task_Eskalation_ID == TASK_Task_Eskalation_ID);
        }
        public async Task<V_TASK_Context?> GetContext(long ID, Guid LANG_Language_ID)
        {
            var taskList = await VTaskContextRep.ToListAsync(p => p.ID == ID && p.LANG_LanguagesID == LANG_Language_ID);

            return taskList.FirstOrDefault();
        }

        public async Task<DateTime?> DeferTaskDefaultDeadlineFromContextElementId(long contextId,
            string contextElementId)
        {
            var id = Guid.Empty;
            var ok = Guid.TryParse(contextElementId, out id);
            
            if (!ok)
                return null;
            
            switch (contextId)
            {
                case 1: //Application
                    var application = await _unitOfWork.Repository<FORM_Application>().FirstOrDefaultAsync(e => e.ID == id);
                    if (application != null)
                    {
                        return application.LegalDeadline ?? application.EstimatedDeadline;
                    }
                    break;
                case 2: //Maintenance
                    var maintenance = await _unitOfWork.Repository<FORM_Application>().FirstOrDefaultAsync(e => e.ID == id);
                    if (maintenance != null)
                    {
                        return maintenance.LegalDeadline ?? maintenance.EstimatedDeadline;
                    }
                    break;
                case 3: //Rooms
                    break;
                case 4: //Organizations
                    break;
            }
            return null;
        }

        public async Task<string?> GetContextSyetemTextCode(long contextId)
        {
            return await TaskContextRep.Where(e => e.ID == contextId).Select(e => e.TEXT_SystemTextCode)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<V_TASK_Task?>> GetTaskListByUser(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid AUTH_Users_ID)
        {
            return await VTaskRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.ResponsibleUser != null && p.ResponsibleUser.Contains(AUTH_Users_ID.ToString()));
        }
        public async Task<IEnumerable<V_TASK_Statistik?>> GetStatistik(Guid AUTH_Municipality_ID)
        {
            return await TaskStatistikRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<IEnumerable<V_TASK_Statistik_User?>> GetStatistikByUser(Guid AUTH_Municipality_ID, Guid AUTH_Municipal_Users_ID, Guid LANG_Language_ID, bool Completed = true)
        {
            if (!Completed)
            {
                return await TaskStatistikUserRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID && p.LANG_Language_ID == LANG_Language_ID);
            }

            return await TaskStatistikUserRep.ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID && p.LANG_Language_ID == LANG_Language_ID && p.CompletedAt == null);
        }
        public async Task<IEnumerable<V_TASK_Statistik_User_General?>> GetStatistikByUserGeneral(Guid LANG_Language_ID, Guid AUTH_Municipal_Users_ID)
        {
            return await TaskStatistikUserGeneralRep.ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID && p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);
        }
        public async Task<IEnumerable<V_TASK_Statistik_User_Bucket?>> GetStatistikByUserBucket(Guid LANG_Language_ID, Guid AUTH_Municipal_Users_ID)
        {
            return await TaskStatistikUserBucketRep.ToListAsync(p => p.LANG_Language_ID == LANG_Language_ID && p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);
        }
        public async Task<IEnumerable<V_TASK_Statistik_User_Status?>> GetStatistikByUserStatus(Guid LANG_Language_ID, Guid AUTH_Municipal_Users_ID)
        {
            return await TaskStatistikUserStatusRep.ToListAsync(p => p.LANG_Language_ID == LANG_Language_ID && p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);
        }
        public async Task<IEnumerable<V_TASK_Statistik_User_Priority?>> GetStatistikByUserPriority(Guid LANG_Language_ID, Guid AUTH_Municipal_Users_ID)
        {
            return await TaskStatistikUserPriorityRep.ToListAsync(p => p.LANG_Language_ID == LANG_Language_ID && p.AUTH_Municipal_Users_ID == AUTH_Municipal_Users_ID);
        }
        public async Task<bool> RemoveBucket(Guid ID)
        {
            bool result = await BucketRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveBucketExtended(Guid ID)
        {
            bool result = await BucketExtendedRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemovePriority(Guid ID)
        {
            bool result = await PriorityRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemovePriorityExtended(Guid ID)
        {
            bool result = await PriorityExtendedRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveStatus(Guid ID)
        {
            bool result = await StatusRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveStatusExtended(Guid ID)
        {
            bool result = await StatusExtendedRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTag(Guid ID)
        {
            bool result = await TagRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTagExtended(Guid ID)
        {
            bool result = await TagExtendedRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTask(Guid ID)
        {
            bool result = await TaskRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTaskCheckItem(Guid ID)
        {
            bool result = await TaskCheckItemsRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTaskFile(Guid ID)
        {
            bool result = await TaskFilesRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTaskResponsible(Guid ID)
        {
            bool result = await TaskResponsibleRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTaskTag(Guid ID)
        {
            bool result = await TaskTaskTagRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTaskEskalation(Guid ID)
        {
            bool result = await TaskEskalationRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> RemoveTaskEskalationResponsible(Guid ID)
        {
            bool result = await TaskEskalationResponsibleRep.DeleteAsync(ID);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Bucket?> SetBucket(TASK_Bucket Data)
        {
            TASK_Bucket? result = await BucketRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Bucket_Extended?> SetBucketExtended(TASK_Bucket_Extended Data)
        {
            TASK_Bucket_Extended? result = await BucketExtendedRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Priority?> SetPriority(TASK_Priority Data)
        {
            TASK_Priority? result = await PriorityRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Priority_Extended?> SetPriorityExtended(TASK_Priority_Extended Data)
        {
            TASK_Priority_Extended? result = await PriorityExtendedRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Status?> SetStatus(TASK_Status Data)
        {
            TASK_Status? result = await StatusRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Status_Extended?> SetStatusExtended(TASK_Status_Extended Data)
        {
            TASK_Status_Extended? result = await StatusExtendedRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Tag?> SetTag(TASK_Tag Data)
        {
            TASK_Tag? result = await TagRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Tag_Extended?> SetTagExtended(TASK_Tag_Extended Data)
        {
            TASK_Tag_Extended? result = await TagExtendedRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Task?> SetTask(TASK_Task Data)
        {
            TASK_Task? result = await TaskRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Task_CheckItems?> SetTaskCheckItem(TASK_Task_CheckItems Data)
        {
            TASK_Task_CheckItems? result = await TaskCheckItemsRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Task_Comment?> SetTaskComment(TASK_Task_Comment Data)
        {
            TASK_Task_Comment? result = await TaskCommentRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }

        public async Task<TASK_Task_Comment?> GetTaskComment(Guid id)
        {
            return await TaskCommentRep.FirstOrDefaultAsync(e => e.ID == id);
        }
        public async Task<TASK_Task_Files?> SetTaskFiles(TASK_Task_Files Data)
        {
            if(await TaskFilesRep.FirstOrDefaultAsync(e => e.TASK_Task_ID == Data.TASK_Task_ID && e.FILE_FileInfo_ID == Data.FILE_FileInfo_ID) == null)
            {
                TASK_Task_Files? result = await TaskFilesRep.InsertAsync(Data);
                OnStatistikChange();
                return result;
            }
            return null;
        }
        public async Task<TASK_Task_Responsible?> SetTaskResponsible(TASK_Task_Responsible Data)
        {
            TASK_Task_Responsible? result = await TaskResponsibleRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Task_Tag?> SetTaskTag(TASK_Task_Tag Data)
        {
            TASK_Task_Tag? result = await TaskTaskTagRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Task_Eskalation?> SetEskalation(TASK_Task_Eskalation Data)
        {
            TASK_Task_Eskalation? result = await TaskEskalationRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<TASK_Task_Eskalation_Responsible?> SetEskalationResponsible(TASK_Task_Eskalation_Responsible Data)
        {
            TASK_Task_Eskalation_Responsible? result = await TaskEskalationResponsibleRep.InsertOrUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }
        public async Task<bool> BulkUpdateTask(IEnumerable<TASK_Task?> Data)
        {
            bool result = await TaskRep.BulkUpdateAsync(Data);
            OnStatistikChange();
            return result;
        }

        public async Task<string> GetStatusDescription(Guid statusId, Guid langId)
        {
            var item = await StatusExtendedRep.FirstOrDefaultAsync(e =>
                e.TASK_Status_ID == statusId && e.LANG_Language_ID == langId);
            return item == null ? "" : item.Description;
        }

        public async Task<string> GetPriorityDescription(Guid prioId, Guid langId)
        {
            var item = await PriorityExtendedRep.FirstOrDefaultAsync(e =>
                e.TASK_Priority_ID == prioId && e.LANG_Language_ID == langId);
            return item == null ? "" : item.Description;
        }

        public async Task<string> GetBucketDescription(Guid bucketId, Guid langId)
        {
            var item = await BucketExtendedRep.FirstOrDefaultAsync(
                e => e.TASK_Bucket_ID == bucketId && e.LANG_Language_ID == langId);
            return item == null ? "" : item.Description;
        }

        private void OnStatistikChange()
        {
            if (OnStatistikChanged != null)
            {
                OnStatistikChanged.Invoke();
            }
        }
    }
}