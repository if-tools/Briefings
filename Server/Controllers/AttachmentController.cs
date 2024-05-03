using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using IFToolsBriefings.Server.Data;
using IFToolsBriefings.Shared.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFToolsBriefings.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController(DatabaseContext databaseContext, IAmazonS3 s3Client) : Controller
    {
        private readonly string _s3BucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");
        private long MaxBucketSize = 5000000000;
        
        [HttpPost("[action]")]
        public async Task<ActionResult> Create(IFormFile file)
        {
            if (file == null) return BadRequest("File is null.");
            
            var totalSize = await databaseContext.AttachmentsTotalSize.FirstOrDefaultAsync();
            if (totalSize == null)
            {
                var result = await databaseContext.AddAsync(new AttachmentsTotalSize());
                totalSize = result.Entity;
            }
            if (totalSize.Size >= MaxBucketSize)
            {
                return BadRequest("Exceeded maximum allowed storage size.");
            }
            
            var attachment = new FileAttachment();
            attachment.Guid = Guid.NewGuid().ToString();
            var fileType = file.FileName.Split('.').LastOrDefault();
            attachment.FileName = attachment.Guid + "." + (fileType?.ToLower() ?? "png");

            var request = new PutObjectRequest
            {
                BucketName = _s3BucketName,
                Key = attachment.FileName,
                InputStream = file.OpenReadStream(),
            };
            
            request.Metadata.Add("Content-Type", file.ContentType);
            await s3Client.PutObjectAsync(request);
            
            attachment.CreatedOn = DateTime.Now;
            attachment.FileSize = file.Length;
            
            // increment total size of attachments
            totalSize.Size += attachment.FileSize;

            await databaseContext.Attachments.AddAsync(attachment);
            await databaseContext.SaveChangesAsync();
                
            return Ok(attachment.Guid);
        }

        [HttpDelete("[action]")]
        public async Task<ActionResult> Revert()
        {
            string content;
            using (StreamReader sr = new StreamReader(Request.Body))
            {
                content = await sr.ReadToEndAsync();
            }
            
            var attachment = databaseContext.Attachments.SingleOrDefault(i => i.Guid == content);
            if (attachment == null) return NotFound("File does not exist.");

            var request = new DeleteObjectRequest
            {
                BucketName = _s3BucketName,
                Key = attachment.FileName
            };
            await s3Client.DeleteObjectAsync(request);
            
            // decrement total size of attachments
            var totalSize = await databaseContext.AttachmentsTotalSize.FirstAsync();
            totalSize.Size -= attachment.FileSize;
            
            databaseContext.Attachments.Remove(attachment);
            
            await databaseContext.SaveChangesAsync();
            
            return Ok();
        }
    }
}