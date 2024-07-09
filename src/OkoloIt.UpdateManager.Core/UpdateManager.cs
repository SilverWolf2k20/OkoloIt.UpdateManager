using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using OkoloIt.UpdateManager.Core.Data;

namespace OkoloIt.UpdateManager.Core;

public class UpdateManager
{
    public static BaseVersionInfo CreateBasicUpgradeConfiguration(string basicVersionFolderPath)
    {
        // Получение контрольных сумм
        List<VersionFileInfo> filesInfo = GetUpdateFileModels(basicVersionFolderPath);

        return new BaseVersionInfo() {
            UpdateFileModels = filesInfo
        };
    }

    public static PatchData CreatePatch(string newVersionFolderPath, BaseVersionInfo baseVersionInfo, string patchVersion)
    {
        List<VersionFileInfo> newFilesInfo = GetUpdateFileModels(newVersionFolderPath);

        // Удаленные.
        var deleted = baseVersionInfo.UpdateFileModels.ExceptBy(newFilesInfo.Select(x => x.FilePath), x => x.FilePath)
            .Select(x => new VersionFileInfo(Path.Combine(newVersionFolderPath, x.FilePath), x.Hash))
            .ToList();

        // Добавленные.
        var added   = newFilesInfo.ExceptBy(baseVersionInfo.UpdateFileModels.Select(x => x.FilePath), x => x.FilePath)
            .Select(x => new VersionFileInfo(Path.Combine(newVersionFolderPath, x.FilePath), x.Hash))
            .ToList();

        var updated = baseVersionInfo.UpdateFileModels.Except(deleted)
            .ExceptBy(newFilesInfo.Select(x => x.Hash), x => x.Hash)
            .Select(x => new VersionFileInfo(Path.Combine(newVersionFolderPath, x.FilePath), x.Hash))
            .ToList();

        PatchInfo patchInfo = new(
            baseVersionInfo.Product,
            patchVersion,
            patchVersion.Contains('-'),
            added,
            deleted);

        return new PatchData(patchInfo, updated);
    }

    public void ReadPatchFile(string patchFilePath)
    {
        using Stream fileStream = new FileStream(patchFilePath, FileMode.Open);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Read, true);
        //archive.Entries

        // Получение списка патчей
    }

    private static List<string> GetAllFilePaths(string folderPath)
    {
        DirectoryInfo directoryInfo = new(folderPath);
        List<string> filePaths = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
            .Select(x => x.FullName)
            .ToList();

        return filePaths;
    }

    private static List<VersionFileInfo> GetUpdateFileModels(string basicVersionFolderPath)
    {
        // Чтение всех файлов
        List<string> filePaths = GetAllFilePaths(basicVersionFolderPath);

        // Получение контрольных сумм
        List<VersionFileInfo> filesInfo = new(filePaths.Count);
        foreach (string filePath in filePaths) {
            var inputBytes = File.ReadAllBytes(filePath);
            var hashBytes = MD5.HashData(inputBytes);

            StringBuilder builder = new();
            foreach (byte hashByte in hashBytes)
                builder.Append(hashByte.ToString("X2"));


            filesInfo.Add(new VersionFileInfo(
                Path.GetRelativePath(basicVersionFolderPath, filePath),
                builder.ToString()));
        }

        return filesInfo;
    }
}
