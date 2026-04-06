 using System.Text.Json.Serialization;
namespace PotholeApi.DTO
{

    public class ReceivePotholeTemp
    {
        // The JSON sends "_id", so we map it here.
        // It sends an OBJECT, so we use a class (MongoId), not a string.
        // Timestamp boundrybox Confidence StoredFile
        [JsonPropertyName("_id")]
        public MongoId Id { get; set; }

        [JsonPropertyName("timestamp")]
        public double? Timestamp { get; set; }

        [JsonPropertyName("image_ref")]
        public string? ImageRef { get; set; }

        [JsonPropertyName("bounding_box")]
        public List<int>? BoundingBox { get; set; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }

        [JsonPropertyName("matched_gps")]
        public MatchedGps? MatchedGps { get; set; }

        [JsonPropertyName("_stored_file")]
        public string? StoredFile { get; set; }

        // The JSON sends "createdAt" as an OBJECT, so we use a class (MongoDate).
        [JsonPropertyName("createdAt")]
        public MongoDate? CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public MongoDate? UpdatedAt { get; set; }

        [JsonPropertyName("__v")]
        public int V { get; set; }
    }

    // ---------------------------------------------------------
    // Helper Classes to match the "Boxes" (nested objects) in JSON
    // ---------------------------------------------------------

    public class MatchedGps
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }
    }

    // Represents the { "$oid": "..." } object
    public class MongoId
    {
        [JsonPropertyName("$oid")]
        public string Oid { get; set; }
    }

    // Represents the { "$date": "..." } object
    public class MongoDate
    {
        [JsonPropertyName("$date")]
        public DateTime Date { get; set; }
    }

}
