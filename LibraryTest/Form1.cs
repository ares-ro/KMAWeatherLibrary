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
            //get data
            string apiKey = File.ReadAllText("../../../../SecretFolder/data.txt");
            WeatherParameter parameter = new WeatherParameter(apiKey, 37.55476, 126.97075, DateTime.Now, DateTimeMode.Floor);

            WeatherData weatherData = new WeatherData();
            try
            {
                //weatherData = await GetWeather.NowAsync(parameter);
                //weatherData = await GetWeather.UltraShortPredictAsync(parameter);
                weatherData = await GetWeather.ShortPredictAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(text: ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //show data
            label1.Text = $"정보: {weatherData.baseDateTime} 위치: {weatherData.latitude}, {weatherData.longitude}";

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            dataGridView1.Columns.Add("시간", "시간");
            List<string> categories = weatherData.items.SelectMany(x => x.values.Keys).Distinct().ToList();
            foreach (string category in categories)
            {
                dataGridView1.Columns.Add(category, category);
            }

            foreach (WeatherDataItem wdc in weatherData.items)
            {
                List<string> row = new List<string>();
                row.Add(wdc.fcstDateTime.ToString());
                for (int i = 1; i < dataGridView1.Columns.Count; i++)
                {
                    string category = dataGridView1.Columns[i].Name;

                    if (wdc.values.TryGetValue(category, out string value))
                    {
                        row.Add(value);
                    }
                    else
                    {
                        row.Add("");
                    }
                }

                dataGridView1.Rows.Add(row.ToArray());
            }

            ////save data
            //string json = WeatherDataConvert.ToJson(weatherData);
            //File.WriteAllText("weather.json", json);
        }
    }
}