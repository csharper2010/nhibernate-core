﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq.ByMethod
{
	using System.Threading.Tasks;
	[TestFixture]
	public class OrderByTestsAsync : LinqTestCase
	{
		protected override void Configure(Cfg.Configuration configuration)
		{
			configuration.SetProperty(Environment.ShowSql, "true");
		}

		[Test]
		public async Task GroupByThenOrderByAsync()
		{
			var query = from c in db.Customers
						group c by c.Address.Country into g
						orderby g.Key
						select new { Country = g.Key, Count = g.Count() };

			var ids = await (query.ToListAsync());
			Assert.NotNull(ids);
			AssertOrderedBy.Ascending(ids, arg => arg.Country);
		}

		[Test]
		public async Task AscendingOrderByClauseAsync()
		{
			var query = from c in db.Customers
						orderby c.CustomerId
						select c.CustomerId;

			var ids = await (query.ToListAsync());

			if (ids.Count > 1)
			{
				Assert.Greater(ids[1], ids[0]);
			}
		}

		[Test]
		public async Task DescendingOrderByClauseAsync()
		{
			var query = from c in db.Customers
						orderby c.CustomerId descending
						select c.CustomerId;

			var ids = await (query.ToListAsync());

			if (ids.Count > 1)
			{
				Assert.Greater(ids[0], ids[1]);
			}
		}

		[Test]
		public async Task OrderByCalculatedAggregatedSubselectPropertyAsync()
		{
			if (!TestDialect.SupportsAggregatingScalarSubSelectsInOrderBy)
				Assert.Ignore("Dialect does not support aggregating scalar sub-selects in order by");

			//NH-2781
			var result = await (db.Orders
				.Select(o => new
								 {
									 o.OrderId,
									 TotalQuantity = o.OrderLines.Sum(c => c.Quantity)
								 })
				.OrderBy(s => s.TotalQuantity)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(830));

			AssertOrderedBy.Ascending(result, s => s.TotalQuantity);
		}

		[Test]
		public async Task AggregateAscendingOrderByClauseAsync()
		{
			if (!TestDialect.SupportsAggregatingScalarSubSelectsInOrderBy)
				Assert.Ignore("Dialect does not support aggregating scalar sub-selects in order by");

			var query = from c in db.Customers
						orderby c.Orders.Count
						select c;

			var customers = await (query.ToListAsync());

			// Verify ordering for first 10 customers - to avoid loading all orders.
			AssertOrderedBy.Ascending(customers.Take(10).ToList(), customer => customer.Orders.Count);
		}

		[Test]
		public async Task AggregateDescendingOrderByClauseAsync()
		{
			if (!TestDialect.SupportsAggregatingScalarSubSelectsInOrderBy)
				Assert.Ignore("Dialect does not support aggregating scalar sub-selects in order by");

			var query = from c in db.Customers
						orderby c.Orders.Count descending
						select c;

			var customers = await (query.ToListAsync());

			// Verify ordering for first 10 customers - to avoid loading all orders.
			AssertOrderedBy.Descending(customers.Take(10).ToList(), customer => customer.Orders.Count);
		}

		[Test]
		public async Task ComplexAscendingOrderByClauseAsync()
		{
			var query = from c in db.Customers
						where c.Address.Country == "Belgium"
						orderby c.Address.Country, c.Address.City
						select c.Address.City;

			var ids = await (query.ToListAsync());

			if (ids.Count > 1)
			{
				Assert.Greater(ids[1], ids[0]);
			}
		}

		[Test]
		public async Task ComplexDescendingOrderByClauseAsync()
		{
			var query = from c in db.Customers
						where c.Address.Country == "Belgium"
						orderby c.Address.Country descending, c.Address.City descending
						select c.Address.City;

			var ids = await (query.ToListAsync());

			if (ids.Count > 1)
			{
				Assert.Greater(ids[0], ids[1]);
			}
		}

		[Test]
		public async Task ComplexAscendingDescendingOrderByClauseAsync()
		{
			var query = from c in db.Customers
						where c.Address.Country == "Belgium"
						orderby c.Address.Country ascending, c.Address.City descending
						select c.Address.City;

			var ids = await (query.ToListAsync());

			if (ids.Count > 1)
			{
				Assert.Greater(ids[0], ids[1]);
			}
		}

		[Test]
		public async Task OrderByDoesNotFilterResultsOnJoinAsync()
		{
			// Check preconditions.
			var allAnimalsWithNullFather = from a in db.Animals where a.Father == null select a;
			Assert.Greater(await (allAnimalsWithNullFather.CountAsync()), 0);
			// Check join result.
			var allAnimals = db.Animals;
			var orderedAnimals = from a in db.Animals orderby a.Father.SerialNumber select a;
			// ReSharper disable RemoveToList.2
			// We to ToList() first or it skips the generation of the joins.
			Assert.AreEqual((await (allAnimals.ToListAsync())).Count(), (await (orderedAnimals.ToListAsync())).Count());
			// ReSharper restore RemoveToList.2
		}

		[Test]
		public async Task OrderByWithSelfReferencedSubquery1Async()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore("Dialect does not support scalar sub-selects");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			if (Dialect is Oracle8iDialect)
				Assert.Ignore("On Oracle this generates a correlated subquery two levels deep which isn't supported until Oracle 10g.");

			//NH-3044
			var result = await ((from order in db.Orders
						  where order == db.Orders.OrderByDescending(x => x.OrderDate).First(x => x.Customer == order.Customer)
						  orderby order.Customer.CustomerId
						  select order).ToListAsync());

			AssertOrderedBy.Ascending(result.Take(5).ToList(), x => x.Customer.CustomerId);
		}

		[Test]
		public async Task OrderByWithSelfReferencedSubquery2Async()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore("Dialect does not support scalar sub-selects");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			if (Dialect is Oracle8iDialect)
				Assert.Ignore("On Oracle this generates a correlated subquery two levels deep which isn't supported until Oracle 10g.");

			//NH-3044
			var result = await ((from order in db.Orders
						  where order == db.Orders.OrderByDescending(x => x.OrderDate).First(x => x.Customer == order.Customer)
						  orderby order.ShippingDate descending
						  select order).ToListAsync());

			// Different databases may sort null either first or last.
			// We only bother about the non-null values here.
			result = result.Where(x => x.ShippingDate != null).ToList();

			AssertOrderedBy.Descending(result.Take(5).ToList(), x => x.ShippingDate);
		}

		[Test(Description = "NH-3217")]
		public async Task OrderByNullCompareAndSkipAndTakeAsync()
		{
			await (db.Orders.OrderBy(o => o.Shipper == null ? 0 : o.Shipper.ShipperId).Skip(3).Take(4).ToListAsync());
		}

		[Test(Description = "NH-3445"), KnownBug("NH-3445")]
		public async Task OrderByWithSelectDistinctAndTakeAsync()
		{
			await (db.Orders.Select(o => o.ShippedTo).Distinct().OrderBy(o => o).Take(1000).ToListAsync());
		}
		
		[Test]
		public async Task BooleanOrderByDescendingClauseAsync()
		{
			var query = from c in db.Customers
			            orderby c.Address.Country == "Belgium" descending, c.Address.Country
			            select c;

			var customers = await (query.ToListAsync());
			if (customers.Count > 1)
			{
				Assert.That(customers[0].Address.Country, Is.EqualTo("Belgium"));
			}
		}
	}
}
