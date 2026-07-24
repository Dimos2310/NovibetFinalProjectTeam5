using Infrastructure.Cache;
using Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Infrastructure.Tests
{
    public class MemoryCacheServiceTests
    {
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly MemoryCacheService _sut;

        public MemoryCacheServiceTests()
        {
            _sut = new MemoryCacheService(_cache, Options.Create(new CacheOptions()));
        }

        [Fact]
        public async Task SetAsync_then_GetAsync_returnStoredValue()
        {
            var key = "any-key";
            var value = "Greece";

            await _sut.SetAsync(key, value);
            var result = await _sut.GetAsync<string>(key);

            Assert.Equal(value, result);
        }

        [Fact]
        public async Task GetAsync_returns_default_when_key_is_missing()
        {
            var result = await _sut.GetAsync<string>("missing-key");

            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveAsync_deletes_the_entry()
        {
            var key = "any-key";
            await _sut.SetAsync(key, "Greece");

            await _sut.RemoveAsync(key);
            var result = await _sut.GetAsync<string>(key);

            Assert.Null(result);
        }
    }
}
