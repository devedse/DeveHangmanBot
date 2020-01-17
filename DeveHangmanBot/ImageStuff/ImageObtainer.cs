﻿using DeveCoolLib.Logging;
using DeveHangmanBot.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeveHangmanBot.ImageStuff
{
    public class ImageObtainer
    {
        private readonly ILogger _logger;
        private readonly BotConfig _botConfig;

        public ImageObtainer(ILogger logger, BotConfig botConfig)
        {
            _logger = logger;
            _botConfig = botConfig;
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
                _logger.WriteError(ex.ToString());

                //_logger.WriteError($"Length of GoogleApiKey: {_botConfig.GoogleApiKey.Length}");
                //_logger.WriteError($"Length of GoogleCxToken: {_botConfig.GoogleCxToken.Length}");
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
                _logger.WriteError(ex.ToString());

                //_logger.WriteError($"Length of GoogleApiKey: {_botConfig.GoogleApiKey.Length}");
                //_logger.WriteError($"Length of GoogleCxToken: {_botConfig.GoogleCxToken.Length}");
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
                _logger.WriteError(ex.ToString());

                //_logger.WriteError($"Length of GiphyApiKey: {_botConfig.GiphyApiKey.Length}");
            }

            return null;
        }
    }
}
