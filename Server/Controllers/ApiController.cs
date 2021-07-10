using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IFToolsBriefings.Server.Data;
using IFToolsBriefings.Shared.Data.Models;
using IFToolsBriefings.Shared.Data.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace IFToolsBriefings.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : Controller
    {
        private readonly DatabaseContext _databaseContext = new ();
        
        [HttpGet("[action]")]
        public async Task<ActionResult<bool>> CheckIfBriefingExists(int id)
        {
            var briefing = await _databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id == id);
            
            return briefing != null;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<Briefing>> GetBriefing(int id, string viewPassword = null)
        {
            var briefing = await _databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id == id);
            if (briefing == null) return NotFound();

            if (briefing.IsPrivate)
            {
                // let the caller know that this briefing is private
                if (string.IsNullOrEmpty(viewPassword)) return new Briefing { Id = id, ViewPasswordHash = "notnull" };
                if (!PasswordHasher.Check(briefing.ViewPasswordHash, viewPassword)) return Unauthorized();
            }

            return briefing;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<Briefing>> GetBriefingToEdit(int id, string editPassword)
        {
            if (string.IsNullOrEmpty(editPassword)) return BadRequest();
            
            var briefing = await _databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id == id);
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
            var briefings = _databaseContext.Briefings.Where(entry => entry.ViewPasswordHash.Trim().Length == 0);

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
            
            await _databaseContext.Briefings.AddAsync(newBriefing);
            await _databaseContext.SaveChangesAsync();

            return Ok(newBriefing.Id);
        }
        
        [HttpPost("[action]")]
        public async Task<ActionResult> EditBriefing(EditBriefingPostParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.EditPassword))
                return Unauthorized();

            var editedBriefing = parameters.EditedBriefing;
            
            var originalBriefing = await _databaseContext.Briefings.SingleOrDefaultAsync(entry => entry.Id == parameters.Id);
            if (originalBriefing == null) return NotFound();

            if (!PasswordHasher.Check(originalBriefing.EditPasswordHash, parameters.EditPassword))
                return Unauthorized();
            
            editedBriefing.ViewPasswordHash = editedBriefing.ViewPassword == "none"
                ? ""
                : string.IsNullOrWhiteSpace(editedBriefing.ViewPassword) ? editedBriefing.ViewPasswordHash : PasswordHasher.Hash(editedBriefing.ViewPassword);
            
            _databaseContext.Entry(originalBriefing).CurrentValues.SetValues(editedBriefing);
            await _databaseContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<bool>> CheckPassword(int id, string editPassword = null, string viewPassword = null)
        {
            if (!string.IsNullOrEmpty(editPassword) && !string.IsNullOrEmpty(viewPassword)) return BadRequest();
            
            var briefing = await _databaseContext.Briefings.SingleOrDefaultAsync(entity => entity.Id == id);
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
                var attachment = await _databaseContext.Attachments.SingleOrDefaultAsync(entity => entity.Guid == guid);
                
                if(attachment != null) attachments.Add(attachment);
            }

            return attachments.ToArray();
        }

        [HttpGet("[action]")]
        public async Task<string> GetAppVersion()
        {
            return JsonConvert.DeserializeObject<VersionFile>(await System.IO.File.ReadAllTextAsync("appVersion.json"))
                ?.Version ?? "N/A";
        }
    }
}
