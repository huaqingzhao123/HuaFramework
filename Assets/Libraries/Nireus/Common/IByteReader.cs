using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
    public interface IByteReader
    {
        bool readBool();
        byte readInt8();
        short readInt16();
        int readInt32();
        long readInt64();
        float readFloat32();
        double readFloat64();
        string readString();
        int getReadOffset();
        int getDataSize();
        void readBytes(byte[] bytes, int offset = 0, int length = 0);
    }
}
