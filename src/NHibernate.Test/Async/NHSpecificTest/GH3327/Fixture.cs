﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3327
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			Sfi.Statistics.IsStatisticsEnabled = true;
		}

		protected override void OnTearDown()
		{
			Sfi.Statistics.IsStatisticsEnabled = false;
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from ChildEntity").ExecuteUpdate();
				session.CreateQuery("delete from Entity").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public async Task NotIsCorrectlyHandledAsync()
		{
			using (var session = OpenSession())
			using (var t = session.BeginTransaction())
			{
				var parent = new Entity { Name = "Parent" };
				var child = new ChildEntity { Name = "Child", Parent = parent };
				await (session.SaveAsync(parent));
				await (session.SaveAsync(child));
				await (t.CommitAsync());
			}

			using (var session = OpenSession())
			using (var _ = session.BeginTransaction())
			{
				var q = session.CreateQuery(
					@"SELECT COUNT(ROOT.Id)
					FROM Entity AS ROOT
					WHERE (
						EXISTS (FROM ChildEntity AS CHILD WHERE CHILD.Parent = ROOT)
						AND ROOT.Name = 'Parent'
					)");
				Assert.That((await (q.ListAsync()))[0], Is.EqualTo(1));

				q = session.CreateQuery(
					@"SELECT COUNT(ROOT.Id)
					FROM Entity AS ROOT
					WHERE NOT (
						EXISTS (FROM ChildEntity AS CHILD WHERE CHILD.Parent = ROOT)
						AND ROOT.Name = 'Parent'
					)");
				Assert.That((await (q.ListAsync()))[0], Is.EqualTo(0));
			}
		}
	}
}
