using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PotholeApi.DTO
{
    public class DetectionData
    {
        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }

        [JsonPropertyName("image_id")]
        public string? ImageId { get; set; }

        [JsonPropertyName("image_path")]
        public string? ImagePath { get; set; }

        // IMPORTANT: You must check your Python code to see what key is used 
        // for the image (e.g., "image", "data", "base64", "image_base64").
        // Replace "image_base64" below with that exact key.
        [JsonPropertyName("image_base64")]
        public string? ImageBase64 { get; set; }

        [JsonPropertyName("bounding_box")]
        public List<double[]>? BoundingBox { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("levelOfSeverity")]
        public int LevelOfSeverity { get; set; }

        [JsonPropertyName("matched_gps")]
        public MatchedGpsv MatchedGps { get; set; }
    }

    public class MatchedGpsv
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }
    }
}