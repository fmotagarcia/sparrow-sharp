using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace Sparrow.ResourceLoading
{
	public abstract class LoaderBase
	{
		protected bool _isLoaded = false;
		public bool IsLoaded { get {return _isLoaded;}}

		public LoaderBase LoadRaw(byte [] data) 
		{
			_isLoaded = false;
			DecodeRawResult (data);
			return this;
		}

		virtual public LoaderBase LoadRemoteResource(Uri remoteURL) 
		{
			_isLoaded = false;
			WebClient webClient = new WebClient(); // TODO make sure that this does not block the UI thread
			webClient.DownloadDataCompleted += onRemoteResourceLoaded;
			webClient.DownloadDataAsync(remoteURL);
			return this;
		}

		virtual public LoaderBase LoadLocalResource (string pathToFile) {
			_isLoaded = false;
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

		private void onRemoteResourceLoaded (object sender, DownloadDataCompletedEventArgs args) 
		{
			_isLoaded = false;
			byte[] result = args.Result;
			DecodeRawResult (result);
		}

		// override if you need your own decoding logic
		abstract protected void DecodeRawResult (byte[] data);

	}
}

