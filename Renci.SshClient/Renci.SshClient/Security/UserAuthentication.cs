﻿using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
{
    internal abstract class UserAuthentication
    {
        public abstract string Name { get; }

        public bool IsAuthenticated { get; private set; }

        public string ErrorMessage { get; private set; }

        protected Session Session { get; private set; }

        public UserAuthentication(Session session)
        {
            this.Session = session;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>true if method was execute; otherwise false.</returns>
        public bool Execute()
        {
            this.Session.RegisterMessageType<FailureMessage>(MessageTypes.UserAuthenticationFailure);
            this.Session.RegisterMessageType<SuccessMessage>(MessageTypes.UserAuthenticationSuccess);
            this.Session.RegisterMessageType<BannerMessage>(MessageTypes.UserAuthenticationBanner);

            this.Session.MessageReceived += Session_MessageReceived;

            var result = this.Run();

            this.Session.MessageReceived -= Session_MessageReceived;

            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationFailure);
            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationSuccess);
            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationBanner);

            return result;
        }

        protected abstract bool Run();

        protected abstract void HandleMessage<T>(T message) where T : Message;

        protected virtual void HandleMessage(SuccessMessage message)
        {
            this.IsAuthenticated = true;
        }

        protected virtual void HandleMessage(FailureMessage message)
        {
            this.ErrorMessage = message.Message;
            this.IsAuthenticated = false;
        }

        private void Session_MessageReceived(object sender, Common.MessageReceivedEventArgs e)
        {
            dynamic message = e.Message;
            this.HandleMessage(message);
        }
    }
}