using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

using OkoloIt.UpdateManager.Core.Data;

namespace OkoloIt.UpdateManager.Core;

public class UpdateManager
{
    public static BasicUpgradeConfiguration CreateBasicUpgradeConfiguration(string basicVersionFolderPath)
    {
        // Получение контрольных сумм
        List<UpdateFileModel> filesInfo = GetUpdateFileModels(basicVersionFolderPath);

        return new BasicUpgradeConfiguration() {
            UpdateFileModels = filesInfo
        };
    }

    public static void CreatePatch(string newVersionFolderPath, BasicUpgradeConfiguration upgradeConfiguration)
    {
        List<UpdateFileModel> newFilesInfo = GetUpdateFileModels(newVersionFolderPath);

        // Удаленные.
        var deleted = upgradeConfiguration.UpdateFileModels.ExceptBy(newFilesInfo.Select(x => x.FilePath), x => x.FilePath)
            .ToList();

        // Добавленные.
        var added   = newFilesInfo.ExceptBy(upgradeConfiguration.UpdateFileModels.Select(x => x.FilePath), x => x.FilePath)
            .ToList();

        var updated = upgradeConfiguration.UpdateFileModels.Except(deleted)
            .ExceptBy(newFilesInfo.Select(x => x.Hash), x => x.Hash)
            .ToList();

        added.ForEach(x => Trace.WriteLine($"[DBG]: Добавлен {x}"));
        deleted.ForEach(x => Trace.WriteLine($"[DBG]: Удален {x}"));
        updated.ForEach(x => Trace.WriteLine($"[DBG]: Обновлен {x}"));
    }

    public void CreatePatchFile(Patch patch, string patchFolderPath)
    {
        string patchFilePath = $"{Path.Combine(patchFolderPath, patch.Info.ProductName)} {patch.Info.Version}.patch";
        using Stream fileStream = new FileStream(patchFilePath, FileMode.CreateNew);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Create, true);

        GeneratePatchInfoFile(patch.Info, archive);

        foreach (string newFilePath in patch.NewFilesPaths) {
            var fileBytes = File.ReadAllBytes(newFilePath);
            var fileName = Path.GetFileName(newFilePath);

            ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(fileName, CompressionLevel.SmallestSize);
            using Stream zipStream = zipArchiveEntry.Open();
            zipStream.Write(fileBytes, 0, fileBytes.Length);
        }
    }

    public void ReadPatchFile(string patchFilePath)
    {
        using Stream fileStream = new FileStream(patchFilePath, FileMode.Open);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Read, true);
        //archive.Entries

        // Получение списка патчей
    }

    private void GeneratePatchInfoFile(PatchInfo info, ZipArchive archive)
    {
        using Stream memoryStream = new MemoryStream();
        using TextWriter streamWriter = new StreamWriter(memoryStream);

        streamWriter.WriteLine($"Product: {info.ProductName}");
        streamWriter.WriteLine($"Version: {info.Version}");
        streamWriter.Flush();
        memoryStream.Position = 0;

        ZipArchiveEntry zipArchiveEntry = archive.CreateEntry("Manifest.json", CompressionLevel.SmallestSize);
        using Stream zipStream = zipArchiveEntry.Open();
        memoryStream.CopyTo(zipStream);
    }

    private static List<string> GetAllFilePaths(string folderPath)
    {
        DirectoryInfo directoryInfo = new(folderPath);
        List<string> filePaths = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
            .Select(x => x.FullName)
            .ToList();

        return filePaths;
    }

    private static List<UpdateFileModel> GetUpdateFileModels(string basicVersionFolderPath)
    {
        // Чтение всех файлов
        List<string> filePaths = GetAllFilePaths(basicVersionFolderPath);

        // Получение контрольных сумм
        List<UpdateFileModel> filesInfo = new(filePaths.Count);
        foreach (string filePath in filePaths) {
            var inputBytes = File.ReadAllBytes(filePath);
            var hashBytes = MD5.HashData(inputBytes);

            StringBuilder builder = new();
            foreach (byte hashByte in hashBytes)
                builder.Append(hashByte.ToString("X2"));


            filesInfo.Add(new UpdateFileModel(
                Path.GetRelativePath(basicVersionFolderPath, filePath),
                builder.ToString()));
        }

        return filesInfo;
    }
}
