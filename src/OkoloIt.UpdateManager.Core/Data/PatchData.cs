using System.IO.Compression;
using System.Text.Json;

namespace OkoloIt.UpdateManager.Core.Data;

public class PatchData(PatchInfo patchInfo, IEnumerable<VersionFileInfo> newFilesPaths)
{
    public PatchInfo Header { get; set; } = patchInfo;
    public IEnumerable<VersionFileInfo> NewFilesInfo { get; set; } = newFilesPaths;

    public void ExportTo(string patchFolderPath)
    {
        string patchFilePath = $"{Path.Combine(patchFolderPath, Header.Product)} {Header.Version}.patch";
        using Stream fileStream  = new FileStream(patchFilePath, FileMode.Create);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Create, true);

        GeneratePatchInfoFile(Header, archive);

        foreach (VersionFileInfo newFilePath in NewFilesInfo) {
            var fileBytes = File.ReadAllBytes(newFilePath.FilePath);
            var fileName  = Path.GetFileName(newFilePath.FilePath);

            fileName = Path.Combine("Files", fileName);

            ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(fileName, CompressionLevel.SmallestSize);
            using Stream zipStream = zipArchiveEntry.Open();
            zipStream.Write(fileBytes, 0, fileBytes.Length);
        }
    }

    private static void GeneratePatchInfoFile(PatchInfo info, ZipArchive archive)
    {
        using Stream memoryStream = new MemoryStream();
        using TextWriter streamWriter = new StreamWriter(memoryStream);

        string manifest = JsonSerializer.Serialize(info);
        streamWriter.Write(manifest);
        streamWriter.Flush();
        memoryStream.Position = 0;

        ZipArchiveEntry zipArchiveEntry = archive.CreateEntry("Manifest.json", CompressionLevel.SmallestSize);
        using Stream zipStream = zipArchiveEntry.Open();
        memoryStream.CopyTo(zipStream);
    }
}
