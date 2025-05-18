using System.Diagnostics;
using System.Windows.Forms;
using KMAWeatherLibrary;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            string apiKey = File.ReadAllText("../../../../SecretFolder/data.txt");
            WeatherParameter parameter = new WeatherParameter(apiKey, 37.55476, 126.97075, DateTime.Now, DateTimeMode.Floor);

            WeatherResult wr = new WeatherResult();
            try
            {
                //wr = await GetWeather.NowAsync(parameter);
                //wr = await GetWeather.UltraShortPredictAsync(parameter);
                wr = await GetWeather.ShortPredictAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            label1.Text = $"발표시점: {wr.BaseTime}";

            List<string> keyList = wr.Result.SelectMany(d => d.Keys).Distinct().ToList();

            dataGridView1.Columns.Clear();
            foreach (string key in keyList)
            {
                dataGridView1.Columns.Add(key, key);
            }

            dataGridView1.Rows.Clear();
            foreach (Dictionary<string, string> dict in wr.Result)
            {
                List<string> row = new();
                for (int i = 0; i < keyList.Count; i++)
                {
                    if (dict.TryGetValue(keyList[i], out string value))
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
        }
    }
}