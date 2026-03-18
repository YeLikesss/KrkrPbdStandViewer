using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Pbd.Commom
{
    /// <summary>
    /// TJS类型
    /// </summary>
    internal enum TJSVariantType : byte
    {
        /// <summary>
        /// 空对象
        /// </summary>
        Void = 0x00,
        /// <summary>
        /// 对象
        /// </summary>
        Object = 0x01,
        /// <summary>
        /// 字符串
        /// </summary>
        String = 0x02,
        /// <summary>
        /// 二进制数据
        /// </summary>
        Octet = 0x03,
        /// <summary>
        /// 64位数据
        /// </summary>
        Integer = 0x04,
        /// <summary>
        /// 双精度浮点
        /// </summary>
        Real = 0x05,
        /// <summary>
        /// 数组
        /// </summary>
        ArrayObject = 0x81,
        /// <summary>
        /// 字典
        /// </summary>
        DictionaryObject = 0xC1,
    }
    /// <summary>
    /// tTJSVariant变量
    /// </summary>
    internal class TJSVariant
    {
        public TJSVariantType Type { get; private set; }
        public object? Value { get; private set; }

        public TJSVariant(TJSVariantType type, object? value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        public string AsString()
        {
            if (this.Type != TJSVariantType.String)
            {
                throw new TJSVariantException(this.Type);
            }
            return (string)this.Value!;
        }

        /// <summary>
        /// 获取二进制流
        /// </summary>
        public byte[] AsOctet()
        {
            if (this.Type != TJSVariantType.Octet)
            {
                throw new TJSVariantException(this.Type);
            }
            return (byte[])this.Value!;
        }

        /// <summary>
        /// 获取64位整数
        /// </summary>
        public long AsInteger()
        {
            if (this.Type != TJSVariantType.Integer)
            {
                throw new TJSVariantException(this.Type);
            }
            return (long)this.Value!;
        }

        /// <summary>
        /// 获取64位浮点
        /// </summary>
        public double AsReal()
        {
            if (this.Type != TJSVariantType.Real)
            {
                throw new TJSVariantException(this.Type);
            }
            return (double)this.Value!;
        }

        /// <summary>
        /// 获取TJS数组
        /// </summary>
        public List<TJSVariant> AsArray()
        {
            if (this.Type != TJSVariantType.ArrayObject)
            {
                throw new TJSVariantException(this.Type);
            }
            return (List<TJSVariant>)this.Value!;
        }

        /// <summary>
        /// 获取TJS字典
        /// </summary>
        public Dictionary<string, TJSVariant> AsDictionary()
        {
            if (this.Type != TJSVariantType.DictionaryObject)
            {
                throw new TJSVariantException(this.Type);
            }
            return (Dictionary<string, TJSVariant>)this.Value!;
        }
    }
    /// <summary>
    /// TJS反序列化
    /// </summary>
    internal class TJSDeserializer
    {
        private readonly Stream mStream;
        private readonly bool mBigEndian;
        private readonly Func<byte, byte, bool>? mChecker;

        private unsafe T Read<T>() where T : struct
        {
            Span<byte> temp = stackalloc byte[Unsafe.SizeOf<T>()];
            this.mStream.Read(temp);
            if (this.mBigEndian)
            {
                temp.Reverse();
            }
            return MemoryMarshal.Read<T>(temp);
        }

        private TJSVariantType ReadType()
        {
            byte type = (byte)this.mStream.ReadByte();

            //校验类型
            this.mChecker?.Invoke(type, (byte)this.mStream.ReadByte());

            return (TJSVariantType)type;
        }

        private string ReadString()
        {
            int length = this.Read<int>();
            if (length > 0)
            {
                byte[] buffer = new byte[sizeof(char) * length];
                this.mStream.Read(buffer);
                return Encoding.Unicode.GetString(buffer);
            }
            else
            {
                return string.Empty;
            }
        }

        private byte[] ReadOctet()
        {
            uint length = this.Read<uint>();
            if (length != 0u)
            {
                byte[] buffer = new byte[length];
                this.mStream.Read(buffer);
                return buffer;
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        private long ReadInteger()
        {
            return this.Read<long>();
        }

        private double ReadReal()
        {
            return this.Read<double>();
        }

        private List<TJSVariant> ReadArray()
        {
            int count = this.Read<int>();
            List<TJSVariant> arr = new(count);
            for (int i = 0; i < count; ++i)
            {
                arr.Add(this.Deserialize());
            }
            return arr;
        }

        private Dictionary<string, TJSVariant> ReadDictionary()
        {
            int count = this.Read<int>();
            Dictionary<string, TJSVariant> dict = new(count);
            for (int i = 0; i < count; ++i)
            {
                dict.Add(this.ReadString(), this.Deserialize());
            }
            return dict;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public TJSVariant Deserialize()
        {
            TJSVariantType type = this.ReadType();
            return type switch
            {
                TJSVariantType.Void => new TJSVariant(TJSVariantType.Void, null),
                TJSVariantType.Object => new TJSVariant(TJSVariantType.Object, null),
                TJSVariantType.String => new TJSVariant(TJSVariantType.String, this.ReadString()),
                TJSVariantType.Octet => new TJSVariant(TJSVariantType.Octet, this.ReadOctet()),
                TJSVariantType.Integer => new TJSVariant(TJSVariantType.Integer, this.ReadInteger()),
                TJSVariantType.Real => new TJSVariant(TJSVariantType.Real, this.ReadReal()),
                TJSVariantType.ArrayObject => new TJSVariant(TJSVariantType.ArrayObject, this.ReadArray()),
                TJSVariantType.DictionaryObject => new TJSVariant(TJSVariantType.DictionaryObject, this.ReadDictionary()),
                _ => throw new TJSVariantException($"读取到未知类型({type})"),
            };
        }

        /// <summary>
        /// TJS对象反序列化器
        /// </summary>
        /// <param name="stream">二进制流</param>
        /// <param name="bigEndian">大端:True 小端:False</param>
        /// <param name="checker">校验函数</param>
        public TJSDeserializer(Stream stream, bool bigEndian, Func<byte, byte, bool>? checker = null)
        {
            this.mStream = stream;
            this.mBigEndian = bigEndian;
            this.mChecker = checker;
        }
    }

    /// <summary>
    /// TJS序列化
    /// </summary>
    internal class TJSSerializer
    {
        private readonly Stream mStream;

        private void Write<T>(T v) where T : struct
        {
            Span<byte> p = stackalloc byte[Unsafe.SizeOf<T>()];
            MemoryMarshal.Write(p, ref v);
            this.mStream.Write(p);
        }

        private void WriteType(TJSVariantType type)
        {
            this.mStream.WriteByte((byte)type);
        }

        private void WriteString(string s)
        {
            this.Write(s.Length);
            if(s.Length > 0)
            {
                this.mStream.Write(MemoryMarshal.AsBytes(s.AsSpan()));
            }
        }

        private void WriteOctet(byte[] octet)
        {
            this.Write(octet.Length);
            if (octet.Length != 0)
            {
                this.mStream.Write(octet);
            }
        }

        private void WriteInteger(long v)
        {
            this.Write(v);
        }

        private void WriteReal(double v)
        {
            this.Write(v);
        }

        private void WriteArray(List<TJSVariant> arr)
        {
            this.Write(arr.Count);
            foreach(TJSVariant v in arr)
            {
                this.Serialize(v);
            }
        }

        private void WriteDictionary(Dictionary<string, TJSVariant> dict)
        {
            this.Write(dict.Count);
            foreach (KeyValuePair<string, TJSVariant> pair in dict)
            {
                this.WriteString(pair.Key);
                this.Serialize(pair.Value);
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public void Serialize(TJSVariant variant)
        {
            TJSVariantType type = variant.Type;
            this.WriteType(type);
            switch (type)
            {
                case TJSVariantType.Void:
                case TJSVariantType.Object:
                    break;
                case TJSVariantType.String:
                    this.WriteString(variant.AsString());
                    break;
                case TJSVariantType.Octet:
                    this.WriteOctet(variant.AsOctet());
                    break;
                case TJSVariantType.Integer:
                    this.WriteInteger(variant.AsInteger());
                    break;
                case TJSVariantType.Real:
                    this.WriteReal(variant.AsReal());
                    break;
                case TJSVariantType.ArrayObject:
                    this.WriteArray(variant.AsArray());
                    break;
                case TJSVariantType.DictionaryObject:
                    this.WriteDictionary(variant.AsDictionary());
                    break;
                default:
                    throw new TJSVariantException($"读取到未知类型({type})");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">输出流</param>
        public TJSSerializer(Stream stream)
        {
            this.mStream = stream;
        }
    }

    /// <summary>
    /// TJS异常
    /// </summary>
    internal class TJSVariantException : Exception
    {
        public TJSVariantException(string message) : base(message)
        {
        }
        public TJSVariantException(TJSVariantType type) : this($"类型错误, 当前类型为[{type}]")
        {
        }
        public TJSVariantException(TJSVariantType srcType, TJSVariantType cvtType) : this($"无法转换类型[{srcType}]到[{cvtType}]")
        {
        }
    }
}