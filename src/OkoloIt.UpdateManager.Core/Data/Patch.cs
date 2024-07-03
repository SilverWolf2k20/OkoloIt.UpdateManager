namespace OkoloIt.UpdateManager.Core.Data;

public class Patch(PatchInfo patchInfo, IEnumerable<string> newFilesPaths)
{
    public PatchInfo Info { get; set; } = patchInfo;
    public IEnumerable<string> NewFilesPaths { get; set; } = newFilesPaths;
}
