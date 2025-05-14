using System.Xml;

namespace KMAWeatherLibrary
{
    public class GetWeather
    {
        public static async Task<WeatherResult> NowAsync(WeatherParameter parameter)
        {
            //api url
            string responseBody;
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtNcst";
            string parameterString = "";

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

            string base_date = dateTimeConvert.Year.ToString() + dateTimeConvert.Month.ToString("D2") + dateTimeConvert.Day.ToString("D2");
            string base_time = dateTimeConvert.Hour.ToString("D2") + dateTimeConvert.Minute.ToString("D2");


            parameterString += "serviceKey=" + parameter.serviceKey + "&";
            parameterString += "numOfRows=" + "1000" + "&";
            parameterString += "pageNo=" + "1" + "&";
            parameterString += "base_date=" + base_date + "&";
            parameterString += "base_time=" + base_time + "&";
            parameterString += "nx=" + nx + "&";
            parameterString += "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            //api call
            try
            {
                HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                if (GetState(responseBody) != WeatherResultState.NORMAL_SERVICE)
                {
                    throw new KMAWeatherApiException($"Api Error: {GetState(responseBody)}");
                }

                //data add
                List<Dictionary<string, string>> buffer = new();

                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(responseBody);
                XmlNode xmlNode = xmlDoc.DocumentElement;
                XmlNodeList xnl = xmlNode.SelectNodes("//body/items/item");
                foreach (XmlNode xn in xnl)
                {
                    string date = xn.SelectSingleNode("baseDate").InnerText;
                    string time = xn.SelectSingleNode("baseTime").InnerText;
                    DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);

                    Dictionary<string, string> bufferSelect = null;

                    foreach (Dictionary<string, string> a in buffer)
                    {
                        if (a["dateTime"] == dt.ToString("yyyyMMddHHmm"))
                        {
                            bufferSelect = a;
                            break;
                        }
                    }
                    if (bufferSelect == null)
                    {
                        bufferSelect = new Dictionary<string, string> { { "dateTime", dt.ToString("yyyyMMddHHmm") } };
                        buffer.Add(bufferSelect);
                    }

                    string dataType = xn.SelectSingleNode("category").InnerText;
                    DataAdd(dataType, xn, bufferSelect, "obsrValue");
                }

                WeatherResult wr = new WeatherResult();
                wr._baseTime = GetBaseTime(responseBody);
                wr._result = buffer;
                return wr;
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
        public static async Task<WeatherResult> UltraShortPredictAsync(WeatherParameter parameter)
        {
            string responseBody;
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtFcst";
            string parameterString = "";

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

            string base_date = dateTimeConvert.Year.ToString() + dateTimeConvert.Month.ToString("D2") + dateTimeConvert.Day.ToString("D2");
            string base_time = dateTimeConvert.Hour.ToString("D2") + dateTimeConvert.Minute.ToString("D2");

            parameterString += "serviceKey=" + parameter.serviceKey + "&";
            parameterString += "numOfRows=" + "1000" + "&";
            parameterString += "pageNo=" + "1" + "&";
            parameterString += "base_date=" + base_date + "&";
            parameterString += "base_time=" + base_time + "&";
            parameterString += "nx=" + nx + "&";
            parameterString += "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            //api call
            try
            {
                HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                if (GetState(responseBody) != WeatherResultState.NORMAL_SERVICE)
                {
                    throw new KMAWeatherApiException($"Api Error: {GetState(responseBody)}");
                }

                //data add
                List<Dictionary<string, string>> buffer = new();

                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(responseBody);
                XmlNode xmlNode = xmlDoc.DocumentElement;
                XmlNodeList xnl = xmlNode.SelectNodes("//body/items/item");
                foreach (XmlNode xn in xnl)
                {
                    string date = xn.SelectSingleNode("fcstDate").InnerText;
                    string time = xn.SelectSingleNode("fcstTime").InnerText;
                    DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);

                    Dictionary<string, string> bufferSelect = null;

                    foreach (Dictionary<string, string> a in buffer)
                    {
                        if (a["dateTime"] == dt.ToString("yyyyMMddHHmm"))
                        {
                            bufferSelect = a;
                            break;
                        }
                    }
                    if (bufferSelect == null)
                    {
                        bufferSelect = new Dictionary<string, string> { { "dateTime", dt.ToString("yyyyMMddHHmm") } };
                        buffer.Add(bufferSelect);
                    }

                    string dataType = xn.SelectSingleNode("category").InnerText;
                    DataAdd(dataType, xn, bufferSelect, "fcstValue");
                }

                WeatherResult wr = new WeatherResult();
                wr._baseTime = GetBaseTime(responseBody);
                wr._result = buffer;
                return wr;
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
        public static async Task<WeatherResult> ShortPredictAsync(WeatherParameter parameter)
        {
            string responseBody;
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst";
            string parameterString = "";

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

            string base_date = dateTimeConvert.Year.ToString() + dateTimeConvert.Month.ToString("D2") + dateTimeConvert.Day.ToString("D2");
            string base_time = dateTimeConvert.Hour.ToString("D2") + dateTimeConvert.Minute.ToString("D2");

            parameterString += "serviceKey=" + parameter.serviceKey + "&";
            parameterString += "numOfRows=" + "1000" + "&";
            parameterString += "pageNo=" + "1" + "&";
            parameterString += "base_date=" + base_date + "&";
            parameterString += "base_time=" + base_time + "&";
            parameterString += "nx=" + nx + "&";
            parameterString += "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            //api call
            try
            {
                HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                if (GetState(responseBody) != WeatherResultState.NORMAL_SERVICE)
                {
                    throw new KMAWeatherApiException($"Api Error: {GetState(responseBody)}");
                }

                //data add
                List<Dictionary<string, string>> buffer = new();

                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(responseBody);
                XmlNode xmlNode = xmlDoc.DocumentElement;
                XmlNodeList xnl = xmlNode.SelectNodes("//body/items/item");
                foreach (XmlNode xn in xnl)
                {
                    string date = xn.SelectSingleNode("fcstDate").InnerText;
                    string time = xn.SelectSingleNode("fcstTime").InnerText;
                    DateTime dt = DateTime.ParseExact(date + time, "yyyyMMddHHmm", null);

                    Dictionary<string, string> bufferSelect = null;

                    foreach (Dictionary<string, string> a in buffer)
                    {
                        if (a["dateTime"] == dt.ToString("yyyyMMddHHmm"))
                        {
                            bufferSelect = a;
                            break;
                        }
                    }
                    if (bufferSelect == null)
                    {
                        bufferSelect = new Dictionary<string, string> { { "dateTime", dt.ToString("yyyyMMddHHmm") } };
                        buffer.Add(bufferSelect);
                    }

                    string dataType = xn.SelectSingleNode("category").InnerText;
                    DataAdd(dataType, xn, bufferSelect, "fcstValue");
                }

                WeatherResult wr = new WeatherResult();
                wr._baseTime = GetBaseTime(responseBody);
                wr._result = buffer;
                return wr;
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        static void DataAdd(string dataType, XmlNode xn, Dictionary<string, string> bufferSelect, string valueString)
        {
            //초단기 실황 + 초단기 예보
            if (dataType == "T1H")
            {
                bufferSelect.Add("temperature", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "RN1")
            {
                bufferSelect.Add("rainPerHour", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "SKY")
            {
                string buffer = xn.SelectSingleNode(valueString).InnerText;
                string buffer2 = "";
                if (buffer == "1")
                {
                    buffer2 = "맑음";
                }
                else if (buffer == "3")
                {
                    buffer2 = "구름많음";
                }
                else if (buffer == "4")
                {
                    buffer2 = "흐림";
                }
                else
                {
                    buffer2 = buffer;
                }
                bufferSelect.Add("skyState", buffer2);
            }
            else if (dataType == "UUU")
            {
                bufferSelect.Add("windDirectionEW", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "VVV")
            {
                bufferSelect.Add("windDirectionNS", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "REH")
            {
                bufferSelect.Add("humidity", xn.SelectSingleNode(valueString).InnerText);
            }

            else if (dataType == "PTY")
            {
                string buffer = xn.SelectSingleNode(valueString).InnerText;
                string buffer2 = "";
                if (buffer == "0")
                {
                    buffer2 = "없음";
                }
                else if (buffer == "1")
                {
                    buffer2 = "비";
                }
                else if (buffer == "2")
                {
                    buffer2 = "비/눈";
                }
                else if (buffer == "3")
                {
                    buffer2 = "눈";
                }
                else if (buffer == "4")
                {
                    buffer2 = "소나기";
                }
                else if (buffer == "5")
                {
                    buffer2 = "빗방울";
                }
                else if (buffer == "6")
                {
                    buffer2 = "빗방울눈날림";
                }
                else if (buffer == "7")
                {
                    buffer2 = "눈날림";
                }
                else
                {
                    buffer2 = buffer;
                }
                bufferSelect.Add("rainState", buffer2);
            }
            else if (dataType == "LGT")
            {
                bufferSelect.Add("lightning", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "VEC")
            {
                bufferSelect.Add("windDirection", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "WSD")
            {
                bufferSelect.Add("windStrength", xn.SelectSingleNode(valueString).InnerText);
            }

            //+ 단기 예보
            if (dataType == "POP")
            {
                bufferSelect.Add("rainPercent", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "PCP")
            {
                bufferSelect.Add("rainPerHour", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "SNO")
            {
                bufferSelect.Add("snowPerHour", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "TMP")
            {
                bufferSelect.Add("temperature", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "TMN")
            {
                bufferSelect.Add("dayMinTemperature", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "TMX")
            {
                bufferSelect.Add("dayMaxTemperature", xn.SelectSingleNode(valueString).InnerText);
            }
            else if (dataType == "WAV")
            {
                bufferSelect.Add("waveHeight", xn.SelectSingleNode(valueString).InnerText);
            }
        }
        static string GetBaseTime(string xml)
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.DocumentElement;

            string date = xmlNode.SelectSingleNode("//body/items/item/baseDate").InnerText;
            string time = xmlNode.SelectSingleNode("//body/items/item/baseTime").InnerText;
            return date + time;
        }
        static WeatherResultState GetState(string xml)
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.DocumentElement;
            XmlNode state = xmlNode.SelectSingleNode("//header/resultCode");
            if (state == null)
            {
                state = xmlNode.SelectSingleNode("//OpenAPI_ServiceResponse/cmmMsgHeader/returnReasonCode");
            }
            return (WeatherResultState)int.Parse(state.InnerText);
        }
        static DateTime DateFloor(DateTime inputTime, TimeSpan baseTime, int stepMin, int delayMin)
        {
            DateTime inputDt = inputTime - new TimeSpan(0, delayMin, 0);
            DateTime baseDt = new DateTime(inputDt.Year, inputDt.Month, inputDt.Day, baseTime.Hours, baseTime.Minutes, baseTime.Seconds);

            TimeSpan delta = inputDt - baseDt;
            double deltaMin = delta.TotalMinutes;

            int flooredDeltaMin = (int)Math.Floor(deltaMin / stepMin) * stepMin;

            DateTime flooredTime = baseDt.AddMinutes(flooredDeltaMin);
            return flooredTime;
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

    public class WeatherResult
    {
        public string BaseTime => _baseTime;
        public IReadOnlyList<Dictionary<string, string>> Result => _result;

        internal string _baseTime  = "";
        internal List<Dictionary<string, string>> _result = new();
    }
    public class WeatherParameter(string serviceKey, double latitude, double longitude, DateTime dateTime, DateTimeMode dateTimeMode)
    {
        public string serviceKey = serviceKey;
        public double latitude = latitude; //위도
        public double longitude = longitude; //경도
        public DateTimeMode dateTimeMode = dateTimeMode;

        public DateTime dateTime = dateTime;
    }
    class KMAWeatherApiException : Exception
    {
        public KMAWeatherApiException(string message) : base(message) { }
    }

    public enum WeatherResultState
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
