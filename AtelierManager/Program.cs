namespace AtelierManager
{
    public class Program
    {
        public static void Main(string[] args) => CommandLine.Init(args);

        public static async void Run(Options o)
        {
            var download = DownloadBundles(o);
            download.Wait();
        }

        public static async Task<bool> DownloadBundles(Options o)
        {
            try
            {
                var dllManager = new DLLManager(o.Api);
                var data = await dllManager.DownloadFile(Utils.CatalogPath);
                if (data != null && data.Length > 0)
                {
                    File.WriteAllBytes(Utils.GetCatalogPath(), data);
                }
                if (o.Catalog != null && o.Catalog.Exists)
                {
                    data = File.ReadAllBytes(o.Catalog.FullName);
                }
                var bundleInfos = CatalogParser.Parse(data);
                var tasks = bundleInfos.Select(async x => await DownloadBundle(dllManager, x));
                Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                return false;
            }
            return true;
        }
        public static async Task<bool> DownloadBundle(DLLManager dllManager, BundleInfo bundleInfo)
        {
            var bytes = await dllManager.DownloadFile(bundleInfo.RelativePath);
            if (bytes == null || bytes.Length == 0) return false;
            bytes = Decryptor.Decrypt(bytes, bundleInfo.Key);
            File.WriteAllBytes(Utils.GetBundlePath(bundleInfo), bytes);
            return true;
        }
    } 
}