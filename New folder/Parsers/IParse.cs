namespace HOMEManager
{
    public interface IParse<T>
    {
        static abstract T Parse(BinaryReader reader);
    }
}
