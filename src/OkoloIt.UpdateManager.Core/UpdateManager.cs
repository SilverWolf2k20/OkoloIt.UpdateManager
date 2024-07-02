using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace OkoloIt.UpdateManager.Core;

// Чтение json
// 
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
