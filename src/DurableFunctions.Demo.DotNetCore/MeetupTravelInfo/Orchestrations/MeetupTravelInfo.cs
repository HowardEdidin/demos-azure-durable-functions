using System.Threading.Tasks;
using DurableFunctions.Demo.DotNetCore.MeetupTravelInfo.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableFunctions.Demo.DotNetCore.MeetupTravelInfo.Orchestrations
{
    public static class MeetupTravelInfo
    {
        [FunctionName(nameof(MeetupTravelInfo))]
        public static async Task<TravelInfo> Run(
            [OrchestrationTrigger]DurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            var meetupTravelInfoInput = orchestrationContext.GetInput<MeetupTravelInfoInput>();
            var meetupEvent = await orchestrationContext.CallActivityAsync<MeetupEvent>(
                "GetNextEventForGroup", 
                meetupTravelInfoInput);
            var travelTimeInput = GetTravelTimeInput(meetupTravelInfoInput, meetupEvent);
            var travelInfoResult = await orchestrationContext.CallActivityAsync<TravelInfo>(
                    "GetTravelTime",
                    travelTimeInput);

            return travelInfoResult;
        }

        private static TravelTimeInput GetTravelTimeInput(MeetupTravelInfoInput input, MeetupEvent meetupEvent)
        {
            return new TravelTimeInput
            {
                EventName = meetupEvent.Name,
                GroupName = meetupEvent.Group.Name,
                EventStartUnixTimeSeconds = meetupEvent.UnixTimeMilliseconds / 1000,
                TravelMode = input.TravelMode,
                DepartureAddress = input.DepartureAddress,
                DestinationAddress = $"{meetupEvent.Venue.Name}, {meetupEvent.Venue.Address}, {meetupEvent.Venue.City}",
                TrafficModel = "pessimistic"
            };
        }
    }
}
