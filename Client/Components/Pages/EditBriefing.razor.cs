using System;
using System.Linq;
using System.Threading.Tasks;
using IFToolsBriefings.Client.Components.Shared;
using IFToolsBriefings.Client.Services;
using IFToolsBriefings.Client.Types;
using IFToolsBriefings.Shared.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace IFToolsBriefings.Client.Components.Pages;

public partial class EditBriefing
{
    [Parameter]
    public string BriefingId { get; set; }

    private bool _showLoadingIndicator;
        
    private bool _authenticated;
    private AuthenticationModal _authModal;
        
    private Briefing _editedBriefing;
        
    private string _briefingType = "Public";

    private string _editPassword;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(BriefingId))
        {
            NavManager.NavigateTo("/");
            
            return;
        }
            
        CurrentPage.SetCurrentPageName("Edit Briefing");
            
        var briefingExists = await ApiService.CheckIfBriefingExists(BriefingId);
        if (!briefingExists)
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

            GeneralJsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/editBriefing.js");
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
            
        var filepondStatus = await GeneralJsModule.InvokeAsync<bool>("checkIfFilepondBusy");
        if (filepondStatus)
        {
            CurrentPage.ShowNotification("Files are still uploading.");
            return;
        }
            
        _showLoadingIndicator = true;
        await InvokeAsync(StateHasChanged);

        var attachments = await GeneralJsModule.InvokeAsync<string[]>("getFilepondFileIds");
            
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
        _editedBriefing.LastEdited = DateTime.UtcNow;
            
        _editedBriefing.AttachmentsArray = attachments;

        _editedBriefing.ViewPassword = _briefingType == "Public" ? "none" : ViewPassword;

        await ApiService.EditBriefing(BriefingId, _editedBriefing, _editPassword);
            
        // Done editing, return to the briefing page.
        NavManager.NavigateTo($"/b/{BriefingId}");
    }

    private async void Authenticate(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return;
            
        if (await ApiService.CheckPassword(BriefingId, password))
        {
            _editedBriefing = await ApiService.GetBriefingToEdit(BriefingId, password);
            _editPassword = password;
                
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

        var attachments = (await ApiService.GetAttachments(JsonConvert.SerializeObject(_editedBriefing.AttachmentsArray)))
            .ToList().ConvertAll(a => a.FileUrl);
            
        await GeneralJsModule.InvokeVoidAsync("registerEvents");
        await GeneralJsModule.InvokeVoidAsync("createFilePond", JsonConvert.SerializeObject(attachments.ToArray<object>()));
        await JsRuntime.InvokeVoidAsync("startTime");
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
        GeneralJsModule.InvokeVoidAsync("destroyFilePond");
        GeneralJsModule.InvokeVoidAsync("unregisterEvents");

        JsRuntime.InvokeVoidAsync("stopTime");
    }
}