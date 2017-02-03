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
    public class CM
    {
        static CMProxy _cmProxy = null;

        public static void Main(string[] args)
        {
            CampaignInfo _campaign          = null;
            UserInfo _userInfo              = null;
            RecipeInfo _baseRecipe          = new RecipeInfo();
            List<BatchInfo> batches         = new List<BatchInfo>();
            EquipmentTrain _equipmentTrain  = new EquipmentTrain();
            Formula _formula                = new Formula();

            string CMServer                 = null;
            string BatchExecutive           = null;
            string CampaignID               = null;
            string RecipeName               = null;
            string Unit                     = null;
            
            //user credentials
            string _domain                  = string.Empty;
            string _username                = string.Empty;
            string _password                = string.Empty;

            string _address                 = string.Empty;
            string _upn                     = string.Empty;

            string _openTimeout             = string.Empty;
            string _receiveTimeout          = string.Empty;
            string _sendTimeout             = string.Empty;
            string _closeTimeout            = string.Empty;
            string _inactivityTimeout       = string.Empty;

            string argument                 = "";
            string argumentInput            = "";
            Int16  a                        = 0;


            //Process arguments

            if (args == null || args.Length < 1)
            {
                PrintUsage();
            }
            else
            {
                for (int i = 0; i < args.Length; i++) // Loop through the argument array
                {
                    //Assign the current argument to a temporary string
                    argument = args[i];

                    if (argument.Substring(0, 1) == "/") //Check to see if a switch was encountered
                    {
                        if ((args.Length - 1) == i) //Check to see in the current argument does not have associated input
                        {
                            Console.Write("Error: missing input for switch ");
                            Console.WriteLine(argument);
                            PrintUsage(); //Show usage and exit
                            Environment.Exit(1);
                        }

                        argumentInput = args[i + 1];

                        switch (argument.Substring(1, 1))
                        {
                            case "f": //Formula Name
                                _formula.Name = argumentInput.ToString();
                                break;

                            case "n":   //Batch Name
                                CampaignID = argumentInput;
                                a++;
                                break;

                            case "r":   //Recipe
                                RecipeName = argumentInput;
                                a++;
                                break;

                            case "c": //CM Server Name
                                BatchExecutive = argumentInput;
                                CMServer = argumentInput;
                                a++;
                                break;

                            case "u": //Unit Name
                                Unit = argumentInput;
                                break;

                            default:
                                //Argument not correct
                                Console.Write("Error: argument ");
                                Console.Write(argument);
                                Console.WriteLine(" option is incorrect");
                                PrintUsage();
                                Environment.Exit(1);
                                break;
                        }

                    }

                }//End of argument loop

                if (a < 3)
                {
                    Console.WriteLine("Error: too few arguments were passed");
                    PrintUsage(); //Show usage and exit
                }
            }
            
            // Read the configuration from the app.config file and assign to variables

            _address = ConfigurationManager.AppSettings["defaultAddress"];

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


            //Set variables
            _baseRecipe.Executive       = BatchExecutive;
            _baseRecipe.Name            = RecipeName;
            _baseRecipe.EquipmentTrain  = _equipmentTrain;

            if (_formula != null)
            {
                _baseRecipe.Formula = _formula;
            }

            if (Unit != null)
            {
                _baseRecipe.UnitBindings = new UnitBinding[1];
                _baseRecipe.UnitBindings[0] = new UnitBinding();
                _baseRecipe.UnitBindings[0].UnitName = Unit;
                _baseRecipe.UnitBindings[0].StepName = RecipeName;
            }
            
            //Create a campaign

            _campaign = new CampaignInfo();

            _campaign.ID = CampaignID;

            _campaign.BatchList = new BatchInfo[1];
            _campaign.BatchList[0] = new BatchInfo();
            _campaign.BatchList[0].ID = CampaignID;

            _campaign.MaxActiveBatches.ToString();
            _campaign.MinRemoveDelay.ToString();
            _campaign.BatchExecutionMode = BatchExecutionMode.AutoRelease;
            _campaign.ContinuousIteration = false;
            _campaign.AutoRemoveInManual = true;
            _campaign.AutoRemoveInAutoRelease = true;
            _campaign.AutoRemoveInAutoStart = true;
            _campaign.MinRemoveDelay = 0;
            _campaign.StartSequence = 1;
            _campaign.BaseRecipe = _baseRecipe;


            //Establish a connection to the web service
            
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
                              

                //timeouts
                wsBinding.OpenTimeout = TimeSpan.Parse(_openTimeout);
                wsBinding.ReceiveTimeout = TimeSpan.Parse(_receiveTimeout);
                wsBinding.SendTimeout = TimeSpan.Parse(_sendTimeout);
                wsBinding.CloseTimeout = TimeSpan.Parse(_closeTimeout);
                
                //Authenticate with the Campaign Manager

                _cmProxy = new CMProxy(
                    wsBinding,
                    _address,
                    _domain,
                    _username,
                    _password,
                    _upn);

                
                _cmProxy.CreateChannel();

                //Console.WriteLine(string.Format(_address, _address));
                //Console.WriteLine("Connection state: " + _cmProxy.State);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
            

            //Create a batch

            try
            {

                _cmProxy.CampaignManager.CreateCampaign(CMServer, _campaign, _userInfo);

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }


            //Cleanup
            _cmProxy.Disconnect();
            //Console.WriteLine("Connection state: " + _cmProxy.State);            
            _cmProxy.Dispose();

            Environment.Exit(0);

        }    
 
            static void PrintUsage()
        {            
            Console.WriteLine("");
            Console.WriteLine("  DeltaV Campaign Manager utility");
            Console.WriteLine("");
            Console.WriteLine("  /c\tCampaign Manager Server Name");
            Console.WriteLine("  /n\tCampaign name");
            Console.WriteLine("  /r\tRecipe name");
            Console.WriteLine("");
            Console.WriteLine("  Optional Arguments:");
            Console.WriteLine("");
            Console.WriteLine("  /f\tFormula name");
            Console.WriteLine("  /u\tUnit name");
            Console.WriteLine("");
            Console.WriteLine("  Note: For any arguments containing spaces, use \"\"'s");
            Console.WriteLine("");
            Console.WriteLine("  Rob Horton - Feb-2016");
            Console.WriteLine("  Tested with DeltaV 12.3 and 13.3");
            Environment.Exit(1); 
        }
    }
}
