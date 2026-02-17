using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TicketsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets()
    {
        var client = _httpClientFactory.CreateClient();
        var apiUrl = _configuration["ThirdPartyApi:BaseUrl"];
        var apiKey = _configuration["ThirdPartyApi:ApiKey"];

        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();


        var odataResponse = JsonSerializer.Deserialize<ODataResponse<TicketDto>>(content,
           new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var tickets = odataResponse?.Value ?? new List<TicketDto>();

        var calendarEvents = tickets.Select(t => new
        {
            id = t.Id,
            title = t.Title,
            start = t.DueDate.ToString("yyyy-MM-dd"),
            allDay = true
        });

        return Ok(calendarEvents);

    }
}

public class TicketDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime DueDate { get; set; }
}

public class ODataResponse<T>
{
    public List<T> Value { get; set; }
}
   