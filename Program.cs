using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ProxyPooler
{
    internal class Program
    {
        private static string[] links = new string[]
        {
            // REDACTED
        };
        private static int FoundAlive = 0;
        private static int Checked = 0;
        private static int TotalUnchecked = 0;
        private static dynamic ipData = null;
        private static string confirmLink = "https://www.google.com/";
        private static int timeOut = 5000;
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeOut) };
        private static string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string dir = Path.Combine(exeDir ?? ".", "FoundAlive.txt");
        private static bool isAuthed = false;
        private static string accessKey = null;
        private static int retries = 2;
        private static int delayMs = 500;
        private static async Task Main()
        {
            if (isAuthed)
            {
                //Memory.AntiTamper(); Not working yet
                if (accessKey == null) 
                {
                    isAuthed = false;
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                if (accessKey == "")
                {
                    isAuthed = false;
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                else
                {
                    // REDACTED
                    using (// REDACTED)
                    {
                        // REDACTED
                        if (// REDACTED)
                        {
                            isAuthed = false;
                            Colorful.Console.WriteLine("         Access Denied!", Color.White);
                            await Task.Delay(3000);
                            Environment.Exit(0);
                        }
                        else
                        {
                            isAuthed = true;
                            Colorful.Console.Clear();
                            FoundAlive = 0;
                            Checked = 0;
                            TotalUnchecked = 0;
                            ipData = null;
                            Colorful.Console.Title = "Safeguarding your virtual vessel is the key to a secure voyage.";
                            Logo();
                            Colorful.Console.WriteLine("\n" + Center("[1]: Detailed Mode"), Color.White);
                            Colorful.Console.WriteLine(Center("[2]: Filtered Mode"), Color.White);
                            Colorful.Console.WriteLine(Center("[3]: ReCheck Mode"), Color.White);
                            Colorful.Console.WriteLine(Center("[4]: Settings"), Color.White);
                            Colorful.Console.WriteLine(Center("[5]: Discord"), Color.White);
                            Colorful.Console.WriteLine(Center("[6]: Exit"), Color.White);
                            Colorful.Console.Write("\n" + Center("--> "), Color.White);
                            string input = Colorful.Console.ReadLine();
                            // Detailed Mode
                            if (input == "1")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                StartTitleUpdate();
                                await ScrapeDetailedMode(links);
                                Colorful.Console.ReadKey();
                                Environment.Exit(0);
                            }
                            // Filtered Mode
                            if (input == "2")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                Colorful.Console.WriteLine("\n" + Center("[1]: Private/High Quality(HQ) ONLY"), Color.White);
                                Colorful.Console.WriteLine(Center("[2]: Go Back (More Filtered Modes Coming Soon!)"), Color.White);
                                Colorful.Console.Write("\n" + Center("--> "), Color.White);
                                string input2 = Colorful.Console.ReadLine();
                                if (input2 == "1")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    StartTitleUpdate();
                                    await ScrapeHQONLYMode(links);
                                    Colorful.Console.ReadKey();
                                    Environment.Exit(0);
                                }
                                if (input2 != "2")
                                {
                                    await Main();
                                }
                                else
                                {
                                    await Main();
                                }
                            }
                            // ReCheck Mode
                            if (input == "3")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                StartTitleUpdate();
                                await ReCheckONLYMode();
                                Colorful.Console.ReadKey();
                                Environment.Exit(0);
                            }
                            // Settings (TODO: Add error handling for invalid values/entries)
                            if (input == "4")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                Colorful.Console.WriteLine("\n" + Center("[1]: Link to Confirm Alive Proxies = " + confirmLink), Color.White);
                                Colorful.Console.WriteLine(Center($"[2]: Timeout before Dead Proxies are Confirmed = {timeOut}ms"), Color.White);
                                Colorful.Console.WriteLine(Center($"[3]: Amount of Retries to check if each proxy is Alive or Dead = {retries}"), Color.White);
                                Colorful.Console.WriteLine(Center($"[4]: Delay before rechecking the same proxy and or moving on to another = {delayMs}ms"), Color.White);
                                Colorful.Console.WriteLine(Center("[5]: Go Back"), Color.White);
                                Colorful.Console.Write("\n" + Center("--> "), Color.White);
                                string input2 = Colorful.Console.ReadLine();
                                if (input2 == "1")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center("[CURRENT]: Link to Confirm Alive Proxies = " + confirmLink), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Link to Confirm Alive Proxies = "), Color.White);
                                    confirmLink = Colorful.Console.ReadLine();
                                    await Main();
                                }
                                if (input2 == "2")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Timeout before Dead Proxies are Confirmed = {timeOut}ms"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Timeout before Dead Proxies are Confirmed = "), Color.White);
                                    timeOut = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 == "3")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Amount of Retries to check if each proxy is Alive or Dead = {retries}"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Amount of Retries to check if each proxy is Alive or Dead = "), Color.White);
                                    retries = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 == "4")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Delay before rechecking the same proxy and or moving on to another = {delayMs}ms"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Delay before rechecking the same proxy and or moving on to another = "), Color.White);
                                    delayMs = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 != "5")
                                {
                                    await Main();
                                }
                                else
                                {
                                    await Main();
                                }
                            }
                            // Discord
                            if (input == "5")
                            {
                                Process.Start("https://discord.gg/bMT4CFmPmH");
                                await Main();
                            }
                            // Exit
                            if (input == "6")
                            {
                                Environment.Exit(0);
                            }
                            // Anything other than a valid input
                            else
                            {
                                await Main();
                            }
                        }
                    }
                }
            }
            else
            {
                //Memory.AntiTamper(); Not working yet
                Colorful.Console.Clear();
                FoundAlive = 0;
                Checked = 0;
                TotalUnchecked = 0;
                ipData = null;
                Colorful.Console.Title = "Safeguarding your virtual vessel is the key to a secure voyage.";
                Logo();
                Colorful.Console.WriteLine($"         [HWID]: {Memory.GetHWID()}", Color.White);
                Colorful.Console.Write("         Access Key --> ", Color.White);
                accessKey = Colorful.Console.ReadLine();
                // REDACTED
                using (// REDACTED)
                {
                    // REDACTED
                    if (// REDACTED)
                    {
                        isAuthed = false;
                        Colorful.Console.WriteLine("         Access Denied!", Color.White);
                        await Task.Delay(3000);
                        Environment.Exit(0);
                    }
                    else
                    {
                        isAuthed = true;
                        await Main();
                    }
                }
            }
        }
        // Scrapes proxies with detailed info (country, ISP) and saves live proxies to file.
        private static async Task ScrapeDetailedMode(string[] links)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(links.Length);
            ConcurrentDictionary<string, byte> uniqueProxies = new ConcurrentDictionary<string, byte>();
            List<Task> tasks = new List<Task>();
            foreach (string link in links)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        string content = await client.GetStringAsync(link);
                        MatchCollection matches = Regex.Matches(content, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{2,5}");
                        foreach (Match match in matches)
                        {
                            string proxy = match.Value;
                            if (uniqueProxies.TryAdd(proxy, 0))
                            {
                                bool isAlive = await Check(proxy);
                                Interlocked.Increment(ref Checked);
                                if (isAlive)
                                {
                                    string[] array = proxy.Split(':');
                                    ipData = await ParseLink($"// REDACTED{array[0]}?fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");
                                    Interlocked.Increment(ref FoundAlive);
                                    File.AppendAllText(dir, proxy + "\n");
                                    if (((string)ipData["proxy"]).Contains("False") && ((string)ipData["hosting"]).Contains("False"))
                                    {
                                        if (((string)ipData["mobile"]).Contains("True"))
                                        {
                                            await Memory.db($"**`{proxy}`**", "MOBILE");
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(" MOBILE", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                        }
                                        else
                                        {
                                            await Memory.db($"**`{proxy}`**", "HQ");
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(" HQ", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                        }
                                    }
                                    else
                                    {
                                        await Memory.db($"**`{proxy}`**", "ALIVE");
                                        Colorful.Console.Write("         [", Color.White);
                                        Colorful.Console.Write("FOUND", Color.LimeGreen);
                                        Colorful.Console.Write("]", Color.White);
                                        Colorful.Console.Write("> ", Color.Gray);
                                        Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                        Colorful.Console.Write(" in ", Color.LightGray);
                                        Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                        Colorful.Console.Write(" by ", Color.LightGray);
                                        Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                    }
                                }
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {
                        Colorful.Console.WriteLine($"[DEAD LINK]> {link}", Color.Red);
                    }
                    catch (Exception ex)
                    {
                        Colorful.Console.WriteLine($"[ERROR]> {link}: {ex.Message}", Color.Red);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            await Task.WhenAll(tasks);
            Colorful.Console.WriteLine($"         Scan complete!", Color.White);
            await Task.Delay(3000);
            await Main();
        }
        // Scrapes only high-quality (non-proxy flagged) proxies with detailed info and saves to file.
        private static async Task ScrapeHQONLYMode(string[] links)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(links.Length); // Increased concurrency for faster processing
            ConcurrentDictionary<string, byte> uniqueProxies = new ConcurrentDictionary<string, byte>();
            List<Task> tasks = new List<Task>();
            foreach (string link in links)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        string content = await client.GetStringAsync(link);
                        MatchCollection matches = Regex.Matches(content, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{2,5}");
                        foreach (Match match in matches)
                        {
                            string proxy = match.Value;
                            if (uniqueProxies.TryAdd(proxy, 0))
                            {
                                bool isAlive = await Check(proxy);
                                Interlocked.Increment(ref Checked);
                                if (isAlive)
                                {
                                    string[] array = proxy.Split(':');
                                    ipData = await ParseLink($"// REDACTED{array[0]}?fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");
                                    if (((string)ipData["proxy"]).Contains("False") && ((string)ipData["hosting"]).Contains("False"))
                                    {
                                        if (((string)ipData["mobile"]).Contains("True"))
                                        {
                                            Interlocked.Increment(ref FoundAlive);
                                            File.AppendAllText(dir, proxy + "\n");
                                            await Memory.db($"**`{proxy}`**", "MOBILE");
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(" MOBILE", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                        }
                                        else
                                        {
                                            Interlocked.Increment(ref FoundAlive);
                                            File.AppendAllText(dir, proxy + "\n");
                                            await Memory.db($"**`{proxy}`**", "HQ");
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(" HQ", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {
                        Colorful.Console.WriteLine($"[DEAD LINK]> {link}", Color.Red);
                    }
                    catch (Exception ex)
                    {
                        Colorful.Console.WriteLine($"[ERROR]> {link}: {ex.Message}", Color.Red);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            await Task.WhenAll(tasks);
            Colorful.Console.WriteLine($"         Scan complete!", Color.White);
            await Task.Delay(3000);
            await Main();
        }
        // Re-checks proxies from FoundAlive.txt, clears the file, and saves newly confirmed live proxies.
        private static async Task ReCheckONLYMode()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(links.Length); // Increased concurrency for faster processing
            ConcurrentDictionary<string, byte> uniqueProxies = new ConcurrentDictionary<string, byte>();
            if (File.Exists(dir))
            {
                string[] proxies = File.ReadAllLines(dir).Distinct().ToArray(); // Remove duplicates before rechecking
                File.WriteAllText(dir, string.Empty); // Clear the file
                List<Task> tasks = new List<Task>();
                foreach (string proxy in proxies)
                {
                    if (!string.IsNullOrWhiteSpace(proxy) && uniqueProxies.TryAdd(proxy, 0))
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                bool isAlive = await Check(proxy);
                                Interlocked.Increment(ref Checked);
                                if (isAlive)
                                {
                                    string[] array = proxy.Split(':');
                                    ipData = await ParseLink($"// REDACTED{array[0]}?fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");
                                    Interlocked.Increment(ref FoundAlive);
                                    File.AppendAllText(dir, proxy + "\n");
                                    if (((string)ipData["proxy"]).Contains("False") && ((string)ipData["hosting"]).Contains("False"))
                                    {
                                        if (((string)ipData["mobile"]).Contains("True"))
                                        {
                                            await Memory.db($"**`{proxy}`**", "MOBILE");
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(" MOBILE", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                        }
                                        else
                                        {
                                            await Memory.db($"**`{proxy}`**", "HQ");
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(" HQ", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                        }
                                    }
                                    else
                                    {
                                        await Memory.db($"**`{proxy}`**", "ALIVE");
                                        Colorful.Console.Write("         [", Color.White);
                                        Colorful.Console.Write("FOUND", Color.LimeGreen);
                                        Colorful.Console.Write("]", Color.White);
                                        Colorful.Console.Write("> ", Color.Gray);
                                        Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                        Colorful.Console.Write(" in ", Color.LightGray);
                                        Colorful.Console.Write($"{ipData["city"]}, {ipData["country"]} ({ipData["countryCode"]})", Color.White);
                                        Colorful.Console.Write(" by ", Color.LightGray);
                                        Colorful.Console.Write($"{ipData["isp"]} - {ipData["asname"]}\n", Color.White);
                                    }
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }
                }
                await Task.WhenAll(tasks);
                Colorful.Console.WriteLine($"         ReCheck complete!", Color.White);
                await Task.Delay(3000);
                await Main();
            }
            else
            {
                Colorful.Console.WriteLine($"[ERROR]> {dir} does not exist!", Color.Red);
            }
        }

        private static async Task<bool> Check(string proxy)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy),
                    UseProxy = true
                };
                using (var checkClient = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeOut) })
                {
                    try
                    {
                        using (var response = await checkClient.GetAsync(confirmLink).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                                return true;
                        }
                    }
                    catch
                    {
                        if (attempt == retries)
                            return false;
                    }
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }
            return false;
        }
        private static async Task<JToken> ParseLink(string link)
        {
            try
            {
                string jsonText = await client.GetStringAsync(link);
                return JToken.Parse(jsonText);
            }
            catch
            {
                return JToken.Parse("{\"status\":\"noInternet\"}");
            }
        }
        private static void Logo()
        {
            Colorful.Console.WriteWithGradient("\r\n\r\n                                        |                   |     |     |                          \r\n                                        |    ,---.,---.,---.|,---.|--- ' ,---.                     \r\n                                        |    |   ||   ||   |||   ||      `---.                     \r\n                                        `---'`---'`   '`---|``   '`---'  `---'                     \r\n                                                       `---'                                                                                \r\n                            ,---.                   ,---.          |    |              \r\n                            |---',---.,---..  ,,   .|  _.,---.,---.|---.|---.,---.,---.\r\n                            |    |    |   | >< |   ||   ||    ,---||   ||   ||---'|    \r\n                            `    `    `---''  ``---|`---'`    `---^`---'`---'`---'` v2.0   \r\n                                               `---'                                   \n", Color.Red, Color.White, 5);
        }
        private static string Center(string text)
        {
            int count = (Colorful.Console.WindowWidth - text.Length) / 2;
            return new string(' ', count) + text;
        }
        private static void StartTitleUpdate()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Colorful.Console.Title = $"Currently you have Checked {Checked} Proxies & Found {FoundAlive} Alive Proxies";
                    // Added a delay here dose not effect CPU %
                }
            });
        }
    }
}