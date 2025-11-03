using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Enterspeed.Query.Sdk.Domain.Services;
using Xunit;

namespace Enterspeed.Query.Sdk.Tests.Domain.Services
{
    public class EnterspeedQueryServiceTests
    {
        public class EnterspeedQueryServiceTestFixture : Fixture
        {
            public EnterspeedQueryServiceTestFixture()
            {
                Customize(new AutoNSubstituteCustomization());
            }
        }

        [Fact]
        public async Task ApiKey_Null_Throws()
        {
            var fixture = new EnterspeedQueryServiceTestFixture();

            var sut = fixture.Create<EnterspeedQueryService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Query(null, null));
        }

        [Fact]
        public async Task ApiKey_EmptyString_Throws()
        {
            var fixture = new EnterspeedQueryServiceTestFixture();

            var sut = fixture.Create<EnterspeedQueryService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Query("   ", null));
        }
    }
}