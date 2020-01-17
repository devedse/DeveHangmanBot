using DeveHangmanBot.Config;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DeveHangmanBot.ImageStuff
{
    public class ImageObtainer
    {
        private readonly BotConfig _botConfig;

        public ImageObtainer(BotConfig botConfig)
        {
            this._botConfig = botConfig;
        }

        public async Task<byte[]> GetGoogleImageBytes(string word)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var result = await httpClient.GetAsync($"https://www.googleapis.com/customsearch/v1?key={_botConfig.GoogleApiKey}&cx={_botConfig.GoogleCxToken}&q={word}&searchType=image");

                    var stringified = await result.Content.ReadAsStringAsync();

                    var parsed = JsonConvert.DeserializeObject<PocoGoogleCustomSearch>(stringified);

                    var imgurl = parsed.items[0].link;

                    if (!string.IsNullOrWhiteSpace(imgurl))
                    {
                        var obtainedImage = await httpClient.GetAsync(imgurl);

                        var byteStuff = await obtainedImage.Content.ReadAsByteArrayAsync();

                        return byteStuff;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public async Task<List<string>> GetGoogleImagesLinks(string word)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var result = await httpClient.GetAsync($"https://www.googleapis.com/customsearch/v1?key={_botConfig.GoogleApiKey}&cx={_botConfig.GoogleCxToken}&q={word}&searchType=image");

                    var stringified = await result.Content.ReadAsStringAsync();

                    var parsed = JsonConvert.DeserializeObject<PocoGoogleCustomSearch>(stringified);

                    var imgurls = parsed.items.Where(t => t.link.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || t.link.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || t.link.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)).Select(t => t.link);

                    return imgurls.ToList();
                }
            }
            catch (Exception ex)
            {
            }

            return new List<string>();
        }

        public async Task<string> GetGiphyImageLink(string word)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var result = await httpClient.GetAsync($"https://api.giphy.com/v1/gifs/search?api_key={_botConfig.GiphyApiKey}&q={word}&limit=1&offset=0&rating=G&lang=en");

                    var stringified = await result.Content.ReadAsStringAsync();

                    var parsed = JsonConvert.DeserializeObject<PocoGiphySearch>(stringified);

                    if (parsed.data.Any())
                    {
                        var imgurl = parsed.data[0].images.downsized_large.url;

                        if (!string.IsNullOrWhiteSpace(imgurl))
                        {
                            return imgurl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }
    }
}
