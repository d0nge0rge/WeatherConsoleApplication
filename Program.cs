using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main()
    {
        string apiUrl = "http://www.bom.gov.au/fwo/IDS60901/IDS60901.94672.json";

        try
        {
            string weatherData = await GetWeatherData(apiUrl);
            double averageTemperature = CalcAvgTemp(weatherData);

            Console.WriteLine($"Average Temperature for the Previous 72 Hours: {averageTemperature} °C");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task<string> GetWeatherData(string apiUrl)
    {
        using (HttpClient client = new HttpClient())
        {

            client.DefaultRequestHeaders.Accept.Clear();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Failed to fetch data. Status code: {response.StatusCode}");
            }
        }
    }

    static double CalcAvgTemp(string weatherData)
    {
        try
        {
            JObject jsonData = JObject.Parse(weatherData);
            JArray temperatureObservations = jsonData["observations"]["data"] as JArray;

            if (temperatureObservations == null)
            {
                throw new Exception("Unable to find temperature observations in the data.");
            }

            double temperatureSum = 0;
            int count = 0;

            foreach (JToken observation in temperatureObservations)
            { 
                string dateString = observation.Value<String>("aifstime_utc");
                string format = "yyyyMMddHHmmss";
                DateTime WeatherdateTime = DateTime.ParseExact(dateString, format, null);
                double temperature = observation.Value<double>("air_temp");

                if (DateTime.UtcNow - WeatherdateTime <= TimeSpan.FromHours(72))
                {
                    temperatureSum += temperature;
                    count++;
                }
            }

            return count > 0 ? temperatureSum / count : 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while parsing weather data: {ex.Message}");
        }
    }
}
