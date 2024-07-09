using System.Text.Json;

namespace OkoloIt.UpdateManager.Core.Data;

public class BaseVersionInfo
{
    public string Product { get; set; } = string.Empty;
    public string BaseVersion {  get; set; } = string.Empty;

    public IReadOnlyCollection<VersionFileInfo> UpdateFileModels { get; init; } = [];

    public void ExportTo(string filePath)
    {
        string jsonFile = JsonSerializer.Serialize(this);
        File.WriteAllText(filePath, jsonFile);
    }
}
