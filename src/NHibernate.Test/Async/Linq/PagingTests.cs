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

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;

	[TestFixture]
	public class PagingTestsAsync : LinqTestCase
	{
		[Test]
		public async Task PageBetweenProjectionsAsync()
		{
			// NH-3326
			var list = await (db.Products
						 .Select(p => new { p.ProductId, p.Name })
						 .Skip(5).Take(10)
						 .Select(a => new { a.Name, a.ProductId })
						 .ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningNestedAnonymousAsync()
		{
			// The important part in this query is that the outer select
			// grabs the entire element from the inner select, plus more.

			// NH-3326
			var list = await (db.Products
							.Select(p => new { p.ProductId, p.Name })
							.Skip(5).Take(10)
							.Select(a => new { ExpandedElement = a, a.Name, a.ProductId })
							.ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningNestedClassAsync()
		{
			// NH-3326
			var list = await (db.Products
				.Select(p => new ProductProjection { ProductId = p.ProductId, Name = p.Name })
				.Skip(5).Take(10)
				.Select(a => new { ExpandedElement = a, a.Name, a.ProductId })
				.ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedNestedAnonymousAsync()
		{
			// Variation of NH-3326 with order
			var list = await (db.Products
				.Select(p => new { p.ProductId, p.Name })
				.OrderBy(x => x.ProductId)
				.Skip(5).Take(10)
				.Select(a => new { ExpandedElement = a, a.Name, a.ProductId })
				.ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedNestedClassAsync()
		{
			// Variation of NH-3326 with order
			var list = await (db.Products
				.Select(p => new ProductProjection { ProductId = p.ProductId, Name = p.Name })
				.OrderBy(x => x.ProductId)
				.Skip(5).Take(10)
				.Select(a => new { ExpandedElement = a, a.Name, a.ProductId })
				.ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedConstrainedNestedAnonymousAsync()
		{
			// Variation of NH-3326 with where
			var list = await (db.Products
				.Select(p => new { p.ProductId, p.Name })
				.Where(p => p.ProductId > 0)
				.OrderBy(x => x.ProductId)
				.Skip(5).Take(10)
				.Select(a => new { ExpandedElement = a, a.Name, a.ProductId })
				.ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task PageBetweenProjectionsReturningOrderedConstrainedNestedClassAsync()
		{
			// Variation of NH-3326 with where
			var list = await (db.Products
				.Select(p => new ProductProjection { ProductId = p.ProductId, Name = p.Name })
				.Where(p => p.ProductId > 0)
				.OrderBy(x => x.ProductId)
				.Skip(5).Take(10)
				.Select(a => new { ExpandedElement = a, a.Name, a.ProductId })
				.ToListAsync());

			Assert.That(list, Has.Count.EqualTo(10));
		}

		[Test]
		public async Task Customers1to5Async()
		{
			var q = (from c in db.Customers select c.CustomerId).Take(5);
			var query = await (q.ToListAsync());

			Assert.AreEqual(5, query.Count);
		}

		[Test]
		public async Task Customers11to20Async()
		{
			var query = await ((from c in db.Customers
						 orderby c.CustomerId
						 select c.CustomerId).Skip(10).Take(10).ToListAsync());
			Assert.AreEqual("BSBEV", query[0]);
			Assert.AreEqual(10, query.Count);
		}

		[Test]
		public async Task Customers11to20And21to30ShouldNoCacheQueryAsync()
		{
			var query = await ((from c in db.Customers
							orderby c.CustomerId
							select c.CustomerId).Skip(10).Take(10).ToListAsync());
			Assert.AreEqual("BSBEV", query[0]);
			Assert.AreEqual(10, query.Count);

			query = await ((from c in db.Customers
						orderby c.CustomerId
						select c.CustomerId).Skip(20).Take(10).ToListAsync());
			Assert.AreNotEqual("BSBEV", query[0]);
			Assert.AreEqual(10, query.Count);

			query = await ((from c in db.Customers
						orderby c.CustomerId
						select c.CustomerId).Skip(10).Take(20).ToListAsync());
			Assert.AreEqual("BSBEV", query[0]);
			Assert.AreEqual(20, query.Count);
		}

		[Test]
		public async Task OrderedPagedProductsWithOuterProjectionAsync()
		{
			//NH-3108
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderBy(p => p.ProductId)
				.Skip(10).Take(20)
				.Select(p => p.ProductId)
				.ToList();

			var ids = await (db.Products 
				.OrderBy(p => p.ProductId) 
				.Skip(10).Take(20) 
				.Select(p => p.ProductId) 
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task OrderedPagedProductsWithInnerProjectionAsync()
		{
			//NH-3108 (not failing)
			var inMemoryIds = (await (db.Products.ToListAsync())) 
				.OrderBy(p => p.ProductId) 
				.Select(p => p.ProductId)
				.Skip(10).Take(20)
				.ToList();

			var ids = await (db.Products 
				.OrderBy(p => p.ProductId) 
				.Select(p => p.ProductId)
				.Skip(10).Take(20)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task DescendingOrderedPagedProductsWithOuterProjectionAsync()
		{
			//NH-3108
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(p => p.ProductId)
				.Skip(10).Take(20)
				.Select(p => p.ProductId)
				.ToList();

			var ids = await (db.Products
				.OrderByDescending(p => p.ProductId) 
				.Skip(10).Take(20) 
				.Select(p => p.ProductId) 
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task DescendingOrderedPagedProductsWithInnerProjectionAsync()
		{
			//NH-3108 (not failing)
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(p => p.ProductId) 
				.Select(p => p.ProductId)
				.Skip(10).Take(20)
				.ToList();

			var ids = await (db.Products
				.OrderByDescending(p => p.ProductId) 
				.Select(p => p.ProductId)
				.Skip(10).Take(20)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");

			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.ToList();

			var ids = await (db.Products
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseResortAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");

			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.OrderBy(x => x.Name)
				.ToList();

			var ids = await (db.Products
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.OrderBy(x => x.Name)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithInnerAndOuterWhereClausesAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");

			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.Where(x => x.UnitsInStock < 100)
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.OrderBy(x => x.Name)
				.ToList();

			var ids = await (db.Products
				.Where(x => x.UnitsInStock < 100)
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.OrderBy(x => x.Name)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseEquivalentAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");

			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.ToList();

			var subquery = db.Products
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20);

			var ids = await (db.Products
				.Where(x => subquery.Contains(x))
				.Where(x => x.UnitsInStock > 0)
				.OrderByDescending(x => x.ProductId)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}

		[Test]
		public async Task PagedProductsWithOuterWhereClauseAndProjectionAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");

			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.Select(x => x.ProductId)
				.ToList();

			var ids = await (db.Products
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.Select(x => x.ProductId)
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}
		
		[Test]
		public async Task PagedProductsWithOuterWhereClauseAndComplexProjectionAsync()
		{
			if (Dialect is MySQLDialect)
				Assert.Ignore("MySQL does not support LIMIT in subqueries.");

			//NH-2588
			var inMemoryIds = (await (db.Products.ToListAsync()))
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.Select(x => new { x.ProductId })
				.ToList();

			var ids = await (db.Products
				.OrderByDescending(x => x.ProductId)
				.Skip(10).Take(20)
				.Where(x => x.UnitsInStock > 0)
				.Select(x => new { x.ProductId })
				.ToListAsync());

			Assert.That(ids, Is.EqualTo(inMemoryIds));
		}
	}
}
