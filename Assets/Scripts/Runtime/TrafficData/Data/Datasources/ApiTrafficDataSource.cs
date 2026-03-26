using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

using Cysharp.Threading.Tasks;
using CustomLogger;

namespace NewFrogger.Traffic.Data.Datasources
{
    using Data.DTO;
    
    public class ApiTrafficDataSource : ITrafficDataSource
    {
        private readonly string _baseURL;

        public ApiTrafficDataSource(string baseURL = null)
        {
            _baseURL = baseURL ?? throw new ArgumentNullException("API url must not be null");
        }

        public async UniTask<TrafficStatsDTO> GetStats(int level, CancellationToken ct)
        {
            const string endpoint = "/v1/traffic/status?level=";
            string completePath = $"{_baseURL}{endpoint}{level}";
            
            using UnityWebRequest webRequest = UnityWebRequest.Get(completePath);
            
            try
            {
                await webRequest.SendWebRequest().WithCancellation(ct);

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    string errorMsg = $"Traffic API error: {webRequest.result}";
                    Log.log(errorMsg);
                    throw new System.Net.Http.HttpRequestException(errorMsg);
                }

                string json = webRequest.downloadHandler.text;
                if (string.IsNullOrEmpty(json))
                {
                    throw new InvalidDataException("Received empty response from Traffic API");
                }

                var result = JsonUtility.FromJson<TrafficStatsDTO>(json);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.log($"Failed to fetch traffic stats from API: {ex.Message}");
                throw;
            }
        }
    }
}