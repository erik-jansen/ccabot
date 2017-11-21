using System.Configuration;

namespace LuisBot.Utilities
{
    public static class Settings
    {
        public static string GetSubscriptionKey()
        {
            return ConfigurationManager.AppSettings["TranslatorTextSubscriptionKey"];
        }
        public static string GetCognitiveServicesTokenUri()
        {
            return ConfigurationManager.AppSettings["CognitiveServicesTokenUri"];
        }
        public static string GetTranslatorUri()
        {
            return ConfigurationManager.AppSettings["TranslatorUri"];
        }
        public static string GetProductApiUri()
        {
            return ConfigurationManager.AppSettings["ProductApiUri"];
        }
        public static string GetDeliveryApiUri()
        {
            return ConfigurationManager.AppSettings["DeliveryApiUri"];
        }
        public static string GetRecommendationApiUri()
        {
            return ConfigurationManager.AppSettings["RecommendationApiUri"];
        }
        public static string GetRecommendationApiKey()
        {
            return ConfigurationManager.AppSettings["RecommendationApiKey"];
        }
        
    }
}