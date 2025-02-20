using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace ElysiaNBT;

public class WarppedTextReader(TextReader reader, int bufferSize, bool leaveOpen = true) : IDisposable
{
    public const int EOF = -1;

    protected char[] _buffer = ArrayPool<char>.Shared.Rent(bufferSize);
    protected int _bufferPos = 0;
    protected int _bufferRealSize = 0;
    protected bool _noMore = false;
    protected bool _leaveOpen = leaveOpen;
    protected TextReader _baseReader = reader;

    private bool _disposed = false;

    public int Position { get; protected set; }

    public WarppedTextReader(Stream stream, int bufferSize, Encoding? encoding = null, bool leaveOpen = true)
        : this(new StreamReader(stream, encoding, true, -1, leaveOpen), bufferSize, false) { }
    public WarppedTextReader(string str, int bufferSize)
        : this(new StringReader(str), bufferSize, false) { }

    protected int FillBuffer()
    {
        if (_bufferPos > 0)
        {
            _buffer.AsSpan(_bufferPos, _bufferRealSize).CopyTo(_buffer);
            _bufferPos = 0;
        }
        int remaining = _buffer.Length - _bufferRealSize;
        if (remaining > 0)
            _bufferRealSize += _baseReader.Read(_buffer, 0, remaining);
        return _bufferRealSize;
    }
    protected int FillBuffer(int minSize)
    {
        Debug.Assert(minSize >= 0);
        Debug.Assert(minSize <= _buffer.Length);

        if (minSize == 0)
            return FillBuffer();

        if (_bufferPos > 0)
        {
            _buffer.AsSpan(_bufferPos, _bufferRealSize).CopyTo(_buffer);
            _bufferPos = 0;
        }
        int remaining = _buffer.Length - _bufferRealSize;
        int read;
        while (_bufferRealSize < minSize)
        {
            read = _baseReader.Read(_buffer, _bufferRealSize, remaining);
            if (read == 0)
                break;
            _bufferRealSize += read;
            remaining -= read;
        }
        return _bufferRealSize;
    }
    protected int ClearBuffer(Span<char> outBuffer)
    {
        Debug.Assert(outBuffer.Length >= _bufferRealSize);

        int size = _bufferRealSize;
        _buffer.AsSpan(_bufferPos, size).CopyTo(outBuffer);
        _bufferPos = 0;
        _bufferRealSize = 0;
        Position += size;
        return size;
    }
    protected int ReadBlock(Span<char> buffer)
    {
        int count = buffer.Length;

        if (count <= _bufferRealSize)
        {
            _buffer.AsSpan(_bufferPos, count).CopyTo(buffer);
            _bufferRealSize -= count;
            _bufferPos += count;
            Position += count;
            return count;
        }

        int read = ClearBuffer(buffer);
        count -= read;
        read += _baseReader.Read(buffer.Slice(read, count));
        Position += read;
        return read;
    }
    protected int ReadUntilWhitespace(Span<char> buffer)
    {
        int offset = 0;
        int count = buffer.Length;

        int read = 0;
        while (count > 0)
        {
            int r = ReadChar();
            if (r == EOF)
                return read;
            char c = (char)r;
            if (char.IsWhiteSpace(c))
                return read;
            buffer[offset++] = c;
            count--;
            read++;
        }
        return read;
    }
    protected int PeekChar()
    {
        if (_bufferRealSize > 0)
        {
            return _buffer[_bufferPos];
        }
        return _baseReader.Peek();
    }
    protected int PeekChar(int offset)
    {
        Debug.Assert(offset >= 0);
        Debug.Assert(offset < _buffer.Length);

        if (FillBuffer(offset + 1) > offset)
        {
            return _buffer[_bufferPos + offset];
        }
        return EOF;
    }
    protected int ReadChar()
    {
        if (_bufferRealSize > 0)
        {
            _bufferRealSize--;
            Position++;
            return _buffer[_bufferPos++];
        }
        int read = _baseReader.Read();
        if (read == EOF)
        {
            if (_noMore)
                return EOF;
            _noMore = true;
            Position++;
        }
        Position++;
        return read;
    }
    protected bool SkipChars(int skip)
    {
        while (skip > 0 && _bufferRealSize > 0)
        {
            _bufferRealSize--;
            Position++;
            _bufferPos++;
            skip--;
        }
        int read = 0;
        while (skip > 0 && (read = _baseReader.Read()) != EOF)
        {
            Position++;
            skip--;
        }
        if (read == EOF)
        {
            Position++;
            _noMore = true;
        }
        return _noMore;
    }
    protected void SkipWhitespace()
    {
        int read;
        while ((read = ReadChar()) != EOF && char.IsWhiteSpace((char)read)) ;
        UngetChar(read);
    }
    protected bool UngetChar(int c)
    {
        if (c < 0)
            return true;
        int newPos = _buffer.Length - _bufferRealSize;
        Array.Copy(_buffer, _bufferPos, _buffer, newPos, _bufferRealSize);
        _bufferPos = newPos;
        if (_bufferPos > 0)
        {
            Position--;
            _buffer[--_bufferPos] = (char)c;
            _bufferRealSize++;
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        ArrayPool<char>.Shared.Return(_buffer);
        if (!_leaveOpen)
            _baseReader.Dispose();
        GC.SuppressFinalize(this);
    }
}
