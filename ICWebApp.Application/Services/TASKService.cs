using Blazored.LocalStorage;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore;
using ICWebApp.Application.Helper;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Blazored.SessionStorage;
using ICWebApp.Application.Cache.Tasks;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces;

namespace ICWebApp.Application.Services
{
    public class TASKService : ITASKService
    {
        private ISessionWrapper _sessionWrapper;
        private ILANGProvider _langProvider;
        private ITASKProvider _TaskProvider;
        private ITEXTProvider _TextProvider;
        private IAUTHProvider _AuthProvider;
        private IMessageService _MessageService;
        private IMailerService _MailerService;
        private IFILEProvider _FileProvider;
        private long? _Task_Context_ID;
        private string? _ContextElementID;
        private string? _contextName;
        private V_TASK_Context? _context;

        public long? TASK_Context_ID
        {
            get
            {
                return _Task_Context_ID;
            }
            set
            {
                _Task_Context_ID = value;
                NotifyContextChanged();
            }
        }
        public string? ContextElementID 
        {
            get
            {
                return _ContextElementID;
            }
            set
            {
                _ContextElementID = value;
                NotifyContextChanged();
            }
        }
        public string? ContextName
        {
            get
            {
                return _contextName;
            }
            set
            {
                _contextName = value;
            }
        }

        public V_TASK_Context? Context 
        {
            get
            {
                return _context;
            }
        }

        public bool ShowToolbar { get; set; } = true;
        public bool TaskNotifyCreator { get; set; } = false;
        
        public DateTime? TaskDefaultDeadline { get; set; }
        public event Action? OnContextChanged;

        public TaskChangesCache ChangesCache { get; }
        public TASKService(ISessionWrapper sessionWrapper, 
                           ILANGProvider _langProvider, 
                           ITASKProvider _TaskProvider,
                           ITEXTProvider _TextProvider,
                           IAUTHProvider _AuthProvider,
                           IMessageService _MessageService,
                           IMailerService _MailerService,
                           IFILEProvider fileProvider)
        {
            this._sessionWrapper = sessionWrapper;
            this._langProvider = _langProvider;
            this._TaskProvider = _TaskProvider;
            this._TextProvider = _TextProvider;
            this._AuthProvider = _AuthProvider;
            this._MessageService = _MessageService;
            this._MailerService = _MailerService;
            this._FileProvider = fileProvider;
            ChangesCache = new TaskChangesCache(sessionWrapper, _langProvider, _TaskProvider, _TextProvider,
                _AuthProvider, _MessageService, _MailerService, fileProvider);
        }

        public async Task<List<V_TASK_Bucket?>> GetBucketList(long? TASK_Context_ID, bool OnlyEnabled = true)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var langList = await _langProvider.GetAll();

                var existingTagList = await _TaskProvider.GetBucketList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (!existingTagList.Any() && langList != null)
                {
                    var defaultTagList = await _TaskProvider.GetBucketDefaultList(_langProvider.GetCurrentLanguageID());

                    foreach (var defaultItem in defaultTagList)
                    {
                        if (defaultItem != null)
                        {
                            var newItemExtended = new List<TASK_Bucket_Extended>();

                            var newItem = new TASK_Bucket()
                            {
                                ID = Guid.NewGuid(),
                                AUTH_Municipality_ID = _sessionWrapper.AUTH_Municipality_ID,
                                Enabled = true,
                                Icon = defaultItem.Icon,
                                SortOrder = defaultItem.SortOrder,
                                Default = defaultItem.Default,
                                TASK_Context_ID = TASK_Context_ID
                            };

                            foreach(var lang in langList)
                            {
                                newItemExtended.Add(new TASK_Bucket_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    TASK_Bucket_ID = newItem.ID,
                                    LANG_Language_ID = lang.ID,
                                    Description = _TextProvider.Get(defaultItem.TEXT_SystemTexts_Code, lang.ID)
                                });
                            }

                            await this.SetBucket(newItem, newItemExtended);
                        }
                    }
                }

                var data = await _TaskProvider.GetBucketList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (OnlyEnabled)
                {
                    return data.Where(p => p.Enabled == true).ToList();
                }

                return data.ToList();
            }

            return new List<V_TASK_Bucket?>();
        }
        public async Task<List<V_TASK_Priority?>> GetPriorityList(long? TASK_Context_ID, bool OnlyEnabled = true)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var langList = await _langProvider.GetAll();

                var existingTagList = await _TaskProvider.GetPriorityList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (!existingTagList.Any() && langList != null)
                {
                    var defaultTagList = await _TaskProvider.GetPriorityDefaultList(_langProvider.GetCurrentLanguageID());

                    foreach (var defaultItem in defaultTagList)
                    {
                        if (defaultItem != null)
                        {
                            var newItemExtended = new List<TASK_Priority_Extended>();

                            var newItem = new TASK_Priority()
                            {
                                ID = Guid.NewGuid(),
                                AUTH_Municipality_ID = _sessionWrapper.AUTH_Municipality_ID,
                                Enabled = true,
                                Icon = defaultItem.Icon,
                                SortOrder = defaultItem.SortOrder,
                                Default = defaultItem.Default,
                                TASK_Context_ID = TASK_Context_ID
                            };

                            foreach (var lang in langList)
                            {
                                newItemExtended.Add(new TASK_Priority_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    TASK_Priority_ID = newItem.ID,
                                    LANG_Language_ID = lang.ID,
                                    Description = _TextProvider.Get(defaultItem.TEXT_SystemTexts_Code, lang.ID)
                                });
                            }

                            await this.SetPriority(newItem, newItemExtended);
                        }
                    }
                }

                var data = await _TaskProvider.GetPriorityList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (OnlyEnabled)
                {
                    return data.Where(p => p.Enabled == true).ToList();
                }

                return data.ToList();
            }

            return new List<V_TASK_Priority?>();
        }
        public async Task<List<V_TASK_Status?>> GetStatusList(long? TASK_Context_ID, bool OnlyEnabled = true)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var langList = await _langProvider.GetAll();

                var existingTagList = await _TaskProvider.GetStatusList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (!existingTagList.Any() && langList != null)
                {
                    var defaultTagList = await _TaskProvider.GetStatusDefaultList(_langProvider.GetCurrentLanguageID());

                    foreach (var defaultItem in defaultTagList)
                    {
                        if (defaultItem != null)
                        {
                            var newItemExtended = new List<TASK_Status_Extended>();

                            var newItem = new TASK_Status()
                            {
                                ID = Guid.NewGuid(),
                                AUTH_Municipality_ID = _sessionWrapper.AUTH_Municipality_ID,
                                Enabled = true,
                                Icon = defaultItem.Icon,
                                SortOrder = defaultItem.SortOrder,
                                CompleteTask = defaultItem.CompleteTask,
                                Default = defaultItem.Default,
                                DefaultCompleteTask = defaultItem.DefaultCompleteTask,
                                TASK_Context_ID = TASK_Context_ID
                            };

                            foreach (var lang in langList)
                            {
                                newItemExtended.Add(new TASK_Status_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    TASK_Status_ID = newItem.ID,
                                    LANG_Language_ID = lang.ID,
                                    Description = _TextProvider.Get(defaultItem.TEXT_SystemTexts_Code, lang.ID)
                                });
                            }

                            await this.SetStatus(newItem, newItemExtended);
                        }
                    }
                }

                var data = await _TaskProvider.GetStatusList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (OnlyEnabled)
                {
                    return data.Where(p => p.Enabled == true).ToList();
                }

                return data.ToList();
            }

            return new List<V_TASK_Status?>();
        }
        public async Task<List<V_TASK_Tag?>> GetTagList(long? TASK_Context_ID, bool OnlyEnabled = true)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var langList = await _langProvider.GetAll();

                var existingTagList = await _TaskProvider.GetTagList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (!existingTagList.Any() && langList != null)
                {
                    var defaultTagList = await _TaskProvider.GetTagDefaultList(_langProvider.GetCurrentLanguageID());

                    foreach (var defaultItem in defaultTagList)
                    {
                        if (defaultItem != null)
                        {
                            var newItemExtended = new List<TASK_Tag_Extended>();

                            var newItem = new TASK_Tag()
                            {
                                ID = Guid.NewGuid(),
                                AUTH_Municipality_ID = _sessionWrapper.AUTH_Municipality_ID,
                                Enabled = true,
                                Color = defaultItem.Color,
                                SortOrder = defaultItem.SortOrder,
                                TASK_Context_ID = TASK_Context_ID
                            };

                            foreach (var lang in langList)
                            {
                                newItemExtended.Add(new TASK_Tag_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    TASK_Tag_ID = newItem.ID,
                                    LANG_Language_ID = lang.ID,
                                    Description = _TextProvider.Get(defaultItem.TEXT_SystemTexts_Code, lang.ID)
                                });
                            }

                            await this.SetTag(newItem, newItemExtended);
                        }
                    }
                }

                var data = await _TaskProvider.GetTagList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID);

                if (OnlyEnabled)
                {
                    return data.Where(p => p.Enabled == true).ToList();
                }

                return data.ToList();
            }

            return new List<V_TASK_Tag?>();
        }
        public async Task<TASK_Bucket> SetBucket(TASK_Bucket Item, List<TASK_Bucket_Extended> ItemExtended)
        {
            await _TaskProvider.SetBucket(Item);

            foreach(var extended in ItemExtended)
            {
                await _TaskProvider.SetBucketExtended(extended);
            }

            return Item;
        }
        public async Task<TASK_Priority> SetPriority(TASK_Priority Item, List<TASK_Priority_Extended> ItemExtended)
        {
            await _TaskProvider.SetPriority(Item);

            foreach (var extended in ItemExtended)
            {
                await _TaskProvider.SetPriorityExtended(extended);
            }

            return Item;
        }
        public async Task<TASK_Status> SetStatus(TASK_Status Item, List<TASK_Status_Extended> ItemExtended)
        {
            await _TaskProvider.SetStatus(Item);

            foreach (var extended in ItemExtended)
            {
                await _TaskProvider.SetStatusExtended(extended);
            }

            return Item;
        }
        public async Task<TASK_Tag> SetTag(TASK_Tag Item, List<TASK_Tag_Extended> ItemExtended)
        {
            await _TaskProvider.SetTag(Item);

            foreach (var extended in ItemExtended)
            {
                await _TaskProvider.SetTagExtended(extended);
            }

            return Item;
        }
        public async Task<long> GetTaskPosition(Guid? TASK_Bucket_ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var tasks = await _TaskProvider.GetTaskList(_sessionWrapper.AUTH_Municipality_ID.Value, _langProvider.GetCurrentLanguageID(), TASK_Context_ID, ContextElementID);

                if (tasks != null)
                {
                    var order = tasks.Where(p => p.TASK_Bucket_ID == TASK_Bucket_ID).Max(p => p.SortOrder);

                    if (order != null)
                    {
                        return order.Value + 1;
                    }
                }
            }

            return 1;
        }
        public async Task<bool> SetContext(long? TASK_Context_ID, string? ContextElementID, string contextName, bool notifyCreator = false, DateTime? defaultDeadline = null)
        {
            if (TASK_Context_ID != null)
            {
                _context = await _TaskProvider.GetContext(TASK_Context_ID.Value, _langProvider.GetCurrentLanguageID());
                _Task_Context_ID = TASK_Context_ID;
                _ContextElementID = ContextElementID;
                _contextName = contextName;
                TaskNotifyCreator = notifyCreator;
                TaskDefaultDeadline = defaultDeadline;
            }
            else
            {
                _context = null;
                _Task_Context_ID = null;
                _ContextElementID = null;
                _contextName = null;
                TaskNotifyCreator = false;
                TaskDefaultDeadline = null;
            }

            NotifyContextChanged();

            return true;
        }
        public async Task<TASK_Task?> CreateTask(long? TASK_Context_ID, string? ContextElementID, Guid? CreatorUserID, bool NotifyCreator = false, string? ContextExternalID = null, string? Description = null, string? Url = null, DateTime? Deadline = null, List<AUTH_Municipal_Users>? Responsible = null, Guid? TaskID = null, string? contextName = null)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var bucketList = await GetBucketList(TASK_Context_ID, true);
                var prioityList = await GetPriorityList(TASK_Context_ID, true);
                var statusList = await GetStatusList(TASK_Context_ID, true);

                var defaultBucket = bucketList.FirstOrDefault(p => p.Default == true);
                var defaultPrio = prioityList.FirstOrDefault(p => p.Default == true);
                var defaultStatus = statusList.FirstOrDefault(p => p.Default == true);

                var newTask = new TASK_Task();
                newTask.CreatedAt = DateTime.Now;

                if (TaskID == null)
                {
                    newTask.ID = Guid.NewGuid();
                }
                else
                {
                    newTask.ID = TaskID.Value;
                }

                newTask.SortOrder = 1;

                if (defaultBucket != null)
                {
                    newTask.TASK_Bucket_ID = defaultBucket.ID;
                }

                if (defaultPrio != null)
                {
                    newTask.TASK_Priority_ID = defaultPrio.ID;
                }

                if (defaultStatus != null)
                {
                    newTask.TASK_Status_ID = defaultStatus.ID;
                }

                newTask.Deadline = Deadline;
                newTask.AUTH_Municipality_ID = _sessionWrapper.AUTH_Municipality_ID.Value;
                newTask.TASK_Context_ID = TASK_Context_ID;
                newTask.ContextElementID = ContextElementID;
                newTask.ContextExternalID = ContextExternalID;
                newTask.Title = Description;
                newTask.Url = Url;
                newTask.CreatedByAUTH_Municipal_User_ID = CreatorUserID;
                newTask.NotifyCreator = NotifyCreator;
                newTask.ContextName = ContextName;
                if (contextName != null)
                {
                    newTask.ContextName = contextName;
                }

                await _TaskProvider.SetTask(newTask);

                if (Responsible != null)
                {
                    foreach (var resp in Responsible)
                    {
                        var newResp = new TASK_Task_Responsible();

                        newResp.ID = Guid.NewGuid();
                        newResp.AUTH_Municipal_Users_ID = resp.ID;
                        newResp.TASK_Task_ID = newTask.ID;
                        newResp.SortDesc = resp.Firstname + " " + resp.Lastname;

                        await _TaskProvider.SetTaskResponsible(newResp);
                    }
                }

                return newTask;
            }

            return null;
        }
        public async Task<bool> CreateEskalation(Guid TASK_Task_ID, DateTime NotificationDate, List<Guid> AUTH_Users_ID)
        {
            var Task = await _TaskProvider.GetTask(TASK_Task_ID);

            if (Task == null)
                return false;

            if (AUTH_Users_ID != null && AUTH_Users_ID.Count() > 0)
            {
                var newEskalation = new TASK_Task_Eskalation()
                {
                    ID = Guid.NewGuid(),
                    PlannedNotificationDate = NotificationDate,
                    TASK_Task_ID = Task.ID
                };

                await _TaskProvider.SetEskalation(newEskalation);

                foreach(var user in AUTH_Users_ID)
                {
                    var dbuser = await _AuthProvider.GetMunicipalUser(user);

                    if (dbuser != null) 
                    {
                        var newResponsible = new TASK_Task_Eskalation_Responsible()
                        {
                            ID = Guid.NewGuid(),
                            AUTH_Municipal_Users_ID = user,
                            TASK_Task_Eskalation_ID = newEskalation.ID,
                            SortDesc = dbuser.Firstname + " " + dbuser.Lastname
                        };

                        await _TaskProvider.SetEskalationResponsible(newResponsible);
                    }
                }
                
            }

            return true;
        }
        public async Task<TASK_Task_Responsible?> SetResponsible(TASK_Task_Responsible Data)
        {
            if (Data != null && Data.TASK_Task_ID != null)
            {
                var Task = await _TaskProvider.GetTask(Data.TASK_Task_ID.Value);

                if (Task != null)
                {
                    if (Data.NotificationDate == null && Data.AUTH_Municipal_Users_ID != null && _sessionWrapper.AUTH_Municipality_ID != null)
                    {
                        var context = "";
                        if (Task.Title != Task.ContextName)
                            context = Task.ContextName;
                        var msg = await _MessageService.GetMessage(Data.AUTH_Municipal_Users_ID.Value, _sessionWrapper.AUTH_Municipality_ID.Value, "NOTIF_TASK_RESPONSIBLE_SET_TEXT", 
                                                                   "NOTIF_TASK_RESPONSIBLE_SET_SHORTTEXT", "NOTIF_TASK_RESPONSIBLE_SET_TITLE", Guid.Parse("7D03E491-5826-4131-A6A1-06C99BE991C9"), true,
                                                                   new List<MSG_Message_Parameters>()
                                                                   {
                                                                       new MSG_Message_Parameters()
                                                                       {
                                                                           Code = "{Aufgabe}",
                                                                           Message = Task.Title + "<br/>" + context
                                                                       }
                                                                   });  //MAIL

                        if (msg != null)
                        {
                            await _MessageService.SendMessage(msg, Task.Url, null);

                            Data.NotificationDate = DateTime.Now;
                        }
                    }

                    return await _TaskProvider.SetTaskResponsible(Data);
                }
            }

            return null;
        }
        private void NotifyContextChanged() => OnContextChanged?.Invoke();
    }
}
