namespace RoundReports
{
#pragma warning disable SA1402
#pragma warning disable SA1649
#pragma warning disable SA1600
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an embed URL.
    /// </summary>
    [JsonObject]
    public interface IEmbedUrl
    {
        [JsonProperty("url")]
        string Url { get; set; }
    }

    /// <summary>
    /// Represents an embed proxy URL.
    /// </summary>
    [JsonObject]
    public interface IEmbedProxyUrl
    {
        [JsonProperty("proxy_url")]
        string ProxyUrl { get; set; }
    }

    /// <summary>
    /// Represents an embed icon URL.
    /// </summary>
    [JsonObject]
    public interface IEmbedIconUrl
    {
        [JsonProperty("icon_url")]
        string IconUrl { get; set; }
    }

    /// <summary>
    /// Represents an embed proxy icon URL.
    /// </summary>
    [JsonObject]
    public interface IEmbedIconProxyUrl
    {
        [JsonProperty("proxy_icon_url")]
        string ProxyIconUrl { get; set; }
    }

    /// <summary>
    /// Represents an embed dimension.
    /// </summary>
    [JsonObject]
    public interface IEmbedDimension
    {
        [JsonProperty("height")]
        int Height { get; set; }

        [JsonProperty("width")]
        int Width { get; set; }
    }

    /// <summary>
    /// Internal discord webhook structure.
    /// </summary>
    public class DiscordHook
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("tts")]
        public bool IsTTS { get; set; }

        [JsonProperty("embeds")]
        public List<Embed> Embeds { get; set; } = new List<Embed>();
    }

    /// <summary>
    /// Internal discord embed structure.
    /// </summary>
    [JsonObject]
    public class Embed : IEmbedUrl
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "rich";

        [JsonProperty("description")]
        public string Description { get; set; }

        public string Url { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? TimeStamp { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("footer")]
        public EmbedFooter Footer { get; set; }

        [JsonProperty("image")]
        public EmbedImage Image { get; set; }

        [JsonProperty("thumbnail")]
        public EmbedThumbnail Thumbnail { get; set; }

        [JsonProperty("video")]
        public EmbedVideo Video { get; set; }

        [JsonProperty("provider")]
        public EmbedProvider Provider { get; set; }

        [JsonProperty("author")]
        public EmbedAuthor Author { get; set; }

        [JsonProperty("fields")]
        public List<EmbedField> Fields { get; set; } = new List<EmbedField>();
    }

    /// <summary>
    /// Internal embed footer structure.
    /// </summary>
    [JsonObject]
    public class EmbedFooter : IEmbedIconUrl, IEmbedIconProxyUrl
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        public string IconUrl { get; set; }

        public string ProxyIconUrl { get; set; }
    }

    /// <summary>
    /// Internal embed image structure.
    /// </summary>
    [JsonObject]
    public class EmbedImage : EmbedProxyUrl, IEmbedDimension
    {
        public int Height { get; set; }

        public int Width { get; set; }
    }

    /// <summary>
    /// Internal embed thumbnail structure.
    /// </summary>
    [JsonObject]
    public class EmbedThumbnail : EmbedProxyUrl, IEmbedDimension
    {
        public int Height { get; set; }

        public int Width { get; set; }
    }

    /// <summary>
    /// Internal embed video structure.
    /// </summary>
    [JsonObject]
    public class EmbedVideo : EmbedUrl, IEmbedDimension
    {
        public int Height { get; set; }

        public int Width { get; set; }
    }

    /// <summary>
    /// Internal embed provider.
    /// </summary>
    [JsonObject]
    public class EmbedProvider : EmbedUrl
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Internal embed author structure.
    /// </summary>
    [JsonObject]
    public class EmbedAuthor : EmbedUrl, IEmbedIconUrl, IEmbedIconProxyUrl
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public string IconUrl { get; set; }

        public string ProxyIconUrl { get; set; }
    }

    /// <summary>
    /// Internal embed field structure.
    /// </summary>
    [JsonObject]
    public class EmbedField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }

    /// <summary>
    /// Internal embed URL structure.
    /// </summary>
    [JsonObject]
    public abstract class EmbedUrl : IEmbedUrl
    {
        public string Url { get; set; }
    }

    /// <summary>
    /// Internal embed proxy URL structure.
    /// </summary>
    [JsonObject]
    public abstract class EmbedProxyUrl : EmbedUrl, IEmbedProxyUrl
    {
        public string ProxyUrl { get; set; }
    }
#pragma warning restore SA1402
#pragma warning restore SA1649
#pragma warning restore SA1600
}