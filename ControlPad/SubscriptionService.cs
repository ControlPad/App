using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControlPad
{
    public static class SubscriptionService
    {
        private const string SubscriptionsUrl =
            "https://raw.githubusercontent.com/ControlPad/App/main/subscriptions.json";

        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Fetches the subscriptions file from GitHub and returns the badge type
        /// for the given device serial number.
        /// </summary>
        public static async Task<BadgeType> GetBadgeTypeAsync(string? serial)
        {
            if (string.IsNullOrEmpty(serial))
                return BadgeType.None;

            try
            {
                string json = await _httpClient.GetStringAsync(SubscriptionsUrl);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<SubscriptionsData>(json, options);
                if (data?.Subscriptions == null)
                    return BadgeType.None;

                foreach (var entry in data.Subscriptions)
                {
                    if (string.Equals(entry.SerialNumber, serial, StringComparison.OrdinalIgnoreCase))
                        return (BadgeType)entry.BadgeType;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Subscription fetch failed: {ex.Message}");
            }

            return BadgeType.None;
        }

        private class SubscriptionsData
        {
            public List<SubscriptionEntry>? Subscriptions { get; set; }
        }

        private class SubscriptionEntry
        {
            public string SerialNumber { get; set; } = "";
            public int BadgeType { get; set; }
        }
    }
}
