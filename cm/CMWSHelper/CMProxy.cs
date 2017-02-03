using System;
using System.Collections.Generic;
using System.Text;
using CMWSHelper.CMService;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ComponentModel;

namespace CMWSHelper
{
	public class CMProxy : INotifyPropertyChanged, IDisposable
	{
		private ChannelFactory<ICMService> _factory = null;
		private ICMService _cm = null;
        private ICommunicationObject _cmObject = null;
        
        //private string _state = null;
        private string _domain = null;
        private string _username = null;
        private string _password = null;

		public CMProxy(Binding binding, string address, string domain, string username, string password, string upn)
		{
            _domain = domain;
            _username = username;
            _password = password;

			_factory = new ChannelFactory<ICMService>(binding, 
                new EndpointAddress(
                    new Uri(address), 
                    EndpointIdentity.CreateUpnIdentity(upn), 
                    new AddressHeaderCollection()));

			_factory.Credentials.Windows.ClientCredential.UserName = username;
			_factory.Credentials.Windows.ClientCredential.Password = password;
			_factory.Credentials.Windows.ClientCredential.Domain   = domain;

			_factory.Credentials.Windows.AllowedImpersonationLevel = 
				System.Security.Principal.TokenImpersonationLevel.Delegation;
		}

        public void CreateChannel()
        {
            _cm = _factory.CreateChannel();
            _cmObject = (ICommunicationObject)_cm;
            //_cmObject.Closed += new EventHandler(_cmObject_StateChanged);
            //_cmObject.Closing += new EventHandler(_cmObject_StateChanged);
            //_cmObject.Faulted += new EventHandler(_cmObject_StateChanged);
            //_cmObject.Opened += new EventHandler(_cmObject_StateChanged);
            //_cmObject.Opening += new EventHandler(_cmObject_StateChanged);
            
            _cm.Connect(_username, _domain, _password);
        }

        void _cmObject_StateChanged(object sender, EventArgs e)
        {
            //_state = _cmObject.State.ToString();
            _propertyChanged("State");
        }

		~CMProxy()
		{
			Dispose(true);
		}

		public ICMService CampaignManager
		{
			get { return _cm; }
		}

		public CommunicationState State
		{
			//get { return ((IClientChannel)_cm).State; }
			get { return _cmObject.State; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				//dispose proxy here
				if (_cm != null)
				{
					this.Disconnect();
				}
			}
		}

        private void _propertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

		public void Disconnect()
		{
			IClientChannel channel = (IClientChannel)_cm;
			try
			{
				if (channel.State != CommunicationState.Closed)
					channel.Close();
			}
			catch
			{
				channel.Abort();
			}
			_factory.Close();
			_cm = null;
		}

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
