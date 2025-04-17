using Microsoft.AspNetCore.Http;

namespace BLL.common;

public class ImageTemplate
{
    public static string UploadImage(IFormFile file, string folderPath)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return fileName;
    }
}
