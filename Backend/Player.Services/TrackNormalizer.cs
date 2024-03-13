using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class TrackNormalizer : ITrackNormalizer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrackNormalizer> _logger;

        public TrackNormalizer(IConfiguration configuration, ILogger<TrackNormalizer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        public void Normalize(string path)
        {
            _logger.LogTrace("Normalization for {Path} started", path);
            var mp3GainPath = _configuration.GetValue<string>("Player:Mp3GainPath");
            var mp3Gain = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = mp3GainPath,
                    Arguments = $"-r -c {path}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            mp3Gain.Start();
            mp3Gain.WaitForExit();
            _logger.LogTrace("Normalization for {Path} ended", path);
        }
    }
}

/*
 * curl -o  mp3gain.zip -sL https://sourceforge.net/projects/mp3gain/files/mp3gain/1.6.1/mp3gain-1_6_1-src.zip
*/