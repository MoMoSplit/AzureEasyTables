using System;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace MoMoChat
{
	public class Group
	{
		string id;
		string name;

		[JsonProperty(PropertyName = "id")]
		public string Id
		{
			get { return id; }
			set { id = value;}
		}

		[JsonProperty(PropertyName = "name")]
		public string Name
		{
			get { return name; }
			set { name = value;}
		}

        [Version]
        public string Version { get; set; }
	}
}

