using CashPrism.Domain.Imports;

namespace CashPrism.Domain.Tests.Unit.Imports;

public sealed class RawRowTests
{
    private const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";

    private static readonly Guid AnImportRunId = Guid.Parse("8c1f7a52-0b3d-4e6f-9a10-2b3c4d5e6f70");

    private static RawRow Create(
        string json = """{"Buchungstag":"14.03.2026"}""",
        string fingerprint = AFingerprint,
        Guid? importRunId = null)
        => new(importRunId ?? AnImportRunId, fingerprint, json);

    public sealed class Constructor
    {
        [Fact]
        public void Refuses_A_Blank_Json_Payload()
            => Assert.Throws<ArgumentException>(() => Create(json: " "));

        [Fact]
        public void Refuses_A_Row_That_Does_Not_Say_Which_Run_Read_It()
            => Assert.Throws<ArgumentException>(() => Create(importRunId: Guid.Empty));
    }

    public sealed class HasSameContentAs
    {
        [Fact]
        public void Says_Yes_When_A_Re_Import_Read_The_Same_Row_Again()
        {
            var stored = Create(importRunId: AnImportRunId);
            var reread = Create(importRunId: Guid.NewGuid());

            Assert.True(reread.HasSameContentAs(stored));
        }

        [Fact]
        public void Says_No_When_The_Booking_Was_Enriched_Since_The_Last_Import()
        {
            var stored = Create(json: """{"Beguenstigter/Auftraggeber":"UNKNOWN"}""");
            var enriched = Create(json: """{"Beguenstigter/Auftraggeber":"A shop"}""");

            Assert.False(enriched.HasSameContentAs(stored));
        }
    }
}
