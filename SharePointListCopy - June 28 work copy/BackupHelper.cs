using System;
using System.Net;
using Microsoft.SharePoint.Client;
using log4net;
using log4net.Config;
using System.Linq;
using System.Data;
using ClosedXML.Excel;
using System.Text;
using System.IO;
using MSDN.Samples.ClaimsAuth;

namespace SharePointListCopy
{
    /// <summary>
    /// Helper class for backup
    /// 
    /// this class will backup a list to an excel spreadsheet
    /// It will then remove the items from the list
    /// Finally it will take the pipeline list and copy the items to the backup list
    /// </summary>
    public class BackupHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BackupHelper));
        //  private static string[] stringArray = { "OpportunityName", "HPEOppID", "OpportunityOwner",  "SalesStage", "OpportunityType", "LineOfBusiness", "TotalMarginAmount",  "TotalOpportunityValue",  "FiscalPeriod", "CloseDate",  "TotalContractLengthMonths",  "Forecast",  "Closed",  "Won", "Scope", "TAcompleted", "Workshare", "TotalCosts", "EGM",  "EGM %", "OperatingProfit", "IRR",  "Payback", "ALR", "LowMarginJustification", "BASOARcomments", "BASOAR_RejectionReason" };
        private static string[] badTitleArray = { "LinkTitleNoMenu","LinkTitle","LinkTitle2","ContentType","Edit","SelectTitle","PermMask",
        "HTML_x0020_File_x0020_Type","_EditMenuTableStart","_EditMenuTableStart2","_EditMenuTableEnd",
        "LinkFilenameNoMenu","LinkFilename2","DocIcon","ServerUrl","EncodedAbsUrl","BaseName","LinkFilename","ID","_HasCopyDestinations",
        "_CopySource","Attachment","Attachments"};
        /// <summary>
        /// Backups and exports lists
        /// </summary>
        /// <param name="clientContextWeb">client context url</param>
        /// <param name="backupListTarget">list that is exported and deleted</param>
        /// <param name="backupListSource">list that needs to be backedup</param>
        /// <param name="pipelineBackupDocLib">doc library for backup of excel file</param>
        public BackupHelper(string clientContextWeb, string backupListTarget, string backupListSource, string pipelineBackupDocLib)
        {
            try
            {

                //using (var clientContext = new ClientContext(clientContextWeb))
                using (ClientContext clientContext = ClaimClientContext.GetAuthenticatedContext(clientContextWeb))
                {

                    try
                    {
                        Web web = clientContext.Web;

                        List oldList = web.Lists.GetByTitle(backupListTarget);
                        List newList = web.Lists.GetByTitle(backupListSource);

                        // This creates a CamlQuery that has a RowLimit of 100, and also specifies Scope="RecursiveAll" 
                        // so that it grabs all list items, regardless of the folder they are in. 
                        // DEV NOTES: 
                        //     Aug. 4, 2017 - Removed 2000  limitation from CamlQueries   (MAT)
                        // 
                        CamlQuery query = CamlQuery.CreateAllItemsQuery();
                        CamlQuery newquery = CamlQuery.CreateAllItemsQuery();
                        ListItemCollection oldItems = oldList.GetItems(query);
                        ListItemCollection newItems = newList.GetItems(newquery);
                        int counter = 0;

                        var listFields = newList.Fields;
                        clientContext.Load(listFields, fields => fields.Include(field => field.Title, field => field.InternalName, field => field.ReadOnlyField));


                        clientContext.Load(oldItems);
                        clientContext.Load(newItems);
                        clientContext.ExecuteQuery();

                        // Retrieve all items in the ListItemCollection from List.GetItems(Query).
                        DataTable dt = new DataTable("ExportData");


                        foreach (Field f in listFields)
                        {
                            //Console.WriteLine("Checking Field: {0}", f.Title);
                            try
                            {

                                //   if (Array.IndexOf(stringArray, f.Title) >= 0)
                                //    {
                                if (Array.IndexOf(badTitleArray, f.InternalName) < 0 && !f.ReadOnlyField)
                                {
                                    //Console.WriteLine("Adding Field: {0}", f.Title);
                                    dt.Columns.Add(f.Title);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("error creating table", ex);
                            }
                        }


                        foreach (ListItem item in oldItems)
                        {
                            DataRow dr = dt.NewRow();

                            foreach (Field f in listFields)
                            {


                                if (Array.IndexOf(badTitleArray, f.InternalName) < 0 && !f.ReadOnlyField)
                                {
                                    try
                                    {
                                        dr[f.Title] = item[f.InternalName];
                                    }
                                    catch (Exception ex)
                                    {
                                        //log.Debug("found a problem with field");
                                        log.Debug(string.Format("ERROR: Problem with field ({0}).", f.InternalName.ToString()));
                                        Console.WriteLine("Error with: {0} \n", f.InternalName.ToString());
                                        Console.WriteLine("Error message {0}", ex.Message);
                                    }

                                }

                            }

                            dt.Rows.Add(dr);

                            // WORKING DELETE OBJECTS
                            oldItems.GetById(item.Id).DeleteObject();
                            if (counter > 50)
                            {
                                try
                                {
                                    clientContext.ExecuteQuery();
                                    counter = 0;
                                }
                                catch (Exception ex)
                                {
                                    log.Error("caught and exception in delete object", ex);
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(string.Format("We had an issue:{0}", ex.ToString()));
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Connectivity was lost please try again");
                                    throw ex;
                                }
                            }
                            else
                                counter++;

                        }
                        //finish up the final documents
                        counter = 0;
                        try
                        {
                            clientContext.ExecuteQuery();
                        }
                        catch (Exception ex)
                        {
                            log.Error("caught and exception while cleaning up objects", ex);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(string.Format("We had an issue:{0}", ex.ToString()));
                            Console.WriteLine("Connectivity was lost please try again");
                            throw ex;
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Items are all deleted we are about to save our file");

                        // save excel file
                        XLWorkbook wb = new XLWorkbook();
                        wb.Worksheets.Add(dt, "WorksheetName");
                        string timestamp = DateTime.Now.ToString("yyyyMMddhhmm");
                        string fileName = "backup" + timestamp + ".xlsx";
                        wb.SaveAs(fileName);

                        using (var fs = new FileStream(fileName, FileMode.Open))
                        {
                            var fi = new FileInfo(fileName);
                            var list = clientContext.Web.Lists.GetByTitle(pipelineBackupDocLib);
                            clientContext.Load(list.RootFolder);
                            clientContext.ExecuteQuery();
                            var fileUrl = String.Format("{0}/{1}", list.RootFolder.ServerRelativeUrl, fi.Name);

                            Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext, fileUrl, fs, true);
                        }


                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("The file was successfully saved and we are about to copy lists");



                        foreach (ListItem item in newItems)
                        {

                            var itemCreateInfo = new ListItemCreationInformation();
                            var newListItem = oldList.AddItem(itemCreateInfo);
                            StringBuilder sbLog = new StringBuilder();
                            foreach (Field f in listFields)
                            {



                                //   if (Array.IndexOf(stringArray, f.Title) >= 0)
                                //    {
                                if (Array.IndexOf(badTitleArray, f.InternalName) < 0 && !f.ReadOnlyField)
                                {
                                    if (f.InternalName.ToLower() == "title")
                                        sbLog.Append(string.Format("Field Name:{0}", f.InternalName));

                                    newListItem[f.InternalName] = item[f.InternalName];
                                }
                                //}


                            }


                            newListItem.Update();
                            if (counter > 50)
                            {
                                try
                                {
                                    clientContext.ExecuteQuery();
                                }
                                catch (Exception ex)
                                {
                                    log.Error("caught an update exception", ex);
                                    log.Debug(string.Format("ERROR: {0} with update.", newListItem["HPOppID"].ToString()));
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(string.Format("We had an issue:{0}", ex.ToString()));
                                    Console.WriteLine("Connectivity was lost please try again");
                                    // throw ex;
                                }
                            }
                            else
                            {
                                counter++;
                                //log.Debug(string.Format("added item:{0}", sbLog.ToString()));
                            }



                        }

                        //finish up the final documents
                        counter = 0;
                        try
                        {
                            clientContext.ExecuteQuery();
                        }
                        catch (Exception ex)
                        {
                            log.Error("caught and exception", ex);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(string.Format("We had an issue:{0}", ex.ToString()));
                            Console.WriteLine("Connectivity was lost please try again");
                            log.Error(string.Format("Connectivity was lost. Please try again:{0}", ex.ToString()));
                            // throw ex;
                        }




                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("We had an issue Please record, exit and try again:{0}", ex.ToString()));
                        log.Error(string.Format("We had an issue Please record, exit and try again:{0}", ex.ToString()));
                        //Console.ReadLine();
                    }

                }
            }
            catch (MissingMethodException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[MAT] Missing Method exception:  {0}: ", ex.Message);

                log.Error("Client context was lost", ex);
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Client context was lost:  Pleasse record and press enter");
                log.Error("Client context was lost", ex);
                //Console.ReadLine();
            }


            log.Debug("------------------------------------------------------------");
            log.Debug("Backup Completed.");
            log.Debug("------------------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Successfully completed Backup.");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.ReadLine();
        }
    }
}

