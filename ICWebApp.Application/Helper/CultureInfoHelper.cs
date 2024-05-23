using System.Globalization;
using ICWebApp.Application.Settings;

namespace ICWebApp.Application.Helper;

public class CultureInfoHelper
{
    public static CultureInfo CultureGerman => CultureInfo.GetCultureInfo("de-DE");
    public static CultureInfo CultureItalian => CultureInfo.GetCultureInfo("it-IT");

    public static CultureInfo GetCultureInfoByLangId(Guid langId)
    {
        if (langId == LanguageSettings.German)
            return CultureGerman;
        if (langId == LanguageSettings.Italian)
            return CultureItalian;
        return CultureGerman;
    }
}