namespace ICWebApp.Application.Helper;

public static class OrgRequestStatusHelper
{
    public static Guid ToSign => Guid.Parse("75B8B15C-9FDA-4748-A7A2-1F2D409A521E");
    public static Guid Committed => Guid.Parse("D09BFDF6-406B-44B8-9DEF-D37481B0828A");
    public static Guid Accepted => Guid.Parse("334B499B-E6FF-42AF-A083-2438622BBD15");
    public static Guid Declined => Guid.Parse("4DE5C7F3-0D29-4AF6-A362-8CD22BB1F201");
    public static bool IsCommitted(Guid? requestStatusId)
    {
        if(requestStatusId == null)
            return false;

        return requestStatusId != ToSign;
    }
}