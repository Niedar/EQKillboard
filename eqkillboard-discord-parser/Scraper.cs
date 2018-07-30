using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace eqkillboard_discord_parser {
    public class Scraper {
    private IConfiguration config { get; set; }
    public string charBrowserUrl { get; set; }

    public Scraper() {
        config = Configuration.Default.WithDefaultLoader();
        charBrowserUrl = "https://riseofzek.com/charbrowser/index.php?page=character&char=";
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

    }
}