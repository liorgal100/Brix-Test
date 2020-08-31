using BrixTest.Data;
using BrixTest.Models;
using BrixTest.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BrixTest
{
    public class Program
    {
        public static IConfigurationRoot configuration;
        public static List<Task<BankTransaction>> tasks = new List<Task<BankTransaction>>();
        static void Main ( string[] args )
        {
            var services = new ServiceCollection();
            ConfigureServices( services );
          
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                IBankSimulatorService _bankSimulator = serviceProvider.GetService<IBankSimulatorService>();
                ILogServices _log = serviceProvider.GetService<ILogServices>();
                Bank _newBank =  _bankSimulator.CreateBank( "Brix Test" ).Result;

                _log.WriteLine( "WELCOME TO " + _newBank.Name +" Bank",ConsoleColor.Blue);
                _log.WriteLine( "========================== " , ConsoleColor.Blue );

                string MenuInput = string.Empty;
                while(MenuInput != "E" && MenuInput != "e")
                {
                    _log.WriteLine( "Menu :O = open an account | R = add owner | D = Deposit | W = Withdrawal | E = Exit" , ConsoleColor.Blue );
                    MenuInput = HandleuserInput(_bankSimulator,_log,_newBank).Result;
                }

                Task.WaitAll(tasks.ToArray());
            }           
        }

        private static async Task<string> HandleuserInput(IBankSimulatorService bankSimulator, ILogServices log, Bank bank)
        {
            string MenuInput = Console.ReadLine();
            string CommanInput = string.Empty;
            try
            {
                switch (MenuInput)
                {
                    case "O": //open account
                    case "o":
                        Account _newAccount = await bankSimulator.CreateAccount( bank.ID );
                        log.WriteLine( "Account number " + _newAccount.ID.ToString() + " created." , ConsoleColor.Green );
                        break;

                    case "R": // add owner to an account
                    case "r":
                        log.WriteLine( "Please enter owner id (up to 9 digits), name, account id (123456789,lior gal,1). " , ConsoleColor.White );
                        CommanInput = Console.ReadLine();
                        AccountOwner _newAccountOwner = await bankSimulator.AddAccountOwner( CommanInput.Split( "," )[ 0 ].Trim() , CommanInput.Split( "," )[ 1 ].Trim() ,
                                                                                              Convert.ToInt32( CommanInput.Split( "," )[ 2 ].Trim() ) );

                        log.WriteLine( string.Format("User id {0} was added as account {1} owner.", _newAccountOwner.OwnerID, _newAccountOwner.AcoountID.ToString() ) , ConsoleColor.Green );
                        break;

                    case "D": // Deposit
                    case "d":
                        log.WriteLine( "Please enter account id, owner id,amount (1,123456789,100.5). " , ConsoleColor.White );
                        CommanInput = Console.ReadLine();

                        Task<BankTransaction> depositTask =   Task.Run( () => bankSimulator.Deposit( Convert.ToInt32( CommanInput.Split( "," )[ 0 ].Trim() ) , CommanInput.Split( "," )[ 1 ].Trim() ,
                                                                                              Convert.ToDouble( CommanInput.Split( "," )[ 2 ].Trim() ) ));
                        tasks.Add( depositTask );
                        break;

                    case "W": // Withdrawal
                    case "w":
                        log.WriteLine( "Please enter account id, owner id,amount (1,123456789,100.5). " , ConsoleColor.White );
                        CommanInput = Console.ReadLine();

                        Task<BankTransaction> withdrawalTask = Task.Run( () => bankSimulator.Withdrawal( Convert.ToInt32( CommanInput.Split( "," )[ 0 ].Trim() ) , CommanInput.Split( "," )[ 1 ].Trim() ,
                                                                                            Convert.ToDouble( CommanInput.Split( "," )[ 2 ].Trim() ) ) );

                        tasks.Add( withdrawalTask );
                        break;

                    case "E": //exit program
                    case "e":
                        log.WriteLine( "Thanks for using " + bank.Name + " bank :-)" , ConsoleColor.Blue );
                        break;
                    default:
                        log.WriteLine( "invalid menu item." , ConsoleColor.Red );
                        break;
                }
            }
            catch(Exception ex)
            {
                log.WriteLine( "Error. " + ex.Message , ConsoleColor.Red );
            }
            return MenuInput;
        }

       
        private static void ConfigureServices ( ServiceCollection services )
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath( Directory.GetParent( AppContext.BaseDirectory ).FullName )
                .AddJsonFile( "appsettings.json" , true , true )
                .Build();

            services.AddSingleton<IConfigurationRoot>( configuration ); // Add access to generic IConfigurationRoot
            services.AddLogging( configure => configure.AddConfiguration( configuration ) );

            //use an internal memory db since this is a test project. Please notice relation constraint are not enforce in an in memory DB!!.
            //If possible use an sql server.
            var ServiceProvider = services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
            services.AddDbContext<BrixDBContext>( options => options.UseInMemoryDatabase( "BrixDB" ).UseInternalServiceProvider( ServiceProvider ) );
            // services.AddDbContext<BrixDBContext>( options => options.UseSqlServer( @"Server =.\SQLEXPRESS; Database = BrixDB; Trusted_Connection = True;" ) );
            //services.AddDbContext<BrixDBContext>();

            //add program services
            services.AddSingleton<IBankSimulatorService,BankSimulatorService>();
            services.AddTransient<IDBServices,DBServices>();
            services.AddTransient<ILogServices , ConsoleLogger>();
        }
    }
}
