public class LoadRequest
{
    public int PageX { get; }
    public int PageY { get; }
    public int MipLevel { get; }

    public LoadRequest(int x, int y, int mipLevel)
    {
        PageX = x;
        PageY = y;
        MipLevel = mipLevel;
    }
}
