using System.Linq;
using System.Threading.Tasks;
using IFToolsBriefings.Data;
using IFToolsBriefings.Data.Models;
using IFToolsBriefings.Data.Types;
using IFToolsBriefings.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace IFToolsBriefings.Pages
{
    public partial class EditBriefing
    {
        [Parameter]
        public string BriefingId { get; set; }

        private int _actualBriefingId;
        private bool _showLoadingIndicator;
        
        private bool _authenticated;
        private AuthenticationModal _authModal;
        
        private Briefing _editedBriefing;
        
        private string _briefingType = "Public";
        
        public EditBriefing() : base(new DatabaseContext()) { }
        
        protected override void OnInitialized()
        {
            if (string.IsNullOrEmpty(BriefingId) || !int.TryParse(BriefingId, out _actualBriefingId))
            {
                NavManager.NavigateTo("/");
            
                return;
            }
            
            CurrentPage.SetCurrentPageName("Edit Briefing");
            
            _editedBriefing = DatabaseContext.Briefings.SingleOrDefault(entity => entity.Id == _actualBriefingId);
            if (_editedBriefing == null)
            {
                NavManager.NavigateTo("/");
            
                return;
            }
            
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _authModal.Authenticate += Authenticate;
                FplModal.FplReceived += FlightPlanReceived;

                JsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/editBriefing.js");
            }
        }

        private void LoadData(Briefing briefing)
        {
            Server = briefing.Server;
            DepartureAirport = briefing.Origin;
            DepartureRunway = briefing.OriginRunway;
            ArrivalAirport = briefing.Destination;
            ArrivalRunway = briefing.DestinationRunway;
            FlightLevel = briefing.FlightLevel;
            CruiseSpeed = briefing.CruiseSpeed;
            DepartureTimeSelect.SetDateTime(briefing.DepartureTime);
            TimeEnrouteSelect.SetTimeSpan(briefing.GetTimeEnroute());
            FlightPlan = briefing.FlightPlan;
            Author = briefing.Author;
            Remarks = briefing.Remarks;
            
            _briefingType = string.IsNullOrWhiteSpace(briefing.ViewPasswordHash) ? "Public" : "Private";

            SpeedType = CruiseSpeed < 4 ? SpeedType.Mach : SpeedType.TrueAirspeed;
            ChangeSpeedType(SpeedType);
            
            StateHasChanged();
        }

        private async void Submit()
        {
            if (!_authenticated) return;
            if (!ValidateInputData()) return;
            
            var filepondStatus = await JsModule.InvokeAsync<bool>("checkIfFilepondBusy");
            if (filepondStatus)
            {
                CurrentPage.ShowNotification("Files are still uploading.");
                return;
            }
            
            _showLoadingIndicator = true;
            await InvokeAsync(StateHasChanged);

            var attachments = await JsModule.InvokeAsync<string[]>("getFilepondFileIds");
            
            // Edit the original briefing entity
            _editedBriefing.Server = Server;
            _editedBriefing.Origin = DepartureAirport.ToUpper();
            _editedBriefing.OriginRunway = DepartureRunway.ToUpper();
            _editedBriefing.Destination = ArrivalAirport.ToUpper();
            _editedBriefing.DestinationRunway = ArrivalRunway.ToUpper();
            _editedBriefing.FlightLevel = FlightLevel;
            _editedBriefing.CruiseSpeed = GetActualCruiseSpeed(CruiseSpeed);
            _editedBriefing.DepartureTime = DepartureTimeSelect.GetDateTime();
            _editedBriefing.TimeEnroute = TimeEnrouteSelect.GetTimeSpan().Ticks;
            _editedBriefing.FlightPlan = FlightPlan.ToUpper();
            _editedBriefing.Author = Author;
            _editedBriefing.Remarks = Remarks;
            
            _editedBriefing.AttachmentsArray = attachments;

            _editedBriefing.ViewPasswordHash = _briefingType == "Public"
                ? ""
                : string.IsNullOrWhiteSpace(ViewPassword) ? _editedBriefing.ViewPasswordHash : PasswordHasher.Hash(ViewPassword);

            await DatabaseContext.SaveChangesAsync();
            
            // Done editing, return to the briefing page.
            NavManager.NavigateTo($"/b/{BriefingId}");
        }

        private void Authenticate(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return;
            
            if (PasswordHasher.Check(_editedBriefing.EditPasswordHash, password))
            {
                _authenticated = true;
                _authModal.Hide();
                StateHasChanged();
                
                LoadData(_editedBriefing);
            }
            else
            {
                CurrentPage.ShowNotification("Wrong password.");
                return;
            }
            
            JsModule.InvokeVoidAsync("registerEvents");
            JsModule.InvokeVoidAsync("createFilePond", JsonConvert.SerializeObject(_editedBriefing.AttachmentsArray.ToArray<object>()));
            JsRuntime.InvokeVoidAsync("startTime");
        }

        private bool ValidateInputData()
        {
            if (string.IsNullOrWhiteSpace(DepartureAirport) || string.IsNullOrWhiteSpace(ArrivalAirport)
                                                             || string.IsNullOrWhiteSpace(DepartureAirport)
                                                             || string.IsNullOrWhiteSpace(ArrivalRunway)
                                                             || string.IsNullOrWhiteSpace(FlightPlan))
            {
                CurrentPage.ShowNotification("Check required fields.");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            JsModule.InvokeVoidAsync("destroyFilePond");
            JsModule.InvokeVoidAsync("unregisterEvents");

            JsRuntime.InvokeVoidAsync("stopTime");
            DatabaseContext.Dispose();
        }
    }
}