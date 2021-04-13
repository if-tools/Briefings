using System;
using System.Collections.Generic;
using IFToolsBriefings.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IFToolsBriefings.Data.Types
{
    public abstract class BriefingCreation : ComponentBase
    {
        protected IJSObjectReference JsModule;
    
        private readonly Dictionary<int, bool> _categoryStates = new ();

        protected readonly DatabaseContext DatabaseContext;
        
        protected GetFlightPlanModal FplModal;

        protected string Server = "Casual";
        protected string DepartureAirport = "";
        protected string ArrivalAirport = "";
        protected string DepartureRunway = "";
        protected string ArrivalRunway = "";
        protected int FlightLevel = 0;
        protected double CruiseSpeed = 0;
        protected SpeedType SpeedType = SpeedType.Mach;
        protected TimeSelect DepartureTimeSelect;
        protected TimeSelect TimeEnrouteSelect;
        protected string FlightPlan = "";
        protected string Author = "";
        protected string Remarks = "";
        protected string EditPassword = "";
        protected string ViewPassword = "";

        protected string CruiseSpeedDisplayedAs = "M0.00";

        protected BriefingCreation(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }
        
        protected void FlightPlanReceived(string fpl)
        {
            FlightPlan = fpl;
            StateHasChanged();
        }

        protected bool GetCategoryState(int categoryId)
        {
            if (!_categoryStates.ContainsKey(categoryId))
            {
                _categoryStates.Add(categoryId, true);
            }

            return _categoryStates[categoryId];
        }

        protected void ChangeCategoryState(int categoryId)
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

        protected void UpdateCruiseSpeedDisplayedAsText()
        {
            CruiseSpeedDisplayedAs = $"{(SpeedType == SpeedType.Mach ? "M" : "TAS")}{GetActualCruiseSpeed(CruiseSpeed):0.00}";
            StateHasChanged();
        }

        protected double GetActualCruiseSpeed(double cruiseSpeed)
        {
            if (SpeedType == SpeedType.TrueAirspeed) return cruiseSpeed;

            // As described in https://github.com/if-tools/Briefings/issues/3
            switch (cruiseSpeed)
            {
                case >= 400:
                    return 0;
                case < 4:
                    return Math.Round(cruiseSpeed, 2);
                default:
                    return Math.Round(cruiseSpeed / 100, 2);
            }
        }
    }
}