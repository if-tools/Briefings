using System;
using System.Collections.Generic;
using IFToolsBriefings.Client.Shared.Components;
using IFToolsBriefings.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IFToolsBriefings.Client.Types
{
    public abstract class BriefingCreation : ComponentBase
    {
        protected IJSObjectReference JsModule;
    
        private readonly Dictionary<int, bool> _categoryStates = new ();
        
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
        protected string SpeedTypeSwitchText = "Mach";
        
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

        protected void SpeedTypeSwitchClicked()
        {
            switch (SpeedType)
            {
                case SpeedType.Mach:
                    SpeedType = SpeedType.TrueAirspeed;
                    SpeedTypeSwitchText = "TAS";
                    break;
                case SpeedType.TrueAirspeed:
                    SpeedType = SpeedType.Mach;
                    SpeedTypeSwitchText = "Mach";
                    break;
            }
            
            UpdateCruiseSpeedDisplayedAsText();
        }

        protected void ChangeSpeedType(SpeedType newSpeedType)
        {
            switch (newSpeedType)
            {
                case SpeedType.Mach:
                    SpeedTypeSwitchText = "Mach";
                    break;
                case SpeedType.TrueAirspeed:
                    SpeedTypeSwitchText = "TAS";
                    break;
            }
            
            UpdateCruiseSpeedDisplayedAsText();
        }

        protected void UpdateCruiseSpeedDisplayedAsText()
        {
            var speedToDisplay = SpeedType == SpeedType.Mach ? $"{GetActualCruiseSpeed(CruiseSpeed):0.00}" : $"{GetActualCruiseSpeed(CruiseSpeed)}";
            
            CruiseSpeedDisplayedAs = $"{(SpeedType == SpeedType.Mach ? "M" : "TAS ")}{speedToDisplay}";
            StateHasChanged();
        }

        protected double GetActualCruiseSpeed(double cruiseSpeed)
        {
            if (SpeedType == SpeedType.TrueAirspeed)
            {
                // Deny values lower than 4.
                if (cruiseSpeed < 4) return 4;
                
                return cruiseSpeed;
            }

            // As described in https://github.com/if-tools/Briefings/issues/3
            switch (cruiseSpeed)
            {
                case >= 400:
                case < 0:
                    return 0;
                case < 4:
                    return Math.Round(cruiseSpeed, 2);
                default:
                    return Math.Round(cruiseSpeed / 100, 2);
            }
        }
    }
}