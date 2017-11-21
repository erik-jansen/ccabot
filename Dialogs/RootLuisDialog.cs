namespace LuisBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using System.Net.Http;
    using System.Data;
    using Utilities;
    using Newtonsoft.Json;
    using Models;
    using System.Text;

    [LuisModel("3e548cee-9f41-456b-bc50-d3169689aa41", "f49e70b9ef8345dc8a501da2798be1a2")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string EntityProductId = "productId";
        private const string EntityDateTime = "builtin.datetimeV2.date";
        private const string EntityProductName = "ProductName";
        private const string EntityItemSize = "ItemSize";
        private const string EntityPackageSize = "PackageSize";
        private const string EntityNumber = "builtin.number";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi! Try asking me things like 'where is my driver' or 'what is a recommendation for D5PXGP'".ToUserLocale(context));

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("DriverETA")]
        public async Task DriverEta(IDialogContext context, LuisResult result)
        {
            var schedule = await GetDeliveryDateAsync();

            string response = $"Your driver will arrive on {schedule.DeliveryDate.ToLongDateString()}";
            
            await context.PostAsync(response.ToUserLocale(context));

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("FixFridge")]
        public async Task FixFridge(IDialogContext context, LuisResult result)
        {
            string response = "We'll send someone over on December 4th.";

            await context.PostAsync(response.ToUserLocale(context));

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetSpecials")]
        public async Task GetSpecials(IDialogContext context, LuisResult result)
        {
                await context.PostAsync("Getting this week's specials...");

                var specials = await GetProductSpecialsAsync();

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var product in specials)
                {
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = $"{product.Id} - {product.Brand}",
                        Text = $"{product.Description}, {product.ItemSize}, {product.PackageSize} in {product.PackageType}.\n\rPrice: ${product.Price}.00",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = product.ImageUrl }
                        },
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
        }

        [LuisIntent("ScheduleDelivery")]
        public async Task ScheduleDelivery(IDialogContext context, LuisResult result)
        {
            EntityRecommendation deliveryDate;
            if (result.TryFindEntity(EntityDateTime, out deliveryDate))
            {
                var dateparser = new Chronic.Parser();
                var datetime = dateparser.Parse(deliveryDate.Entity).ToTime();

                var schedule = new DeliverySchedule()
                {
                    Id = 1,
                    DeliveryDate = datetime
                };

                await UpdateDeliveryDateAsync(schedule);

                string resultMessage = $"We'll schedule a delivery date for {datetime.ToLongDateString()}";
                await context.PostAsync(resultMessage);
            }
        }

        [LuisIntent("GetRecommendation")]
        public async Task GetRecommendation(IDialogContext context, LuisResult result)
        {
            EntityRecommendation productId;

            if (result.TryFindEntity(EntityProductId, out productId))
            {
                await context.PostAsync($"{"Looking for recommendations for".ToUserLocale(context)} '{productId.Entity} - {(await GetProductAsync(productId.Entity)).Description}'...");

                var recommendations = await GetProductRecommendationAsync(productId.Entity);

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var product in recommendations)
                {
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = $"{product.Id} - {product.Brand}",
                        Text = $"{product.Description}, {product.ItemSize}, {product.PackageSize} in {product.PackageType}.\n\rPrice: ${product.Price}.00",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = product.ImageUrl }
                        },
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
        }

        [LuisIntent("AddToOrder")]
        public async Task AddToOrder(IDialogContext context, LuisResult result)
        {
            EntityRecommendation productNameEntityRecommendation;
            EntityRecommendation itemSizeEntityRecommendation;
            EntityRecommendation packageSizeEntityRecommendation;
            EntityRecommendation quantityEntityRecommendation;

            if ((result.TryFindEntity(EntityProductName, out productNameEntityRecommendation)) &&
                (result.TryFindEntity(EntityItemSize, out itemSizeEntityRecommendation)) &&
                (result.TryFindEntity(EntityPackageSize, out packageSizeEntityRecommendation)) &&
                (result.TryFindEntity(EntityNumber, out quantityEntityRecommendation)))
            {
                
                var product = await SearchProductAsync(productNameEntityRecommendation.Entity, packageSizeEntityRecommendation.Entity, itemSizeEntityRecommendation.Entity);

                if (product != null)
                {
                    await context.PostAsync($"Added {quantityEntityRecommendation.Entity} {productNameEntityRecommendation.Entity} to your order...");

                    var resultMessage = context.MakeMessage();
                    resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    resultMessage.Attachments = new List<Attachment>();

                    HeroCard heroCard = new HeroCard()
                    {
                        Title = product.Id + " - " + product.Brand,
                        Subtitle = $"Description: {product.Description}\n\rPackage Size: {product.PackageSize}\n\rItem Size: {product.ItemSize}ML\n\rPackage Type: {product.PackageType}\n\rPrice: ${product.Price}.00",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = product.ImageUrl }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+"
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                    await context.PostAsync(resultMessage);
                }
                else
                {
                    await context.PostAsync($"Product not found...");
                }
            }

            context.Wait(this.MessageReceived);
        }

        private async Task<DeliverySchedule> GetDeliveryDateAsync()
        {
            var deliverySchedule = new DeliverySchedule();

            using (var httpClient = new HttpClient())
            {
                var deliveryApiUri = Settings.GetDeliveryApiUri();
                var url = string.Format("{0}delivery/1", deliveryApiUri);

                var response = await httpClient.GetStringAsync(new Uri(url));

                deliverySchedule = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<DeliverySchedule>(response));
            }

            return deliverySchedule;
        }

        private async Task<DeliverySchedule> UpdateDeliveryDateAsync(DeliverySchedule schedule)
        {
            using (var httpClient = new HttpClient())
            {
                var deliveryApiUri = Settings.GetDeliveryApiUri();
                var url = string.Format("{0}delivery/1", deliveryApiUri);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "relativeAddress");
                request.Content = new StringContent(JsonConvert.SerializeObject(schedule),
                                                    Encoding.UTF8,
                                                    "application/json");


                httpClient.BaseAddress = new Uri(url);

                await httpClient.SendAsync(request)
                  .ContinueWith(responseTask =>
                  {
                      Console.WriteLine("Response: {0}", responseTask.Result);
                  });
            }

            return schedule;
        }

        private async Task<IEnumerable<Product>> GetProductSpecialsAsync()
        {
            var product = new List<Product>();

            using (var httpClient = new HttpClient())
            {
                var productApiUri = Settings.GetProductApiUri();
                var url = string.Format("{0}product/", productApiUri);

                var response = await httpClient.GetStringAsync(new Uri(url));

                product = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Product>>(response));
                product = product.Where(p => p.IsSpecial).ToList();
            }

            return product;
        }

        private async Task<IEnumerable<Product>> GetProductRecommendationAsync(string productId)
        {
            var products = new List<Product>();

            using (var httpClient = new HttpClient())
            {
                var recommendationApiUri = Settings.GetRecommendationApiUri();
                var url = string.Format("{0}recommend?itemId={1}", recommendationApiUri, productId);

                httpClient.DefaultRequestHeaders.Add("x-api-key", Settings.GetRecommendationApiKey());
                var response = await httpClient.GetStringAsync(new Uri(url));

                var productItems = JsonConvert.DeserializeObject<List<Product>>(response);
                
                foreach (var product in productItems)
                {
                    var p = await GetProductAsync(product.RecommendedId);
                    p.Score = product.Score;
                    products.Add(p);
                }
                
            }

            return products;
        }

        private async Task<Product> GetProductAsync(string productId)
        {
            var product = new Product();

            using (var httpClient = new HttpClient())
            {
                var productApiUri = Settings.GetProductApiUri();
                var url = string.Format("{0}product/{1}", productApiUri, productId);

                var response = await httpClient.GetStringAsync(new Uri(url));

                product = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Product>(response));
            }

            return product;
        }

        private async Task<Product> SearchProductAsync(string productName, string packageSize, string itemSize)
        {
            var product = new List<Product>();

            using (var httpClient = new HttpClient())
            {
                var productApiUri = Settings.GetProductApiUri();
                var url = string.Format("{0}product?productName={1}&itemSize={2}&packageSize={3}", productApiUri, productName, itemSize, packageSize);

                var response = await httpClient.GetStringAsync(new Uri(url));

                product = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Product>>(response));
            }

            if (product.Count > 0)
                return product.First();
            else
                return null;
        }
    }
}