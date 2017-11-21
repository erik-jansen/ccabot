namespace LuisBot.Models
{
    using Newtonsoft.Json;
    using System;

    [Serializable]
    public class Product
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("recommendedItemId")]
        public string RecommendedId { get; set; }

        [JsonProperty("score")]
        public string Score { get; set; }
        public string Description { get; set; }

        public string Brand { get; set; }

        public string ItemSize { get; set; }
        public string PackageType { get; set; }

        public string PackageSize { get;  set; }

        public string ImageUrl { get; set; }

        public bool IsSpecial { get; set; }

        public bool BoughtBefore { get; set; }
        public double Price { get; set; }
    }
}