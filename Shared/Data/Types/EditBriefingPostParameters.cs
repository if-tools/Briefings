using IFToolsBriefings.Shared.Data.Models;

namespace IFToolsBriefings.Shared.Data.Types
{
    public class EditBriefingPostParameters
    {
        public string Id { get; set; }
        public Briefing EditedBriefing { get; set; }
        public string EditPassword { get; set; }

        public EditBriefingPostParameters(string id, Briefing editedBriefing, string editPassword)
        {
            Id = id;
            EditedBriefing = editedBriefing;
            EditPassword = editPassword;
        }
    }
}