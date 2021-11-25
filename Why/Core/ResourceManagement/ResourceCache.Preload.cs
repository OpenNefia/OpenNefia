using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Why.Core.Graphics;
using Why.Core;
using Why.Core.IoC;
using Why.Core.Log;

namespace Why.Core.ResourceManagement
{
    internal partial class ResourceCache
    {
        // TODO [Dependency] private readonly IClyde _clyde = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        // TODO [Dependency] private readonly IConfigurationManager _configurationManager = default!;

        public void PreloadTextures()
        {
            var sawmill = _logManager.GetSawmill("res.preload");

            //if (!_configurationManager.GetCVar(CVars.ResTexturePreloadingEnabled))
            //{
            //    sawmill.Debug($"Skipping texture preloading due to CVar value.");
            //    return;
            //}

            PreloadTextures(sawmill);
        }

        private void PreloadTextures(ISawmill sawmill)
        {
            /*
            sawmill.Debug("Preloading textures...");
            var sw = Stopwatch.StartNew();
            var resList = GetTypeDict<TextureResource>();

            var texList = ContentFindFiles("/Textures/")
                // Skip PNG files inside RSIs.
                .Where(p => p.Extension == "png" && !p.ToString().Contains(".rsi/") && !resList.ContainsKey(p))
                .Select(p => new TextureResource.LoadStepData {Path = p})
                .ToArray();

            Parallel.ForEach(texList, data =>
            {
                try
                {
                    TextureResource.LoadPreTexture(this, data);
                }
                catch (Exception e)
                {
                    // Mark failed loads as bad and skip them in the next few stages.
                    // Avoids any silly array resizing or similar.
                    sawmill.Error($"Exception while loading RSI {data.Path}:\n{e}");
                    data.Bad = true;
                }
            });

            foreach (var data in texList)
            {
                if (data.Bad)
                    continue;

                try
                {
                    TextureResource.LoadTexture(_clyde, data);
                }
                catch (Exception e)
                {
                    sawmill.Error($"Exception while loading RSI {data.Path}:\n{e}");
                    data.Bad = true;
                }
            }

            var errors = 0;
            foreach (var data in texList)
            {
                if (data.Bad)
                {
                    errors += 1;
                    continue;
                }

                try
                {
                    var texResource = new TextureResource();
                    texResource.LoadFinish(this, data);
                    resList[data.Path] = texResource;
                }
                catch (Exception e)
                {
                    sawmill.Error($"Exception while loading RSI {data.Path}:\n{e}");
                    data.Bad = true;
                    errors += 1;
                }
            }

            sawmill.Debug(
                "Preloaded {CountLoaded} textures ({CountErrored} errored) in {LoadTime}",
                texList.Length,
                errors,
                sw.Elapsed);
            */
        }
    }
}
