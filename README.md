# KMA Weather Library
![C#](https://img.shields.io/badge/C%23-68217A?style=flat-square)
![image](https://github.com/ares-ro/KMAWeatherLibrary/blob/main/docs/%EC%A0%9C%EB%AA%A9%20%EC%97%86%EC%9D%8C-1.png)
대한민국 기상청 KMA에서 제공하는 단기예보 API를 기반으로 제작된 C# 라이브러리입니다.  
API에 관한 상세정보는 https://www.data.go.kr/data/15084084/openapi.do 에서 확인 가능합니다.

# 사용방법
1. https://www.data.go.kr/index.do 공공데이터포털에 로그인 후 API키를 발급받습니다.
2. https://www.data.go.kr/data/15084084/openapi.do 기상청 단기예보의 활용신청을 진행합니다.
3. dll 파일을 다운로드 후 프로젝트에 참조하여 사용합니다.

# 예제
- 코드 예제
```
//get data
string apiKey = "api key";
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
}
```
- 프로젝트 예제 (Windows Forms)

https://github.com/ares-ro/KMAWeatherLibrary/tree/main/LibraryTest

# 상세
### 파라미터
```
new WeatherParameter(serviceKey, latitude, longitude, dateTime, dateTimeMode);
```
| 파라미터 | 설명 |
| :--- | :--- |
| serviceKey | 공공데이터포털에서 발급받은 API키 |
| latitude | 기상정보 위치의 위도 |
| longitude | 기상정보 위치의 경도 |
| dateTime | 기상정보 발표 시점 |
| dateTimeMode | 기상정보 발표 시점 설정 |

- DateTimeMode  
기상청 API 호출 시 발표 시점 설정이 필요합니다. DateTimeMode를 이용하여 해당 설정을 간편하게 진행할 수 있습니다.

| 설정 | 설명 |
| :--- | :--- |
| DateTimeMode.Raw | 입력된 발표 시점을 그대로 사용합니다. |
| DateTimeMode.Floor | 호출 가능한 가장 가까운 발표 시점으로 설정합니다. 예측 데이터가 예상시간 내에 생성되었을 경우 사용하기 적합합니다. |
| DateTimeMode.FloorBefore | 호출 가능한 가장 가까운 발표 시점에서 바로 전 시점으로 설정합니다. 예측 데이터가 예상시간 내에 생성되지 못했을 경우 이전 데이터를 가져오기 위해 사용하기 적합합니다. |

### API 호출
| 메소드 | 설명 |
| :--- | :--- |
| GetWeather.NowAsync | 초단기실황 |
| GetWeather.UltraShortPredictAsync | 초단기예보 |
| GetWeather.ShortPredictAsync | 단기예보 |

### 결과 데이터
- WeatherData

| 데이터 | 설명 |
| :--- | :--- |
| baseDateTime | 기상 데이터의 발표 시점 |
| latitude | 위도 |
| longitude | 경도 |
| items | 기상 데이터 |

- WeatherData.items  
호출 메소드마다 반환되는 데이터에 차이가 있습니다. 상세정보는 https://www.data.go.kr/data/15084084/openapi.do 에서 확인 가능합니다.

- 단기예보

| 항목값 | 항목명      | 단위      |
| :-- | :------- | :------ |
| POP | 강수확률     | %       |
| PTY | 강수형태     | 코드값     |
| PCP | 1시간강수량   | 범주(1mm) |
| REH | 습도       | %       |
| SNO | 1시간신적설   | 범주(1cm) |
| SKY | 하늘상태     | 코드값     |
| TMP | 1시간기온    | ℃       |
| TMN | 일최저기온    | ℃       |
| TMX | 일최고기온    | ℃       |
| UUU | 풍속(동서성분) | m/s     |
| VVV | 풍속(남북성분) | m/s     |
| WAV | 파고       | M       |
| VEC | 풍향       | deg     |
| WSD | 풍속       | m/s     |

- 초단기실황

| 항목값 | 항목명    | 단위  |
| :-- | :----- | :-- |
| T1H | 기온     | ℃   |
| RN1 | 1시간강수량 | mm  |
| UUU | 동서바람성분 | m/s |
| VVV | 남북바람성분 | m/s |
| REH | 습도     | %   |
| PTY | 강수형태   | 코드값 |
| VEC | 풍향     | deg |
| WSD | 풍속     | m/s |

- 초단기예보

| 항목값 | 항목명    | 단위      |
| :-- | :----- | :------ |
| T1H | 기온     | ℃       |
| RN1 | 1시간강수량 | 범주(1mm) |
| SKY | 하늘상태   | 코드값     |
| UUU | 동서바람성분 | m/s     |
| VVV | 남북바람성분 | m/s     |
| REH | 습도     | %       |
| PTY | 강수형태   | 코드값     |
| LGT | 낙뢰     | kA      |
| VEC | 풍향     | deg     |
| WSD | 풍속     | m/s     |

- 하늘상태

| 하늘상태(SKY) 코드 | 데이터 |
| :---------------- | :----- |
| 1 | 맑음 |
| 3 | 구름많음 |
| 4 | 흐림 |

- 강수상태

| 강수상태(PTY) 코드 | 데이터 |
| :---------------- | :----- |
| 0 | 없음 |
| 1 | 비 |
| 2 | 비/눈 |
| 3 | 눈 |
| 4 | 소나기 |
| 5 | 빗방울 |
| 6 | 빗방울눈날림 |
| 7 | 눈날림 |

### Json 직렬화
- WeatherDataConvert  
결과 데이터를 Json으로 직렬화합니다.

| 메서드 | 설명 |
| :---------  | :--- |
| ToJson | Json으로 직렬화하며 문자 이스케이프는 하지 않습니다. |
| ToJsonEscaped | Json으로 직렬화하며 문자를 이스케이프 합니다. |
