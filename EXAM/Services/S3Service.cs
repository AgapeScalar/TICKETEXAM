using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace EXAM.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucketName;
        private readonly string _region;

        public S3Service(IAmazonS3 s3, string bucketName, string region)
        {
            _s3 = s3;
            _bucketName = bucketName;
            _region = region;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string key)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            var response = await _s3.PutObjectAsync(request);
            // Construct public URL (depends on bucket ACL / policies). For simple public buckets:
            var url = $"https://{_bucketName}.s3.{_region}.amazonaws.com/{Uri.EscapeDataString(key)}";
            return url;
        }
    }
}
