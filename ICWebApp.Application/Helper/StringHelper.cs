using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ICWebApp.Application.Helper
{
    public static class StringHelper
    {
        public static bool MarkupStringIsNullOrWhiteSpace([NotNullWhen(false)] string? Value)
        {
            string purgedValue = Regex.Replace(Value ?? "", "<.*?>", String.Empty);
            return string.IsNullOrWhiteSpace(purgedValue.Replace(" ", "").Replace(Environment.NewLine, ""));
        }
    }
}
