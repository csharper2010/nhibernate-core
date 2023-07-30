using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3334
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			using var session = OpenSession();
			using var t = session.BeginTransaction();
			var parent = new Entity
			{
				Name = "Parent1",
				Children = { new ChildEntity { Name = "Child", Child = new GrandChildEntity { Name = "GrandChild" } } }
			};
			session.Save(parent);
			parent = new Entity
			{
				Name = "Parent2",
				Children = { new ChildEntity { Name = "Child", Child = new GrandChildEntity { Name = "XGrandChild" } } }
			};
			var other = new OtherEntity { Name = "ABC", Entities = {parent}};
			parent.OtherEntity = other;
			session.Save(parent);
			session.Save(other);
			t.Commit();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from ChildEntity").ExecuteUpdate();
			session.CreateQuery("delete from GrandChildEntity").ExecuteUpdate();
			session.CreateQuery("delete from Entity").ExecuteUpdate();
			session.CreateQuery("delete from OtherEntity").ExecuteUpdate();

			transaction.Commit();
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsCorrelatedColumnsInSubselectJoin;
		}

		public class TestCaseItem
		{
			public string Name { get; }
			public string Hql { get; }
			public int LineNumber { get; }

			public TestCaseItem(string name, string hql, [CallerLineNumber] int lineNumber = 0)
			{
				Name = name;
				Hql = hql;
				LineNumber = lineNumber;
			}

			public override string ToString() => $"{LineNumber:0000}: {Name}";
		}

		public static IEnumerable<TestCaseItem> GetNoExceptionOnExecuteQueryTestCases()
		{
			/* does not work because of inner join or theta join created for many-to-one
				@"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ELEMENTS(ROOT.Children) AS child
						WHERE
							child.Child.Name like 'G%'
							OR ROOT.OtherEntity.Name like 'A%'
						)");*/

			yield return new("Basic Elements case 1 FoundViaGrandChildG", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ELEMENTS(ROOT.Children) AS child
							LEFT JOIN child.Child AS grandChild
						WHERE
							grandChild.Name like 'G%'
						)");
			yield return new("Basic Elements case 2 FoundViaOtherEntityA", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ELEMENTS(ROOT.OtherEntity) AS otherEntity
						WHERE
							otherEntity.Name like 'A%'
						)");
			yield return new("HQL Elements FoundViaGrandChildG", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ELEMENTS(ROOT.Children) AS child
							LEFT JOIN child.Child AS grandChild
							LEFT JOIN ROOT.OtherEntity AS otherEntity
						WHERE
							grandChild.Name like 'G%'
							OR otherEntity.Name like 'G%'
						)");
			yield return new("HQL Elements FoundViaOtherEntityA", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ELEMENTS(ROOT.Children) AS child
							LEFT JOIN child.Child AS grandChild
							LEFT JOIN ROOT.OtherEntity AS otherEntity
						WHERE
							grandChild.Name like 'A%'
							OR otherEntity.Name like 'A%'
						)");
			yield return new("HQL Entity FoundViaGrandChildG", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ChildEntity AS child
							LEFT JOIN child.Child AS grandChild
							LEFT JOIN ROOT.OtherEntity AS otherEntity
						WHERE
							child.Parent = ROOT
							AND (
								grandChild.Name like 'G%'
								OR otherEntity.Name like 'G%'
							)
						)");
			yield return new("HQL Entity FoundViaOtherEntityA", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ChildEntity AS child
							LEFT JOIN child.Child AS grandChild
							LEFT JOIN ROOT.OtherEntity AS otherEntity
						WHERE
							child.Parent = ROOT
							AND (
								grandChild.Name like 'A%'
								OR otherEntity.Name like 'A%'
							)
						)");
			yield return new("FROM ROOT.Children FoundViaGrandChildG", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ROOT.Children AS child
							LEFT JOIN child.Child AS grandChild
						WHERE
							grandChild.Name like 'G%'
						)");
			yield return new("FROM ROOT.OtherEntity FoundViaOtherEntityA", @"
				SELECT ROOT 
				FROM Entity AS ROOT 
				WHERE
					EXISTS 
						(FROM ROOT.OtherEntity AS otherEntity 
							LEFT JOIN ChildEntity AS child ON child.Parent = ROOT 
							LEFT JOIN child.Child AS grandChild
						WHERE
								grandChild.Name like 'A%'
								OR otherEntity.Name like 'A%'
						)");
		}

		[Test, TestCaseSource(nameof(GetNoExceptionOnExecuteQueryTestCases))]
		public void NoExceptionOnExecuteQuery(TestCaseItem testCase)
		{
			using var session = OpenSession();
			var q = session.CreateQuery(testCase.Hql);
			Assert.That(q.List(), Has.Count.EqualTo(1));
		}

		protected override bool CheckDatabaseWasCleaned()
		{
			// same set of objects for each test
			return true;
		}
	}
}
