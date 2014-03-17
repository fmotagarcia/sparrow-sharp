using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace Sparrow.ResourceLoading
{
	public abstract class Resource
	{
		public ResourceType ResType { get {return _resourceType;}}
		public bool IsLoaded { get {return _isLoaded;}}
		public object GetResource () {return _resource;	}
		public delegate void EventHandler (object resource, Resource resourceLoader);
		public event EventHandler ResourceLoaded;
		protected ResourceType _resourceType;
		protected object _resource;
		protected bool _isLoaded = false;

		public Resource LoadRaw(byte [] data, ResourceType resType) 
		{
			_resourceType = resType;
			DecodeRawResult (data);
			return this;
		}

		public Resource LoadRemoteResource(Uri remoteURL, ResourceType resType) 
		{
			_resourceType = resType;
			WebClient webClient = new WebClient(); // TODO make sure that this does not block the UI thread
			webClient.DownloadDataCompleted += onRemoteResourceLoaded;
			webClient.DownloadDataAsync(remoteURL);
			return this;
		}

		public Resource LoadLocalResource (string pathToFile) {
			if (!File.Exists (pathToFile)) {
				throw new Exception ("File does not exist:" + pathToFile);
			}
			LoadLocalResourceAsync(pathToFile);
			return this;
		}

		private async void LoadLocalResourceAsync(string pathToFile)
		{
			FileStream fs = File.OpenRead (pathToFile);
			if (fs.CanRead) {
				byte[] buffer = new byte[fs.Length];
				await fs.ReadAsync (buffer, 0, (int)fs.Length);
				DecodeRawResult (buffer);
			} else {
				throw new Exception ("File can not be read: " + pathToFile);
			}
		}

		private void onRemoteResourceLoaded (object sender, DownloadDataCompletedEventArgs args) {
			byte[] result = args.Result;
			DecodeRawResult (result);
		}

		// override if you need your own decoding logic
		protected void DecodeRawResult(byte[] data) {
			if (_resourceType == ResourceType.IMAGE) {
				DecodeImage (data);	
			}
			else if (_resourceType == ResourceType.SOUND) {
				// TODO
			}
		}

		abstract protected void DecodeImage (byte[] data);

		protected void InvokeComplete() {
			if (ResourceLoaded != null) {
				ResourceLoaded (_resource, this);
			}
		}

	}
}

