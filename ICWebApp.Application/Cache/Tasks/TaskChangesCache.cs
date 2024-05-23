using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Timer = System.Timers.Timer;

namespace ICWebApp.Application.Cache.Tasks;

public class TaskChangesCache
{
    private readonly List<TaskChanges> _buffer = new List<TaskChanges>();


    private ISessionWrapper _sessionWrapper;
    private ILANGProvider _langProvider;
    private ITASKProvider _taskProvider;
    private ITEXTProvider _textProvider;
    private IAUTHProvider _authProvider;
    private IMessageService _messageService;
    private IMailerService _mailerService;
    private IFILEProvider _fileProvider;
    public TaskChangesCache(ISessionWrapper sessionWrapper, 
        ILANGProvider langProvider, 
        ITASKProvider taskProvider,
        ITEXTProvider textProvider,
        IAUTHProvider authProvider,
        IMessageService messageService,
        IMailerService mailerService,
        IFILEProvider fileProvider)
    {
        this._sessionWrapper = sessionWrapper;
        this._langProvider = langProvider;
        this._taskProvider = taskProvider;
        this._textProvider = textProvider;
        this._authProvider = authProvider;
        this._messageService = messageService;
        this._mailerService = mailerService;
        this._fileProvider = fileProvider;
    }

    /**
     * Buffers a already or about to be !committed to db! change of a task
     * AddChange should always be called IMMEDIATELY when a change is being committed to the db
     * and ENSURE the correct ORDER OF CHANGES (if a subtask is added and then edited, first applying the edit change and
     * then the add change to the buffer will cause issues!)
     */
    public void AddChange(TASK_Task task, TaskChangedValue changedValue, bool fetchOriginal)
    {
        List<TaskChanges> buffersForTask;
        lock (_buffer)
        {
            buffersForTask = _buffer.Where(e => e.TaskId == task.ID).ToList();
        }
        
        var found = false;
        
        //Check if suitable buffer exists
        foreach(var buffer in buffersForTask)
        {
            lock (buffer)
            {
                if (!buffer.ChangeAllowedFlag) continue;
                
                buffer.AddChanges(changedValue);
                found = true;
                break;
            }
        }

        if (found) return;
        
        //create new
        if (fetchOriginal)
            task = _taskProvider.GetTaskSync(task.ID) ?? task;
        var newBufferItem = new TaskChanges(task);
        newBufferItem.AddChanges(changedValue);
        lock (_buffer)
        {
            _buffer.Add(newBufferItem);
        }
        Console.WriteLine("[TaskChangesCache] Starting new thread for sending update email for task: " + task.Title);
        
        //Start email sending thread
        var thread = new Thread(async void() => await StartRoutine(newBufferItem.Id));
        thread.IsBackground = true; //important but not extremely important
                                    //(foreground threads prevent the process from shutting down before they are done
                                    //which could stall termination of the app before publishing by up to 3 minutes)
        thread.Start();
    }
    public void AddChange(Guid taskId, TaskChangedValue changedValue)
    {
        var task = _taskProvider.GetTaskSync(taskId);
        if (task != null)
        {
            AddChange(task, changedValue, false);
        }
    }
    public void TaskWasDeleted(Guid taskId)
    {
        lock (_buffer)
        {
            _buffer.RemoveAll(e => e.TaskId == taskId);
        }
    }
    private async Task<bool> StartRoutine(Guid bufferElementId)
    {
        var success = false;
        while (true)
        {
            TaskChanges? bufferElement = null;
            lock (_buffer)
            {
                bufferElement = _buffer.FirstOrDefault(e => e.Id == bufferElementId);
            }

            if (bufferElement == null)
                break;

            var exit = false;
            var sendMail = false;
            lock (bufferElement)
            {
                if (!bufferElement.ChangeAllowedFlag) exit = true;
                if (bufferElement.ReadyToSend) sendMail = true;
            }

            if (exit) break;
            if (sendMail)
            {
                Console.WriteLine("[Update Mailer Thread] Sending Mail");
                await SendUpdateMail(bufferElementId);
                success = true;
                break;
            }
            else
            {
                Console.WriteLine("[Update Mailer Thread] Not enough time passed since last change... Sleeping 1 minute");
                Thread.Sleep(60000);
            }
        }
        Console.WriteLine("[Update Mailer Thread] Thread done - Success: " + success);
        return success;
    }
    /**
     * do not call this!
     */
    private async Task SendUpdateMail(Guid bufferElementId)
    {
        TaskChanges? buffer;
        lock (_buffer)
        {
            buffer = _buffer.FirstOrDefault(e => e.Id == bufferElementId);
        }

        if (buffer == null) return;
        lock (buffer)
        {
            buffer.ChangeAllowedFlag = false;
        }

        //after setting ChangeAllowedFlag to false we no longer need to lock the object because no more write
        //accesses will be made
        
        //get actual changes
        var changes = buffer.GetChanges();
        
        //filter out changes that should not trigger email
        changes = changes.Where(e => e.Type != ChangeType.FilesRemoved 
                                     && e.Type != ChangeType.ResponsibleRemoved
                                     && e.Type != ChangeType.Bucket
                                     && e.Type != ChangeType.Bucket ).ToList();
        
        //buffer has no changes
        if (changes.Count == 0) return;

        if (_sessionWrapper.AUTH_Municipality_ID == null) return;

        //Get current state of task from db
        var task = await _taskProvider.GetTask(buffer.TaskId);
        if (task == null) return;
        
        //Send stuff
        //Determine users to be notified
        var usersToBeNotified = new List<Guid>();
        var responsibles = await _taskProvider.GetTaskResponsibleList(buffer.TaskId);
        foreach (var responsible in responsibles)
        {
            if(responsible == null || responsible.AUTH_Municipal_Users_ID == null || responsible.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser?.ID) continue;
            if (changes.Any(e => e.Type == ChangeType.ResponsibleAdded &&
                                 e.Values.Any(uid => uid == _sessionWrapper.CurrentMunicipalUser?.ID.ToString()))) continue;
            usersToBeNotified.Add(responsible.AUTH_Municipal_Users_ID.Value);
        }
        //Meldung notify creator
        if (task.CreatedByAUTH_Municipal_User_ID != null 
            && _sessionWrapper.CurrentMunicipalUser?.ID != task.CreatedByAUTH_Municipal_User_ID.Value
            && (task.NotifyCreator ?? false)
            && usersToBeNotified.All(e => e != task.CreatedByAUTH_Municipal_User_ID.Value))
        {
            usersToBeNotified.Add(task.CreatedByAUTH_Municipal_User_ID.Value);
        }
        
        foreach (var userId in usersToBeNotified)
        {
            var user = await _authProvider.GetMunicipalUser(userId);
            if(user == null) continue;

            var userSettings = await _authProvider.GetMunicipalSettings(user.ID);
            var langId = LanguageSettings.German;
            if (userSettings != null && userSettings.LANG_Languages_ID != null)
                langId = userSettings.LANG_Languages_ID.Value;
            
            var subjectText = _textProvider.Get("MAIL_TASK_UPDATED_SUBJECT", langId);
            subjectText = subjectText.Replace("{Name}", task.Title);

            var titleText = _textProvider.Get("MAIL_TASK_UPDATED_TITLE", langId);
            titleText = titleText.Replace("{Name}", task.Title);
            titleText = titleText.Replace("{ContextName}", task.ContextName);
            
            var mail = new MSG_Mailer();
            mail.ToAdress = user.Email;
            mail.Subject = subjectText;
            mail.MailTitle = titleText;
            mail.Body = await GetMailBody(task, changes, langId);
            mail.PlannedSendDate = DateTime.Now;

            await _mailerService.SendMail(mail, null, _sessionWrapper.AUTH_Municipality_ID.Value, null);
        }

        //Delete from buffer
        lock (_buffer)
        {
            _buffer.Remove(buffer);
        }
    }
    private async Task<string> GetMailBody(TASK_Task task, List<TaskChangedValue> changes, Guid langId)
    {
        var taskWasCheckedOrUnchecked = changes.Any(e => e.Type == ChangeType.TaskChecked 
                                                          || e.Type == ChangeType.TaskUnchecked);
        //remove status changes
        if (taskWasCheckedOrUnchecked)
            changes = changes.Where(e => e.Type != ChangeType.Status).ToList();
        
        //filter out tag changes
        var tagChanges = changes.Where(e => e.Type is ChangeType.TagsAdded or ChangeType.TagsRemoved).ToList();
        changes = changes.Where(e => e.Type is not (ChangeType.TagsAdded or ChangeType.TagsRemoved)).ToList();
        
        //filter out check item changes
        var checkItemChanges = changes.Where(e =>
            e.Type is ChangeType.CheckItemsAdded 
                or ChangeType.CheckItemsRemoved 
                or ChangeType.CheckItemsChecked 
                or ChangeType.CheckItemsUnchecked 
                or ChangeType.CheckItemsEdited).ToList();
        changes = changes.Where(e =>
            e.Type is not (ChangeType.CheckItemsAdded
                or ChangeType.CheckItemsRemoved
                or ChangeType.CheckItemsChecked
                or ChangeType.CheckItemsUnchecked
                or ChangeType.CheckItemsEdited)).ToList();

        var bodyText = _textProvider.Get("MAIL_TASK_UPDATED_BODY_TEXT", langId);
        bodyText = bodyText.Replace("{Name}", task.Title);
        
        var tableTemplate = "<style nonce=\"comunix123\">.task-table td {padding: 5px;}</style>" +
                            "<table class='task-table' width='100%'>{Content}</table>";

        var tableRows = "";
        foreach (var item in changes)
        {
            tableRows += await GetTableRow(task, item, langId);
        }

        if (tagChanges.Count > 0)
        {
            var rowTemplate = "<tr><td valign='top' width='30%'><b>{Title}</b></td><td>{Content}</td></tr>";
            rowTemplate = rowTemplate.Replace("{Title}", _textProvider.Get("TASK_VAL_CHANGED_TAGS", langId));
            var content = "";
            foreach (var tagChange in tagChanges)
            {
                content += await GetTableRow(task, tagChange, langId);
            }
            rowTemplate = rowTemplate.Replace("{Content}", content);
            tableRows += rowTemplate;
        }

        if (checkItemChanges.Count > 0)
        {
            var rowTemplate = "<tr><td valign='top' width='30%'><b>{Title}</b></td><td>{Content}</td></tr>";
            rowTemplate = rowTemplate.Replace("{Title}", _textProvider.Get("TASK_VAL_CHANGED_CHECKITEMS", langId));
            var content = "";
            foreach (var chg in checkItemChanges)
            {
                content += await GetTableRow(task, chg, langId);
            }
            rowTemplate = rowTemplate.Replace("{Content}", content);
            tableRows += rowTemplate;
        }


        tableTemplate = tableTemplate.Replace("{Content}", tableRows);
        bodyText = bodyText.Replace("{ChangedValues}", tableTemplate);
        return bodyText;
    }
    private async Task<string> GetTableRow(TASK_Task task, TaskChangedValue changes, Guid langId)
    {
        var rowTemplate = "<tr><td valign='top' width='30%'><b>{Title}</b></td><td>{Content}</td></tr>";
        var title = "";
        switch (changes.Type)
        {
            case ChangeType.Title:
                title = _textProvider.Get("TASK_VAL_CHANGED_TITLE", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", changes.Values[0]);
                return rowTemplate;
            case ChangeType.Description:
                title = _textProvider.Get("TASK_VAL_CHANGED_DESCRIPTION", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", changes.Values[0]);
                return rowTemplate;
            case ChangeType.Bucket:
                /*title = _textProvider.Get("TASK_VAL_CHANGED_BUCKET", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", await _taskProvider.GetBucketDescription(task.TASK_Bucket_ID!.Value, langId));
                return rowTemplate;*/
                //Not used
                return "";
            case ChangeType.TaskChecked:
                title = _textProvider.Get("TASK_VAL_CHANGED_STATUS", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", _textProvider.Get("TASK_VAL_CHANGED_CHECKED", langId));
                return rowTemplate;
            case ChangeType.TaskUnchecked:
                title = _textProvider.Get("TASK_VAL_CHANGED_STATUS", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", _textProvider.Get("TASK_VAL_CHANGED_UNCHECKED", langId));
                return rowTemplate;
            case ChangeType.TagsAdded:
                var tagsAddedString = _textProvider.Get("TASK_VAL_CHANGED_TAGS_ADDED", langId) + "<br/>";
                foreach (var tagId in changes.Values)
                {
                    Guid.TryParse(tagId, out var id);
                    if (id != Guid.Empty)
                    {
                        tagsAddedString += await _taskProvider.GetTagDescription(id, langId);
                        tagsAddedString += "<br/>";
                    }
                }
                tagsAddedString += "<br/>";
                return tagsAddedString;
            case ChangeType.TagsRemoved:
                var tagsRemovedString = _textProvider.Get("TASK_VAL_CHANGED_TAGS_REMOVED", langId) + "<br/>";
                foreach (var tagId in changes.Values)
                {
                    Guid.TryParse(tagId, out var id);
                    if (id != Guid.Empty)
                    {
                        tagsRemovedString += await _taskProvider.GetTagDescription(id, langId) + "<br/>";
                    }
                }
                tagsRemovedString += "<br/>";
                return tagsRemovedString;
            case ChangeType.Status:
                title = _textProvider.Get("TASK_VAL_CHANGED_STATUS", langId);
                var statusDesc = await _taskProvider.GetStatusDescription(task.TASK_Status_ID.Value, langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", statusDesc);
                return rowTemplate;
            case ChangeType.Priority:
                title = _textProvider.Get("TASK_VAL_CHANGED_PRIORITY", langId);
                var prioDesc = await _taskProvider.GetPriorityDescription(task.TASK_Priority_ID.Value, langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", prioDesc);
                return rowTemplate;
            case ChangeType.CommentsAdded:
                title = _textProvider.Get("TASK_VAL_CHANGED_COMMENTS_ADDED", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                var commentString = "";
                foreach (var commentId in changes.Values)
                {
                    Guid.TryParse(commentId, out var id);
                    if (id != Guid.Empty)
                    {
                        var comment = await _taskProvider.GetTaskComment(id);
                        if (comment != null)
                        {
                            var user = await _authProvider.GetUser(comment.AUTH_Users_ID.Value);
                            commentString += user.Lastname + " " + user.Firstname +
                                             ": <br/>" + comment.Message + "<br/><br/>";
                        }
                    }
                }
                rowTemplate = rowTemplate.Replace("{Content}", commentString);
                return rowTemplate;
            case ChangeType.FilesAdded:
                rowTemplate = rowTemplate.Replace("{Title}",
                    _textProvider.Get("TASK_VAL_CHANGED_FILES_ADDED", langId));
                var filesString = "";
                foreach (var fileInfoId in changes.Values)
                {
                    Guid.TryParse(fileInfoId, out var id);
                    if (id != Guid.Empty)
                    {
                        filesString += await _fileProvider.GetFileName(id) + "<br/>";
                    }
                }
                rowTemplate = rowTemplate.Replace("{Content}", filesString);
                return rowTemplate;
            case ChangeType.FilesRemoved:
                //Not used
                return "";
            case ChangeType.CheckItemsAdded:
                var checkItemsAddedString = _textProvider.Get("TASK_VAL_CHANGED_CHECKITEMS_ADDED", langId) + "<br/>";
                foreach (var checkItemId in changes.Values)
                {
                    Guid.TryParse(checkItemId, out var id);
                    if (id != Guid.Empty)
                    {
                        checkItemsAddedString += await _taskProvider.GetTaskCheckItemDescription(id) + "<br/>";
                    }
                }
                checkItemsAddedString += "<br/>";
                return checkItemsAddedString;
            case ChangeType.CheckItemsRemoved:
                var checkItemsRemovedString = _textProvider.Get("TASK_VAL_CHANGED_CHECKITEMS_REMOVED", langId) + "<br/>";
                foreach (var checkItemString in changes.Values)
                {
                    checkItemsRemovedString += checkItemString + "<br/>";
                }
                checkItemsRemovedString += "<br/>";
                return checkItemsRemovedString;
            case ChangeType.CheckItemsChecked:
                var checkItemsCheckedString = _textProvider.Get("TASK_VAL_CHANGED_CHECKITEMS_CHECKED", langId) + "<br/>";
                foreach (var checkItemId in changes.Values)
                {
                    Guid.TryParse(checkItemId, out var id);
                    if (id != Guid.Empty)
                    {
                        checkItemsCheckedString += await _taskProvider.GetTaskCheckItemDescription(id) + "<br/>";
                    }
                }
                checkItemsCheckedString += "<br/>";
                return checkItemsCheckedString;
            case ChangeType.CheckItemsUnchecked:
                var checkItemsUnCheckedString = _textProvider.Get("TASK_VAL_CHANGED_CHECKITEMS_UNCHECKED", langId) + "<br/>";
                foreach (var checkItemId in changes.Values)
                {
                    Guid.TryParse(checkItemId, out var id);
                    if (id != Guid.Empty)
                    {
                        checkItemsUnCheckedString += await _taskProvider.GetTaskCheckItemDescription(id) + "<br/>";
                    }
                }
                checkItemsUnCheckedString += "<br/>";
                return checkItemsUnCheckedString;
            case ChangeType.CheckItemsEdited:
                var checkItemsEditedString = _textProvider.Get("TASK_VAL_CHANGED_CHECKITEMS_EDITED", langId) + "<br/>";
                foreach (var checkItemId in changes.Values)
                {
                    Guid.TryParse(checkItemId, out var id);
                    if (id != Guid.Empty)
                    {
                        checkItemsEditedString += await _taskProvider.GetTaskCheckItemDescription(id) + "<br/>";
                    }
                }
                checkItemsEditedString += "<br/>";
                return checkItemsEditedString;
            case ChangeType.Deadline:
                var dateString = "";
                if (task.Deadline != null)
                {
                    dateString = task.Deadline.Value.ToShortDateString();
                }
                title = _textProvider.Get("TASK_VAL_CHANGED_DUEDATE", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                rowTemplate = rowTemplate.Replace("{Content}", dateString);
                return rowTemplate;
            case ChangeType.ResponsibleAdded:
                title = _textProvider.Get("TASK_VAL_CHANGED_RESPONSIBLE_ADDED", langId);
                rowTemplate = rowTemplate.Replace("{Title}", title);
                var usersString = "";
                foreach (var userId in changes.Values)
                {
                    Guid id = Guid.Empty;
                    Guid.TryParse(userId, out id);
                    if (id != Guid.Empty)
                    {
                        var user = await _authProvider.GetUser(id);
                        if (user != null)
                        {
                            usersString += user.Lastname + " " + user.Firstname + "<br/>";
                        }
                    }
                }
                rowTemplate = rowTemplate.Replace("{Content}", usersString);
                return rowTemplate;
            case ChangeType.ResponsibleRemoved:
                //Not used;
                return "";
            default:
                return "";
                
        }
    }
}