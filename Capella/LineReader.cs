using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Capella
{
    public class LineReader : BinaryReader
    {
        private Encoding _encoding;
        private Decoder _decoder;

        const int bufferSize = 1024;
        private char[] _LineBuffer = new char[bufferSize];

        public LineReader(Stream stream, int bufferSize, Encoding encoding)
            : base(stream, encoding)
        {
            this._encoding = encoding;
            this._decoder = encoding.GetDecoder();
        }

        public string ReadLine()
        {
            int pos = 0;

            char[] buf = new char[2];

            StringBuilder stringBuffer = new StringBuilder();
            bool lineEndFound = false;

            while (base.Read(buf, 0, 2) > 0)
            {
                if (buf[0] == '\r' && buf[1] == '\n')
                {
                    lineEndFound = true;
                } else if (buf[1] == '\r')
                {
                    // grab buf[0]
                    this._LineBuffer[pos++] = buf[0];
                    // get the '\n'
                    char ch = base.ReadChar();
                    if (ch == '\n')
                        lineEndFound = true;
                }
                else
                {
                    this._LineBuffer[pos] = buf[0];
                    this._LineBuffer[pos + 1] = buf[1];
                    pos += 2;

                    if (pos >= bufferSize)
                    {
                        stringBuffer.Append(this._LineBuffer, 0, bufferSize);
                        pos = 0;
                    }
                }

                if (lineEndFound)
                {
                    if (stringBuffer == null)
                    {
                        if (pos > 0)
                            return new string(this._LineBuffer, 0, pos);
                        else
                            return string.Empty;
                    }
                    else
                    {
                        if (pos > 0)
                            stringBuffer.Append(this._LineBuffer, 0, pos);
                        return stringBuffer.ToString();
                    }
                }
            }

            if (stringBuffer != null)
            {
                if (pos > 0)
                    stringBuffer.Append(this._LineBuffer, 0, pos);
                return stringBuffer.ToString();
            }
            else
            {
                if (pos > 0)
                    return new string(this._LineBuffer, 0, pos);
                else
                    return null;
            }
        }

    }
}
