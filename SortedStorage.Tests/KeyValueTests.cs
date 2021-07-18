using SortedStorage.Application;
using System;
using System.Text;
using Xunit;

namespace SortedStorage.Tests
{
    public class KeyValueTests
    {
        [Fact]
        public void keyvalue_should_build_byte_array()
        {
            KeyValueEntry keyValue = new KeyValueEntry("key", "a��o");

            byte[] data = keyValue.ToBytes();

            Assert.Equal(keyValue.GetHashCode(), BitConverter.ToInt32(data, 0));
            Assert.Equal(3, BitConverter.ToInt32(data, 4));
            Assert.Equal(6, BitConverter.ToInt32(data, 8));
            Assert.Equal("keya��o", Encoding.UTF8.GetString(data, 12, 9));
        }
    }
}
