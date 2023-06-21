using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Grpc.Client
{
    public interface ITokenProvider
    {
        Task<string?> GetTokenAsync();
    }

    public class AuthTokenProvider : ITokenProvider
    {
        private string? _token;
        private readonly string _authUrl;

        public AuthTokenProvider(string host, int port, bool useSsl)
        {
            _authUrl = $"http{(useSsl ? "s" : "")}://{host}:{port}/Auth/login";
        }

        public async Task<string?> GetTokenAsync()
        {
            if (_token == null)
            {
                var handler = new HttpClientHandler();
                handler.UseCookies = true;
                var client = new HttpClient(handler);

                var body = JsonConvert.SerializeObject(new
                {
                    UserName = "user",
                    Password = "ps"
                });

                var response = await client.PostAsync(_authUrl, new StringContent(body, Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                var auth = handler.CookieContainer.GetAllCookies().FirstOrDefault(i => i.Name.Contains("Bearer"));
                _token = auth?.Value;
            }

            return _token;
        }
    }
}
