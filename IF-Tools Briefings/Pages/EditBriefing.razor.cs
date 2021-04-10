using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IFToolsBriefings.Data;
using IFToolsBriefings.Data.Models;
using IFToolsBriefings.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace IFToolsBriefings.Pages
{
    public partial class EditBriefing
    {
        private IJSObjectReference _jsModule;
    
        private readonly Dictionary<int, bool> _categoryStates = new ();

        private readonly DatabaseContext _databaseContext;
        
        [Parameter]
        public string BriefingId { get; set; }

        private int _actualBriefingId;
        private bool _showLoadingIndicator;
        
        private bool _authenticated;
        private AuthenticationModal _authModal;
        
        private GetFlightPlanModal _fplModal;

        private Briefing _editedBriefing;

        private string _server = "Casual";
        private string _departureAirport = "";
        private string _arrivalAirport = "";
        private string _departureRunway = "";
        private string _arrivalRunway = "";
        private int _flightLevel = 0;
        private double _cruiseSpeed = 0;
        private TimeSelect _departureTimeSelect = new ();
        private TimeSelect _timeEnrouteSelect = new ();
        private string _flightPlan = "";
        private string _author = "";
        private string _remarks = "";
        private string _viewPassword = "";
        private string _briefingType = "Public";
        
        public EditBriefing()
        {
            _databaseContext = new DatabaseContext();
        }
        
        protected override void OnInitialized()
        {
            if (string.IsNullOrEmpty(BriefingId) || !int.TryParse(BriefingId, out _actualBriefingId))
            {
                NavManager.NavigateTo("/");
            
                return;
            }
            
            CurrentPage.SetCurrentPageName("Edit Briefing");
            
            _editedBriefing = _databaseContext.Briefings.SingleOrDefault(entity => entity.Id == _actualBriefingId);
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
                _fplModal.FplReceived += FlightPlanReceived;

                _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/editBriefing.js");
            }
        }

        private void LoadData(Briefing briefing)
        {
            _server = briefing.Server;
            _departureAirport = briefing.Origin;
            _departureRunway = briefing.OriginRunway;
            _arrivalAirport = briefing.Destination;
            _arrivalRunway = briefing.DestinationRunway;
            _flightLevel = briefing.FlightLevel;
            _cruiseSpeed = briefing.CruiseSpeed;
            _departureTimeSelect.SetDateTime(briefing.DepartureTime);
            _timeEnrouteSelect.SetTimeSpan(briefing.GetTimeEnroute());
            _flightPlan = briefing.FlightPlan;
            _author = briefing.Author;
            _remarks = briefing.Remarks;
            
            _briefingType = string.IsNullOrWhiteSpace(briefing.ViewPasswordHash) ? "Public" : "Private";
            
            StateHasChanged();
        }

        private async void Submit()
        {
            if (!_authenticated) return;
            if (!ValidateInputData()) return;
            
            var filepondStatus = await _jsModule.InvokeAsync<bool>("checkIfFilepondBusy");
            if (filepondStatus)
            {
                CurrentPage.ShowNotification("Files are still uploading.");
                return;
            }
            
            _showLoadingIndicator = true;
            await InvokeAsync(StateHasChanged);

            var attachments = await _jsModule.InvokeAsync<string[]>("getFilepondFileIds");
            
            // Edit the original briefing entity
            _editedBriefing.Server = _server;
            _editedBriefing.Origin = _departureAirport.ToUpper();
            _editedBriefing.OriginRunway = _departureRunway.ToUpper();
            _editedBriefing.Destination = _arrivalAirport.ToUpper();
            _editedBriefing.DestinationRunway = _arrivalRunway.ToUpper();
            _editedBriefing.FlightLevel = _flightLevel;
            _editedBriefing.CruiseSpeed = _cruiseSpeed;
            _editedBriefing.DepartureTime = _departureTimeSelect.GetDateTime();
            _editedBriefing.TimeEnroute = _timeEnrouteSelect.GetTimeSpan().Ticks;
            _editedBriefing.FlightPlan = _flightPlan.ToUpper();
            _editedBriefing.Author = _author;
            _editedBriefing.Remarks = _remarks;
            
            _editedBriefing.AttachmentsArray = attachments;

            _editedBriefing.ViewPasswordHash = _briefingType == "Public"
                ? ""
                : string.IsNullOrWhiteSpace(_viewPassword) ? _editedBriefing.ViewPasswordHash : PasswordHasher.Hash(_viewPassword);

            await _databaseContext.SaveChangesAsync();
            
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
            
            _jsModule.InvokeVoidAsync("registerEvents");
            _jsModule.InvokeVoidAsync("createFilePond", JsonConvert.SerializeObject(_editedBriefing.AttachmentsArray.ToArray<object>()));
            JsRuntime.InvokeVoidAsync("startTime");
        }

        private bool ValidateInputData()
        {
            if (string.IsNullOrWhiteSpace(_departureAirport) || string.IsNullOrWhiteSpace(_arrivalAirport)
                                                             || string.IsNullOrWhiteSpace(_departureAirport)
                                                             || string.IsNullOrWhiteSpace(_arrivalRunway)
                                                             || string.IsNullOrWhiteSpace(_flightPlan))
            {
                CurrentPage.ShowNotification("Check required fields.");
                return false;
            }

            return true;
        }
        
        private void FlightPlanReceived(string fpl)
        {
            _flightPlan = fpl;
            StateHasChanged();
        }

        private bool GetCategoryState(int categoryId)
        {
            if (!_categoryStates.ContainsKey(categoryId))
            {
                _categoryStates.Add(categoryId, true);
            }

            return _categoryStates[categoryId];
        }

        private void ChangeCategoryState(int categoryId)
        {
            if (!_categoryStates.ContainsKey(categoryId))
            {
                _categoryStates.Add(categoryId, false);
            }
            else
            {
                _categoryStates[categoryId] = !_categoryStates[categoryId];
            }
        
            StateHasChanged();
        }

        public void Dispose()
        {
            _jsModule.InvokeVoidAsync("destroyFilePond");
            _jsModule.InvokeVoidAsync("unregisterEvents");

            JsRuntime.InvokeVoidAsync("stopTime");
            _databaseContext.Dispose();
        }
    }
}