using CS4760GrantApplication.Controllers;
using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS4760GrantApplicationTest
{
    [TestClass]
    public class ReviewGrantTest
    {
        private ReviewController _controller;

        private CS4760GrantApplicationContext _context;

        private readonly IWebHostEnvironment _environment;

        // A dummy test session so that I can hardcode the session gets
        public class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();

            public string Id => Guid.NewGuid().ToString();
            public bool IsAvailable => true;
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear() => _store.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Remove(string key) => _store.Remove(key);

            public void Set(string key, byte[] value) => _store[key] = value;

            public bool TryGetValue(string key, out byte[] value)
                => _store.TryGetValue(key, out value);
        }

        [TestInitialize]
        public void SetUp()
        {

            // Setup session so that HttpContext.Session.GetInt32() will return 1
            var session = new TestSession();
            session.SetInt32("UserID", 1);

            // Setup the _context and _controller
            var options = new DbContextOptionsBuilder<CS4760GrantApplicationContext>()
                .UseInMemoryDatabase(databaseName: "ReviewTestingDb")
                .Options;

            _context = new CS4760GrantApplicationContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _controller = new ReviewController(_context, _environment);

            var httpContext = new DefaultHttpContext
            {
                Session = session
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task CreateReview_IsValid()
        {
            // Arrange
            var grant = new CS4760GrantApplication.Models.Grant
            {
                Title = "Dummy",
                Description = "Dummy",
                ProjectSummary = "Dummy",
                Justification = "Dummy",
                ProjectImpact = 1,
                ProjectTimeline = "Dummy",
            };

            _context.Grants.Add(grant);
            await _context.SaveChangesAsync();

            var user = new CS4760GrantApplication.Models.User
            {
                FirstName = "Dummy",
                LastName = "Dummy",
                Email = "Dummy@mail.com",
                PasswordHash = "DummyHash"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var review = new CS4760GrantApplication.Models.Review
            {
                AverageScore = 97.2M,
                Notes = "Testing CreateReview",
                GrantId = 1 
            };

            // Act
            var result = await _controller.CreateReview(review);

            // Assert
            Assert.AreEqual(review.AverageScore, _context.Reveiws.Where(r => r.Id == 1).Select(r => r.AverageScore).SingleOrDefault());
            Assert.AreEqual(review.Notes, _context.Reveiws.Where(r => r.Id == 1).Select(r => r.Notes).SingleOrDefault());
            Assert.AreEqual(grant.Id, _context.Reveiws.Where(r => r.Id == 1).Select(r => r.GrantId).SingleOrDefault());
            Assert.AreEqual(user.Id, _context.Reveiws.Where(r => r.Id == 1).Select(r => r.UserId).SingleOrDefault());

        }

    }
}
