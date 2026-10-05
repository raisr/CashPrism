using CashPrism.Web.StoredData;

namespace CashPrism.Web.Tests.Unit.StoredData;

public sealed class StoredDataChangesTests
{
    public sealed class Notify
    {
        [Fact]
        public void Tells_Every_Listener()
        {
            var changes = new StoredDataChanges();
            var heard = 0;
            changes.Changed += () => heard++;
            changes.Changed += () => heard++;

            changes.Notify();

            Assert.Equal(2, heard);
        }

        [Fact]
        public void Does_Nothing_Without_A_Listener()
        {
            var changes = new StoredDataChanges();

            var exception = Record.Exception(changes.Notify);

            Assert.Null(exception);
        }
    }
}
