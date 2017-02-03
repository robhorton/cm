using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using CMWSHelper;
using CMWSHelper.CMService;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using Channels = System.ServiceModel.Channels;


namespace cm
{
    public class Program
    {
        static CMProxy _cmProxy = null;
        
        

        public static void Main(string[] args)
        {
            CampaignInfo _campaign = null;
            UserInfo _userInfo = null;


            //user credential
            string _domain              = string.Empty;
            string _username            = string.Empty;
            string _password            = string.Empty;

            string _currentAddress      = string.Empty;
            string _currentBinding      = string.Empty;
            string _upn                 = string.Empty;

            string _openTimeout         = string.Empty;
            string _receiveTimeout      = string.Empty;
            string _sendTimeout         = string.Empty;
            string _closeTimeout        = string.Empty;
            string _inactivityTimeout   = string.Empty;

            string _gateway             = "Gateway: {0}";


            // Read the configuration from the app.config file and assign to variables

            _currentAddress = ConfigurationManager.AppSettings["defaultAddress"];
            _currentBinding = ConfigurationManager.AppSettings["defaultBinding"];

            _domain = ConfigurationManager.AppSettings["domain"];
            _username = ConfigurationManager.AppSettings["username"];
            _password = ConfigurationManager.AppSettings["password"];
            _upn = ConfigurationManager.AppSettings["userPrincipalName"];

            _userInfo = new UserInfo();
            _userInfo.ConfirmerUsername = ConfigurationManager.AppSettings["confirmerUsername"];
            _userInfo.ConfirmerPassword = ConfigurationManager.AppSettings["confirmerPassword"];
            _userInfo.VerifierUsername = ConfigurationManager.AppSettings["verifierUsername"];
            _userInfo.VerifierPassword = ConfigurationManager.AppSettings["verifierPassword"];

            //timeouts
            _openTimeout = ConfigurationManager.AppSettings["OpenTimeout"];
            _receiveTimeout = ConfigurationManager.AppSettings["ReceiveTimeout"];
            _sendTimeout = ConfigurationManager.AppSettings["SendTimeout"];
            _closeTimeout = ConfigurationManager.AppSettings["CloseTimeout"];
            _inactivityTimeout = ConfigurationManager.AppSettings["InactivityTimeout"];




            //Set variables for creating a batch


            //Create a campaign

            _campaign = new CampaignInfo();


            _campaign.ID = "";
            _campaign.BatchCount = 1;
            _campaign.MaxActiveBatches.ToString();
            _campaign.MinRemoveDelay.ToString();
            _campaign.Prefix = "";
            //_campaign.BaseRecipe.Name = "TestRecipe";
            _campaign.StartSequence = 1;


            //Establish a connection

            _connect(_address);


            //Create a batch






        }


        private static void _connect(string address)
        {
            try
            {
                _cmProxy = null;
                Channels.Binding wsBinding = null;

                
                wsBinding = new WSHttpBinding();
                ((WSHttpBinding)wsBinding).ReliableSession.Enabled = true;
                ((WSHttpBinding)wsBinding).Security.Mode = SecurityMode.Message;
                ((WSHttpBinding)wsBinding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                ((WSHttpBinding)wsBinding).Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                ((WSHttpBinding)wsBinding).Security.Message.ClientCredentialType = MessageCredentialType.Windows;
                ((WSHttpBinding)wsBinding).Security.Message.NegotiateServiceCredential = true;
                ((WSHttpBinding)wsBinding).Security.Message.EstablishSecurityContext = true;
                ((WSHttpBinding)wsBinding).ReliableSession.InactivityTimeout = TimeSpan.Parse(_inactivityTimeout);
                ((WSHttpBinding)wsBinding).ReaderQuotas.MaxStringContentLength = int.MaxValue;
                ((WSHttpBinding)wsBinding).ReaderQuotas.MaxArrayLength = int.MaxValue;
                ((WSHttpBinding)wsBinding).ReaderQuotas.MaxBytesPerRead = int.MaxValue;
                ((WSHttpBinding)wsBinding).ReaderQuotas.MaxDepth = int.MaxValue;
                ((WSHttpBinding)wsBinding).ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
                ((WSHttpBinding)wsBinding).MaxBufferPoolSize = int.MaxValue;
                ((WSHttpBinding)wsBinding).MaxReceivedMessageSize = int.MaxValue;
                
                /*
                else
                {
                    throw new Exception("Invalid binding: " + binding);
                }
                 * */


                //timeouts
                /*
                wsBinding.OpenTimeout = TimeSpan.Parse(_openTimeout);
                wsBinding.ReceiveTimeout = TimeSpan.Parse(_receiveTimeout);
                wsBinding.SendTimeout = TimeSpan.Parse(_sendTimeout);
                wsBinding.CloseTimeout = TimeSpan.Parse(_closeTimeout);
                 */

                _cmProxy = new CMProxy(
                    wsBinding,
                    address,
                    _domain,
                    _username,
                    _password,
                    _upn);

                //_cmProxy.PropertyChanged += new PropertyChangedEventHandler(_cmProxy_PropertyChanged);
                _cmProxy.CreateChannel();
                //_setCMStateEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //Console.Writeline = string.Format(_gateway, _currentAddress);
            }
        }



        // Variable functions


        public static string _domain
        {
            get { return _domain; }
        }

        public static string _username
        {
            get { return _username; }
        }

        public static string _password
        {
            get { return _password; }
        }

        public static string _address
        {
            get { return _address; }
        }

        public static string _upn
        {
            get { return _upn; }
        }

        public static string _currentAddress
        {
            get { return _currentAddress; }
        }

        public static string _gateway
        {
            get { return _gateway; }
        }

        public static string _inactivityTimeout
        {
            get { return _inactivityTimeout; }
        }
    }
}
