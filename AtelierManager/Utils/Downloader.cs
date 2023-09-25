namespace AtelierManager
{
    public class DLLManager
    {
        public const string API = "https://asset.resleriana.jp/asset/1695186894_yFRlDhTt4xOHNPdw/Android/";
        
        private readonly Uri Api;
        private readonly HttpClient Client;
        public DLLManager(string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                url = API;
            }
            Api = new Uri(url);
            Client = new HttpClient
            {
                BaseAddress = Api
            };
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 6.0; Windows 98; Trident/5.1)");
        }

        public async Task<byte[]> DownloadFile(string path)
        {
            byte[] data = Array.Empty<byte>();
            if (Uri.TryCreate(Api, path, out var uri))
            {
                Console.WriteLine($"Downloading {Path.GetFileName(uri.AbsolutePath)}...");
                try
                {
                    data = await Client.GetByteArrayAsync(uri);
                }
                catch(Exception)
                {
                    Console.WriteLine($"Error while downloading {Path.GetFileName(uri.LocalPath)}");
                }
            }
            return data;
        }
    }
}
