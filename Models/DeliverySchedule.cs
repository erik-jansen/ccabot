using Newtonsoft.Json;
using System;

namespace LuisBot.Models
{
    public class DeliverySchedule
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("deliveryDate")]
        public DateTime DeliveryDate { get; set; }
    }
}