namespace AtelierManager
{
    public static class Utils
    {
        private const string CATALOG = "catalog.json";
        private readonly static string OUTPUT = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundles");

        static Utils()
        {
            Directory.CreateDirectory(OUTPUT);
        }

        public static string CatalogPath => CATALOG;
        public static string GetCatalogPath()
        {
            var path = Path.Combine(OUTPUT, CATALOG);
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            return path;
        }
        public static string GetBundlePath(BundleInfo bundleInfo)
        {
            var path = Path.Combine(OUTPUT, bundleInfo.RelativePath.Replace(".bundle", ".unity3d"));
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            return path;
        }
    }
}
