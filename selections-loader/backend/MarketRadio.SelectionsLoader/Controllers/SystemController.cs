using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [Route("/api/system")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly WindowKeeper _windowKeeper;

        public SystemController(WindowKeeper windowKeeper)
        {
            _windowKeeper = windowKeeper;
        }

        [HttpPost("open-folder")]
        public async Task<SelectionFolderOpenResult> OpenFolderDialog()
        {
            var result = await Electron.Dialog.ShowOpenDialogAsync(_windowKeeper.CurrentWindow, new OpenDialogOptions
            {
                Properties = new []
                {
                    OpenDialogProperty.openDirectory,
                },
            });

            var fullPath = result.SingleOrDefault();

            if (fullPath == null)
            {
                return null;
            }

            return new SelectionFolderOpenResult
            {
                SelectionName = Path.GetFileName(fullPath),
                Tracks = Directory.GetFiles(fullPath).Select(Path.GetFileName).ToList(),
                FullPath = fullPath, 
            };
        }
    }
}