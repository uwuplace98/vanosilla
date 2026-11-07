using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.NosMall;
using WingsAPI.Data.GameData;

namespace Plugin.ResourceLoader.Loaders
{
    public class NosMallResourceFileLoader : IResourceLoader<NosMallItemDto>
    {
        private const string FILE_NAME = "uk_nosmall.dat";
        private readonly ResourceLoadingConfiguration _configuration;
        private readonly List<NosMallItemDto> _items = new();

        public NosMallResourceFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<NosMallItemDto>> LoadAsync()
        {
            if (_items.Any())
            {
                return _items;
            }

            string filePath = Path.Combine(_configuration.GameDataPath, FILE_NAME);

            if (!File.Exists(filePath))
            {
                Log.Warn($"[RESOURCE_LOADER] {filePath} not found. Skipping NosMall load.");
                return _items;
            }

            using var stream = new StreamReader(filePath, Encoding.GetEncoding(1252));

            var item = new NosMallItemDto();
            bool itemAreaBegin = false;
            int itemCounter = 0;
            bool isDescription = false;
            StringBuilder descriptionBuilder = new StringBuilder();

            string line;
            while ((line = await stream.ReadLineAsync()) != null)
            {
                try
                {
                    string[] currentLine = line.Split('\t');
                    if (currentLine.Length == 0) continue;

                    string key = currentLine[0];

                    if (isDescription)
                    {
                        if (key == "DEND")
                        {
                            isDescription = false;
                            item.Description = descriptionBuilder.ToString().Trim();
                            descriptionBuilder.Clear();
                        }
                        else
                        {
                            descriptionBuilder.AppendLine(line);
                        }
                        continue;
                    }

                    switch (key)
                    {
                        case "VNUM":
                            // VNUM {id} 163 {limited} {hit} {new} {activated} {selling}
                            if (currentLine.Length > 6 && currentLine[6] != "0")
                            {
                                itemAreaBegin = false;
                                break;
                            }

                            itemAreaBegin = true;
                            if (currentLine.Length > 1) item.Id = int.Parse(currentLine[1]);
                            if (currentLine.Length > 3) item.IsLimited = currentLine[3] == "1";
                            if (currentLine.Length > 4) item.IsHit = currentLine[4] == "1";
                            if (currentLine.Length > 5) item.IsNew = currentLine[5] == "1";
                            if (currentLine.Length > 7) item.IsSelling = currentLine[7] == "1";
                            break;

                        case "ITEM":
                            // ITEM 1 0 {img} {itemid} 0 {window}
                            if (currentLine.Length > 4) item.ItemVnum = int.Parse(currentLine[4]);
                            if (currentLine.Length > 6) item.TabId = int.Parse(currentLine[6]);
                            break;

                        case "TITLE1": //Name
                            if (currentLine.Length > 1) item.Name = currentLine[1];
                            break;

                        case "TITLE2": //Description
                            if (currentLine.Length > 1) item.Title2 = currentLine[1];
                            break;

                        case "COST":
                            // COST {price} 0 1 {amount} {durType} {durVal}
                            if (currentLine.Length > 1) item.Price = long.Parse(currentLine[1]);
                            if (currentLine.Length > 4) item.Amount = int.Parse(currentLine[4]);
                            if (currentLine.Length > 5) item.DurationType = byte.Parse(currentLine[5]);
                            if (currentLine.Length > 6) item.DurationValue = int.Parse(currentLine[6]);
                            break;

                        case "DSTART":
                            isDescription = true;
                            break;

                        case "END":
                            if (itemAreaBegin)
                            {
                                _items.Add(item);
                                itemCounter++;
                            }
                            
                            item = new NosMallItemDto();
                            itemAreaBegin = false;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error while parsing line '{line}' in {FILE_NAME}", e);
                }
            }

            Log.Info($"[RESOURCE_LOADER] {itemCounter} NosMall items loaded");
            return _items;
        }
    }
}
