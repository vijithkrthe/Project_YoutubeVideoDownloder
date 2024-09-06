using Microsoft.AspNetCore.Mvc;
using Project_YoutubeVideoDownloder.Models;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Exceptions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;

namespace Project_YoutubeVideoDownloder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetMediaDetails(string url, string mediaType)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(mediaType))
            {
                return BadRequest("Invalid parameters.");
            }

            try
            {
                var youtubeClient = new YoutubeClient();
                var video = await youtubeClient.Videos.GetAsync(url);

                var thumbnailUrl = video.Thumbnails
                    .OrderByDescending(t => t.Resolution.Height)
                    .FirstOrDefault()?.Url;

                var qualities = mediaType == "video"
                    ? await GetVideoQualities(youtubeClient, url)
                    : Array.Empty<string>();

                var result = new
                {
                    thumbnailUrl,
                    qualities
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching media details.");
                return StatusCode(500, "Internal server error while fetching media details.");
            }
        }

        private async Task<string[]> GetVideoQualities(YoutubeClient youtubeClient, string url)
        {
            try
            {
                var videoId = VideoId.Parse(url);
                var manifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoId);

                return manifest.GetVideoStreams()
                               .Select(s => s.VideoQuality.Label)
                               .Distinct()
                               .ToArray();
            }
            catch (YoutubeExplodeException ytex)
            {
                _logger.LogError(ytex, "YouTube API error while fetching video qualities.");
                return Array.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching video qualities.");
                return Array.Empty<string>();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadMedia(string url, string quality, string mediaType)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(mediaType))
            {
                return BadRequest("Invalid parameters.");
            }

            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(url);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var audioStreamInfo = streamManifest
                    .GetAudioStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestBitrate();
                var videoStreamInfo = streamManifest
                    .GetVideoStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .First(s => s.VideoQuality.Label == quality);
                var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
                var validTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
                string outputPath = $@"E:\Workshop\YT Downloder\{validTitle}.mp4";

                await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(outputPath).Build());
                var data = new { validTitle };
                    return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading media.");
                return StatusCode(500, "Internal server error while downloading media.");
            }
        }


    }
}
