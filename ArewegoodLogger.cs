using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arewegood_sdk_cs
{
    /// <summary>
    /// Represents the different levels of logging
    /// </summary>
    public enum ArewegoodLoggerLevel
    {
        FATAL,
        ERROR,
        WARN,
        INFO,
        DEBUG,
        TRACE
    };

    /// <summary>
    /// Represents a logger
    /// </summary>
    public interface IArewegoodLogger
    {
        Windows.Foundation.IAsyncOperation<bool> FlushStream();
        Windows.Foundation.IAsyncOperationWithProgress<uint, uint> SetStream(Windows.Storage.Streams.IOutputStream stream);
        Windows.Storage.Streams.IOutputStream GetStream();
        void Fatal(params object[] args);
        void Error(params object[] args);
        void Warn(params object[] args);
        void Info(params object[] args);
        void Debug(params object[] args);
        void Trace(params object[] args);
    }

    /// <summary>
    /// Provides a default Logger, that writes JSON objects to a stream
    /// </summary>
    public class ArewegoodLogger : IArewegoodLogger
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">name of the logger stream</param>
        public ArewegoodLogger(string name, Windows.Storage.Streams.IOutputStream stream)
        {
            Name = name;
            SetStream(stream);
        }

        /// <summary>
        /// The name of the logger
        /// </summary>
        private string Name;

        /// <summary>
        /// the stream the logger writes to
        /// </summary>
        private Windows.Storage.Streams.IOutputStream OutputStream;

        private Windows.Foundation.IAsyncOperationWithProgress<uint, uint> InternalWrite(string level, params object[] args)
        {
            var obj = new Windows.Data.Json.JsonObject();
            obj["level"] = Windows.Data.Json.JsonValue.CreateStringValue(level);

            var data = new Windows.Data.Json.JsonObject();
            obj["data"] = data;

            var buf = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(obj.ToString(), Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            return OutputStream.WriteAsync(buf);
        }

        public void Fatal(params object[] args)
        {
            InternalWrite("Fatal", args);
        }

        public void Error(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Debug(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(params object[] args)
        {
            throw new NotImplementedException();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<uint,uint> SetStream(Windows.Storage.Streams.IOutputStream stream)
        {
            OutputStream = stream;

            var obj = new Windows.Data.Json.JsonObject();
            obj["name"] = Windows.Data.Json.JsonValue.CreateStringValue(Name);
            var buf = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(obj.ToString(), Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            return OutputStream.WriteAsync(buf);
        }

        public Windows.Storage.Streams.IOutputStream GetStream()
        {
            return OutputStream;
        }

        public Windows.Foundation.IAsyncOperation<bool> FlushStream()
        {
            if (OutputStream == null) return new Task<bool>(() => { return false; }).AsAsyncOperation();
            return OutputStream.FlushAsync();
        }
}
}
