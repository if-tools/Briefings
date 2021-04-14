using System.Collections.Generic;
using System.Threading.Tasks;
using IFToolsBriefings.Data;
using IFToolsBriefings.Data.Types;
using IFToolsBriefings.Data.Models;
using IFToolsBriefings.Shared.Components;
using Microsoft.JSInterop;

namespace IFToolsBriefings.Pages
{
    public partial class NewBriefing
    {
        private bool _showLoadingIndicator;
        private bool _showCompleted;
        private int _newBriefingId;

        private GetFlightPlanModal _fplModal;
        
        public NewBriefing() : base(new DatabaseContext()) { }
        
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

                JsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/newBriefing.js");
            
                await JsModule.InvokeVoidAsync("createFilePond");
                await JsModule.InvokeVoidAsync("registerEvents");

                await JsRuntime.InvokeVoidAsync("startTime");
            }
        }

        private async void Submit()
        {
            if (!ValidateInputData()) return;
            
            var filepondStatus = await JsModule.InvokeAsync<bool>("checkIfFilepondBusy");
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
                EditPasswordHash = PasswordHasher.Hash(EditPassword),
                ViewPasswordHash = string.IsNullOrEmpty(ViewPassword) ? "" : PasswordHasher.Hash(ViewPassword)
            };
            
            var attachments = await JsModule.InvokeAsync<string[]>("getFilepondFileIds");
            newBriefing.AttachmentsArray = attachments;

            await DatabaseContext.Briefings.AddAsync(newBriefing);
            await DatabaseContext.SaveChangesAsync();
            
            _showLoadingIndicator = false;
            _showCompleted = true;
            _newBriefingId = newBriefing.Id;
            
            await JsModule.InvokeVoidAsync("unregisterEvents");
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
                CurrentPage.ShowNotification("Check the required fields.");
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