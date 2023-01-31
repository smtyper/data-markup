namespace DataMarkup.Frontend;

public class DataMarkupClient
{
    private readonly HttpClient _httpClient;

    public DataMarkupClient(HttpClient httpClient) => _httpClient = httpClient;

    // public async ValueTask<HttpStatusCode> RegisterAsync(RegisterParameters registerModel)
    // {
    //     const string uri = "Account/Register";
    //     using var request = new HttpRequestMessage(HttpMethod.Post, uri);
    //     request.Content = JsonContent.Create(registerModel);
    //
    //     using var response = await _httpClient.SendAsync(request);
    //
    //     return response.StatusCode;
    // }
}
