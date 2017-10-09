using System;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace MoMoChat
{
	public class Message
	{
		string id;
		string content;
        DateTime sentTime;
        string groupId;

        [JsonProperty(PropertyName = "id")]
		public string Id
		{
			get { return id; }
			set { id = value;}
		}

		[JsonProperty(PropertyName = "content")]
		public string Content
		{
			get { return content; }
			set { content = value;}
		}

        [JsonProperty(PropertyName = "groupId")]
        public string GroupId
        {
            get { return groupId; }
            set { groupId = value; }
        }

        [JsonProperty(PropertyName = "sentTime")]
        public DateTime SentTime
        {
            get { return sentTime; }
            set { sentTime = value; }
        }

        [Version]
        public string Version { get; set; }
	}
}

