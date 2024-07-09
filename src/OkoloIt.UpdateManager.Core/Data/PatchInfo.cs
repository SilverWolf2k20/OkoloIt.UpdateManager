namespace OkoloIt.UpdateManager.Core.Data;

public record struct PatchInfo(
    string Product,
    string Version,
    bool IsDraftVersion,
    IEnumerable<VersionFileInfo> AddedFilesPaths,
    IEnumerable<VersionFileInfo> DeletedFilesPaths);
