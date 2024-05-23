using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using PdfSharp.Internal;

namespace ICWebApp.Application.Cache.Tasks;


public enum ChangeType : ushort
{
    Title,
    Description,
    Bucket,
    TaskChecked,
    TaskUnchecked,
    TagsAdded,
    TagsRemoved,
    Status,
    Priority,
    CommentsAdded,
    FilesAdded,
    FilesRemoved,
    CheckItemsAdded,
    CheckItemsRemoved,
    CheckItemsChecked,
    CheckItemsUnchecked,
    CheckItemsEdited,
    Deadline,
    ResponsibleAdded,
    ResponsibleRemoved,
}

public class TaskChangedValue
{
    public ChangeType Type;
    public List<string> Values;
    
    //only needed for removing check items
    public List<string>? __ids { get; set; }

    public TaskChangedValue(ChangeType type, List<string> values)
    {
        this.Type = type;
        this.Values = values;
    }
}

public class TaskChanges
{
    public Guid Id;
    
    private string _originalTitle;
    private string _originalDescription;
    private bool _originallyCompleted;
    private Guid? _originalBucketId;
    private Guid? _originalStatusId;
    private Guid? _originalPriorityId;
    private DateTime? _originalDeadline;

    public Guid TaskId;
    public bool ChangeAllowedFlag { get; set; } = true;
    private DateTime _lastChange;

    public bool ReadyToSend => _lastChange.AddMinutes(2) < DateTime.Now;

    private List<TaskChangedValue> _changes = new List<TaskChangedValue>();

    public TaskChanges(TASK_Task originalTask)
    {
        this.Id = Guid.NewGuid();
        this.TaskId = originalTask.ID;
        this._originalTitle = originalTask.Title;
        this._originalDescription = originalTask.Description;
        this._originallyCompleted = originalTask.CompletedAt != null;
        this._originalBucketId = originalTask.TASK_Bucket_ID;
        this._originalStatusId = originalTask.TASK_Status_ID;
        this._originalPriorityId = originalTask.TASK_Priority_ID;
        this._originalDeadline = originalTask.Deadline;
        _lastChange = DateTime.Now;
    }

    /**
     * Adds a change to a task change buffer object
     * Make sure this object is LOCKED when calling this function
     * @param changedValues -- contains the type of change and corresponding values > (see comments in switch case)
     */
    public void AddChanges(TaskChangedValue changedValue)
    {
        _lastChange = DateTime.Now;
        var changeType = changedValue.Type;
        switch (changeType)
        {
                case ChangeType.Title:
                    //changes contains new title
                    AddTitleChange(changedValue);
                    break;
                case ChangeType.Description:
                    //changes contains new description
                    AddDescriptionChange(changedValue);
                    break;
                case ChangeType.Bucket:
                    //changes contains new bucket id
                    AddBucketChange(changedValue);
                    break;
                case ChangeType.TaskChecked:
                    //changes contains new CompletedAt Date string
                    AddTaskCheckedChange(changedValue);
                    break;
                case ChangeType.TaskUnchecked:
                    //changes contains empty string
                    AddTaskCheckedChange(changedValue);
                    break;
                case ChangeType.TagsAdded:
                    //changes contains ids of all added tags
                    AddTaskTagAddedChange(changedValue);
                    break;
                case ChangeType.TagsRemoved:
                    //changes contains ids of all removed tags
                    AddTaskTagRemovedChange(changedValue);
                    break;
                case ChangeType.Status:
                    //changes contains new status id string
                    AddStatusChange(changedValue);
                    break;
                case ChangeType.Priority:
                    //changes contains new priority id string
                    AddPriorityChange(changedValue);
                    break;
                case ChangeType.CommentsAdded:
                    //changes contains list of the new comments ids
                    AddCommentAddedChange(changedValue);
                    break;
                case ChangeType.FilesAdded:
                    //changes contains list of added fileinfo ids
                    AddFilesAddedChange(changedValue);
                    break;
                case ChangeType.FilesRemoved:
                    //changes contains list of removed fileinfoids
                    AddFilesRemovedChange(changedValue);
                    break;
                case ChangeType.CheckItemsAdded:
                    //changes contains list of added checkitems ids
                    AddCheckItemsAddedChange(changedValue);
                    break;
                case ChangeType.CheckItemsRemoved:
                    //changes contains list of removed checkitems DESCRIPTIONS because it was already deleted from the db
                    //additionally the __ids property of changedValue needs to contain the ids corresponding to the descriptions
                    AddCheckItemsRemovedChange(changedValue);
                    break;
                case ChangeType.CheckItemsChecked:
                    //changes contains list checked checkitems ids
                    AddCheckItemsCheckedChange(changedValue);
                    break;
                case ChangeType.CheckItemsUnchecked:
                    //changes contains list checked checkitems ids
                    AddCheckItemsUnCheckedChange(changedValue);
                    break;
                case ChangeType.CheckItemsEdited:
                    //changes contains list checked checkitems ids
                    AddCheckItemsEditedChange(changedValue);
                    break;
                case ChangeType.Deadline:
                    //changes contains new deadline date string or empty string if null
                    AddDeadlineChange(changedValue);
                    break;
                case ChangeType.ResponsibleAdded:
                    //changes contains list of added responsibles user ids strings
                    AddResponsibleAddedChange(changedValue);
                    break;
                case ChangeType.ResponsibleRemoved:
                    //changes contains list of removed responsibles user ids strings
                    AddResponsibleRemovedChange(changedValue);
                    break;
                default:
                    break;
        }
    }

    private void AddTitleChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        
        _changes = _changes.Where(e => e.Type != ChangeType.Title).ToList();
        var newTitle = changedValue.Values[0];
        
        if (!string.Equals(newTitle, _originalTitle))
            _changes.Add(changedValue);
    }
    private void AddDescriptionChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        _changes = _changes.Where(e => e.Type != ChangeType.Description).ToList();
        var newDescription = changedValue.Values[0];
        
        if(!string.Equals(newDescription, _originalDescription))
            _changes.Add(changedValue);
    }
    private void AddBucketChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        _changes = _changes.Where(e => e.Type != ChangeType.Bucket).ToList();
        var newBucketId = changedValue.Values[0];

        Guid.TryParse(newBucketId, out var id);
        if (id != _originalBucketId)
            _changes.Add(changedValue);
    }
    private void AddStatusChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        _changes = _changes.Where(e => e.Type != ChangeType.Status).ToList();
        var newStautsId = changedValue.Values[0];

        Guid.TryParse(newStautsId, out var id);
        if (id != _originalStatusId)
            _changes.Add(changedValue);
    }
    private void AddPriorityChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        _changes = _changes.Where(e => e.Type != ChangeType.Priority).ToList();
        var newPriorityId = changedValue.Values[0];

        Guid.TryParse(newPriorityId, out var id);
        if (id != _originalPriorityId)
            _changes.Add(changedValue);
    }
    private void AddDeadlineChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count >= 0) return;

        _changes = _changes.Where(e => e.Type != ChangeType.Deadline).ToList();
        var newDeadline = changedValue.Values[0];
        if(newDeadline != _originalDeadline.ToString())
            _changes.Add(changedValue);
    }
    private void AddTaskCheckedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        _changes = _changes.Where(e => e.Type != ChangeType.TaskChecked && e.Type != ChangeType.TaskUnchecked).ToList();
        var newCompleted = !string.IsNullOrEmpty(changedValue.Values[0]);
        if (newCompleted != _originallyCompleted)
            _changes.Add(changedValue);
    }
    private void AddTaskTagAddedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        
        var addedTags = _changes.FirstOrDefault(e => e.Type == ChangeType.TagsAdded) ??
                        new TaskChangedValue(ChangeType.TagsAdded, new List<string>());
        var removedTags = _changes.FirstOrDefault(e => e.Type == ChangeType.TagsRemoved) ??
                          new TaskChangedValue(ChangeType.TagsRemoved, new List<string>());
        
        foreach (var tagId in changedValue.Values)
        {
            if (removedTags.Values.Any(e => e == tagId))
            {
                removedTags.Values.RemoveAll(e => e == tagId);
            }
            else
            {
                addedTags.Values.Add(tagId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.TagsAdded))
            _changes.Add(addedTags);
    }
    private void AddTaskTagRemovedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        var addedTags = _changes.FirstOrDefault(e => e.Type == ChangeType.TagsAdded) ??
                        new TaskChangedValue(ChangeType.TagsAdded, new List<string>());
        var removedTags = _changes.FirstOrDefault(e => e.Type == ChangeType.TagsRemoved) ??
                          new TaskChangedValue(ChangeType.TagsRemoved, new List<string>());

        foreach (var tagId in changedValue.Values)
        {
            if (addedTags.Values.Any(e => e == tagId))
            {
                addedTags.Values.RemoveAll(e => e == tagId);
            }
            else
            {
                removedTags.Values.Add(tagId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.TagsRemoved))
            _changes.Add(removedTags);
    }
    private void AddCommentAddedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        var addedComments = _changes.FirstOrDefault(e => e.Type == ChangeType.CommentsAdded) ??
                         new TaskChangedValue(ChangeType.CommentsAdded, new List<string>());
        foreach (var commentId in changedValue.Values)
        {
            addedComments.Values.Add(commentId);
        }
        
        if(_changes.All(e => e.Type != ChangeType.CommentsAdded))
            _changes.Add(addedComments);
    }
    private void AddFilesAddedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        
        var addedFiles = _changes.FirstOrDefault(e => e.Type == ChangeType.FilesAdded) ??
                         new TaskChangedValue(ChangeType.FilesAdded, new List<string>());

        foreach (var fileInfoId in changedValue.Values)
        {
            addedFiles.Values.Add(fileInfoId);
        }
        
        if(_changes.All(e => e.Type != ChangeType.CommentsAdded))
            _changes.Add(addedFiles);
    }
    private void AddFilesRemovedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        
        var addedFiles = _changes.FirstOrDefault(e => e.Type == ChangeType.FilesAdded) ??
                         new TaskChangedValue(ChangeType.FilesAdded, new List<string>());
        var removedFiles = _changes.FirstOrDefault(e => e.Type == ChangeType.FilesRemoved) ??
                           new TaskChangedValue(ChangeType.FilesRemoved, new List<string>());

        foreach (var fileInfoId in changedValue.Values)
        {
            if (addedFiles.Values.Any(e => e == fileInfoId))
            {
                addedFiles.Values.RemoveAll(e => e == fileInfoId);
            }
            else
            {
                removedFiles.Values.Add(fileInfoId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.FilesRemoved))
            _changes.Add(removedFiles);
    }
    private void AddResponsibleAddedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        
        var addedResponsibles = _changes.FirstOrDefault(e => e.Type == ChangeType.ResponsibleAdded) ??
                         new TaskChangedValue(ChangeType.ResponsibleAdded, new List<string>());
        var removedResponsibles = _changes.FirstOrDefault(e => e.Type == ChangeType.ResponsibleRemoved) ??
                                  new TaskChangedValue(ChangeType.ResponsibleRemoved, new List<string>());

        foreach (var userId in changedValue.Values)
        {
            if (removedResponsibles.Values.Any(e => e == userId))
            {
                removedResponsibles.Values.RemoveAll(e => e == userId);
            }
            else
            {
                addedResponsibles.Values.Add(userId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.ResponsibleAdded))
            _changes.Add(addedResponsibles);
    }
    private void AddResponsibleRemovedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        
        var addedResponsibles = _changes.FirstOrDefault(e => e.Type == ChangeType.ResponsibleAdded) ??
                                new TaskChangedValue(ChangeType.ResponsibleAdded, new List<string>());
        var removedResponsibles = _changes.FirstOrDefault(e => e.Type == ChangeType.ResponsibleRemoved) ??
                                  new TaskChangedValue(ChangeType.ResponsibleRemoved, new List<string>());

        foreach (var userId in changedValue.Values)
        {
            if (addedResponsibles.Values.Any(e => e == userId))
            {
                addedResponsibles.Values.RemoveAll(e => e == userId);
            }
            else
            {
                removedResponsibles.Values.Add(userId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.ResponsibleRemoved))
            _changes.Add(removedResponsibles);
    }
    private void AddCheckItemsAddedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        var addedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsAdded) ??
                             new TaskChangedValue(ChangeType.CheckItemsAdded, new List<string>());

        foreach (var checkItemId in changedValue.Values)
        {
          addedCheckItems.Values.Add(checkItemId);  
        }
        
        if(_changes.All(e => e.Type != ChangeType.CheckItemsAdded))
            _changes.Add(addedCheckItems);
    }
    private void AddCheckItemsCheckedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        var checkedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsChecked) ??
                                new TaskChangedValue(ChangeType.CheckItemsChecked, new List<string>());
        var uncheckedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsUnchecked) ??
                                new TaskChangedValue(ChangeType.CheckItemsUnchecked, new List<string>());
        
        foreach (var checkItemId in changedValue.Values)
        {
            if (uncheckedCheckItems.Values.Any(e => e == checkItemId))
            {
                uncheckedCheckItems.Values.RemoveAll(e => e == checkItemId);
            }
            else
            {
                checkedCheckItems.Values.Add(checkItemId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.CheckItemsChecked))
            _changes.Add(checkedCheckItems);
    }
    private void AddCheckItemsUnCheckedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        var checkedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsChecked) ??
                                new TaskChangedValue(ChangeType.CheckItemsChecked, new List<string>());
        var uncheckedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsUnchecked) ??
                                  new TaskChangedValue(ChangeType.CheckItemsUnchecked, new List<string>());
        
        foreach (var checkItemId in changedValue.Values)
        {
            if (checkedCheckItems.Values.Any(e => e == checkItemId))
            {
                checkedCheckItems.Values.RemoveAll(e => e == checkItemId);
            }
            else
            {
                uncheckedCheckItems.Values.Add(checkItemId);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.CheckItemsUnchecked))
            _changes.Add(uncheckedCheckItems);
    }
    private void AddCheckItemsEditedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;

        var addedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsAdded) ??
                              new TaskChangedValue(ChangeType.CheckItemsAdded, new List<string>());
        var editedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsEdited) ??
                               new TaskChangedValue(ChangeType.CheckItemsEdited, new List<string>());

        foreach (var checkItemId in changedValue.Values.
                     Where(checkItemId => addedCheckItems.Values.All(e => e != checkItemId) && 
                                          editedCheckItems.Values.All(e => e != checkItemId)))
        {
            editedCheckItems.Values.Add(checkItemId);
        }
        
        if(_changes.All(e => e.Type != ChangeType.CheckItemsEdited))
            _changes.Add(editedCheckItems);
    }
    private void AddCheckItemsRemovedChange(TaskChangedValue changedValue)
    {
        if (changedValue.Values.Count <= 0) return;
        if (changedValue.__ids == null) return;
        if (changedValue.__ids.Count != changedValue.Values.Count) return;

        var addedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsAdded) ??
                              new TaskChangedValue(ChangeType.CheckItemsAdded, new List<string>());
        var checkedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsChecked) ??
                                new TaskChangedValue(ChangeType.CheckItemsChecked, new List<string>());
        var uncheckedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsUnchecked) ??
                                  new TaskChangedValue(ChangeType.CheckItemsUnchecked, new List<string>());
        var editedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsEdited) ??
                               new TaskChangedValue(ChangeType.CheckItemsEdited, new List<string>());
        var removedCheckItems = _changes.FirstOrDefault(e => e.Type == ChangeType.CheckItemsRemoved) ??
                                new TaskChangedValue(ChangeType.CheckItemsRemoved, new List<string>());

        for(var i = 0; i < changedValue.__ids.Count; i++)
        {
            var checkItemId = changedValue.__ids[i];
            var checkItemName = changedValue.Values[i];
            
            checkedCheckItems.Values.RemoveAll(e => e == checkItemId);
            uncheckedCheckItems.Values.RemoveAll(e => e == checkItemId);
            editedCheckItems.Values.RemoveAll(e => e == checkItemId);
            if (addedCheckItems.Values.Any(e => e == checkItemId))
            {
                addedCheckItems.Values.RemoveAll(e => e == checkItemId);
            }
            else
            {
                removedCheckItems.Values.Add(checkItemName);
            }
        }
        
        if(_changes.All(e => e.Type != ChangeType.CheckItemsRemoved))
            _changes.Add(removedCheckItems);
    }

    public List<TaskChangedValue> GetChanges()
    {
        return _changes.Where(e => e.Values.Count > 0).ToList();
    }
}