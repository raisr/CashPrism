using CashPrism.Domain.Accounts;

namespace CashPrism.Domain.Tests.Unit.Accounts;

public sealed class AccountGroupingTests
{
    private const string Giro = "DE02120300000000202051";
    private const string Savings = "DE89370400440532013000";
    private const string Card = "DE75512108001245126199";

    private static readonly Guid Own = new("0f9a1c3e-0000-4000-8000-000000000001");
    private static readonly Guid Shared = new("0f9a1c3e-0000-4000-8000-000000000002");
    private static readonly Guid Other = new("0f9a1c3e-0000-4000-8000-000000000003");

    /// <summary>Own (Giro, Savings), Shared (Card), Other — every account shown.</summary>
    private static AccountGrouping ThreeGroups()
        => new(
            [new AccountGroup(Own, "Eigene", 0), new AccountGroup(Shared, "Gemeinsam", 1), new AccountGroup(Other, "Andere", 2)],
            [new AccountAssignment(Giro, Own, 0), new AccountAssignment(Savings, Own, 1), new AccountAssignment(Card, Shared, 0)]);

    private static IEnumerable<Guid> Order(AccountGrouping grouping) => grouping.Groups.Select(group => group.Id);

    public sealed class Constructor
    {
        [Fact]
        public void Orders_Groups_And_Accounts_By_Position()
        {
            var grouping = new AccountGrouping(
                [new AccountGroup(Shared, "Gemeinsam", 1), new AccountGroup(Own, "Eigene", 0)],
                [new AccountAssignment(Savings, Own, 1), new AccountAssignment(Giro, Own, 0)]);

            Assert.Equal([Own, Shared], Order(grouping));
            Assert.Equal([Giro, Savings], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Throws_Without_A_Group()
        {
            Assert.Throws<ArgumentException>(() => new AccountGrouping([], []));
        }

        [Fact]
        public void Throws_When_An_Assignment_Points_At_A_Missing_Group()
        {
            Assert.Throws<ArgumentException>(() => new AccountGrouping(
                [new AccountGroup(Own, "Eigene", 0)],
                [new AccountAssignment(Giro, Shared, 0)]));
        }
    }

    public sealed class Start
    {
        [Fact]
        public void Holds_One_Empty_Group()
        {
            var grouping = AccountGrouping.Start(Own, "Konten");

            var group = Assert.Single(grouping.Groups);
            Assert.Equal("Konten", group.Name);
            Assert.Empty(grouping.Assignments);
        }
    }

    public sealed class Place
    {
        [Fact]
        public void Appends_A_New_Account_To_The_Topmost_Group()
        {
            var grouping = ThreeGroups();

            grouping.Place([Card, "DE12500105170648489890", Giro, Savings]);

            Assert.Equal([Giro, Savings, "DE12500105170648489890"], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Appends_New_Accounts_In_The_Order_Given()
        {
            var grouping = AccountGrouping.Start(Own, "Konten");

            grouping.Place([Savings, Giro]);

            Assert.Equal([Savings, Giro], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Hides_An_Assigned_Account_That_Is_Not_Given()
        {
            var grouping = ThreeGroups();

            grouping.Place([Savings, Card]);

            Assert.Equal([Savings], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Keeps_The_Assignment_Of_A_Hidden_Account()
        {
            var grouping = ThreeGroups();

            grouping.Place([Savings, Card]);

            Assert.Contains(grouping.Assignments, assignment => assignment.AccountReference == Giro && assignment.GroupId == Own);
        }
    }

    public sealed class Add
    {
        [Fact]
        public void Appends_The_Group_At_The_Bottom()
        {
            var grouping = AccountGrouping.Start(Own, "Konten");

            grouping.Add(Shared, "Gemeinsam");

            Assert.Equal([Own, Shared], Order(grouping));
        }

        [Fact]
        public void Trims_The_Name()
        {
            var grouping = AccountGrouping.Start(Own, "Konten");

            grouping.Add(Shared, "  Gemeinsam ");

            Assert.Equal("Gemeinsam", grouping.Groups[1].Name);
        }

        [Fact]
        public void Refuses_A_Blank_Name()
        {
            var grouping = AccountGrouping.Start(Own, "Konten");

            var outcome = grouping.Add(Shared, "  ");

            Assert.Equal(AccountGroupingOutcome.NameMissing, outcome);
            Assert.Single(grouping.Groups);
        }
    }

    public sealed class Rename
    {
        [Fact]
        public void Changes_The_Name()
        {
            var grouping = ThreeGroups();

            grouping.Rename(Shared, "Mit Anna");

            Assert.Equal("Mit Anna", grouping.Groups[1].Name);
        }

        [Fact]
        public void Refuses_A_Blank_Name()
        {
            var grouping = ThreeGroups();

            var outcome = grouping.Rename(Shared, "");

            Assert.Equal(AccountGroupingOutcome.NameMissing, outcome);
            Assert.Equal("Gemeinsam", grouping.Groups[1].Name);
        }

        [Fact]
        public void Reports_An_Unknown_Group()
        {
            var outcome = ThreeGroups().Rename(Guid.NewGuid(), "Neu");

            Assert.Equal(AccountGroupingOutcome.GroupNotFound, outcome);
        }
    }

    public sealed class MoveUp
    {
        [Fact]
        public void Swaps_The_Group_With_The_One_Above()
        {
            var grouping = ThreeGroups();

            grouping.MoveUp(Other);

            Assert.Equal([Own, Other, Shared], Order(grouping));
        }

        [Fact]
        public void Leaves_The_Topmost_Group_In_Place()
        {
            var grouping = ThreeGroups();

            var outcome = grouping.MoveUp(Own);

            Assert.Equal(AccountGroupingOutcome.Done, outcome);
            Assert.Equal([Own, Shared, Other], Order(grouping));
        }
    }

    public sealed class MoveDown
    {
        [Fact]
        public void Swaps_The_Group_With_The_One_Below()
        {
            var grouping = ThreeGroups();

            grouping.MoveDown(Own);

            Assert.Equal([Shared, Own, Other], Order(grouping));
        }

        [Fact]
        public void Leaves_The_Bottom_Group_In_Place()
        {
            var grouping = ThreeGroups();

            grouping.MoveDown(Other);

            Assert.Equal([Own, Shared, Other], Order(grouping));
        }

        [Fact]
        public void Makes_The_Moved_Group_Receive_New_Accounts_When_It_Becomes_Topmost()
        {
            var grouping = ThreeGroups();
            grouping.MoveDown(Own);

            grouping.Place([Giro, Savings, Card, "DE12500105170648489890"]);

            Assert.Equal([Card, "DE12500105170648489890"], grouping.AccountsIn(Shared));
        }
    }

    public sealed class Delete
    {
        [Fact]
        public void Removes_An_Empty_Group()
        {
            var grouping = ThreeGroups();

            var outcome = grouping.Delete(Other);

            Assert.Equal(AccountGroupingOutcome.Done, outcome);
            Assert.Equal([Own, Shared], Order(grouping));
        }

        [Fact]
        public void Refuses_A_Group_That_Shows_An_Account()
        {
            var grouping = ThreeGroups();

            var outcome = grouping.Delete(Shared);

            Assert.Equal(AccountGroupingOutcome.GroupNotEmpty, outcome);
            Assert.Equal(3, grouping.Groups.Count);
        }

        [Fact]
        public void Refuses_The_Last_Group()
        {
            var grouping = AccountGrouping.Start(Own, "Konten");

            var outcome = grouping.Delete(Own);

            Assert.Equal(AccountGroupingOutcome.LastGroup, outcome);
            Assert.Single(grouping.Groups);
        }

        [Fact]
        public void Removes_A_Group_Whose_Accounts_Are_All_Hidden()
        {
            var grouping = ThreeGroups();
            grouping.Place([Giro, Savings]);

            var outcome = grouping.Delete(Shared);

            Assert.Equal(AccountGroupingOutcome.Done, outcome);
            Assert.DoesNotContain(grouping.Assignments, assignment => assignment.AccountReference == Card);
        }

        [Fact]
        public void Reports_An_Unknown_Group()
        {
            var outcome = ThreeGroups().Delete(Guid.NewGuid());

            Assert.Equal(AccountGroupingOutcome.GroupNotFound, outcome);
        }
    }

    public sealed class Assign
    {
        [Fact]
        public void Appends_The_Account_To_The_Target_Group()
        {
            var grouping = ThreeGroups();

            grouping.Assign(Giro, Shared);

            Assert.Equal([Card, Giro], grouping.AccountsIn(Shared));
            Assert.Equal([Savings], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Keeps_The_Place_Of_An_Account_Already_In_The_Group()
        {
            var grouping = ThreeGroups();

            grouping.Assign(Giro, Own);

            Assert.Equal([Giro, Savings], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Reports_A_Hidden_Account()
        {
            var grouping = ThreeGroups();
            grouping.Place([Savings, Card]);

            var outcome = grouping.Assign(Giro, Shared);

            Assert.Equal(AccountGroupingOutcome.AccountNotFound, outcome);
        }

        [Fact]
        public void Reports_An_Unknown_Group()
        {
            var outcome = ThreeGroups().Assign(Giro, Guid.NewGuid());

            Assert.Equal(AccountGroupingOutcome.GroupNotFound, outcome);
        }
    }

    public sealed class MoveAccountUp
    {
        [Fact]
        public void Swaps_The_Account_With_The_One_Above()
        {
            var grouping = ThreeGroups();

            grouping.MoveAccountUp(Savings);

            Assert.Equal([Savings, Giro], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Leaves_The_Topmost_Account_In_Place()
        {
            var grouping = ThreeGroups();

            grouping.MoveAccountUp(Giro);

            Assert.Equal([Giro, Savings], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Skips_A_Hidden_Account()
        {
            var grouping = new AccountGrouping(
                [new AccountGroup(Own, "Eigene", 0)],
                [new AccountAssignment(Giro, Own, 0), new AccountAssignment(Card, Own, 1), new AccountAssignment(Savings, Own, 2)]);
            grouping.Place([Giro, Savings]);

            grouping.MoveAccountUp(Savings);

            Assert.Equal([Savings, Giro], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Reports_An_Unknown_Account()
        {
            var outcome = ThreeGroups().MoveAccountUp("DE12500105170648489890");

            Assert.Equal(AccountGroupingOutcome.AccountNotFound, outcome);
        }
    }

    public sealed class MoveAccountDown
    {
        [Fact]
        public void Swaps_The_Account_With_The_One_Below()
        {
            var grouping = ThreeGroups();

            grouping.MoveAccountDown(Giro);

            Assert.Equal([Savings, Giro], grouping.AccountsIn(Own));
        }

        [Fact]
        public void Leaves_The_Bottom_Account_In_Place()
        {
            var grouping = ThreeGroups();

            grouping.MoveAccountDown(Savings);

            Assert.Equal([Giro, Savings], grouping.AccountsIn(Own));
        }
    }

    public sealed class Assignments
    {
        [Fact]
        public void Number_The_Accounts_Of_Each_Group_From_Zero()
        {
            var grouping = ThreeGroups();
            grouping.MoveAccountUp(Savings);

            var positions = grouping.Assignments.ToDictionary(assignment => assignment.AccountReference, assignment => assignment.Position);

            Assert.Equal(0, positions[Savings]);
            Assert.Equal(1, positions[Giro]);
            Assert.Equal(0, positions[Card]);
        }
    }
}
