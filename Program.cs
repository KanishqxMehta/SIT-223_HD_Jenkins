using System.Net.Http;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Endpoint to get location details and elevation
app.MapGet("geocode/{location}", async (string location) =>
{
    // Your Google Maps API key
    string apiKey = "AIzaSyCprcSP7ak4FYOJk4iqSqNy_IXa7Y0eDms";

    // Base URL for the Google Maps Geocoding API
    string geocodeApiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={location}&key={apiKey}";

    // Make an HTTP request to Google Maps Geocoding API
    using (var httpClient = new HttpClient())
    {
        HttpResponseMessage geocodeResponse = await httpClient.GetAsync(geocodeApiUrl);

        if (geocodeResponse.IsSuccessStatusCode)
        {
            // Parse the response to get location details
            var geocodeResponseString = await geocodeResponse.Content.ReadAsStringAsync();

            var geocodeData = Newtonsoft.Json.JsonConvert.DeserializeObject<GeocodeData>(geocodeResponseString);

            
            var locationDetails = $"Location details for {location}:\n{geocodeData!.Results[0].FormattedAddress}\nLatitude: {geocodeData.Results[0].Geometry.Location.Lat}\nLongitude: {geocodeData.Results[0].Geometry.Location.Lng}";

            // Get latitude and longitude for the Elevation API request
            float latitude = geocodeData.Results[0].Geometry.Location.Lat;
            float longitude = geocodeData.Results[0].Geometry.Location.Lng;

            // Base URL for the Google Maps Elevation API
            string elevationApiUrl = $"https://maps.googleapis.com/maps/api/elevation/json?locations={latitude},{longitude}&key={apiKey}";

            // Make an HTTP request to Google Maps Elevation API
            HttpResponseMessage elevationResponse = await httpClient.GetAsync(elevationApiUrl);

            if (elevationResponse.IsSuccessStatusCode)
            {
                // Parse the response to get elevation data
                var elevationResponseString = await elevationResponse.Content.ReadAsStringAsync();
                

                var elevationData = Newtonsoft.Json.JsonConvert.DeserializeObject<ElevationData>(elevationResponseString);
                // Ensure there are results before accessing Elevation
                if (elevationData!.Results.Length > 0)
                {
                    var elevationDetails = $"Elevation: {elevationData.Results[0].Elevation} meters";
                    // Return combined location and elevation details
                    return $"{locationDetails}\n{elevationDetails}";
                }
                else
                {
                    // Handle case where no elevation results are available
                    return $"No elevation data available for {location}";
                }
            }
            else
            {
                // Handle error cases for Elevation API
                return $"Error retrieving elevation data for {location}. Status code: {elevationResponse.StatusCode}";
            }
        }
        else
        {
            // Handle error cases for Geocoding API
            return $"Error retrieving location details for {location}. Status code: {geocodeResponse.StatusCode}";
        }
    }
});

app.Run();

// Define classes to represent the structure of the Google Maps Geocoding API response
public class GeocodeData
{
    public Result[] Results { get; set; }
}

public class Result
{
    public string? FormattedAddress { get; set; }
    public Geometry? Geometry { get; set; }
}

public class Geometry
{
    public Location? Location { get; set; }
}

public class Location
{
    public float Lat { get; set; }
    public float Lng { get; set; }
}

// Define classes to represent the structure of the Google Maps Elevation API response
public class ElevationData
{
    public ElevationResult[]? Results { get; set; }
}

public class ElevationResult
{
    public double Elevation { get; set; }
}
