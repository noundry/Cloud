{{if .IncludeStorage}}
using Amazon.S3;
using Amazon.S3.Model;

public interface IStorageService
{
    Task<string> SaveFileAsync(string fileName, Stream fileStream);
    Task<byte[]?> GetFileAsync(string fileName);
    Task DeleteFileAsync(string fileName);
}

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;

    public S3StorageService(IAmazonS3 s3Client, IOptions<S3Options> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<string> SaveFileAsync(string fileName, Stream fileStream)
    {
        var key = $"{Guid.NewGuid()}-{fileName}";
        
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = GetContentType(fileName)
        };

        await _s3Client.PutObjectAsync(request);
        return key;
    }

    public async Task<byte[]?> GetFileAsync(string fileName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileName
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileName
        };

        await _s3Client.DeleteObjectAsync(request);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }
}

public class S3Options
{
    public string BucketName { get; set; } = "";
    public string Region { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string ServiceUrl { get; set; } = ""; // For MinIO local development
}
{{end}}