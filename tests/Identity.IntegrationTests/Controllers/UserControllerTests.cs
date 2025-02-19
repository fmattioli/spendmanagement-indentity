﻿using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using SpendManagement.Identity.Application.Requests;
using SpendManagement.Identity.Application.Responses;
using SpendManagement.Identity.IntegrationTests.Fixtures;
using System.Net;

namespace SpendManagement.Identity.IntegrationTests.Controllers
{
    [Collection(nameof(SharedFixtureCollection))]
    public class UserControllerTests(HttpFixture httpFixture)
    {
        private readonly HttpFixture _httpFixture = httpFixture;
        private readonly Fixture _fixture = new();

        [Fact]
        public async Task GivenAValidSignUpUserRequest_NewUserShouldBeCreated()
        {
            //Arrange
            var password = _fixture.Create<string>();
            var email = _fixture.Create<string>();
            var signUpUserRequest = _fixture
                .Build<SignUpUserRequest>()
                .With(x => x.Email, $"{email}@test.com")
                .With(x => x.Password, $"{password}cAb!")
                .With(x => x.PasswordConfirmation, $"{password}cAb!")
                .Create();

            //Act
            var (StatusCode, Content) = await _httpFixture.PostAsync("/signUp", signUpUserRequest);

            //AssertSuccess
            StatusCode.Should().Be(HttpStatusCode.Created);
            var userResponse = JsonConvert.DeserializeObject<UserResponse>(Content);
            userResponse?.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GivenAValidSignInUserRequest_NewLoginShoudBeDone()
        {
            //Arrange
            var password = _fixture.Create<string>();
            var email = _fixture.Create<string>();
            var signUpUserRequest = _fixture
                .Build<SignUpUserRequest>()
                .With(x => x.Email, $"{email}@test.com")
                .With(x => x.Password, $"{password}cAb!")
                .With(x => x.PasswordConfirmation, $"{password}cAb!")
                .Create();

            await _httpFixture.PostAsync("/signUp", signUpUserRequest);

            //Act
            var (StatusCode, Content) = await _httpFixture.PostAsync("/login", signUpUserRequest);
            Console.Write(Content);

            //AssertSuccess
            StatusCode.Should().Be(HttpStatusCode.OK);
            var userResponse = JsonConvert.DeserializeObject<UserLoginResponse>(Content);
            userResponse?.Success.Should().BeTrue();
            userResponse?.AccessToken.Should().NotBeNull();
            userResponse?.RefreshToken.Should().NotBeNull();
        }

        [Fact]
        public async Task GivenAValidAddUserInClaimRequest_NewClaimShoudBeCreated()
        {
            //Arrange
            var password = _fixture.Create<string>();
            var email = _fixture.Create<string>();
            var signUpUserRequest = _fixture
                .Build<SignUpUserRequest>()
                .With(x => x.Email, $"{email}@test.com")
                .With(x => x.Password, $"{password}cAb!")
                .With(x => x.PasswordConfirmation, $"{password}cAb!")
                .Create();

            await _httpFixture.PostAsync("/signUp", signUpUserRequest);

            var claim = _fixture
                .Build<UserClaim>()
                .With(x => x.ClaimType, ClaimType.Receipt)
                .With(x => x.ClaimValue, ClaimValue.Read)
                .Create();

            var addUserInClaimRequest = _fixture
                .Build<AddUserInClaimRequest>()
                .With(x => x.Email, $"{email}@test.com")
                .With(x => x.Claims, new List<UserClaim> { claim })
                .Create();

            //Act
            var (StatusCode, Content) = await _httpFixture.PostAsync("/addUserInClaim", addUserInClaimRequest, true);

            //Assert
            StatusCode.Should().Be(HttpStatusCode.Created);
            var userResponse = JsonConvert.DeserializeObject<UserResponse>(Content);
            userResponse?.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GivenAValidEmail_PrevioslyAddedClaimsShouldBeReturned()
        {
            //Arrange
            var password = _fixture.Create<string>();
            var email = _fixture.Create<string>();
            var signUpUserRequest = _fixture
                .Build<SignUpUserRequest>()
                .With(x => x.Email, $"{email}@test.com")
                .With(x => x.Password, $"{password}cAb!")
                .With(x => x.PasswordConfirmation, $"{password}cAb!")
                .Create();

            await _httpFixture.PostAsync("/signUp", signUpUserRequest);

            var claimsRequest = _fixture
                .Build<UserClaim>()
                .With(x => x.ClaimType, ClaimType.Receipt)
                .With(x => x.ClaimValue, ClaimValue.Read)
                .Create();

            var addUserInClaimRequest = _fixture
                .Build<AddUserInClaimRequest>()
                .With(x => x.Email, $"{email}@test.com")
                .With(x => x.Claims, new List<UserClaim> { claimsRequest })
                .Create();

            await _httpFixture.PostAsync("/addUserInClaim", addUserInClaimRequest, true);

            //Act
            var (StatusCode, Content) = await _httpFixture.GetAsync("/getUserClaims", $"{email}@test.com");

            //Assert
            StatusCode.Should().Be(HttpStatusCode.OK);
            Content.Should().NotBeNull();
        }
    }
}
