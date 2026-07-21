using Infrastructure.Cache;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Infrastructure.Tests
{

    public class MemoryCacheServiceTests
    {

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly MemoryCacheService _sut;


        public MemoryCacheServiceTests()
        {
            _sut = new MemoryCacheService(_cache);
        }

        [Fact]

        public async Task SetAsync_then_GetAsync_returnStoredValue()
        {
            //Arrange string key, T value, TimeSpan? ttl = null,   
            //CancellationToken cancellationToken
            var key = "any-key";
            var value = "Greece";
           

            //Act
            await _sut.SetAsync(key, value);
            var result = await _sut.GetAsync<string>(key);

            //Assert
            Assert.Equal(value, result);
        }


        [Fact]
        public async Task GetAsync_returns_default_when_key_is_missing()
        {
            // Arrange
            var key = "missing-key";   // key που δεν το βάλαμε ποτέ στο cache

            // Act
            var result = await _sut.GetAsync<string>(key);

            // Assert
            Assert.Null(result);       // σε miss, το TryGetValue δίνει default(string) = null
        }

        [Fact]
        public async Task RemoveAsync_deletes_the_entry()
        {
            // Arrange
            var key = "any-key";
            await _sut.SetAsync(key, "Greece");   // πρώτα set

            // Act
            await _sut.RemoveAsync(key);          // το σβήνουμε
            var result = await _sut.GetAsync<string>(key);

            // Assert
            Assert.Null(result);                  // μετά το remove δεν πρέπει να υπάρχει
        }
    }
}
