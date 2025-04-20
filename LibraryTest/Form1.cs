using System.Xml;
using KMAWeatherLibrary;

namespace LibraryTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string key = File.ReadAllText("");
            KMAWeatherLibrary.Parameter parameter = new KMAWeatherLibrary.Parameter(key, 37.55476, 126.97075, DateTime.Now - TimeSpan.FromMinutes(0));
            WeatherResult wrNow = new WeatherResult();
            WeatherResult wrUSPredict = new WeatherResult();
            WeatherResult wrSPredict = new WeatherResult();

            wrNow = await GetWeather.NowAsync(parameter);
            wrUSPredict = await GetWeather.UltraShortPredictAsync(parameter);
            wrSPredict = await GetWeather.ShortPredictAsync(parameter);

            if (wrNow.state == WeatherResultState.NORMAL_SERVICE & wrUSPredict.state == WeatherResultState.NORMAL_SERVICE & wrSPredict.state == WeatherResultState.NORMAL_SERVICE)
            {

            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {

        }
    }
}

namespace KMAWeatherLibrary
{
    public class GetWeather
    {
        static HttpClient client = new();

        public static async Task<WeatherResult> NowAsync(Parameter parameter)
        {
            string responseBody;
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtNcst";
            string parameterString = "";

            string base_date = parameter.dateTime.Year.ToString() + parameter.dateTime.Month.ToString("D2") + parameter.dateTime.Day.ToString("D2");
            string base_time = parameter.dateTime.Hour.ToString("D2") + parameter.dateTime.Minute.ToString("D2");
            var (nx, ny) = LambertCCProjection.LatLonToGrid(parameter.latitude, parameter.longitude);

            parameterString += "serviceKey=" + parameter.serviceKey + "&";
            parameterString += "numOfRows=" + "1000" + "&";
            parameterString += "pageNo=" + "1" + "&";
            parameterString += "base_date=" + base_date + "&";
            parameterString += "base_time=" + base_time + "&";
            parameterString += "nx=" + nx + "&";
            parameterString += "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            WeatherResult wr = new WeatherResult();

            try
            {
                HttpResponseMessage response = await client.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                //
                throw;
            }

            wr.state = GetState(responseBody);
            wr.weatherDataList = CallApi(responseBody, "base", "obsr");

            return wr;
        }
        public static async Task<WeatherResult> UltraShortPredictAsync(Parameter parameter)
        {
            string responseBody;
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtFcst";
            string parameterString = "";

            string base_date = parameter.dateTime.Year.ToString() + parameter.dateTime.Month.ToString("D2") + parameter.dateTime.Day.ToString("D2");
            string base_time = parameter.dateTime.Hour.ToString("D2") + parameter.dateTime.Minute.ToString("D2");
            var (nx, ny) = LambertCCProjection.LatLonToGrid(parameter.latitude, parameter.longitude);

            parameterString += "serviceKey=" + parameter.serviceKey + "&";
            parameterString += "numOfRows=" + "1000" + "&";
            parameterString += "pageNo=" + "1" + "&";
            parameterString += "base_date=" + base_date + "&";
            parameterString += "base_time=" + base_time + "&";
            parameterString += "nx=" + nx + "&";
            parameterString += "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            WeatherResult wr = new WeatherResult();

            try
            {
                HttpResponseMessage response = await client.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                //
                throw;
            }

            wr.state = GetState(responseBody);
            wr.weatherDataList = CallApi(responseBody, "fcst", "fcst");

            return wr;
        }
        public static async Task<WeatherResult> ShortPredictAsync(Parameter parameter)
        {
            string responseBody;
            string apiUrl = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst";
            string parameterString = "";

            string base_date = parameter.dateTime.Year.ToString() + parameter.dateTime.Month.ToString("D2") + parameter.dateTime.Day.ToString("D2");
            string base_time = parameter.dateTime.Hour.ToString("D2") + parameter.dateTime.Minute.ToString("D2");
            var (nx, ny) = LambertCCProjection.LatLonToGrid(parameter.latitude, parameter.longitude);

            parameterString += "serviceKey=" + parameter.serviceKey + "&";
            parameterString += "numOfRows=" + "1000" + "&";
            parameterString += "pageNo=" + "1" + "&";
            parameterString += "base_date=" + base_date + "&";
            parameterString += "base_time=" + base_time + "&";
            parameterString += "nx=" + nx + "&";
            parameterString += "ny=" + ny;

            string fullUrl = $"{apiUrl}?{parameterString}";

            WeatherResult wr = new WeatherResult();

            try
            {
                HttpResponseMessage response = await client.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                //
                throw;
            }

            wr.state = GetState(responseBody);
            wr.weatherDataList = CallApi(responseBody, "fcst", "fsct");

            return wr;
        }

        static List<Dictionary<string, string>> CallApi(string xml, string dateString, string valueString)
        {
            List<Dictionary<string, string>> buffer = new();

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.DocumentElement;
            XmlNodeList xnl = xmlNode.SelectNodes("//body/items/item");
            foreach (XmlNode xn in xnl)
            {
                string date = xn.SelectSingleNode(dateString + "Date").InnerText;
                string time = xn.SelectSingleNode(dateString + "Time").InnerText;
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
                DataAdd(dataType, xn, bufferSelect, valueString + "Value");
            }

            return buffer;
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

        static WeatherResultState GetState(string xml)
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.DocumentElement;
            string state = xmlNode.SelectSingleNode("//header/resultCode").InnerText;
            return (WeatherResultState)int.Parse(state);

        }
    }
    public class Parameter
    {
        public string serviceKey;
        public double latitude; //위도
        public double longitude; //경도
        public DateTime dateTime;

        public Parameter(string serviceKey, double latitude, double longitude, DateTime dateTime)
        {
            this.serviceKey = serviceKey;
            this.latitude = latitude;
            this.longitude = longitude;
            this.dateTime = dateTime;
        }
    }

    public class WeatherResult
    {
        public WeatherResultState state;
        public List<Dictionary<string, string>> weatherDataList = new();
    }
    public static class LambertCCProjection
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
}

/*
예보 기반 과거 데이터
예보 기반 현재 데이터
예보 기반 미래 데이터




기온
1시간 강수량
하늘상태
동서바람성분
남북바람성분
습도
강수형태
낙뢰
풍향
풍속

강수확률
강수형태
1시간 강수량
습도
1시간 신적설
하늘상태
1시간 기온
일 최저기온
일 최고기온
풍속 동서성분
풍속 남북성분
파고
풍향
풍속



simple
기온
습도
1시간 강수량
하늘상태
풍향
풍속



Management 를 만듦
해당 객체에 파라미터 설정하고
메소드 실행해서 객체 내부 프로퍼티에 정보 저장
이후 다른 메소드로 데이터 꺼내 쓰기


 */