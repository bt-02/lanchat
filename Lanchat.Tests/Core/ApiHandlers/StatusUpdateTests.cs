using Lanchat.Core.Chat;
using Lanchat.Core.Chat.Handlers;
using Lanchat.Core.Chat.Models;
using Lanchat.Tests.Mock.Network;
using NUnit.Framework;

namespace Lanchat.Tests.Core.ApiHandlers
{
    public class StatusUpdateTests
    {
        private NodeMock nodeMock;
        private UserStatusUpdateHandler userStatusUpdateHandler;

        [SetUp]
        public void Setup()
        {
            nodeMock = new NodeMock();
            userStatusUpdateHandler = new UserStatusUpdateHandler(nodeMock);
        }

        [Test]
        public void NewStatus()
        {
            var statusUpdate = new UserStatusUpdate
            {
                NewUserStatus = UserStatus.AwayFromKeyboard
            };

            userStatusUpdateHandler.Handle(statusUpdate);
            Assert.AreEqual(statusUpdate.NewUserStatus, nodeMock.UserStatus);
        }
    }
}