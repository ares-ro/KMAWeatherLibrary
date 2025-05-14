# KMA Weather Library
![C#](https://img.shields.io/badge/C%23-68217A?style=flat-square)

대한민국 기상청 KMA에서 제공하는 단기예보 api의 C# 라이브러리입니다.  
독스 등 상세정보는 https://www.data.go.kr/data/15084084/openapi.do 에서 확인 가능합니다.

# 사용방법
1. https://www.data.go.kr/index.do 공공데이터포털에 로그인 후 api키를 발급받습니다.
2. https://www.data.go.kr/data/15084084/openapi.do 기상청 단기예보의 활용신청을 진행합니다.
3. dll 파일을 다운로드 후 프로젝트에 참조하여 사용합니다.

# 예제
```
string key = "api key";
WeatherParameter parameter = new WeatherParameter(key, 37.55476, 126.97075, DateTime.Now, DateTimeMode.Floor);

try
{
    WeatherResult wrUSPredict = await GetWeather.UltraShortPredictAsync(parameter);

    Debug.WriteLine(wrUSPredict.BaseTime);

    foreach (Dictionary<string, string> a in wrUSPredict.Result)
    {
        foreach (KeyValuePair<string, string> b in a)
        {
            Debug.WriteLine($"{b.Key}: {b.Value}");
        }
    }
}
catch
{

}
```

# 상세
### 파라미터
```
new WeatherParameter(serviceKey, latitude, longitude, dateTime, dateTimeMode);
```
| 파라미터 | 설명 |
| :--- | :--- |
| serviceKey | 공공데이터포털에서 발급받은 api키 |
| latitude | 기상정보 위치의 위도 |
| longitude | 기상정보 위치의 경도 |
| dateTime | 기상정보 발표 시점 |
| dateTimeMode | 기상정보 발표 시점 설정 |

- DateTimeMode  
기상청 api 호출 시 발표 시점 설정이 필요합니다. DateTimeMode를 이용하여 해당 설정을 간편하게 진행할 수 있습니다.

| 설정 | 설명 |
| :--- | :--- |
| DateTimeMode.Raw | 입력된 발표 시점을 그대로 사용합니다. |
| DateTimeMode.Floor | 호출 가능한 가장 가까운 발표 시점으로 설정합니다. 예측 데이터가 예상시간 내에 생성되었을 경우 사용하기 적합합니다. |
| DateTimeMode.FloorBefore | 호출 가능한 가장 가까운 발표 시점에서 바로 전 시점으로 설정합니다. 예측 데이터가 예상시간 내에 생성되지 못했을 경우 이전 데이터를 가져오기 위해 사용하기 적합합니다. |

### API 호출
| api 호출 메소드 | 설명 |
| :--- | :--- |
| GetWeather.NowAsync | 초단기실황 |
| GetWeather.UltraShortPredictAsync | 초단기예보 |
| GetWeather.ShortPredictAsync | 단기예보 |

### 결과 데이터
- WeatherResult

| 데이터 | 설명 |
| :--- | :--- |
| BaseTime | 기상 데이터의 발표 시점 |
| Result | 기상 데이터 |

- WeatherResult.Result  
호출 메소드마다 반환되는 데이터에 차이가 있습니다. 상세정보는 https://www.data.go.kr/data/15084084/openapi.do 에서 확인 가능합니다.

| 데이터 | 설명 | 비고 |
| :--- | :--- | :--- |
| temperature | 기온 | °C |
| rainPerHour | 1시간 강수량 | mm |
| skyState | 하늘상태 | 맑음, 구름많음, 흐림 |
| windDirectionEW | 동서 풍속 | m/s |
| windDirectionNS | 남북 풍속 | m/s |
| humidity | 습도 | % |
| rainState | 강수형태 | 없음, 비, 비/눈, 눈, 소나기, 빗방울, 빗방울눈날림, 눈날림 |
| lightning | 낙뢰 | kA/km^2 |
| windDirection | 풍향 | deg | 
| windStrength | 풍속 | m/s |
| rainPercent | 강수확률 | % | 
| snowPerHour | 1시간 신적설 | cm | 
| dayMinTemperature | 일 최저기온 | °C | 
| dayMaxTemperature | 일 최고기온 | °C | 
| waveHeight | 파고 | m | 
