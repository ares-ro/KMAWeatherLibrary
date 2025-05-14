using System.Diagnostics;
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
            //string key = "api key" //api key
            string key = File.ReadAllText("../../../../SecretFolder/data.txt");
            WeatherParameter parameter = new WeatherParameter(key, 37.55476, 126.97075, DateTime.Now, DateTimeMode.Floor);

            //WeatherResult wrNow;
            //WeatherResult wrUSPredict;
            WeatherResult wrSPredict = new WeatherResult();
            try
            {
                //wrNow = await GetWeather.NowAsync(parameter);
                //wrUSPredict = await GetWeather.UltraShortPredictAsync(parameter);
                wrSPredict = await GetWeather.ShortPredictAsync(parameter);

                Debug.WriteLine(wrSPredict.BaseTime);
                foreach (Dictionary<string, string> a in wrSPredict.Result)
                {
                    foreach (KeyValuePair<string, string> b in a)
                    {
                        Debug.WriteLine($"{b.Key}: {b.Value}");
                    }
                    Debug.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (wrSPredict != null)
            {
                label1.Text = $"발표 시점: {wrSPredict.BaseTime}";

                listView1.Items.Clear();
                listView1.Columns.Clear();

                listView1.View = View.Details;
                listView1.Columns.Add("날짜", 100);
                listView1.Columns.Add("온도", 75);
                listView1.Columns.Add("동서 풍속", 75);
                listView1.Columns.Add("남북 풍속", 75);
                listView1.Columns.Add("풍향", 75);
                listView1.Columns.Add("풍속", 75);
                listView1.Columns.Add("하늘상태", 75);
                listView1.Columns.Add("강수형태", 75);
                listView1.Columns.Add("강수확률", 75);
                listView1.Columns.Add("파고", 75);
                listView1.Columns.Add("1시간 강수량", 75);
                listView1.Columns.Add("습도", 75);
                listView1.Columns.Add("1시간 신적설", 75);

                foreach (Dictionary<string, string> dict in wrSPredict.Result)
                {
                    ListViewItem item = new ListViewItem(dict["dateTime"]);
                    item.SubItems.Add(dict["temperature"]);
                    item.SubItems.Add(dict["windDirectionEW"]);
                    item.SubItems.Add(dict["windDirectionNS"]);
                    item.SubItems.Add(dict["windDirection"]);
                    item.SubItems.Add(dict["windStrength"]);
                    item.SubItems.Add(dict["skyState"]);
                    item.SubItems.Add(dict["rainState"]);
                    item.SubItems.Add(dict["rainPercent"]);
                    item.SubItems.Add(dict["waveHeight"]);
                    item.SubItems.Add(dict["rainPerHour"]);
                    item.SubItems.Add(dict["humidity"]);
                    item.SubItems.Add(dict["snowPerHour"]);
                    listView1.Items.Add(item);
                }
            }
        }
    }
}