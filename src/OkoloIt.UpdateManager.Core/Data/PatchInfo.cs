namespace OkoloIt.UpdateManager.Core.Data;

public record struct PatchInfo(
    string ProductName,
    string Version,
    IEnumerable<string> AddedFilesPaths,
    IEnumerable<string> DeletedFilesPaths);
