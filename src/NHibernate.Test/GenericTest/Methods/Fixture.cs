using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.DomainModel;
using NUnit.Framework;

namespace NHibernate.Test.GenericTest.Methods
{
	[TestFixture]
	public class Fixture : TestCase
	{
		protected override string[] Mappings
		{
			get
			{
				return new string[] { "One.hbm.xml", "Many.hbm.xml" };
			}
		}

		private One one;

		protected override void OnSetUp()
		{
			base.OnSetUp();

			// create the objects to search on		
			one = new One();
			one.X = 20;
			one.Manies = new HashSet<Many>();

			Many many1 = new Many();
			many1.X = 10;
			many1.One = one;
			one.Manies.Add( many1 );

			Many many2 = new Many();
			many2.X = 20;
			many2.One = one;
			one.Manies.Add( many2 );

			using( ISession s = OpenSession() )
			using( ITransaction t = s.BeginTransaction() )
			{
				s.Save( one );
				s.Save( many1 );
				s.Save( many2 );
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using( ISession session = OpenSession() )
			using( ITransaction tx = session.BeginTransaction() )
			{
				session.Delete( "from Many" );
				session.Delete( "from One" );
				tx.Commit();
			}
			base.OnTearDown();
		}

		[Test]
		public void Criteria()
		{
			using( ISession s2 = OpenSession() )
			using( ITransaction t2 = s2.BeginTransaction() )
			{
				IList<One> results2 = s2.CreateCriteria( typeof( One ) )
					.Add( Expression.Eq( "X", 20 ) )
					.List<One>();

				Assert.AreEqual( 1, results2.Count );

				One one2 = results2[ 0 ];

				Assert.IsNotNull( one2, "Unable to load object" );
				Assert.AreEqual( one.X, one2.X, "Load failed" );
			}
		}

		[Test]
		public void QueryList()
		{
			using( ISession s = OpenSession() )
			using( ITransaction t = s.BeginTransaction() )
			{
				IList<One> results = s.CreateQuery( "from One" ).List<One>();

				Assert.AreEqual( 1, results.Count );
			}
		}

		[Test]
		public void QueryEnumerable()
		{
			using( ISession s = OpenSession() )
			using( ITransaction t = s.BeginTransaction() )
			{
				IEnumerable<One> results = s.CreateQuery( "from One" ).Enumerable<One>();
				IEnumerator<One> en = results.GetEnumerator();

				Assert.IsTrue( en.MoveNext() );
				Assert.IsFalse( en.MoveNext() );
			}
		}

		[Test]
		public void Filter()
		{
			using( ISession s = OpenSession() )
			using( ITransaction t = s.BeginTransaction() )
			{
				One one2 = ( One ) s.CreateQuery( "from One" ).UniqueResult();
				IList<Many> results = s.CreateFilter( one2.Manies, "where X = 10" )
					.List<Many>();

				Assert.AreEqual( 1, results.Count );
				Assert.AreEqual( 10, results[ 0 ].X );
				t.Commit();
			}
		}

		[Test]
		public void FilterEnumerable()
		{
			using( ISession s = OpenSession() )
			using( ITransaction t = s.BeginTransaction() )
			{
				One one2 = ( One ) s.CreateQuery( "from One" ).UniqueResult();
				IEnumerable<Many> results = s.CreateFilter( one2.Manies, "where X = 10" )
					.Enumerable<Many>();
				IEnumerator<Many> en = results.GetEnumerator();

				Assert.IsTrue( en.MoveNext() );
				Assert.AreEqual( 10, en.Current.X );
				Assert.IsFalse( en.MoveNext() );
				t.Commit();
			}
		}
	}
}
