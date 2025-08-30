using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyPooler
{
    internal class Program
    {
        public static readonly object consoleLock = new object();
        private static int FoundAlive = 0;
        private static int Checked = 0;
        private static int TotalProxies = 0;
        private static dynamic ipData = null;
        private static string confirmLink = "REDACTED";
        private static int timeOut = 5000;
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeOut) };
        private static string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static string dir = Path.Combine(exeDir ?? ".", "FoundAlive.txt");
        private static bool isAuthed = false;
        private static string accessKey = null;
        private static int retries = 1;
        private static int delayMs = 0;
        private static int threadCount = 1000;
        private static double rate = 0;
        // TODO:
        // 1. If you start once and then try to go again in the same sesion the rate proxies/sec will stay at 0.0/not be calculated
        private static async Task Main()
        {
            if (isAuthed)
            {
                if (accessKey == null || accessKey == "")
                {
                    isAuthed = false;
                    lock (consoleLock)
                    {
                        Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    }
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                else
                {
                    Memory.AntiTamper();
                    string key = null; // REDACTED
                    long serverTime = await Memory.GetServerTime();
                    string sig = null; // REDACTED
                    string json = null; // REDACTED
                    using (HttpClient client = new HttpClient())
                    {
                        string url = null; //REDACTED
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync(url, content);
                        string result = await response.Content.ReadAsStringAsync();
                        if (result.Contains("REDACTED"))
                        {
                            isAuthed = false;
                            lock (consoleLock)
                            {
                                Colorful.Console.WriteLine("         Access Denied!", Color.White);
                            }
                            await Task.Delay(3000);
                            Environment.Exit(0);
                        }
                        else
                        {
                            isAuthed = true;
                            Colorful.Console.Clear();
                            FoundAlive = 0;
                            Checked = 0;
                            TotalProxies = 0;
                            ipData = null;
                            rate = 0;
                            Colorful.Console.Title = "Safeguarding your virtual vessel is the key to a secure voyage.";
                            Logo();
                            Colorful.Console.WriteLine("\n" + Center("[1]: Start Scan (FAST)"), Color.White);
                            Colorful.Console.WriteLine(Center("[2]: Start Scan (STRICT)"), Color.White);
                            Colorful.Console.WriteLine(Center("[3]: Settings"), Color.White);
                            Colorful.Console.WriteLine(Center("[4]: Discord"), Color.White);
                            Colorful.Console.WriteLine(Center("[5]: Exit"), Color.White);
                            Colorful.Console.Write("\n" + Center("--> "), Color.White);
                            string input = Colorful.Console.ReadLine();
                            if (input == "1")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                StartTitleUpdate();
                                await StartScan(Memory.links, "FAST");
                                Colorful.Console.ReadKey();
                                Environment.Exit(0);
                            }
                            if (input == "2")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                StartTitleUpdate();
                                await StartScan(Memory.links, "STRICT");
                                Colorful.Console.ReadKey();
                                Environment.Exit(0);
                            }
                            if (input == "3")
                            {
                                Colorful.Console.Clear();
                                Logo();
                                Colorful.Console.WriteLine("\n" + Center($"[1]: Amount of Threads to use = {threadCount}"), Color.White);
                                Colorful.Console.WriteLine(Center($"[2]: Link to Confirm Alive Proxies = {confirmLink}"), Color.White);
                                Colorful.Console.WriteLine(Center($"[3]: Timeout before Dead Proxies are Confirmed = {timeOut}ms"), Color.White);
                                Colorful.Console.WriteLine(Center($"[4]: Amount of Retries to check if each proxy is Alive or Dead = {retries}"), Color.White);
                                Colorful.Console.WriteLine(Center($"[5]: Delay before rechecking the same proxy and or moving on to another = {delayMs}ms"), Color.White);
                                Colorful.Console.WriteLine(Center("[6]: Go Back"), Color.White);
                                Colorful.Console.Write("\n" + Center("--> "), Color.White);
                                string input2 = Colorful.Console.ReadLine();
                                if (input2 == "1")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Amount of Threads to use = {threadCount}"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Amount of Threads to use = "), Color.White);
                                    threadCount = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 == "2")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Link to Confirm Alive Proxies = {confirmLink}"), Color.White);
                                    Colorful.Console.Write(Center($"[NEW]: Link to Confirm Alive Proxies = "), Color.White);
                                    confirmLink = Colorful.Console.ReadLine();
                                    await Main();
                                }
                                if (input2 == "3")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Timeout before Dead Proxies are Confirmed = {timeOut}ms"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Timeout before Dead Proxies are Confirmed = "), Color.White);
                                    timeOut = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 == "4")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Amount of Retries to check if each proxy is Alive or Dead = {retries}"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Amount of Retries to check if each proxy is Alive or Dead = "), Color.White);
                                    retries = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 == "5")
                                {
                                    Colorful.Console.Clear();
                                    Logo();
                                    Colorful.Console.WriteLine("\n" + Center($"[CURRENT]: Delay before rechecking the same proxy and or moving on to another = {delayMs}ms"), Color.White);
                                    Colorful.Console.Write(Center("[NEW]: Delay before rechecking the same proxy and or moving on to another = "), Color.White);
                                    delayMs = Convert.ToInt32(Colorful.Console.ReadLine());
                                    await Main();
                                }
                                if (input2 != "6")
                                {
                                    await Main();
                                }
                                else
                                {
                                    await Main();
                                }
                            }
                            if (input == "4")
                            {
                                System.Diagnostics.Process.Start("https://discord.gg/bMT4CFmPmH");
                                await Main();
                            }
                            if (input == "5")
                            {
                                Environment.Exit(0);
                            }
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
                Memory.AntiTamper();
                ipData = await ParseLink($"REDACTED{await Memory.GetIP()}?fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");
                Colorful.Console.Clear();
                Colorful.Console.Title = "Safeguarding your virtual vessel is the key to a secure voyage.";
                Logo();
                Colorful.Console.WriteLine($"         Connection Successful. Nice to see someone from {ipData["city"]}!", Color.White);
                Colorful.Console.WriteLine($"         [HWID]: {Memory.GetHWID()}", Color.White);
                Colorful.Console.Write("         Access Key --> ", Color.White);
                accessKey = Colorful.Console.ReadLine();
                Memory.AntiTamper();
                string key = null; // REDACTED
                long serverTime = await Memory.GetServerTime();
                string sig = null; // REDACTED
                string json = null; // REDACTED
                using (HttpClient client = new HttpClient())
                {
                    string url = null; // REDACTED
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    string result = await response.Content.ReadAsStringAsync();
                    if (result.Contains("REDACTED"))
                    {
                        isAuthed = false;
                        lock (consoleLock)
                        {
                            Colorful.Console.WriteLine("         Access Denied!", Color.White);
                        }
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

        private static async Task StartScan(string[] links, string scanType)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Collect all unique proxies
            ConcurrentDictionary<string, byte> uniqueProxies = new ConcurrentDictionary<string, byte>();
            List<Task> scrapeTasks = new List<Task>();

            foreach (string link in links)
            {
                scrapeTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string content = await client.GetStringAsync(link);
                        MatchCollection matches = Regex.Matches(content, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{2,5}");
                        foreach (Match match in matches)
                        {
                            uniqueProxies.TryAdd(match.Value, 0);
                        }
                    }
                    catch (HttpRequestException)
                    {
                        lock (consoleLock)
                        {
                            Colorful.Console.WriteLine($"[DEAD LINK]> {link}", Color.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (consoleLock)
                        {
                            Colorful.Console.WriteLine($"[ERROR]> {link}: {ex.Message}", Color.Red);
                        }
                    }
                }));
            }

            await Task.WhenAll(scrapeTasks);
            TotalProxies = uniqueProxies.Count;

            // Split proxies into batches based on threadCount
            string[] proxies = uniqueProxies.Keys.ToArray();
            int batchSize = (int)Math.Ceiling((double)proxies.Length / threadCount);
            List<Task> checkTasks = new List<Task>();
            List<string>[] batches = new List<string>[threadCount];

            // Initialize batches
            for (int i = 0; i < threadCount; i++)
            {
                batches[i] = new List<string>();
            }

            // Distribute proxies into batches
            for (int i = 0; i < proxies.Length; i++)
            {
                int batchIndex = i / batchSize;
                if (batchIndex < threadCount) // Ensure we don't exceed array bounds
                {
                    batches[batchIndex].Add(proxies[i]);
                }
            }

            // Initialize rate calculation variables
            int lastCheckedCount = 0;
            DateTime lastUpdate = DateTime.UtcNow;
            object rateLock = new object();

            // Buffer for file output
            ConcurrentBag<string> fileBuffer = new ConcurrentBag<string>();

            // Rate updater task
            Task rateUpdater = Task.Run(async () =>
            {
                while (!checkTasks.All(t => t.IsCompleted))
                {
                    await Task.Delay(1000);
                    lock (rateLock)
                    {
                        int currentChecked = Checked;
                        double elapsedSeconds = (DateTime.UtcNow - lastUpdate).TotalSeconds;
                        if (elapsedSeconds > 0)
                        {
                            double newRate = (currentChecked - lastCheckedCount) / elapsedSeconds;
                            rate = newRate;
                            lastCheckedCount = currentChecked;
                            lastUpdate = DateTime.UtcNow;
                        }
                    }
                }
            });

            // Process each batch in a separate task
            foreach (var batch in batches)
            {
                if (batch.Count > 0) // Only process non-empty batches
                {
                    checkTasks.Add(Task.Run(async () =>
                    {
                        foreach (string proxy in batch)
                        {
                            bool isAlive = await CheckFast(proxy);
                            if (scanType == "FAST") { isAlive = await CheckFast(proxy); }
                            if (scanType == "STRICT") { isAlive = await CheckStrict(proxy); }
                            Interlocked.Increment(ref Checked);
                            if (isAlive)
                            {
                                string[] array = proxy.Split(':');
                                if (array.Length < 2)
                                {
                                    lock (consoleLock)
                                    {
                                        Colorful.Console.WriteLine($"[ERROR]> Invalid proxy format: {proxy}", Color.Red);
                                    }
                                    continue;
                                }
                                JToken localIpData = await ParseLink($"REDACTED{array[0]}?fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");
                                Interlocked.Increment(ref FoundAlive);
                                fileBuffer.Add(proxy + "\n");
                                string dbType = "ALIVE";
                                if (localIpData != null && localIpData["status"] != null && localIpData["status"].ToString() != "noInternet")
                                {
                                    bool isProxy = localIpData["proxy"]?.ToString().Contains("False") ?? false;
                                    bool isHosting = localIpData["hosting"]?.ToString().Contains("False") ?? false;
                                    if (isProxy && isHosting)
                                    {
                                        dbType = localIpData["mobile"]?.ToString().Contains("True") ?? false ? "MOBILE" : "HQ";
                                        lock (consoleLock)
                                        {
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write(dbType == "MOBILE" ? " MOBILE" : " HQ", Color.Lime);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{localIpData["city"] ?? "[RateLimited]"}, {localIpData["country"] ?? "[RateLimited]"} ({localIpData["countryCode"] ?? "[RateLimited]"})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{localIpData["isp"] ?? "[RateLimited]"}\n", Color.White);
                                        }
                                    }
                                    else
                                    {
                                        lock (consoleLock)
                                        {
                                            Colorful.Console.Write("         [", Color.White);
                                            Colorful.Console.Write("FOUND", Color.LimeGreen);
                                            Colorful.Console.Write("]", Color.White);
                                            Colorful.Console.Write("> ", Color.Gray);
                                            Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                            Colorful.Console.Write(" in ", Color.LightGray);
                                            Colorful.Console.Write($"{localIpData["city"] ?? "[RateLimited]"}, {localIpData["country"] ?? "[RateLimited]"} ({localIpData["countryCode"] ?? "[RateLimited]"})", Color.White);
                                            Colorful.Console.Write(" by ", Color.LightGray);
                                            Colorful.Console.Write($"{localIpData["isp"] ?? "[RateLimited]"}\n", Color.White);
                                        }
                                    }
                                }
                                else
                                {
                                    lock (consoleLock)
                                    {
                                        Colorful.Console.Write("         [", Color.White);
                                        Colorful.Console.Write("FOUND", Color.LimeGreen);
                                        Colorful.Console.Write("]", Color.White);
                                        Colorful.Console.Write("> ", Color.Gray);
                                        Colorful.Console.WriteWithGradient(proxy, Color.Red, Color.White, 10);
                                        Colorful.Console.Write(" in ", Color.LightGray);
                                        Colorful.Console.Write($"[RateLimited]", Color.White);
                                        Colorful.Console.Write(" by ", Color.LightGray);
                                        Colorful.Console.Write($"[RateLimited]\n", Color.White);
                                    }
                                }
                                await Memory.db($"**`{proxy}`**", dbType);
                            }
                        }
                    }));
                }
            }

            await Task.WhenAll(checkTasks);
            await rateUpdater;
            // Write buffered file output
            File.AppendAllText(dir, string.Concat(fileBuffer));
            stopwatch.Stop();
            lock (consoleLock)
            {
                Colorful.Console.WriteLine($"         Scan completed in {stopwatch.Elapsed}s!", Color.White);
            }
            await Task.Delay(3000);
            await Main();
        }

        private static async Task<bool> OldCheck(string proxy) // Old Method that uses REDACTED with each proxy and confrims the said returned IP is the same as the proxy!
        {
            string[] proxyParts = proxy.Split(':');
            if (proxyParts.Length < 2)
            {
                return false; // Invalid proxy format
            }
            string proxyIp = proxyParts[0];
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
                        string jsonResponse = await checkClient.GetStringAsync("REDACTED").ConfigureAwait(false);
                        var jsonObject = JToken.Parse(jsonResponse);
                        string publicIp = jsonObject["ip"]?.ToString();
                        if (!string.IsNullOrEmpty(publicIp) && publicIp == proxyIp)
                        {
                            return true; // Proxy IP matches public IP
                        }
                    }
                    catch
                    {
                        if (attempt == retries)
                        {
                            return false; // All retries failed
                        }
                    }
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }
            return false;
        }

        private static async Task<bool> CheckStrict(string proxy) // New Method using the old method but then also checking the confirmLink for a 200 code!!
        {
            string[] proxyParts = proxy.Split(':');
            if (proxyParts.Length < 2)
            {
                return false; // Invalid proxy format
            }
            string proxyIp = proxyParts[0];
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
                        // First check: Verify proxy IP
                        string jsonResponse = await checkClient.GetStringAsync("REDACTED").ConfigureAwait(false);
                        var jsonObject = JToken.Parse(jsonResponse);
                        string publicIp = jsonObject["ip"]?.ToString();
                        if (!string.IsNullOrEmpty(publicIp) && publicIp == proxyIp)
                        {
                            // Second check: Verify connection to confirmLink with 200 status
                            await Task.Delay(delayMs).ConfigureAwait(false);
                            try
                            {
                                var response = await checkClient.GetAsync(confirmLink).ConfigureAwait(false);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    return true; // Proxy IP matches and confirmLink returns 200
                                }
                            }
                            catch
                            {
                                // Failed to connect to confirmLink
                                if (attempt == retries)
                                {
                                    return false; // All retries failed for confirmLink
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (attempt == retries)
                        {
                            return false; // All retries failed for IP check
                        }
                    }
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }
            return false;
        }

        private static async Task<bool> CheckFast(string proxy) // New TEST Method checking the confirmLink(REDACTED) for a 200 code!!
        {
            string[] proxyParts = proxy.Split(':');
            if (proxyParts.Length < 2)
            {
                return false; // Invalid proxy format
            }
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
                        // First check: Verify connection to confirmLink with 200 status
                        await Task.Delay(delayMs).ConfigureAwait(false);
                        try
                        {
                            var response = await checkClient.GetAsync(confirmLink).ConfigureAwait(false);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                return true; // confirmLink returns 200
                            }
                        }
                        catch
                        {
                            // Failed to connect to confirmLink
                            if (attempt == retries)
                            {
                                return false; // All retries failed for confirmLink
                            }
                        }
                    }
                    catch
                    {
                        if (attempt == retries)
                        {
                            return false; // All retries failed for IP check
                        }
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

        public static void Logo()
        {
            lock (consoleLock)
            {
                Colorful.Console.WriteWithGradient("\r\n\r\n                                        |                   |     |     |                          \r\n                                        |    ,---.,---.,---.|,---.|--- ' ,---.                     \r\n                                        |    |   ||   ||   |||   ||      `---.                     \r\n                                        `---'`---'`   '`---|``   '`---'  `---'                     \r\n                                                       `---'                                                                                \r\n                            ,---.                   ,---.          |    |              \r\n                            |---',---.,---..  ,,   .|  _.,---.,---.|---.|---.,---.,---.\r\n                            |    |    |   | >< |   ||   ||    ,---||   ||   ||---'|    \r\n                            `    `    `---''  ``---|`---'`    `---^`---'`---'`---'` v2.0   \r\n                                               `---'                                   \n", Color.Red, Color.White, 5);
            }
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
                    lock (consoleLock)
                    {
                        Colorful.Console.Title = $"You have checked {Checked}/{TotalProxies} proxies and found {FoundAlive} alive at a speed of {rate:F1} proxies/sec!";
                    }
                    await Task.Delay(100);
                }
            });
        }
    }
}