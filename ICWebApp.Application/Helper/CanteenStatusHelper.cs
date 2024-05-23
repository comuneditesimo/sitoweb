namespace ICWebApp.Application.Helper;

public static class CanteenStatusHelper
{
    public static Guid ToSign => Guid.Parse("78D1E01C-3ED0-4AD1-843C-227F66BC83F7");
    public static Guid NotFinished => Guid.Parse("50DF6331-C6C1-4C60-9582-261E9A22509D");
    public static Guid WaitList => Guid.Parse("84A38786-0E41-4563-B29C-4018E1F8941A");
    public static Guid Disabled => Guid.Parse("3687B805-0AB4-4456-B364-418AC8D5168C");
    public static Guid Archived => Guid.Parse("77593932-E209-43C0-A27E-4FA24B1ACC1A");
    public static Guid Committed => Guid.Parse("A5A7CBE4-5879-4BA9-9C76-6B9A3BDF54D0");
    public static Guid Accepted => Guid.Parse("BB45403F-F63B-4314-9A8F-9588C13819FF");
    public static Guid Denied => Guid.Parse("287AE8C6-476C-4F55-8F19-E228403E4787");

    public static bool IsCommitted(Guid? statusId)
    {
        if (statusId == null)
            return false;
        return statusId != ToSign && statusId != NotFinished;
    }
}