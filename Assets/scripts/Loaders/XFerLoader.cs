using System.IO;

public class XFerLoader : Loader
{
    public byte[] auxPalVal; //= //new byte[5,32];
                             //When aux pal is the following
                             //  0xf0   fade to red
                             // 0xf4   fade to blue
                             // 0xf8   fade to green 
                             // ????   fade to white   
                             //????   fade to black

    //   0x0080      fade to red
    // 0x0100      fade to green
    //0x0180      fade to blue
    // 0x0200      fade to white
    // 0x0280      fade to black

    public XFerLoader()
    {
        // byte[] pal_file;
        filePath = Path.Combine(BasePath, "data\\xfer.dat");
        if (ReadStreamFile(filePath, out auxPalVal))
        {
            /*for (int a = 0; a <= auxPalVal.GetUpperBound(0); a++)
            {
                for (int i = 0; i < 32; i++)
                {
                    //auxPalVal[a, i] = (byte)DataLoader.getValAtAddress(pal_file, 0x80 + a * 32 + i, 8);
                }
            }*/
        }
    }

}
