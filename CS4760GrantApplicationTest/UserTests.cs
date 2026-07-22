using CS4760GrantApplication.Controllers;
using CS4760GrantApplication.Data;
using CS4760GrantApplication.Models;
using CS4760GrantApplication.ViewModels;
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
                .UseInMemoryDatabase(databaseName: $"TestingDb_{Guid.NewGuid()}")
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

        [TestMethod]
        public async Task InvalidUser_DuplicateEmail_IsNotCreatedAsync()
        {
            // arrange by creating a user and then another user with the same email
            var existingUser = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "janedoe@example.com",
                PasswordHash = "InitialHash"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var registerUser = new RegisterViewModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "janedoe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // act
            var result = await _controller.Create(registerUser) as ViewResult;

            // assert that a veiw is being returned, the error message appears, and there's still only one user
            Assert.IsNotNull(result, "Action did not return a ViewResult.");

            Assert.AreEqual("An account with that email already exists.", _controller.ViewBag.Error);

            var usersCount = _context.Users.Count();
            Assert.AreEqual(1, usersCount, "A new user was improperly created in the database.");
        }

        [TestMethod]
        public async Task InvalidUser_CannotLogInAsync()
        {
            // arrange by making up a user (not saved to the database)
            var email = "nonexistent@example.com";
            var password = "WrongPassword123!";

            // act
            var result = await _controller.Login(email, password) as ViewResult;

            // assert that we return back to the Login view with an error message
            Assert.IsNotNull(result, "Action did not return a ViewResult.");
            Assert.AreEqual("Invalid email or password.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public async Task ValidUser_CanLogInAsync()
        {
            // arrange
            // fake session so HttpContext.Session doesn't throw NullReferenceException
            var mockSession = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = mockSession.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // add a new user + hash their password to save since the login will check against the hash
            var hasher = new PasswordHasher<User>();
            var testUser = new User
            {
                FirstName = "Alice",
                LastName = "Wonderland",
                Email = "alice@example.com",
                IsAdmin = false
            };
            testUser.PasswordHash = hasher.HashPassword(testUser, "CorrectPassword1!");

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // act
            var result = await _controller.Login(testUser.Email, "CorrectPassword1!") as RedirectToActionResult;

            // assert view is returned and the correct action takes place
            Assert.IsNotNull(result, "Login did not redirect correctly on valid credentials.");
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);
        }
    }

}
