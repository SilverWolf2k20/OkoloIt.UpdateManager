namespace OkoloIt.UpdateManager.Core.Data;

public class BasicUpgradeConfiguration
{
    public IReadOnlyCollection<UpdateFileModel> UpdateFileModels { get; init; } = [];
}

public readonly record struct UpdateFileModel(
    string FilePath,
    string Hash);
