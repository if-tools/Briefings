using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using IFToolsBriefings.Server.Data;
using IFToolsBriefings.Shared.Data.Models;
using IFToolsBriefings.Shared.Data.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace IFToolsBriefings.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController(DatabaseContext databaseContext, IAmazonS3 s3Client) : Controller
    {
        private static readonly string S3BucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");
        private static readonly string S3BucketBasePath = "https://" + S3BucketName + ".s3.amazonaws.com/{0}";
        
        [HttpGet("[action]")]
        public async Task<ActionResult<bool>> CheckIfBriefingExists(string id)
        {
            var briefing = await databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id.ToString() == id.ToString());
            
            return briefing != null;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<Briefing>> GetBriefing(string id, string viewPassword = null)
        {
            var briefing = await databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id.ToString() == id);
            if (briefing == null) return NotFound();

            if (briefing.IsPrivate)
            {
                // let the caller know that this briefing is private
                if (string.IsNullOrEmpty(viewPassword)) return new Briefing { Id = ObjectId.Parse(id), ViewPasswordHash = "notnull" };
                if (!PasswordHasher.Check(briefing.ViewPasswordHash, viewPassword)) return Unauthorized();
            }

            return briefing;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<Briefing>> GetBriefingToEdit(string id, string editPassword)
        {
            if (string.IsNullOrEmpty(editPassword)) return BadRequest();
            
            var briefing = await databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id.ToString() == id.ToString());
            if (briefing == null) return NotFound();

            if (!PasswordHasher.Check(briefing.EditPasswordHash, editPassword))
            {
                return Unauthorized();
            }

            return briefing;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<Briefing[]>> GetBriefings(BriefingSearchMethod searchMethod, string query)
        {
            var briefings = databaseContext.Briefings.Where(entry => entry.ViewPasswordHash.Trim().Length == 0);

            IQueryable<Briefing> searchResults = null;
            switch (searchMethod)
            {
                case BriefingSearchMethod.ByAuthor:
                    searchResults = briefings.Where(entry => entry.Author.ToLower().Contains(query.ToLower()));
                    break;
                case BriefingSearchMethod.ByOrigin:
                    searchResults = briefings.Where(entry => entry.Origin.ToLower().Contains(query.ToLower()));
                    break;
                case BriefingSearchMethod.ByDestination:
                    searchResults = briefings.Where(entry => entry.Destination.ToLower().Contains(query.ToLower()));
                    break;
            }

            if (searchResults == null) return Array.Empty<Briefing>();
            
            var result = searchResults.ToList();

            return result.ToArray();
        }
        
        [HttpPost("[action]")]
        public async Task<ActionResult> MakeBriefing(Briefing newBriefing)
        {
            if (newBriefing == null) return BadRequest();

            newBriefing.EditPasswordHash = PasswordHasher.Hash(newBriefing.EditPassword);
            newBriefing.ViewPasswordHash = string.IsNullOrEmpty(newBriefing.ViewPassword)
                ? ""
                : PasswordHasher.Hash(newBriefing.ViewPassword);

            newBriefing.Id = ObjectId.GenerateNewId();
            newBriefing.StringId = newBriefing.Id.ToString();
            
            await databaseContext.Briefings.AddAsync(newBriefing);
            await databaseContext.SaveChangesAsync();
            return Ok(newBriefing.Id.ToString());
        }
        
        [HttpPost("[action]")]
        public async Task<ActionResult> EditBriefing(EditBriefingPostParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.EditPassword))
                return Unauthorized();

            var editedBriefing = parameters.EditedBriefing;
            
            var originalBriefing = await databaseContext.Briefings.SingleOrDefaultAsync(entry => entry.Id.ToString() == parameters.Id);
            if (originalBriefing == null) return NotFound();

            if (!PasswordHasher.Check(originalBriefing.EditPasswordHash, parameters.EditPassword))
                return Unauthorized();
            
            editedBriefing.ViewPasswordHash = editedBriefing.ViewPassword == "none"
                ? ""
                : string.IsNullOrWhiteSpace(editedBriefing.ViewPassword) ? editedBriefing.ViewPasswordHash : PasswordHasher.Hash(editedBriefing.ViewPassword);

            var removedAttachments = originalBriefing.AttachmentsArray.Except(editedBriefing.AttachmentsArray).ToList();
            
            var totalSize = await databaseContext.AttachmentsTotalSize.FirstAsync();
            foreach (var a in removedAttachments)
            {
                var attachment = databaseContext.Attachments.SingleOrDefault(i => i.Guid == a);
                if (attachment == null) continue;

                var request = new DeleteObjectRequest
                {
                    BucketName = S3BucketName,
                    Key = attachment.FileName
                };
                await s3Client.DeleteObjectAsync(request);

                // decrement total size of attachments
                totalSize.Size -= attachment.FileSize;
                
                databaseContext.Attachments.Remove(attachment);
            }
            
            await databaseContext.SaveChangesAsync();
            
            editedBriefing.Id = originalBriefing.Id;
            databaseContext.Entry(originalBriefing).CurrentValues.SetValues(editedBriefing);
            await databaseContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<bool>> CheckPassword(string id, string editPassword = null, string viewPassword = null)
        {
            if (!string.IsNullOrEmpty(editPassword) && !string.IsNullOrEmpty(viewPassword)) return BadRequest();
            
            var briefing = await databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id.ToString() == id.ToString());
            if (briefing == null) return NotFound();

            if (!string.IsNullOrEmpty(editPassword))
            {
                return PasswordHasher.Check(briefing.EditPasswordHash, editPassword);
            }
            
            if (!string.IsNullOrEmpty(viewPassword))
            {
                return PasswordHasher.Check(briefing.ViewPasswordHash, viewPassword);
            }
            
            // Return true if there's no view password set. This is used by the View Briefing page.
            if (string.IsNullOrEmpty(editPassword) && string.IsNullOrEmpty(viewPassword) &&
                string.IsNullOrEmpty(briefing.ViewPasswordHash)) return true;

            return false;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<FileAttachment[]>> GetAttachments(string guidsJson)
        {
            List<FileAttachment> attachments = new List<FileAttachment>();
            var guids = JsonConvert.DeserializeObject<string[]>(guidsJson);
            if (guids == null) return BadRequest();
            
            foreach (var guid in guids)
            {
                var attachment = await databaseContext.Attachments.SingleOrDefaultAsync(entity => entity.Guid == guid);
                if (attachment == null) continue;
                
                attachment.FileUrl = string.Format(S3BucketBasePath, attachment.FileName);
                attachments.Add(attachment);
            }

            return attachments.ToArray();
        }

        [HttpGet("[action]")]
        public async Task<string> GetAppVersion()
        {
            return JsonConvert.DeserializeObject<VersionFile>(await System.IO.File.ReadAllTextAsync("version.json"))
                ?.Version ?? "N/A";
        }
    }
}
