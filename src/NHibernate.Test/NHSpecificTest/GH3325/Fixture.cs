﻿using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3325
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from ChildEntity").ExecuteUpdate();
				session.CreateQuery("delete from System.Object").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void CanAddChildAfterFlush()
		{
			Guid parentId;
			Guid childId;
			using (var session = OpenSession())
			using (var t = session.BeginTransaction())
			{
				var parent = new Entity { Name = "Parent" };
				var child = new ChildEntity { Name = "Child" };
				var parentChildren = parent.Children;
				parentChildren.Add(child);
				session.Save(parent);
				parentChildren.Remove(child);
				parentId = parent.Id;
				childId = child.Id;
				t.Commit();
			}

			using (var session = OpenSession())
			{
				var parent = session.Get<Entity>(parentId);
				Assert.That(parent, Is.Not.Null);
				Assert.That(parent.Children, Has.Count.EqualTo(0));

				var child = session.Get<ChildEntity>(childId);
				Assert.That(child, Is.Null);
			}
		}
	}
}
