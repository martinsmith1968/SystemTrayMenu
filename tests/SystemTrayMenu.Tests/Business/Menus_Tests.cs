// ReSharper disable InconsistentNaming

using FluentAssertions;
using SystemTrayMenu.Business;
using SystemTrayMenu.Business.Types;
using SystemTrayMenu.DataClasses;
using Xunit;
using Xunit.Abstractions;

namespace SystemTrayMenu.Tests.Business;

public class Menus_Tests : IDisposable
{
    private static readonly Random RandomGenerator = new();

    private readonly ITestOutputHelper _outputHelper;

    private readonly List<DirectoryInfo> _tempDirs = new();
    private readonly List<FileInfo> _tempFiles = new();

    public Menus_Tests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private List<RowData> GenerateRowData(int? count = null)
    {
        count ??= RandomGenerator.Next(25, 50);

        var tempDir = Path.GetTempPath();

        var dir = tempDir;

        var rowData = Enumerable.Range(0, count.Value)
            .Select(
                x =>
                {
                    if (x % 10 == 0)
                        dir = Path.Join(tempDir, $"d{x:00}");

                    var fileName = $"f{x % 3:00}";

                    var extension = $"e{x % 2:00}";

                    var dirInfo = new DirectoryInfo(dir);
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                        _tempDirs.Add(dirInfo);
                    }

                    var fullFileName = Path.Join(dirInfo.FullName, $"{fileName}.{extension}");

                    var fileInfo = new FileInfo(fullFileName);
                    _outputHelper.WriteLine($"Creating: {fullFileName}");
                    File.WriteAllText(fileInfo.FullName, null);
                    _tempFiles.Add(fileInfo);

                    return new RowData(false, false, false, 1, fullFileName);
                })
            .ToList();

        return rowData;
    }

    public void Dispose()
    {
        _tempFiles
            .Where(f => f.Exists)
            .Distinct()
            .ToList()
            .ForEach(f => f.Delete());
        _tempDirs
            .Where(d => d.Exists)
            .ToList()
            .ToList()
            .ForEach(f => f.Delete(true));
    }

    [Fact]
    public void FilterRowData_can_filter_duplicates_by_full_file_name()
    {
        const DuplicateHandlingType duplicateHandlingType = DuplicateHandlingType.FullFileName;

        var rowData = GenerateRowData();

        var uniqueFileNames = rowData
            .Select(x => x.FileInfo.FullName)
            .Distinct()
            .ToList();

        uniqueFileNames.Count.Should().BeLessThan(rowData.Count);

        // Act
        var result = Menus.FilterRowData(rowData, duplicateHandlingType);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(uniqueFileNames.Count);
        result
            .Select(x => x.FileInfo.FullName)
            .Should().BeEquivalentTo(uniqueFileNames);
    }

    [Fact]
    public void FilterRowData_can_filter_duplicates_by_file_name_and_extension()
    {
        const DuplicateHandlingType duplicateHandlingType = DuplicateHandlingType.FileNameAndExtension;

        var rowData = GenerateRowData();

        var uniqueFileNames = rowData
            .Select(x => Path.GetFileName(x.FileInfo.FullName))
            .Distinct()
            .ToList();

        uniqueFileNames.Count.Should().BeLessThan(rowData.Count);

        // Act
        var result = Menus.FilterRowData(rowData, duplicateHandlingType);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(uniqueFileNames.Count);
        result
            .Select(x => Path.GetFileName(x.FileInfo.FullName))
            .Should().BeEquivalentTo(uniqueFileNames);
    }

    [Fact]
    public void FilterRowData_can_filter_duplicates_by_file_name_only()
    {
        const DuplicateHandlingType duplicateHandlingType = DuplicateHandlingType.FileNameOnly;

        var rowData = GenerateRowData();

        var uniqueFileNames = rowData
            .Select(x => Path.GetFileNameWithoutExtension(x.FileInfo.FullName))
            .Distinct()
            .ToList();

        uniqueFileNames.Count.Should().BeLessThan(rowData.Count);

        // Act
        var result = Menus.FilterRowData(rowData, duplicateHandlingType);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(uniqueFileNames.Count);
        result
            .Select(x => Path.GetFileNameWithoutExtension(x.FileInfo.FullName))
            .Should().BeEquivalentTo(uniqueFileNames);
    }
}
