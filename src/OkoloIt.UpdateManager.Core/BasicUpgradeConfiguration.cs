namespace OkoloIt.UpdateManager.Core;

public class BasicUpgradeConfiguration
{
    public IReadOnlyCollection<UpdateFileModel> UpdateFileModels { get; init; } = [];
}

public readonly record struct UpdateFileModel(
    string FilePath,
    string Hash);
