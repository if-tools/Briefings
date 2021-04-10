using System;
using System.IO;
using System.Linq;
using IFToolsBriefings.Data.Models;
using System.Threading.Tasks;
using IFToolsBriefings.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFToolsBriefings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : Controller
    {
        private const string BaseAttachmentsPath = "wwwroot/images/attachments";
        private readonly DatabaseContext _databaseContext = new();

        [HttpPost("[action]")]
        public async Task<ActionResult> Create(IFormFile file)
        {
            if (file == null) return BadRequest("File is null.");

            var attachment = new FileAttachment();
            
            CheckAttachmentsDirectory();
            
            attachment.Guid = Guid.NewGuid().ToString();
            
            var fileType = file.FileName.Split('.').LastOrDefault();
            attachment.FileName = attachment.Guid + "." + (fileType ?? "png");
            
            attachment.CreatedOn = DateTime.Now;
            attachment.FileSize = file.Length;

            await using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                await using (var fs = new FileStream(Path.Combine(BaseAttachmentsPath, attachment.FileName), FileMode.OpenOrCreate))
                {
                    await memoryStream.CopyToAsync(fs);
                    fs.Flush();
                }
            }

            await _databaseContext.Attachments.AddAsync(attachment);
            await _databaseContext.SaveChangesAsync();
                
            return Ok(attachment.Guid);
        }

        [HttpDelete("[action]")]
        public async Task<ActionResult> Revert()
        {
            CheckAttachmentsDirectory();
            
            string content;
            using (StreamReader sr = new StreamReader(Request.Body))
            {
                content = await sr.ReadToEndAsync();
            }
            
            var attachment = _databaseContext.Attachments.SingleOrDefault(i => i.Guid == content);
            if (attachment == null) return BadRequest("File does not exist.");
            
            try
            {
                System.IO.File.Delete(Path.Combine(BaseAttachmentsPath, attachment.FileName));
                
                attachment.Deleted = true;
                
                await _databaseContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest($"Error encountered on server. Message:'{e.Message}' when writing an object.");
            }
        }

        [HttpDelete("[action]")]
        public async Task<ActionResult> Remove(string id)
        {
            CheckAttachmentsDirectory();
            
            var attachment = _databaseContext.Attachments.SingleOrDefault(i => i.Guid == id);
            if (attachment == null) return BadRequest("File does not exist.");

            try
            {
                System.IO.File.Delete(Path.Combine(BaseAttachmentsPath, attachment.FileName));

                attachment.Deleted = true;
                
                await _databaseContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest($"Error encountered on server. Message:'{e.Message}' when writing an object.");
            }
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult> Load(string id)
        {
            var attachment = await _databaseContext.Attachments.SingleOrDefaultAsync(i => i.Guid == id);
            if (attachment == null) return BadRequest("File does not exist.");

            string fileName = attachment.FileName;
            string fileType = fileName.Split('.')[1];

            return File(new FileStream(Path.Combine(BaseAttachmentsPath, fileName), FileMode.Open), fileType == "png" ? "image/png" : "image/jpeg", fileName);        
        }

        private void CheckAttachmentsDirectory()
        {
            if (!Directory.Exists(BaseAttachmentsPath))
            {
                Directory.CreateDirectory(BaseAttachmentsPath);
            }
        }
    }
}