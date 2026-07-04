using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser.Model;

namespace SystemTrayMenu.Utilities.Win32;

public class UrlFileReader : ITargetReader
{
    private const string Section_InternetShortcut = "InternetShortcut";

    private static readonly IniParser.FileIniDataParser IniFileParser = new()
    {
        Parser =
        {
            Configuration =
            {
                CaseInsensitive = false,
                AllowKeysWithoutSection = true,
                AllowDuplicateKeys = false,
                OverrideDuplicateKeys = false,
                ConcatenateDuplicateKeys = false,
                ThrowExceptionsOnError = false,
                AllowDuplicateSections = false,
                AllowCreateSectionsOnFly = true
            }
        }
    };

    private readonly IniData? _data;

    public UrlFileReader(string filePath)
    {
        FilePath = filePath;

        try
        {
            ErrorMessage = null;
            _data = IniFileParser.ReadFile(filePath);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _data = null;
        }
    }

    public string FilePath { get; }

    public bool IsValid => _data?[Section_InternetShortcut] != null;

    public string? ErrorMessage { get; }

    public string? TargetURL => _data?[Section_InternetShortcut]["URL"];

    public string? IconFile => _data?[Section_InternetShortcut]["IconFile"];

    public string? IconIndex => _data?[Section_InternetShortcut]["IconIndex"];
}
