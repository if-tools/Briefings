using System.Collections.Generic;
using System.Threading.Tasks;
using IFToolsBriefings.Data;
using IFToolsBriefings.Data.Models;
using IFToolsBriefings.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IFToolsBriefings.Pages
{
    public partial class NewBriefing
    {
        private IJSObjectReference _jsModule;
    
        private readonly Dictionary<int, bool> _categoryStates = new ();

        private readonly DatabaseContext _databaseContext;

        private bool _showLoadingIndicator;
        private bool _showCompleted;
        private int _newBriefingId;

        private GetFlightPlanModal _fplModal;
        
        private string _server = "Casual";
        private string _departureAirport = "";
        private string _arrivalAirport = "";
        private string _departureRunway = "";
        private string _arrivalRunway = "";
        private int _flightLevel = 0;
        private double _cruiseSpeed = 0;
        private TimeSelect _departureTimeSelect;
        private TimeSelect _timeEnrouteSelect;
        private string _flightPlan = "";
        private string _author = "";
        private string _remarks = "";
        private string _editPassword = "";
        private string _viewPassword = "";
        
        private ElementReference _currentTimeElement;

        public NewBriefing()
        {
            _databaseContext = new DatabaseContext();
        }
        
        protected override void OnInitialized()
        {
            CurrentPage.SetCurrentPageName("New Briefing");
            
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _fplModal.FplReceived += FlightPlanReceived;

                _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/newBriefing.js");
            
                await _jsModule.InvokeVoidAsync("createFilePond");
                await _jsModule.InvokeVoidAsync("registerEvents");

                await JsRuntime.InvokeVoidAsync("startTime", _currentTimeElement);
            }
        }

        private async void Submit()
        {
            if (!ValidateInputData()) return;
            
            var filepondStatus = await _jsModule.InvokeAsync<bool>("checkIfFilepondBusy");
            if (filepondStatus)
            {
                CurrentPage.ShowNotification("Files are still uploading.");
                return;
            }

            _showLoadingIndicator = true;
            await InvokeAsync(StateHasChanged);

            var newBriefing = new Briefing
            {
                Server = _server,
                Origin = _departureAirport.ToUpper(),
                Destination = _arrivalAirport.ToUpper(),
                OriginRunway = _departureRunway.ToUpper(),
                DestinationRunway = _arrivalRunway.ToUpper(),
                FlightLevel = _flightLevel,
                CruiseSpeed = _cruiseSpeed,
                DepartureTime = _departureTimeSelect.GetDateTime(),
                TimeEnroute = _timeEnrouteSelect.GetTimeSpan().Ticks,
                FlightPlan = _flightPlan.ToUpper(),
                Author = _author,
                Remarks = _remarks,
                EditPasswordHash = PasswordHasher.Hash(_editPassword),
                ViewPasswordHash = string.IsNullOrEmpty(_viewPassword) ? "" : PasswordHasher.Hash(_viewPassword)
            };
            
            var attachments = await _jsModule.InvokeAsync<string[]>("getFilepondFileIds");
            newBriefing.AttachmentsArray = attachments;

            await _databaseContext.Briefings.AddAsync(newBriefing);
            await _databaseContext.SaveChangesAsync();
            
            _showLoadingIndicator = false;
            _showCompleted = true;
            _newBriefingId = newBriefing.Id;
            
            await _jsModule.InvokeVoidAsync("unregisterEvents");
            await InvokeAsync(StateHasChanged);
        }

        private bool ValidateInputData()
        {
            if (string.IsNullOrWhiteSpace(_departureAirport) || string.IsNullOrWhiteSpace(_arrivalAirport)
                                                             || string.IsNullOrWhiteSpace(_departureAirport)
                                                             || string.IsNullOrWhiteSpace(_arrivalRunway)
                                                             || string.IsNullOrWhiteSpace(_flightPlan)
                                                             || string.IsNullOrEmpty(_editPassword))
            {
                CurrentPage.ShowNotification("Check the required fields.");
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