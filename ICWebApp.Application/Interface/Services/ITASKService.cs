using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.Application.Cache.Tasks;
using ICWebApp.DataStore;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Interface.Services
{
    public interface ITASKService
    {
        public long? TASK_Context_ID { get; set; }
        public string? ContextElementID { get; set; }
        public string? ContextName { get; set; }
        public bool ShowToolbar { get; set; }
        public bool TaskNotifyCreator { get; set; }
        public DateTime? TaskDefaultDeadline { get; set; }
        public V_TASK_Context? Context { get; }
        public event Action? OnContextChanged;
        public TaskChangesCache ChangesCache { get; }

        public Task<List<V_TASK_Tag?>> GetTagList(long? TASK_Context_ID, bool OnlyEnabled = true);
        public Task<List<V_TASK_Priority?>> GetPriorityList(long? TASK_Context_ID, bool OnlyEnabled = true);
        public Task<List<V_TASK_Status?>> GetStatusList(long? TASK_Context_ID, bool OnlyEnabled = true);
        public Task<List<V_TASK_Bucket?>> GetBucketList(long? TASK_Context_ID, bool OnlyEnabled = true);
        public Task<TASK_Tag> SetTag(TASK_Tag Item, List<TASK_Tag_Extended> ItemExtended);
        public Task<TASK_Priority> SetPriority(TASK_Priority Item, List<TASK_Priority_Extended> ItemExtended);
        public Task<TASK_Status> SetStatus(TASK_Status Item, List<TASK_Status_Extended> ItemExtended);
        public Task<TASK_Bucket> SetBucket(TASK_Bucket Item, List<TASK_Bucket_Extended> ItemExtended);
        public Task<long> GetTaskPosition(Guid? TASK_Bucket_ID);
        public Task<bool> SetContext(long? taskContextId, string? contextElementId, string contextName, bool notifyCreator = false, DateTime? defaultDeadline = null);
        public Task<TASK_Task?> CreateTask(long? TASK_Context_ID, string? ContextElementID, Guid? CreatorUserID, bool NotifyCreator = false, string? ContextExternalID = null, string? Description = null, string? Url = null, DateTime? Deadline = null, List<AUTH_Municipal_Users>? Responsible = null, Guid? TaskID = null, string? contextName = null);
        public Task<bool> CreateEskalation(Guid TASK_Task_ID, DateTime NotificationDate, List<Guid> AUTH_Users_ID);
        public Task<TASK_Task_Responsible?> SetResponsible(TASK_Task_Responsible Data);
    }
}
