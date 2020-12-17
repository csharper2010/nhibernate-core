﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Criterion;
using NHibernate.DomainModel;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.NHSpecificTest.GH2463
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : TestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return Dialect.SupportsScalarSubSelects;
		}

		protected override string[] Mappings
		{
			get { return new[] {"ABC.hbm.xml"}; }
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var a = new A {Name = "A", AnotherName = "X"};
				session.Save(a);

				var b = new B {Name = "B", AnotherName = "X"};
				session.Save(b);

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();

				transaction.Commit();
			}
		}

		//Also see GH-2599
		[Test]
		public async Task CanJoinOnEntityWithDiscriminatorLinqAsync()
		{
			using (var s = OpenSession())
			{
				var list = await (s.Query<A>().Join(
					s.Query<B>(),
					a => a.AnotherName,
					b => b.AnotherName,
					(a, b) =>
						new {a, b}).ToListAsync());
			}
		}

		[Test]
		public async Task CanJoinOnEntityWithDiscriminatorQueryOverAsync()
		{
			using (var s = OpenSession())
			{
				A a = null;
				B b = null;
				var list = await (s.QueryOver<A>(() => a)
							.JoinEntityAlias(() => b, () => a.AnotherName == b.AnotherName)
							.Select((x) => a.AsEntity(), (x) => b.AsEntity()).ListAsync<object[]>());
			}
		}
	}
}