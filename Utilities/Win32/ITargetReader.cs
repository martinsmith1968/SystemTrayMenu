namespace SystemTrayMenu.Utilities.Win32;

public interface ITargetReader
{
    string FilePath { get; }
    bool IsValid { get; }

    string? ErrorMessage { get; }
}
