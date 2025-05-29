# 🌦️ Level 1: Real-Time Weather Forecast API with .NET 9

This minimal API project fetches **real-time weather forecasts** for any city using public APIs. Built with `.NET 9`, it's a professional-grade example of clean minimal API architecture with real external integration.

---

## 🧠 What It Does

- 🌐 Accepts a city name via `GET /weatherforecast`
- 📍 Looks up latitude/longitude using Open-Meteo’s geocoding API
- ☁️ Fetches a 7-day forecast (max temp, precipitation)
- 🔁 Returns a friendly summary for each day

---

## ⚙️ Tech Stack

| Tech                | Use                                          |
|---------------------|-----------------------------------------------|
| .NET 9              | Minimal API setup                             |
| HttpClientFactory   | External API calls                            |
| Open-Meteo APIs     | Weather + geocoding                           |
| Swagger / OpenAPI   | Developer UI and docs                         |
| xUnit + FluentAssertions | Integration tests                        |

---

## 🚀 How to Run

1. Clone the repo
2. Navigate into the API folder

```bash
dotnet run --project BasicWebApi
