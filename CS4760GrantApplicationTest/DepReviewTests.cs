using System;
using System.Collections.Generic;
using System.Text;
using CS4760GrantApplication.Controllers;
using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CS4760GrantApplicationTest
{
    [TestClass]
    public class DepReviewTests
    {
        private GrantController _controller;
        private CS4760GrantApplicationContext _context;
        private IWebHostEnvironment _environment;

        [TestInitialize]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CS4760GrantApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestingDb")
                .Options;

            _context = new CS4760GrantApplicationContext(options);

            _environment = new Mock<IWebHostEnvironment>().Object;

            _context.Database.EnsureDeleted();

            _controller = new GrantController(_context, _environment);
        }

        [TestMethod]
        public async Task DepApprove()
        {
            // Create a dummy grant
            var grant = new CS4760GrantApplication.Models.Grant
            {
                Id = 1,
                Title = "Test Title",
                Description = "Test Description",
                ProjectSummary = "Test Project Summary",
                Justification = "Test Justification",
                ProjectImpact = 1,
                ProjectTimeline = "Test Project Timeline",
                SuccessEvaluation = "Test Success Evaluation",
                Signature = "John Doe",
                IsSaved = false
            };
            _context.Grants.Add(grant);
            await _context.SaveChangesAsync();

            // DeptApprove takes a grant id and a string of notes
            int id = 1;
            string deptReviewNotes = "These are notes";

            // Try to approve the grant
            await _controller.DeptApprove(id, deptReviewNotes);

            // Get approved grant
            var approvedGrant = _context.Grants.FirstOrDefault(g => g.Id == id);

            // Grant should have department review notes
            Assert.IsNotNull(approvedGrant, "Grant was not found.");
            Assert.IsNotNull(approvedGrant.DeptReviewNotes, "No review notes");
            // Grant's Status array should contain Approved by department chair
            Assert.Contains(GrantStatus.ApprovedByDeptChair, approvedGrant.Statuses, "Grant not approved by department");

        }

        [TestMethod]
        public async Task DeptApprove_InvalidGrantId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 999;
            string deptReviewNotes = "These are notes";

            // Act
            var result = await _controller.DeptApprove(invalidId, deptReviewNotes);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DeptReject_ValidGrant_AddsRejectedStatusAndSavesNotes()
        {
            // Arrange
            var grant = new Grant
            {
                Id = 2,
                Title = "Rejected Grant",
                Description = "Test Description",
                ProjectSummary = "Test Project Summary",
                Justification = "Test Justification",
                ProjectImpact = 1,
                ProjectTimeline = "Test Project Timeline",
                SuccessEvaluation = "Test Success Evaluation",
                Signature = "John Doe",
                IsSaved = false
            };

            _context.Grants.Add(grant);
            await _context.SaveChangesAsync();

            string deptReviewNotes = "The department rejected this grant.";

            // Act
            var result = await _controller.DeptReject(grant.Id, deptReviewNotes);

            // Assert
            var rejectedGrant = await _context.Grants.FindAsync(grant.Id);

            Assert.IsNotNull(rejectedGrant);
            Assert.Contains(
                GrantStatus.RejectedByDeptChair,
                rejectedGrant.Statuses,
                "The rejected department-chair status was not added."
            );
            Assert.AreEqual(
                deptReviewNotes,
                rejectedGrant.DeptReviewNotes,
                "The department review notes were not saved correctly."
            );

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;

            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("Dashboard", redirectResult.ControllerName);
        }
    }
    
}
