using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class LatestClientExe
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IConfiguration _configuration;

            public Handler(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var clientDirectory = _configuration.GetValue<string>("Player:ClientDirectory");

                var files = Directory.GetFiles(clientDirectory, "*.exe");

                return files
                    .Where(f => Regex.IsMatch(f, "\\d+\\.\\d+\\.\\d+"))
                    .Select(f => new Response
                    {
                        Version = Version.Parse(Regex.Match(f, "\\d+\\.\\d+\\.\\d+").Value), 
                        File = Path.GetFileName(f),
                    })
                    .OrderByDescending(v => v.Version)
                    .First();
            }
        }

        public class Query : IRequest<Response>
        {
        }

        public class Response
        {
            public Version Version { get; set; }
            public string File { get; set; }
        }
    }
}