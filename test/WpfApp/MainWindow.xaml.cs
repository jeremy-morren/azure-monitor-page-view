using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using Azure.Core;
using Azure.Identity;
using Microsoft.ApplicationInsights;
using Timer = System.Timers.Timer;

namespace WpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(HttpClient client) : this()
    {
        CreateTimer(5, WriteLog, client);
    }

    private void WriteLog(string text)
    {
        Dispatcher.Invoke(() => Log.Text = text + Environment.NewLine + Log.Text);
    }

    private static void CreateTimer(double interval, Action<string> log, HttpClient client)
    {
        var timer = new Timer();
        timer.Interval = TimeSpan.FromSeconds(interval).TotalMilliseconds;

        timer.Elapsed += (_,_) => Task.Run(() => GetWeather(log, client));
        timer.Start();
        Task.Run(() => GetWeather(log, client)); // Initial call
    }

    public static AccessToken? AuthToken;
    private static readonly SemaphoreSlim Mutex = new(1, 1);

    private static async Task GetWeather(Action<string> log, HttpClient client)
    {
        if (Mutex.CurrentCount == 0) return;
        await Mutex.WaitAsync();
        try
        {
            if (AuthToken == null || AuthToken.Value.ExpiresOn < DateTimeOffset.Now)
            {
                AuthToken = await new AzureCliCredential().GetTokenAsync(
                    new TokenRequestContext(["af227a97-db6b-4ffb-bd44-b866e269b6b6/.default"]));
            }

            var request = new HttpRequestMessage()
            {
                // RequestUri = new Uri("http://localhost:5289/api/users"),
                // RequestUri = new Uri("http://localhost:5289/api/customers"),
                RequestUri = new Uri("http://localhost:5289/api/cities"),
                Method = HttpMethod.Get,
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue(AuthToken.Value.TokenType, AuthToken.Value.Token)
                }
            };
            using var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            log($"{DateTime.Now:hh:mm tt} - {(int)response.StatusCode} - {content}");
        }
        catch (Exception e)
        {
            log($"{DateTime.Now:hh:mm tt} - {e.Message}");
        }
        finally
        {
            Mutex.Release();
        }
    }
}