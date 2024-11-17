using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BoxTrackingApi.Services
{
    public class CameraManager
    {
        private readonly HttpClient _httpClient;
        private string _sid; // Session ID
        private readonly string _ipAddress;
        private readonly string _username;
        private readonly string _password;

        public CameraManager(string ipAddress, string username, string password)
        {
            _ipAddress = ipAddress;
            _username = username;
            _password = password;
            _httpClient = new HttpClient();
        }

        public async Task InitializeAsync()
        {
            await AuthenticateAsync();
        }

        private async Task AuthenticateAsync()
        {
            var api = "/webapi/auth.cgi?api=SYNO.API.Auth";
            var payload = new Dictionary<string, string>
            {
                { "method", "Login" },
                { "version", "6" },
                { "account", _username },
                { "passwd", _password },
                { "session", "SurveillanceStation" },
                { "format", "sid" }
            };

            var query = string.Join("&", payload.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var authUrl = $"http://{_ipAddress}:5000{api}&{query}";

            var response = await _httpClient.GetStringAsync(authUrl);
            var json = JObject.Parse(response);

            if (json["success"]?.Value<bool>() == true)
            {
                _sid = json["data"]["sid"].ToString();
            }
            else
            {
                throw new Exception("Authentication failed.");
            }
        }

        private async Task<JObject> SendRequestAsync(string api, Dictionary<string, string> payload, HttpMethod method)
        {
            var url = $"http://{_ipAddress}:5000{api}";
            HttpResponseMessage response;

            if (method == HttpMethod.Get)
            {
                var query = string.Join("&", payload.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var fullUrl = $"{url}?{query}";
                response = await _httpClient.GetAsync(fullUrl);
            }
            else if (method == HttpMethod.Post)
            {
                var content = new FormUrlEncodedContent(payload);
                response = await _httpClient.PostAsync(url, content);
            }
            else
            {
                throw new ArgumentException("Invalid request type!", nameof(method));
            }

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);

            if (jsonResponse["success"]?.Value<bool>() != true)
            {
                throw new Exception($"API request failed: {jsonResponse}");
            }

            return jsonResponse;
        }

        public async Task<byte[]> GetSnapshotAsync(int cameraId)
        {
            var api = "/webapi/entry.cgi?api=SYNO.SurveillanceStation.Camera";
            var payload = new Dictionary<string, string>
            {
                { "method", "GetSnapshot" },
                { "version", "9" },
                { "cameraId", cameraId.ToString() },
                { "_sid", _sid }
            };

            var query = string.Join("&", payload.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = $"http://{_ipAddress}:5000{api}&{query}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<JObject> GetCameraInfoAsync()
        {
            var api = "/webapi/entry.cgi?api=SYNO.SurveillanceStation.Camera";
            var payload = new Dictionary<string, string>
            {
                { "method", "GetInfo" },
                { "version", "9" },
                { "_sid", _sid }
            };

            return await SendRequestAsync(api, payload, HttpMethod.Get);
        }

        public async Task<JObject> GetLiveViewPathAsync(int cameraId)
        {
            var api = "/webapi/entry.cgi?api=SYNO.SurveillanceStation.Camera";
            var payload = new Dictionary<string, string>
            {
                { "method", "GetLiveViewPath" },
                { "version", "9" },
                { "cameraId", cameraId.ToString() },
                { "_sid", _sid }
            };

            return await SendRequestAsync(api, payload, HttpMethod.Get);
        }

        public async Task<JObject> SetupMotionDetectionAsync(int cameraId)
        {
            var api = "/webapi/entry.cgi?api=SYNO.Surveillance.Camera.Event";
            var payload = new Dictionary<string, string>
            {
                { "method", "MDParamSave" },
                { "version", "9" },
                { "cameraId", cameraId.ToString() },
                { "source", "motion" },
                { "_sid", _sid }
            };

            return await SendRequestAsync(api, payload, HttpMethod.Post);
        }

        public async Task<JObject> GetMotionEventsAsync(int cameraId)
        {
            var api = "/webapi/entry.cgi?api=SYNO.Surveillance.Camera.Event";
            var payload = new Dictionary<string, string>
            {
                { "method", "MotionEnum" },
                { "version", "9" },
                { "cameraId", cameraId.ToString() },
                { "_sid", _sid }
            };

            return await SendRequestAsync(api, payload, HttpMethod.Get);
        }

        public async Task<JObject> StartRecordingAsync(int cameraId)
        {
            var api = "/webapi/entry.cgi?api=SYNO.SurveillanceStation.ExternalRecording";
            var payload = new Dictionary<string, string>
            {
                { "method", "Record" },
                { "version", "9" },
                { "cameraId", cameraId.ToString() },
                { "action", "start" },
                { "_sid", _sid }
            };

            return await SendRequestAsync(api, payload, HttpMethod.Post);
        }

        public async Task<JObject> StopRecordingAsync(int cameraId)
        {
            var api = "/webapi/entry.cgi?api=SYNO.SurveillanceStation.ExternalRecording";
            var payload = new Dictionary<string, string>
            {
                { "method", "Record" },
                { "version", "9" },
                { "cameraId", cameraId.ToString() },
                { "action", "stop" },
                { "_sid", _sid }
            };

            return await SendRequestAsync(api, payload, HttpMethod.Post);
        }

        // Additional methods for camera management can be added here
    }
}