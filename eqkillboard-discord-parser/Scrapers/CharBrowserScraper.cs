using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace EQKillboard.DiscordParser.Scrapers {
    public class CharBrowserScraper {
        private IConfiguration config { get; set; }
        private const string charBrowserUrl = "https://riseofzek.com/charbrowser/index.php?page=character&char=";
        private const string npcSearchUrl = "https://riseofzek.com/test/?a=advanced_npcs&isearch=Search&iname=";

        public CharBrowserScraper() {
            config = Configuration.Default.WithDefaultLoader();
        }

        public async Task<string> ScrapeCharInfo(string charName) {
            var address = charBrowserUrl + charName;
            var document = await BrowsingContext.New(config).OpenAsync(address);
            var cellSelector = "div.InventoryStats table tbody tr:nth-child(5)";
            var cells = document.QuerySelector(cellSelector);
            
            string classLevel = string.Empty;
            if (cells != null)
            {
                classLevel = cells.TextContent;
            }
            //Select(m => m.TextContent).FirstOrDefault();

            //var classLevel = 
            // Remove deity from retrieved string!!

            return classLevel;
        }

        public async Task<bool> ScrapeIsNpc(string charName)
        {
            var address = npcSearchUrl + charName;
            var document = await BrowsingContext.New(config).OpenAsync(address);
            var cellSelector = "div.page-content-ajax li a";
            var cells = document.QuerySelectorAll(cellSelector);

            return cells.Any(x => x.TextContent.Equals(charName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}