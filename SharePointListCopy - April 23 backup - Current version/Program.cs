﻿using System;
using System.Net;
using Microsoft.SharePoint.Client;
using log4net;
using log4net.Config;
using System.Linq;
using System.Data;
using ClosedXML.Excel;
using System.Text;
using System.Configuration;
using System.Data.OleDb;
using Excel;
using System.IO;

namespace SharePointListCopy
{
    /// <summary>
    /// SharePoint list copy will allow the backup of the current pipeline list
    /// 
    /// Assumptions:
    ///   We do not have access to the sharepoint server so we need to use client context
    ///   We are using a console app so we can can add in a scheduled event
    /// </summary>
    class Program
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        
       // private static object yesnoarray;

        /// <summary>
        /// entry point into the backup list program
        /// </summary>
        /// <param name="args">Not really used</param>
        static void Main(string[] args)
        {
            try
            {
                string clientContextWeb = ConfigurationManager.AppSettings["clientContextWeb"] ?? @"https://bi.sharepoint.hpe.com/teams/GXT/";
                //string backupListTarget = ConfigurationManager.AppSettings["backupListTarget"] ?? @"MikeTestPipelineBackup";
                string backupListTarget = @"MikeTestPipelineBackup";
                //string backupListSource = ConfigurationManager.AppSettings["backupListSource"] ?? @"Pipeline";
                string backupListSource = @"PipelineBackup";
                string pipelineBackupDocLib = ConfigurationManager.AppSettings["pipelineBackupDocLib"] ?? @"PipelineBackup";
                //string updateList = ConfigurationManager.AppSettings["updateList"] ?? @"Pipeline";
                string updateList = @"MikeTestPipelineBackup";


                // ------  Two fields for index chnages -------
                // sharepointIndexTitle   - Name of Index Field in the SharePoit table
                // excelIndexTitle        - Name of Index Field in Excel Spreadsheet
                string sharepointIndexTitle = ConfigurationManager.AppSettings["sharepointIndexTitle"] ?? @"OppID";
                string excelIndexTitle = ConfigurationManager.AppSettings["excelIndexTitle"] ?? @"Sales Opportunity Id";

                log.Debug("---------------------------------------------------------------");
                log.Debug("------------------   Log Starts Here. -------------------------");
                log.Debug("---------------------------------------------------------------");
                
                log.Debug(string.Format("context web:{0}  target backup:{1}  source backup {2}", clientContextWeb, backupListTarget, backupListSource));
                log.Debug(string.Format("ClientContextWeb: {0}", clientContextWeb));
                log.Debug(string.Format("backupListTarget: {0}", backupListTarget));
                log.Debug(string.Format("backupListSource: {0}", backupListSource));
                log.Debug(string.Format("pipelineBackupDocLib: {0}", pipelineBackupDocLib));
                log.Debug(string.Format("updateList: {0}", updateList));
                log.Debug(string.Format("excelIndexTitle: {0}", excelIndexTitle));
                log.Debug(string.Format("sharepointIndexTitle: {0}", sharepointIndexTitle));


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Welcome to the C&I capture utility");
                Console.WriteLine("Sit back and grab some coffee and maybe a danish.");

                Console.WriteLine("---------- DEBUG INFO -------------------------"); ;
                Console.WriteLine("ClientContextWeb: {0}", clientContextWeb);
                Console.WriteLine("backupListTarget: {0}", backupListTarget);
                Console.WriteLine("backupListSource: {0}", backupListSource);
                Console.WriteLine("pipelineBackupDocLib: {0}", pipelineBackupDocLib);
                Console.WriteLine("updateList: {0}", updateList);
                Console.WriteLine("excelIndexTitle: {0}", excelIndexTitle);
                Console.WriteLine("sharepointIndexTitle: {0}", sharepointIndexTitle);
                Console.WriteLine("-----------------------------------------------"); ;


                Console.WriteLine("Would you like to run backup (1) or List Update (2) or Both(3), or (4) To Exit");
 
                int itest =  int.Parse(Console.ReadLine());



                // UpdatePipelineList updatelist = new UpdatePipelineList(clientContextWeb, backupListTarget, backupListSource, pipelineBackupDocLib);
                
                switch (itest)
                {
                    case 1:
                        //Console.WriteLine("Instide 1");
                        Console.WriteLine("Starting Backup...");
                        BackupHelper helper = new BackupHelper(clientContextWeb, backupListTarget, backupListSource, pipelineBackupDocLib);
                        break;
                    case 2:
                        //Console.WriteLine("Instide 2");
                        Console.WriteLine("Starting Update...");
                        UpdatePipelineList updatelist = new UpdatePipelineList(clientContextWeb, updateList, excelIndexTitle, sharepointIndexTitle);
                        break;
                    case 3:
                        //Console.WriteLine("Instide 3");
                        Console.WriteLine("1) Starting Backup w/ Update to follow.");
                        BackupHelper helperComplete = new BackupHelper(clientContextWeb, backupListTarget, backupListSource, pipelineBackupDocLib);

                        // Console.WriteLine("Instide 3a");
                        Console.WriteLine("2) Starting Update...");
                        UpdatePipelineList updatelistComplete = new UpdatePipelineList(clientContextWeb, updateList, excelIndexTitle, sharepointIndexTitle);
                        
                        break;
                    case 4:
                        //Console.WriteLine("Instide 4");
                        break;
                    default:
                        Console.WriteLine("Invalid input.  Aborting Program.");
                        break;
                }
                //if (itest == 1 || itest == 3)
                //{
                //    Console.WriteLine("Instide 1");
                //    // BackupHelper helper = new BackupHelper(clientContextWeb, backupListTarget, backupListSource, pipelineBackupDocLib);
                //}
                
                //if (itest == 2 || itest == 3)
                //{
                //    Console.WriteLine("Instide 2");
                //    //UpdatePipelineList updatelist = new UpdatePipelineList(clientContextWeb, updateList);
                //}

                //if ((itest != 1) 
                //{
                //    Console.WriteLine("Invalid input.  Aborting Program.");
                //}
                Console.WriteLine("Program exiting. (Press enter to exit)");           
                Console.ReadLine();

            }
            catch (FormatException ex)
            {
                log.Error("Format Exception in initial input", ex);

                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine(string.Format("We had an issue:{0}", ex.ToString()));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format(" exception caught.{0}", ex.Message));

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                log.Error("Format Exception in main", ex);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("We had an issue:{0}", ex.Message));

                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.ReadLine();
            }
        }

      
    }
}
