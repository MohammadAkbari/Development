using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace XmlWork
{
    class Program
    {
        static void Main(string[] args)
        {
            var vast = GetVastObject();

            var xml = Serialize(vast);

            var vast2 = Deserialize(xml);

            Console.WriteLine("Hello World!");
        }

        private static Vast Deserialize(string xml)
        {
            var serializer = new XmlSerializer(typeof(Vast));

            using (TextReader reader = new StringReader(xml))
            {
                var result = (Vast)serializer.Deserialize(reader);
                return result;
            }
        }

        private static string Serialize(Vast vast)
        {
            var serializer = new XmlSerializer(typeof(Vast));

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            var xml = string.Empty;

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, vast, ns);
                    xml = stringWriter.ToString();
                }
            }

            return xml;
        } 

        private static Vast GetVastObject()
        {
            var vast = new Vast
            {
                Version = "3.0",
                Ad = new Ad
                {
                    Id = "static",
                    InLine = new InLine
                    {
                        AdSystem = "DMS",
                        AdTitle = "DMS Video Ad - antenvideo",
                        Impression = new Impression
                        {
                            Id = "DMSVideoAd",
                            Text = "https://example.com"
                        },
                        Creatives = new List<Creative>
                        {
                            new Creative
                            {
                                Linear = new Linear
                                {
                                    SkipOffset = "00:00:05",
                                    Duration = "00:00:10",
                                    TrackingEvents = new List<Tracking>
                                    {
                                        new Tracking{Event = "start", Text = "https://example.com"},
                                        new Tracking{Event = "firstQuartile", Text = "https://example.com"},
                                        new Tracking{Event = "midpoint", Text = "https://example.com"},
                                        new Tracking{Event = "thirdQuartile", Text = "https://example.com"},
                                        new Tracking{Event = "complete", Text = "https://example.com"}
                                    },
                                    VideoClicks = new List<ClickThrough>
                                    {
                                        new ClickThrough{Text = "https://example.com"}
                                    },
                                    MediaFiles = new List<MediaFile>
                                    {
                                        new MediaFile
                                        {
                                            BitRate = 36,
                                            Delivery = "progressive",
                                            Height = 480,
                                            MaintainAspectRatio = true,
                                            Scalable = true,
                                            Text = "https://example.com/1a62c9f0ceb04b7fb730ac57e3ec4724.mp4",
                                            Type = "video/mp4",
                                            Width = 720
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return vast;
        }
    }

    #region VAST Elements

    [XmlRoot("VAST")]
    public class Vast
    {
        [XmlAttribute]
        public string Version { get; set; }

        public Ad Ad { get; set; }
    }

    public class Ad
    {
        [XmlAttribute]
        public string Id { get; set; }

        public InLine InLine { get; set; }
    }

    public class InLine
    {
        public string AdSystem { get; set; }

        public string AdTitle { get; set; }

        public Impression Impression { get; set; }
        public Creative Creative { get; set; }

        [XmlArray("Creatives")]
        [XmlArrayItem("Creative")]
        public List<Creative> Creatives { get; set; }
    }

    public class Impression
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlIgnore]
        [XmlText]
        public string Text { get; set; }

        [XmlText]
        public XmlNode[] CDataText
        {
            get
            {
                var dummy = new XmlDocument();
                return new XmlNode[] { dummy.CreateCDataSection(Text) };
            }
            set
            {
                if (value == null)
                {
                    Text = null;
                    return;
                }

                if (value.Length != 1)
                {
                    throw new InvalidOperationException($"Invalid array length {value.Length}");
                }

                Text = value[0].Value;
            }
        }
    }

    public class Creative
    {
        public Linear Linear { get; set; }
    }

    public class Linear
    {
        [XmlAttribute("skipoffset")]
        public string SkipOffset { get; set; }

        [XmlElement]
        public string Duration { get; set; }

        [XmlArray("TrackingEvents")]
        [XmlArrayItem("Tracking")]
        public List<Tracking> TrackingEvents { get; set; }

        [XmlArray("VideoClicks")]
        [XmlArrayItem("ClickThrough")]
        public List<ClickThrough> VideoClicks { get; set; }

        [XmlArray("MediaFiles")]
        [XmlArrayItem("MediaFile")]
        public List<MediaFile> MediaFiles { get; set; }
    }

    public class Tracking
    {
        [XmlAttribute]
        public string Event { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class ClickThrough
    {
        [XmlText]
        public string Text { get; set; }
    }

    public class MediaFile
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("bitrate")]
        public int BitRate { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("delivery")]
        public string Delivery { get; set; }

        [XmlAttribute("scalable")]
        public bool Scalable { get; set; }

        [XmlAttribute("maintainAspectRatio")]
        public bool MaintainAspectRatio { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    #endregion
}
