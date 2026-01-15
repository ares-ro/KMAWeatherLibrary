using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;

namespace KMAWeatherLibrary
{
    public class GetWeather
    {
        static HttpClient client = new HttpClient();

        public static async Task<WeatherData> NowAsync(WeatherParameter parameter)
        {
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtNcst";
            var (nx, ny) = LambertCCProjection.LatLonToGrid(parameter.latitude, parameter.longitude);

            //date convert
            DateTime dateTimeConvert = new();
            if (parameter.dateTimeMode == DateTimeMode.Raw)
            {
                dateTimeConvert = parameter.dateTime;
            }
            else if (parameter.dateTimeMode == DateTimeMode.Floor)
            {
                dateTimeConvert = DateFloor(parameter.dateTime, new TimeSpan(0, 0, 0), 60, 10);
            }
            else if (parameter.dateTimeMode == DateTimeMode.FloorBefore)
            {
                dateTimeConvert = DateFloor(parameter.dateTime, new TimeSpan(0, 0, 0), 60, 10 + 60);
            }

            //set url
            string parameterString = "serviceKey=" + parameter.serviceKey + "&" +
                                     "pageNo=" + "1" + "&" +
                                     "numOfRows=" + "1000" + "&" +
                                     "dataType=" + "JSON" + "&" +
                                     "base_date=" + dateTimeConvert.ToString("yyyyMMdd") + "&" +
                                     "base_time=" + dateTimeConvert.ToString("HHmm") + "&" +
                                     "nx=" + nx + "&" +
                                     "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            //api call
            HttpResponseMessage response = await client.GetAsync(fullUrl);
            if (response.IsSuccessStatusCode == false)
            {
                throw new HttpRequestException($"HTTP ERROR: {(int)response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            ApiResultState wrState = GetApiResultState(responseBody);
            if (wrState != ApiResultState.NORMAL_SERVICE)
            {
                throw new KMAWeatherApiException($"API ERROR: {wrState}");
            }

            //data add
            WeatherData weatherData = new WeatherData();
            weatherData.latitude = parameter.latitude;
            weatherData.longitude = parameter.longitude;
            weatherData.baseDateTime = GetBaseTime(responseBody);

            JsonNode root = JsonNode.Parse(responseBody);
            JsonArray items = root["response"]["body"]["items"]["item"].AsArray();

            foreach (JsonNode item in items)
            {
                string date = item["baseDate"].GetValue<string>();
                string time = item["baseTime"].GetValue<string>();
                DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);

                var target = weatherData.items.FirstOrDefault(x => x.fcstDateTime == dt);
                if (target == null)
                {
                    string category = item["category"].GetValue<string>();
                    string value = item["obsrValue"].GetValue<string>();
                    weatherData.items.Add(new WeatherDataItem() { fcstDateTime = dt, values = new Dictionary<string, string> { { category, value } } });
                }
                else
                {
                    string category = item["category"].GetValue<string>();
                    string value = item["obsrValue"].GetValue<string>();
                    target.values.Add(category, value);
                }
            }

            return weatherData;
        }
        public static async Task<WeatherData> UltraShortPredictAsync(WeatherParameter parameter)
        {
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtFcst";
            var (nx, ny) = LambertCCProjection.LatLonToGrid(parameter.latitude, parameter.longitude);

            //date convert
            DateTime dateTimeConvert = new();
            if (parameter.dateTimeMode == DateTimeMode.Raw)
            {
                dateTimeConvert = parameter.dateTime;
            }
            else if (parameter.dateTimeMode == DateTimeMode.Floor)
            {
                dateTimeConvert = DateFloor(parameter.dateTime, new TimeSpan(0, 30, 0), 60, 15);
            }
            else if (parameter.dateTimeMode == DateTimeMode.FloorBefore)
            {
                dateTimeConvert = DateFloor(parameter.dateTime, new TimeSpan(0, 30, 0), 60, 15 + 60);
            }

            //set url
            string parameterString = "serviceKey=" + parameter.serviceKey + "&" +
                                     "pageNo=" + "1" + "&" +
                                     "numOfRows=" + "1000" + "&" +
                                     "dataType=" + "JSON" + "&" +
                                     "base_date=" + dateTimeConvert.ToString("yyyyMMdd") + "&" +
                                     "base_time=" + dateTimeConvert.ToString("HHmm") + "&" +
                                     "nx=" + nx + "&" +
                                     "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            //api call
            HttpResponseMessage response = await client.GetAsync(fullUrl);
            if (response.IsSuccessStatusCode == false)
            {
                throw new HttpRequestException($"HTTP ERROR: {(int)response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            ApiResultState wrState = GetApiResultState(responseBody);
            if (wrState != ApiResultState.NORMAL_SERVICE)
            {
                throw new KMAWeatherApiException($"API ERROR: {wrState}");
            }

            //data add
            WeatherData weatherData = new WeatherData();
            weatherData.latitude = parameter.latitude;
            weatherData.longitude = parameter.longitude;
            weatherData.baseDateTime = GetBaseTime(responseBody);

            JsonNode root = JsonNode.Parse(responseBody);
            JsonArray items = root["response"]["body"]["items"]["item"].AsArray();

            foreach (JsonNode item in items)
            {
                string date = item["fcstDate"].GetValue<string>();
                string time = item["fcstTime"].GetValue<string>();
                DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);

                var target = weatherData.items.FirstOrDefault(x => x.fcstDateTime == dt);
                if (target == null)
                {
                    string category = item["category"].GetValue<string>();
                    string value = item["fcstValue"].GetValue<string>();
                    weatherData.items.Add(new WeatherDataItem() { fcstDateTime = dt, values = new Dictionary<string, string> { { category, value } } });
                }
                else
                {
                    string category = item["category"].GetValue<string>();
                    string value = item["fcstValue"].GetValue<string>();
                    target.values.Add(category, value);
                }
            }

            return weatherData;
        }
        public static async Task<WeatherData> ShortPredictAsync(WeatherParameter parameter)
        {
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst";
            var (nx, ny) = LambertCCProjection.LatLonToGrid(parameter.latitude, parameter.longitude);

            //date convert
            DateTime dateTimeConvert = new();
            if (parameter.dateTimeMode == DateTimeMode.Raw)
            {
                dateTimeConvert = parameter.dateTime;
            }
            else if (parameter.dateTimeMode == DateTimeMode.Floor)
            {
                dateTimeConvert = DateFloor(parameter.dateTime, new TimeSpan(2, 0, 0), 180, 10);
            }
            else if (parameter.dateTimeMode == DateTimeMode.FloorBefore)
            {
                dateTimeConvert = DateFloor(parameter.dateTime, new TimeSpan(2, 0, 0), 180, 10 + 180);
            }

            //set url
            string parameterString = "serviceKey=" + parameter.serviceKey + "&" +
                                     "pageNo=" + "1" + "&" +
                                     "numOfRows=" + "1000" + "&" +
                                     "dataType=" + "JSON" + "&" +
                                     "base_date=" + dateTimeConvert.ToString("yyyyMMdd") + "&" +
                                     "base_time=" + dateTimeConvert.ToString("HHmm") + "&" +
                                     "nx=" + nx + "&" +
                                     "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            //api call
            HttpResponseMessage response = await client.GetAsync(fullUrl);
            if (response.IsSuccessStatusCode == false)
            {
                throw new HttpRequestException($"HTTP ERROR: {(int)response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            ApiResultState wrState = GetApiResultState(responseBody);
            if (wrState != ApiResultState.NORMAL_SERVICE)
            {
                throw new KMAWeatherApiException($"API ERROR: {wrState}");
            }

            //data add
            WeatherData weatherData = new WeatherData();
            weatherData.latitude = parameter.latitude;
            weatherData.longitude = parameter.longitude;
            weatherData.baseDateTime = GetBaseTime(responseBody);

            JsonNode root = JsonNode.Parse(responseBody);
            JsonArray items = root["response"]["body"]["items"]["item"].AsArray();

            foreach (JsonNode item in items)
            {
                string date = item["fcstDate"].GetValue<string>();
                string time = item["fcstTime"].GetValue<string>();
                DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);

                string category = item["category"].GetValue<string>();
                string value = item["fcstValue"].GetValue<string>();
                
                var target = weatherData.items.FirstOrDefault(x => x.fcstDateTime == dt);
                if (target == null)
                {
                    weatherData.items.Add(new WeatherDataItem() { fcstDateTime = dt, values = new Dictionary<string, string> { { category, value } } });
                }
                else
                {
                    target.values.Add(category, value);
                }
            }

            return weatherData;
        }

        static (string, string) WeatherValueConvertDefault(string category, string value)
        {
            if (int.TryParse(value, out int result) && (result <= -900 || result >= 900))
            {
                return(category, "");
            }
            return (category, value);
        }
        static (string, string) WeatherValueConvertEnglish(string category, string value)
        {
            //초단기 실황, 초단기 예보
            if (category == "T1H") { return ("temperature", value); }
            if (category == "RN1") { return ("rainPerHour", value); }
            if (category == "SKY")
            {
                string valueBuffer = value;
                if (value == "1") { valueBuffer = "clear"; }
                if (value == "3") { valueBuffer = "mostlyCloudy"; }
                if (value == "4") { valueBuffer = "overcast"; }
                return ("skyType", valueBuffer);
            }
            if (category == "UUU") { return ("windComponentU", value); }
            if (category == "VVV") { return ("windComponentV", value); }
            if (category == "REH") { return ("humidity", value); }
            if (category == "PTY")
            {
                string valueBuffer = value;
                if (value == "0") { valueBuffer = "none"; }
                if (value == "1") { valueBuffer = "rain"; }
                if (value == "2") { valueBuffer = "rainSnow"; }
                if (value == "3") { valueBuffer = "snow"; }
                if (value == "4") { valueBuffer = "shower"; }
                if (value == "5") { valueBuffer = "drizzle"; }
                if (value == "6") { valueBuffer = "drizzleSnowFlurry"; }
                if (value == "7") { valueBuffer = "snowFlurry"; }
                return ("precipitationType", valueBuffer);
            }
            if (category == "LGT") { return ("lightning", value); }
            if (category == "VEC") { return ("windDirection", value); }
            if (category == "WSD") { return ("windSpeed", value); }

            //단기 예보
            if (category == "POP") { return ("precipitationProbability", value); }
            if (category == "PCP") { return ("rainPerHour", value); } //
            if (category == "SNO") { return ("snowPerHour", value); }
            if (category == "TMP") { return ("temperature", value); } //
            if (category == "TMN") { return ("dailyMinTemperature", value); }
            if (category == "TMX") { return ("dailyMaxTemperature", value); }
            if (category == "WAV") { return ("waveHeight", value); }

            return (category,  value);
        }
        static DateTime GetBaseTime(string json)
        {
            JsonNode root = JsonNode.Parse(json);
            JsonArray items = root["response"]["body"]["items"]["item"].AsArray();

            string date = items[0]["baseDate"].GetValue<string>();
            string time = items[0]["baseTime"].GetValue<string>();

            DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);
            return dt;
        }
        static ApiResultState GetApiResultState(string json)
        {
            //게이트웨이 자체에서 결과를 돌려주는 경우가 있음 (서버 쪽 문제)
            //원래 xml 구조가 아닌 특정 xml 형태로 결과코드 반환
            //json 요청이더라도 xml로 반환
            if (json.Contains("<OpenAPI_ServiceResponse>"))
            {
                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(json);
                XmlNode resultCode = xmlDoc.SelectSingleNode("//OpenAPI_ServiceResponse/cmmMsgHeader/returnReasonCode");

                return (ApiResultState)int.Parse(resultCode.InnerText);
            }
            //api 결과코드
            else
            {
                JsonNode root = JsonNode.Parse(json);
                string resultCode = root["response"]["header"]["resultCode"].GetValue<string>();
                return (ApiResultState)int.Parse(resultCode);
            }
        }
        static DateTime DateFloor(DateTime inputTime, TimeSpan baseTime, int stepMin, int delayMin)
        {
            inputTime = inputTime.AddMinutes(-delayMin);
            DateTime baseDt = new DateTime(inputTime.Year, inputTime.Month, inputTime.Day, baseTime.Hours, baseTime.Minutes, baseTime.Seconds);

            int stepCount = (int)Math.Floor((inputTime - baseDt).TotalMinutes / stepMin);

            return baseDt.AddMinutes(stepCount * stepMin);
        }
    }

    public class WeatherParameter(string serviceKey, double latitude, double longitude, DateTime dateTime, DateTimeMode dateTimeMode)
    {
        public string serviceKey = serviceKey;
        public double latitude = latitude; //위도
        public double longitude = longitude; //경도
        public DateTimeMode dateTimeMode = dateTimeMode;
        public DateTime dateTime = dateTime;
    }
    public class WeatherData
    {
        public DateTime baseDateTime { get; set; }
        public double latitude { get; set; } //위도
        public double longitude { get; set; } //경도
        public List<WeatherDataItem> items { get; set; } = new();
    }
    public class WeatherDataItem
    {
        public DateTime fcstDateTime { get; set; }
        public Dictionary<string, string> values { get; set; } = new();
    }

    public static class WeatherDataConvert
    {
        public static string ToJson(WeatherData weatherData)
        {
            return JsonSerializer.Serialize(weatherData, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }
        public static string ToJsonEscaped(WeatherData weatherData)
        {
            return JsonSerializer.Serialize(weatherData, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true });
        }
    }
    class LambertCCProjection
    {
        private const double PI = Math.PI;
        private const double DEG_TO_RAD = PI / 180.0;
        private const double RE = 6371.00877;
        private const double GRID = 5.0;
        private const double SLAT1 = 30.0 * DEG_TO_RAD;
        private const double SLAT2 = 60.0 * DEG_TO_RAD;
        private const double OLON = 126.0 * DEG_TO_RAD;
        private const double OLAT = 38.0 * DEG_TO_RAD;
        private const double XO = 210 / GRID;
        private const double YO = 675 / GRID;

        private static readonly double Sn;
        private static readonly double Sf;
        private static readonly double Ro;

        static LambertCCProjection()
        {
            double re = RE / GRID;

            Sn = Math.Log(Math.Cos(SLAT1) / Math.Cos(SLAT2)) /
                 Math.Log(Math.Tan(PI * 0.25 + SLAT2 * 0.5) / Math.Tan(PI * 0.25 + SLAT1 * 0.5));
            Sf = Math.Pow(Math.Tan(PI * 0.25 + SLAT1 * 0.5), Sn) * Math.Cos(SLAT1) / Sn;
            Ro = re * Sf / Math.Pow(Math.Tan(PI * 0.25 + OLAT * 0.5), Sn);
        }

        public static (double X, double Y) LatLonToGrid(double lat, double lon)
        {
            double ra = (RE / GRID) * Sf / Math.Pow(Math.Tan((PI * 0.25) + (lat * DEG_TO_RAD * 0.5)), Sn);
            double theta = lon * DEG_TO_RAD - OLON;
            theta = (theta > PI) ? theta - 2.0 * PI : (theta < -PI ? theta + 2.0 * PI : theta);
            theta *= Sn;

            double x = ra * Math.Sin(theta) + XO + 1;
            double y = Ro - ra * Math.Cos(theta) + YO + 1;

            return (Math.Round(x), Math.Round(y));
        }
    }
    class KMAWeatherApiException : Exception
    {
        public KMAWeatherApiException(string message) : base(message) { }
    }

    public enum ApiResultState
    {
        NORMAL_SERVICE = 0,
        APPLICATION_ERROR = 1,
        DB_ERROR = 2,
        NODATA_ERROR = 3,
        HTTP_ERROR = 4,
        SERVICETIME_OUT = 5,
        INVALID_REQUEST_PARAMETER_ERROR = 10,
        NO_MANDATORY_REQUEST_PARAMETERS_ERROR = 11,
        NO_OPENAPI_SERVICE_ERROR = 12,
        SERVICE_ACCESS_DENIED_ERROR = 20,
        TEMPORARILY_DISABLE_THE_SERVICEKEY_ERROR = 21,
        LIMITED_NUMBER_OF_SERVICE_REQUESTS_EXCEEDS_ERROR = 22,
        SERVICE_KEY_IS_NOT_REGISTERED_ERROR = 30,
        DEADLINE_HAS_EXPIRED_ERROR = 31,
        UNREGISTERED_IP_ERROR = 32,
        UNSIGNED_CALL_ERROR = 33,
        UNKNOWN_ERROR = 99
    }
    public enum DateTimeMode
    {
        Raw,
        Floor,
        FloorBefore
    }
}
