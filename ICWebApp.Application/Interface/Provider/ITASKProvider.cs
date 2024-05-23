using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ITASKProvider
    {
        public event Action OnStatistikChanged;
        public Task<IEnumerable<V_TASK_Status_Default?>> GetStatusDefaultList(Guid LANG_Language_ID);
        public Task<IEnumerable<V_TASK_Priority_Default?>> GetPriorityDefaultList(Guid LANG_Language_ID);
        public Task<IEnumerable<V_TASK_Tag_Default?>> GetTagDefaultList(Guid LANG_Language_ID);
        public Task<IEnumerable<V_TASK_Bucket_Default?>> GetBucketDefaultList(Guid LANG_Language_ID);
        public Task<IEnumerable<V_TASK_Status?>> GetStatusList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID);
        public Task<IEnumerable<V_TASK_Priority?>> GetPriorityList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID);
        public Task<IEnumerable<V_TASK_Tag?>> GetTagList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID);
        public Task<IEnumerable<V_TASK_Bucket?>> GetBucketList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID);
        public Task<IEnumerable<V_TASK_Task?>> GetTaskList(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, long? TASK_Context_ID, string? ContextElementID);
        public Task<IEnumerable<V_TASK_Task?>> GetTaskListByUser(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid AUTH_Users_ID);
        public Task<IEnumerable<V_TASK_Task_Tag?>> GetTaskTagList(Guid TASK_Task_ID, Guid LANG_Language_ID);
        public Task<IEnumerable<TASK_Task_Tag?>> GetTaskTagList(Guid TASK_Task_ID);
        public Task<IEnumerable<TASK_Task_CheckItems?>> GetTaskCheckItemsList(Guid TASK_Task_ID);
        public Task<string> GetTaskCheckItemDescription(Guid id);
        public Task<IEnumerable<TASK_Task_Comment?>> GetTaskCommentList(Guid TASK_Task_ID);
        public Task<IEnumerable<TASK_Task_Files?>> GetTaskFilesList(Guid TASK_Task_ID);
        public Task<IEnumerable<TASK_Task_Responsible?>> GetTaskResponsibleList(Guid TASK_Task_ID);
        public Task<IEnumerable<TASK_Task_Eskalation?>> GetTaskEskalationList(Guid TASK_Task_ID);
        public Task<IEnumerable<TASK_Task_Eskalation_Responsible?>> GetTaskEskalationResponsibleList(Guid TASK_Task_Eskalation_ID);
        public Task<IEnumerable<TASK_Task?>> GetTaskListToReorder(Guid? TASK_Bucket_ID, Guid AUTH_Municipality_ID, long? TASK_Context_ID, string? ContextElementID);
        public Task<IEnumerable<V_TASK_Context?>> GetTaskContextList(Guid LANG_Language_ID, Guid AUTH_Municipality_ID);
        public Task<IEnumerable<V_TASK_Statistik_Dashboard>> GetTaskDashboard(long TASK_Context_ID, Guid? AUTH_Municipality_ID = null, bool getCompleted = false);
        public Task<List<V_TASK_Statistik_Dashboard>> GetTaskDashboardForAuthority(long taskContextId,
            Guid authorityId, Guid? municipalityId = null, bool getCompleted = false);
        public Task<List<V_TASK_Statistik_Dashboard>> GetTaskDashboardForAuthorities(long taskContextId,
            List<Guid> authorityId, Guid? municipalityId = null, bool getCompleted = false);
        public Task<TASK_Status?> GetStatus(Guid ID);
        public Task<TASK_Priority?> GetPriority(Guid ID);
        public Task<TASK_Tag?> GetTag(Guid ID);
        public Task<string> GetTagDescription(Guid id, Guid langId);
        public Task<TASK_Bucket?> GetBucket(Guid ID);
        public Task<TASK_Task?> GetTask(Guid ID);
        public TASK_Task? GetTaskSync(Guid id);
        public Task<TASK_Task?> GetTask(long? TASK_Context_ID, string? ContextElementID, string? ContextExternalID);
        public Task<V_TASK_Task?> GetVTask(Guid ID, Guid LANG_Language_ID);
        public Task<List<V_TASK_Task>> GetUncompletedVTasksForUser(Guid userId, Guid langId);
        public V_TASK_Task? GetVTaskSync(Guid ID, Guid LANG_Language_ID);
        public Task<V_TASK_Context?> GetContext(long ID, Guid LANG_Language_ID);

        public Task<DateTime?> DeferTaskDefaultDeadlineFromContextElementId(long contextId,
            string contextElementId);
        public Task<string?> GetContextSyetemTextCode(long contextId);
        public Task<IEnumerable<TASK_Status_Extended?>> GetStatusExtendedList(Guid TASK_Status_ID);
        public Task<IEnumerable<TASK_Priority_Extended?>> GetPriorityExtendedList(Guid TASK_Priority_ID);
        public Task<IEnumerable<TASK_Tag_Extended?>> GetTagExtendedList(Guid TASK_Tag_ID);
        public Task<IEnumerable<TASK_Bucket_Extended?>> GetBucketExtendedList(Guid TASK_Bucket_ID);
        public Task<IEnumerable<V_TASK_Statistik?>> GetStatistik(Guid AUTH_Municipality_ID);
        public Task<IEnumerable<V_TASK_Statistik_User?>> GetStatistikByUser(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, bool Completed = true);
        public Task<IEnumerable<V_TASK_Statistik_User_General?>> GetStatistikByUserGeneral(Guid LANG_Language_ID, Guid AUTH_Users_ID);
        public Task<IEnumerable<V_TASK_Statistik_User_Bucket?>> GetStatistikByUserBucket(Guid LANG_Language_ID, Guid AUTH_Users_ID);
        public Task<IEnumerable<V_TASK_Statistik_User_Status?>> GetStatistikByUserStatus(Guid LANG_Language_ID, Guid AUTH_Users_ID);
        public Task<IEnumerable<V_TASK_Statistik_User_Priority?>> GetStatistikByUserPriority(Guid LANG_Language_ID, Guid AUTH_Users_ID);
        public Task<TASK_Status?> SetStatus(TASK_Status Data);
        public Task<TASK_Priority?> SetPriority(TASK_Priority Data);
        public Task<TASK_Tag?> SetTag(TASK_Tag Data);
        public Task<TASK_Bucket?> SetBucket(TASK_Bucket Data);
        public Task<TASK_Task?> SetTask(TASK_Task Data);
        public Task<TASK_Task_Tag?> SetTaskTag(TASK_Task_Tag Data);
        public Task<TASK_Task_CheckItems?> SetTaskCheckItem(TASK_Task_CheckItems Data);
        public Task<TASK_Task_Comment?> SetTaskComment(TASK_Task_Comment Data);
        public Task<TASK_Task_Comment?> GetTaskComment(Guid id);
        public Task<TASK_Task_Files?> SetTaskFiles(TASK_Task_Files Data);
        public Task<TASK_Task_Responsible?> SetTaskResponsible(TASK_Task_Responsible Data);
        public Task<TASK_Status_Extended?> SetStatusExtended(TASK_Status_Extended Data);
        public Task<TASK_Priority_Extended?> SetPriorityExtended(TASK_Priority_Extended Data);
        public Task<TASK_Tag_Extended?> SetTagExtended(TASK_Tag_Extended Data);
        public Task<TASK_Bucket_Extended?> SetBucketExtended(TASK_Bucket_Extended Data);
        public Task<TASK_Task_Eskalation?> SetEskalation(TASK_Task_Eskalation Data);
        public Task<TASK_Task_Eskalation_Responsible?> SetEskalationResponsible(TASK_Task_Eskalation_Responsible Data);
        public Task<bool> RemoveStatus(Guid ID);
        public Task<bool> RemovePriority(Guid ID);
        public Task<bool> RemoveTag(Guid ID);
        public Task<bool> RemoveBucket(Guid ID);
        public Task<bool> RemoveTask(Guid ID);
        public Task<bool> RemoveTaskCheckItem(Guid ID);
        public Task<bool> RemoveStatusExtended(Guid ID);
        public Task<bool> RemovePriorityExtended(Guid ID);
        public Task<bool> RemoveTagExtended(Guid ID);
        public Task<bool> RemoveBucketExtended(Guid ID);
        public Task<bool> RemoveTaskFile(Guid ID);
        public Task<bool> RemoveTaskResponsible(Guid ID);
        public Task<bool> RemoveTaskTag(Guid ID);
        public Task<bool> RemoveTaskEskalation(Guid ID);
        public Task<bool> RemoveTaskEskalationResponsible(Guid ID);
        public Task<bool> BulkUpdateTask(IEnumerable<TASK_Task?> Data);
        public Task<string> GetStatusDescription(Guid statusId, Guid langId);
        public Task<string> GetPriorityDescription(Guid prioId, Guid langId);
        public Task<string> GetBucketDescription(Guid bucketId, Guid langId);
    }
}