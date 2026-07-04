using Shellify.Core;

namespace SystemTrayMenu.Utilities.Win32;

using System;
using Shellify;
using SystemTrayMenu.Helpers;

#pragma warning disable SA1309

public class LnkFileReader : ITargetReader
{
    private readonly ShellLinkFile? _shellLinkFile;

    public LnkFileReader(string filePath)
    {
        FilePath = filePath;

        try
        {
            ErrorMessage = null;
            _shellLinkFile = ShellLinkFile.Load(filePath);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _shellLinkFile = null;
        }
    }

    public string FilePath { get; init; }

    public string? ErrorMessage { get; init; }

    public bool IsValid => _shellLinkFile != null;

    public string? TargetPath => _shellLinkFile?.LinkInfo?.LocalBasePath;

    public string? TargetArguments => _shellLinkFile?.Arguments;

    public string? WorkingDirectory => _shellLinkFile?.WorkingDirectory;

    public string? Flags => _shellLinkFile?.Header.LinkFlags.GetFlagsText(LinkFlags.None);
}
