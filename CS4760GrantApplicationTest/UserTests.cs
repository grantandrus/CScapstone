using CS4760GrantApplication.Controllers;
using CS4760GrantApplication.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CS4760GrantApplicationTest
{
    [TestClass]
    public class UserTests
    {
        private UsersController _controller;
        // this isn't going to be the actual database context just an in-memory version for testing
        private CS4760GrantApplicationContext _context;

        [TestInitialize] // this means this will run before any test is run.
        public void SetUp()
        {
            // all tests will probably need to use the controller and the context, so
            // I set them up here to avoid repeated code.

            // configure the context to use an in-memory database
            var options = new DbContextOptionsBuilder<CS4760GrantApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestingDb")
                .Options;

            _context = new CS4760GrantApplicationContext(options);

            // clear the database to ensure a clean slate before each test
            _context.Database.EnsureDeleted();

            _controller = new UsersController(_context);
        }

        // Unit tests have three steps: Arrange, Act, Assert
        [TestMethod]
        public async Task ValidUser_IsCreatedAsync() // the name of the test should be pretty descriptive of what the test does.
        {
            // 1. ARRANGE: this is the setup for the test. It's the "dummy data"
            // that's used to see if the code is working right.
            // So in this case since I'm testing that a valid user can be
            // created, the dummy data will be set up as a user that we would
            // expect to be valid.

            // the method we want to test in this unit test (the Create method
            // from the UsersController) takes a RegisterViewModel, so that is 
            // thetype of object we'll create with the valid dummy data.
            var user = new CS4760GrantApplication.ViewModels.RegisterViewModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "janedoe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
            };

            // 2. ACT: this is where we use the code from the main project with
            // the dummy data

            // call the Create method from the UsersController with the dummy data
            await _controller.Create(user);

            // 3. ASSERT: this is when we verify that the setup and call actually
            // did what we expected it to do. So here we want to make sure that
            // the user was actually created and created as we would expect it
            // to be.
            
            // find a user in the fake database that matches the email of the dummy
            // user. 
            var createdUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

            // Assert has lots of methods that test certain conditions given 
            // what we pass to it. This is making sure the user we found in the 
            // previous step was actually there and is not null. If it is null,
            // the test fails and the message we pass is displayed in the test result
            Assert.IsNotNull(createdUser, "User was not created.");

            // I'm also going to test that the name of the user created matches the 
            // name of the dummy user we set up
            Assert.AreEqual(user.FirstName, createdUser.FirstName, "First name does not match.");
            Assert.AreEqual(user.LastName, createdUser.LastName, "Last name does not match.");

            /*
             * And that's it! Then you just right click and select "Run Tests".
             * 
             * You'll want to make sure all the tests pass, so even if you're checking
             * that something fails (like maybe checking that a user with an invalid email
             * is not created), the test itself should be set up to pass.
             * 
             * Hope my notes helped :)
             * 
             */
        }
    }
    
}
