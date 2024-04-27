using System;
using System.Threading.Tasks;
using IFToolsBriefings.Client.Services;
using IFToolsBriefings.Client.Shared.Components;
using IFToolsBriefings.Shared.Data.Models;
using Microsoft.JSInterop;

namespace IFToolsBriefings.Client.Pages
{
    public partial class NewBriefing
    {
        private IJSObjectReference _browserStorageJsModule;

        private bool _showLoadingIndicator;
        private bool _showCompleted;
        private string _newBriefingId;

        private GetFlightPlanModal _fplModal;

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

                GeneralJsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/newBriefing.js");
                _browserStorageJsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/localStorage.js");
            
                await GeneralJsModule.InvokeVoidAsync("createFilePond");
                await GeneralJsModule.InvokeVoidAsync("registerEvents");

                await JsRuntime.InvokeVoidAsync("startTime");
            }
        }

        private async void Submit()
        {
            if (!ValidateInputData()) return;
            
            var filepondStatus = await GeneralJsModule.InvokeAsync<bool>("checkIfFilepondBusy");
            if (filepondStatus)
            {
                CurrentPage.ShowNotification("Files are still uploading.");
                return;
            }

            _showLoadingIndicator = true;
            await InvokeAsync(StateHasChanged);

            var newBriefing = new Briefing
            {
                Server = Server,
                Origin = DepartureAirport.ToUpper(),
                Destination = ArrivalAirport.ToUpper(),
                OriginRunway = DepartureRunway.ToUpper(),
                DestinationRunway = ArrivalRunway.ToUpper(),
                FlightLevel = FlightLevel,
                CruiseSpeed = GetActualCruiseSpeed(CruiseSpeed),
                DepartureTime = DepartureTimeSelect.GetDateTime(),
                TimeEnroute = TimeEnrouteSelect.GetTimeSpan().Ticks,
                FlightPlan = FlightPlan.ToUpper(),
                Author = Author,
                Remarks = Remarks,
                EditPassword = EditPassword,
                ViewPassword = ViewPassword,
                CreatedOn = DateTime.UtcNow
            };
            
            var attachments = await GeneralJsModule.InvokeAsync<string[]>("getFilepondFileIds");
            newBriefing.AttachmentsArray = attachments;

            _newBriefingId = await ApiService.MakeBriefing(newBriefing);

            await _browserStorageJsModule.InvokeVoidAsync("addCreatedBriefing", _newBriefingId);
            
            _showLoadingIndicator = false;
            _showCompleted = true;
            
            await GeneralJsModule.InvokeVoidAsync("unregisterEvents");
            await InvokeAsync(StateHasChanged);
        }

        private bool ValidateInputData()
        {
            if (string.IsNullOrWhiteSpace(DepartureAirport) || string.IsNullOrWhiteSpace(ArrivalAirport)
                                                             || string.IsNullOrWhiteSpace(DepartureAirport)
                                                             || string.IsNullOrWhiteSpace(ArrivalRunway)
                                                             || string.IsNullOrWhiteSpace(FlightPlan)
                                                             || string.IsNullOrEmpty(EditPassword))
            {
                CurrentPage.ShowNotification("Please make sure all required fields are complete.");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            GeneralJsModule.InvokeVoidAsync("destroyFilePond");
            GeneralJsModule.InvokeVoidAsync("unregisterEvents");

            JsRuntime.InvokeVoidAsync("stopTime");
        }
    }
}