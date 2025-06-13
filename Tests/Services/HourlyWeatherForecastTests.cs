using DotnetLearningShowcase.Models;
using FluentAssertions;
using Xunit;

namespace DotnetLearningShowcase.Tests.Services;

public class HourlyWeatherForecastTests
{
    [Fact]
    public void TemperatureF_ConvertsFromCelsiusCorrectly()
    {
        // Arrange & Act
        var forecast = new HourlyWeatherForecast(
            DateTime.Now,
            20.0, // 20°C should be 68°F
            0.0,
            0
        );

        // Assert
        forecast.TemperatureF.Should().Be(68.0);
    }

    [Fact]
    public void TemperatureF_HandlesNegativeTemperatures()
    {
        // Arrange & Act
        var forecast = new HourlyWeatherForecast(
            DateTime.Now,
            -10.0, // -10°C should be 14°F
            0.0,
            0
        );

        // Assert
        forecast.TemperatureF.Should().Be(14.0);
    }

    [Theory]
    [InlineData(0, "Clear sky")]
    [InlineData(1, "Mainly clear")]
    [InlineData(2, "Partly cloudy")]
    [InlineData(3, "Overcast")]
    [InlineData(45, "Foggy")]
    [InlineData(48, "Foggy")]
    [InlineData(51, "Drizzle")]
    [InlineData(53, "Drizzle")]
    [InlineData(55, "Drizzle")]
    [InlineData(61, "Rain")]
    [InlineData(63, "Rain")]
    [InlineData(65, "Rain")]
    [InlineData(71, "Snow")]
    [InlineData(73, "Snow")]
    [InlineData(75, "Snow")]
    [InlineData(80, "Rain showers")]
    [InlineData(81, "Rain showers")]
    [InlineData(82, "Rain showers")]
    [InlineData(95, "Thunderstorm")]
    [InlineData(999, "Unknown")]
    public void WeatherDescription_MapsWeatherCodesCorrectly(int weatherCode, string expectedDescription)
    {
        // Arrange & Act
        var forecast = new HourlyWeatherForecast(
            DateTime.Now,
            20.0,
            0.0,
            weatherCode
        );

        // Assert
        forecast.WeatherDescription.Should().Be(expectedDescription);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var time = DateTime.Now;
        var temperature = 22.5;
        var precipitation = 1.2;
        var weatherCode = 61;
        var windSpeed = 15.0;
        var humidity = 75.0;
        var apparentTemperature = 20.8;

        // Act
        var forecast = new HourlyWeatherForecast(
            time,
            temperature,
            precipitation,
            weatherCode,
            windSpeed,
            humidity,
            apparentTemperature
        );

        // Assert
        forecast.Time.Should().Be(time);
        forecast.TemperatureC.Should().Be(temperature);
        forecast.Precipitation.Should().Be(precipitation);
        forecast.WeatherCode.Should().Be(weatherCode);
        forecast.WindSpeed.Should().Be(windSpeed);
        forecast.Humidity.Should().Be(humidity);
        forecast.ApparentTemperature.Should().Be(apparentTemperature);
        forecast.WeatherDescription.Should().Be("Rain");
    }

    [Fact]
    public void Constructor_WithOptionalParameters_SetsNullablePropertiesCorrectly()
    {
        // Arrange & Act
        var forecast = new HourlyWeatherForecast(
            DateTime.Now,
            20.0,
            0.0,
            1
        );

        // Assert
        forecast.WindSpeed.Should().BeNull();
        forecast.Humidity.Should().BeNull();
        forecast.ApparentTemperature.Should().BeNull();
    }

    [Fact]
    public void HourlyWeatherResponse_InitializesCorrectly()
    {
        // Arrange
        var location = "Berlin, Germany";
        var forecasts = new[]
        {
            new HourlyWeatherForecast(DateTime.Now, 20.0, 0.0, 0),
            new HourlyWeatherForecast(DateTime.Now.AddHours(1), 19.5, 0.1, 1)
        };

        // Act
        var response = new HourlyWeatherResponse(location, forecasts);

        // Assert
        response.Location.Should().Be(location);
        response.HourlyForecasts.Should().HaveCount(2);
        response.HourlyForecasts.Should().BeEquivalentTo(forecasts);
    }
} 