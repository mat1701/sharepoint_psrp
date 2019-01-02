using Excel;
using log4net;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MSDN.Samples.ClaimsAuth;

namespace SharePointListCopy
{
    /// <summary>
    /// UpdatePipeline Class
    /// 
    /// this class will update a list from an excel spreadsheet
    /// </summary>
    public class UpdatePipelineList
    {
        private static readonly string[] yesnoarray = new string[] { "won", "closed" };
        private static readonly ILog log = LogManager.GetLogger(typeof(UpdatePipelineList));

        private int countupdate = 0;
        private int countnew = 0;
        private int dupcount = 0;
        /// <summary>
        /// Update Pipeline list
        /// </summary>
        /// <param name="clientContextWeb">client context url</param>
        /// <param name="backupListTarget">list that is exported and deleted</param>
        /// <param name="excelIndexTarget">list that needs to be backedup</param>
        /// <param name=sharepointIndexTarget">doc library for backup of excel file</param>
        public UpdatePipelineList(string clientContextWeb, string backupListTarget, string excelIndexTarget, string sharepointIndexTarget)
        {

           
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Export\");

            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Export\";
            string filter = "*.xlsx";
            string[] files = Directory.GetFiles(folder, filter);

            string pipelinefile = "Pipeline.xlsx";
            string dhcfile = "DHCUpdate.xlsx";

            Regex regexPipeline = FindFilesPatternToRegex.Convert("*pipeline*.xlsx");
            Regex regexDHC = FindFilesPatternToRegex.Convert("*dhc*.xlsx");

            foreach (string file in files)
            {
                //Console.WriteLine("Inside File check: {0}", file);
                if (regexPipeline.IsMatch(file.ToLower()))
                    pipelinefile = file;
                else if (regexDHC.IsMatch(file.ToLower()))
                    dhcfile = file;
            }

            Console.WriteLine("------Update Pipeline ----------------");
            //Console.WriteLine("Folder      : {0}", folder);
            Console.WriteLine("Pipelinefile: {0}", pipelinefile);
            Console.WriteLine("DHCfile     : {0}", dhcfile);
            Console.WriteLine("--------------------------------------");
            log.Debug(string.Format("------   Update Pipeline Files   ------")); 
            log.Debug(string.Format("Pipelinefile: {0}", pipelinefile));
            log.Debug(string.Format("DHCfile     : {0}", dhcfile));
            log.Debug(string.Format("---------------------------------------")); 

            FileStream stream, streamDHC;

            try
            {
                //update for reading files
                 stream = System.IO.File.Open(pipelinefile, FileMode.Open, FileAccess.Read);

                //update for reading files
                 streamDHC = System.IO.File.Open(dhcfile, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Please close the excel file and press enter");
                Console.ReadLine();
                //update for reading files
                 stream = System.IO.File.Open(pipelinefile, FileMode.Open, FileAccess.Read);

                //update for reading files
                 streamDHC = System.IO.File.Open(dhcfile, FileMode.Open, FileAccess.Read);

            }



            IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            reader.IsFirstRowAsColumnNames = true;

            DataSet ds = reader.AsDataSet();

            IExcelDataReader readerDHC = ExcelReaderFactory.CreateOpenXmlReader(streamDHC);
            readerDHC.IsFirstRowAsColumnNames = true;

            DataSet dsDHC = readerDHC.AsDataSet();

            DataRowSharepointMappingCollection mapping = MyRetriever.GetTheCollection();
            DataRowSharepointMappingCollection mappingDHC = MyRetriever.GetTheCollection("DataRowDHCMappingsSection");



            DataTable dt = ds.Tables[0];
            DataColumn dcParent = dt.Columns["Opportunity Name"];

            //using (var clientContext = new ClientContext(clientContextWeb))
            using (ClientContext clientContext = ClaimClientContext.GetAuthenticatedContext(clientContextWeb))
            {
                Web web = clientContext.Web;

                //------------------------------------
                // GetItems for PipeLine list
                //------------------------------------
                // DEV NOTES: 
                //     Aug. 4, 2017 - Removed 2000  limitation from CamlQueries   (MAT)
                // 
                List oldList = web.Lists.GetByTitle(backupListTarget);
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection oldItems = oldList.GetItems(query);

                clientContext.Load(oldItems);

                var listFields = oldList.Fields;
                clientContext.Load(listFields, fields => fields.Include(field => field.Title, field => field.InternalName, field => field.ReadOnlyField, field => field.StaticName));

                clientContext.ExecuteQuery();

                //--------------------------------------------------
                // GetItems from LOB (LineOfBusiness list here.....
                //--------------------------------------------------
                // DEV NOTES: 
                //     Aug. 4, 2017 - Removed 1000 limitation from CamlQueries   (MAT)
                // 
                List LOBList = web.Lists.GetByTitle("LOB-MPP-Map");
                CamlQuery LOBquery = CamlQuery.CreateAllItemsQuery();
                ListItemCollection LOBitems = LOBList.GetItems(LOBquery);

                clientContext.Load(LOBitems);

                var LOBFields = LOBList.Fields;
                clientContext.Load(LOBFields, fields => fields.Include(field => field.Title, field => field.InternalName));

                clientContext.ExecuteQuery();

                //UpdateLOBFields( clientContext, oldList, oldItems, LOBitems);
                // Console.WriteLine("Finished return from LOB update");
                //oldList.Update();
                //Console.WriteLine("Finished return from oldList update");
                //clientContext.ExecuteQuery();
                //-------------------------
                //Stop here for now.
                //-------------------------
                // Console.ReadLine();
                // System.Environment.Exit(0);
                 
                //log.Debug(string.Format("Opening List: {0}", backupListTarget));
                //log.Debug("Internal fields");
                //log.Debug("-------------------------");
                //foreach (Field f in listFields)
                //{
                //    log.Debug(string.Format("Title: {0},   Internal Name: {1}", f.Title, f.InternalName));
                //    log.Debug(string.Format("Static Name: {0}", f.StaticName));
                //    log.Debug("-----------------");
                //    //log.Debug(f.InternalName);
                //}

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    //log.Debug("-------  Inside For  -----------------");
                    //log.Debug(dr[excelIndexTarget].ToString());

                    var my_itemlist = oldItems.ToList();

                    // ---------------BEGIN MY COMMENT SECTION --------------------------
                    //Console.WriteLine("Sales Opportunity Id: {0}", dr["Sales Opportunity Id"].ToString());
                    //Console.WriteLine(" My_itemlist count: {0}", my_itemlist.Count);

                    ////---------MAT ----DEBUG TEST--------------------
                    ////--  List out item list for verification ---
                    //// ---------------------------------------------
                    //if (my_itemlist.Count() == 0)
                    //{
                    //    Console.WriteLine("My List count in 0");
                    //}
                    //else
                    //{
                    //    log.Debug("-- Item List ------");
                    //    foreach (ListItem targetListItem in my_itemlist)
                    //    {
                    //        log.Debug(string.Format("Title: {0}, HPOppID: {1}", targetListItem["Title"], targetListItem["HPOppID"].ToString()));
                    //        Console.WriteLine(targetListItem["Title"]);
                    //    }
                    //}
                    //log.Debug(string.Format(".....MAT - Listing COMPLETED HERE"));

                    //Console.WriteLine("  --------  MAT list completed here  ---------------");
                    // ---------------END MY COMMENT SECTION --------------------------

                    var page = from ListItem itemlist in oldItems.ToList()
                                   // var page = from ListItem itemlist in my_itemlist
                                   //where itemlist["HPOppID"].ToString() == dr["Sales Opportunity Id"].ToString()
                               where itemlist["HPOppID"].ToString() == dr[excelIndexTarget.ToString()].ToString()
                               select itemlist;

                    //Console.WriteLine("Page Count is: {0}", page.Count());
                    //this is an update
                    if (page.Count() == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        //Console.WriteLine(string.Format("UPDATE RECORD:  Name:{0}  ID:{1}", dr["Opportunity Name"].ToString(), dr["Sales Opportunity Id"].ToString()));
                        //log.Debug(string.Format("UPDATE: Name:{0}  ID:{1}", dr["Opportunity Name"].ToString(), dr["Sales Opportunity Id"].ToString()));
                        Console.WriteLine(string.Format("UPDATE RECORD:  Name:{0}  ID:{1}", dr["Opportunity Name"].ToString(), dr[excelIndexTarget.ToString()].ToString()));
                        log.Debug(string.Format("UPDATE: Name:{0}  ID:{1}", dr["Opportunity Name"].ToString(), dr[excelIndexTarget.ToString()].ToString()));


                        ListItem item = page.FirstOrDefault();

                        //iterate the mapping between sharepoint list items and excel spreadsheet items
                        foreach (DataRowSharepointMapping map in mapping)
                        {
                            UpdateField(item, map.SharePointColumn, map.DataRowColumn, dr, sharepointIndexTarget);

                        }
                        CompareSalesStage(item, dsDHC, mappingDHC, excelIndexTarget, sharepointIndexTarget);

                        //Console.WriteLine("- Before Item update.");
                        // just update the item
                        item.Update();

                        //Console.WriteLine("- Before List update.");
                        //update the list
                        oldList.Update();


                        countupdate++;


                    }
                    // This is a new record
                    //else if (page.Count() == 0 && !string.IsNullOrEmpty(dr["Sales Opportunity Id"].ToString()))   ----MAT
                    else if (page.Count() == 0 && !string.IsNullOrEmpty(dr[excelIndexTarget.ToString()].ToString()))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        //Console.WriteLine(string.Format("-------  Inside ELSE NEW RECORD-----------------"));
                        //Console.WriteLine(string.Format("NEW RECORD: Name:{0}  ID:{1}", dr["Opportunity Name"].ToString(), dr["Sales Opportunity Id"].ToString()));
                        Console.WriteLine(string.Format("NEW RECORD: Name:{0}  ID:{1}", dr["Opportunity Name"].ToString(), dr[excelIndexTarget.ToString()].ToString()));
                        //log.Debug("-------  Inside ELSE NEW RECORD-----------------");

                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        ListItem oListItem = oldList.AddItem(itemCreateInfo);
                        // -- iterate the mapping between sharepoint list items and excel spreadsheet items
                        foreach (DataRowSharepointMapping map in mapping)
                        {
                            UpdateField(oListItem, map.SharePointColumn, map.DataRowColumn, dr, sharepointIndexTarget);

                        }
                        CompareSalesStage(oListItem, dsDHC, mappingDHC, excelIndexTarget, sharepointIndexTarget);

                        // -- just update the item
                        //Console.WriteLine("- Before Item update.");                       
                        oListItem.Update();
                        // -- update the list
                        //Console.WriteLine("- Before List update.");
                        oldList.Update();

                        countnew++;

                    }

                    else
                    {
                        //-----------------------------------------------------
                        // DUPLICATE RECORDS CHECK  (MAT) - 8/31/17
                        //-----------------------------------------------------
                        //log.Debug(string.Format("DUPLICATE RECORD TEST inside PAGE: {0}", page.ToList().Count()));
                        if (page.ToList().Count() > 1)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            log.Debug(string.Format("DUPLICATE RECORD (NO ACTION TAKEN): Name:{0}  ID:{1} --- ", dr["Opportunity Name"].ToString(), dr[excelIndexTarget.ToString()].ToString()));
                            Console.WriteLine(string.Format("DUPLICATE RECORD (NO ACTION TAKEN): Name:{0}  ID:{1} -- ", dr["Opportunity Name"].ToString(), dr[excelIndexTarget.ToString()].ToString()));
                            dupcount++;
                        }

                        //-------------------------------------------------------

                        //Console.ForegroundColor = ConsoleColor.Red;
                        //Console.WriteLine("ERROR");

                    }

                    //  Not sure about this one. (MAT)
                    clientContext.ExecuteQuery();

                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("We updated: {0} records and added {1} records", countupdate.ToString(), countnew.ToString()));
                Console.WriteLine(string.Format("Duplicates found: {0} records", dupcount.ToString()));
                log.Debug(string.Format("------------------------------------------------------------"));
                log.Debug(string.Format("We updated: {0} records and added {1} records", countupdate.ToString(), countnew.ToString()));
                log.Debug(string.Format("Duplicates found: {0} records", dupcount.ToString()));
                Console.WriteLine("Completed first set of updates. \n");
                Console.WriteLine("Starting LOB checks........ \n");
                log.Debug(string.Format("------------------------------------------------------------"));
                log.Debug(string.Format("Starting LOB Checks........"));
                UpdateLOBFields(clientContext, oldList, oldItems, LOBitems, sharepointIndexTarget);
                //Console.WriteLine("Finished return from LOB update...");
                //oldList.Update();
                clientContext.ExecuteQuery();
                Console.WriteLine("Finished Line Of Business updates... \n");
            }
        }
 
       public static void UpdateLOBFields( ClientContext myclientContext, List myitemList, ListItemCollection myoldItems, ListItemCollection myLOBitems, string sharepointIndexTarget )
        {

            //Console.WriteLine("Inside UpdateLOBField");

            var my_pipecount = 0;
            int counter = 0;
            Boolean my_pipemarker;
            Boolean isAssignable;


            //Console.Write("---- LIST CHECK ---------- \n");
            foreach (ListItem pipeItem in myoldItems)
            {
                my_pipemarker = false;
                isAssignable = true;
                Console.ForegroundColor = ConsoleColor.Green;
                //Console.Write("-------------------------- \n");
                var testobject = pipeItem["AccountName"];

                if (!Object.ReferenceEquals(null, testobject))
                {
                    //Console.Write("AccountName: {0} \n", pipeItem["AccountName"].ToString());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("AccountName -- NOT assignable here. --- Title: {0} entry", pipeItem["Title"].ToString());
                    isAssignable = false;
                }

                //-----------------------------------------------------------
                // Check to see if object is assignable
                //-----------------------------------------------------------
                if (isAssignable)
                {
                    //-----------------------------------------------------------
                    // If the pipeline item is not null, then update MPP the abject.
                    //-----------------------------------------------------------                    
                    var testobject1 = pipeItem["MPP"];
                    if (!Object.ReferenceEquals(null, testobject1))
                    {
                        //Console.Write("MPP {0} \n", pipeItem["MPP"].ToString());
                    }
                    else
                    {
                        //----------------------------------------------
                        // select matching record from LOB items
                        // Note: Try block to check for Null Reference in LOB table
                        //----------------------------------------------
                        try
                        {
                            var page = from ListItem itemlist in myLOBitems
                                       where itemlist["Title"].ToString() == pipeItem["AccountName"].ToString()
                                       select itemlist;

                            ListItem item = page.FirstOrDefault();

                            //----------------------------------------------
                            //Check if LOB item is null
                            //----------------------------------------------
                            var checkobject = item["MPP"];
                            if (checkobject != null)
                            {
                                //Console.WriteLine("MPP reference check");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("{0}: [{1}] -- MPP has been assigned. ({2})", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString(), item["MPP"].ToString());
                                pipeItem["MPP"] = item["MPP"];
                                log.Debug(string.Format("{0}: [{1}]  -- MPP has been assigned. ({2})", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString(), item["MPP"].ToString()));
                                my_pipemarker = true; 
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0}: [{1}] -- MPP has NOT been assigned (Value not present)", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString());
                                log.Debug(string.Format("{0}: [{1}] -- MPP has NOT been assigned (Value not present)", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString()));
                            }
                        }
                        catch ( NullReferenceException ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: {0} not found in LOB list.  MPP is not assignable.", pipeItem["AccountName"].ToString());
                            log.Debug(string.Format("ERROR: {0} not found in LOB list.  MPP is not assignable.", pipeItem["AccountName"].ToString()));
                            //Console.WriteLine("Null Reference found: {0}", ex.Message);
                            //Console.ReadLine();
                        }
                    }
                }

                //-----------------------------------------------------------
                // Check to see if object is assignable
                //-----------------------------------------------------------
                if (isAssignable)
                {
                    //-----------------------------------------------------------
                    // If the pipeline item is not null, then update MPP the abject.
                    //----------------------------------------------------------- 
                    var testobject2 = pipeItem["LineOfBusiness"];
                    if (!Object.ReferenceEquals(null, testobject2))
                    {
                        //Console.Write("LineOfBusiness: {0} \n", pipeItem["LineOfBusiness"].ToString());
                    }
                    else
                    {
                        //----------------------------------------------
                        // select matching record from LOB items
                        // Note: Try block to check for Null Reference in LOB table
                        //----------------------------------------------
                        try
                        {
                            var page = from ListItem itemlist in myLOBitems
                                       where itemlist["Title"].ToString() == pipeItem["AccountName"].ToString()
                                       select itemlist;

                            ListItem item = page.FirstOrDefault();

                            var checkobject = item["LineOfBusiness"];
                            if (checkobject != null)
                            {
                                //Console.WriteLine("Inside LineOfBusiness reference check");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("{0}: [{1}] -- LineOfBusiness has been assigned. ({2})", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString(), item["LineOfBusiness"].ToString());
                                pipeItem["LineOfBusiness"] = item["LineOfBusiness"];
                                log.Debug(string.Format("{0}: [{1}]  -- LineOfBusiness has been assigned. ({2})", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString(), item["LineOfBusiness"].ToString()));
                                my_pipemarker = true;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0}: [{1}] -- LineOfBusiness has NOT been assigned (Value not present)", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString());
                                log.Debug(string.Format("{0}: [{1}] -- LineOfBusiness has NOT been assigned (Value not present)", sharepointIndexTarget.ToString(), pipeItem["HPOppID"].ToString()));
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: {0} not found in LOB list.  LineOfBusiniess is not assignable.", pipeItem["AccountName"].ToString());
                            log.Debug(string.Format("ERROR: {0} not found in LOB list.  LineOfBusiness is not assignable.", pipeItem["AccountName"].ToString()));
                            //Console.WriteLine("Null Reference found: {0}", ex.Message);
                            //Console.ReadLine();
                        }
                    }
                }

                if (my_pipemarker)
                {
                    my_pipecount++;
                    //Console.WriteLine("LOB change count is: {0}", my_pipecount);
                    pipeItem.Update();
                    myitemList.Update();
                    if (counter > 100)
                    {
                        try
                        {
                            myclientContext.ExecuteQuery();
                        }
                        catch (Exception ex)
                        {
                            log.Error("caught an exception in during LOB updates object", ex);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(string.Format("We had an issue within LOB Update:{0}", ex.ToString()));
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            counter = 0;
                            //Console.WriteLine("Connectivity was lost please try again");
                            throw ex;
                        }
                    }
                    else
                    {
                        counter++;
                    }
                        //myitemList.Update();
                    //myoldItems.ToList();
                }

            }


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("-----------------------------------------------")); 
            Console.WriteLine(string.Format("Total LOB changes: {0}", my_pipecount));
            log.Debug(string.Format("Total LOB changes: {0}", my_pipecount));
            // Console.ReadLine();
       
       }

  
     
        public static void UpdateField( ListItem item, string SharePointColumn, string DataRowColumn, DataRow dr, string sharepointIndexColumn)
        {
            // Console.WriteLine("Inside Update Field..");
            // Console.WriteLine("Column: {0}  , Row: {1}", SharePointColumn, DataRowColumn);
            if (dr.Table.Columns.Contains(DataRowColumn) && !string.IsNullOrEmpty(dr[DataRowColumn].ToString()))
            {
                if (yesnoarray.Contains(SharePointColumn.ToLower()))
                {
                    if (dr[DataRowColumn].ToString() == "0")
                        item[SharePointColumn] = "No";
                    else
                        item[SharePointColumn] = "Yes";
                }
                else
                {
                    // Console.WriteLine("Inside Update(field check): SharePointColumn: {0}", SharePointColumn);
                    // Console.WriteLine("Inside Update(field check): dr: {0}", dr[DataRowColumn]);
                    //if (SharePointColumn == "OppID")
                    if (SharePointColumn == sharepointIndexColumn.ToString())
                    {
                        item["HPOppID"] = dr[DataRowColumn];
                    }
                    else
                    {
                        item[SharePointColumn] = dr[DataRowColumn];
                    }
                }
            }
        }

        public static void CompareSalesStage( ListItem dr, DataSet dsDHC, DataRowSharepointMappingCollection mappingDHC, string excelIndexColumn, string sharepointIndexColumn)
        {
            //query sharepoint for the record we are updateing by hpoppid
            //Console.WriteLine("Inside CompareSales");
            //Console.WriteLine("{0}", dr["HPOppID"].ToString());

            var result = (from myRow in dsDHC.Tables[0].AsEnumerable()
                          //where myRow["HPE Opportunity Id"].ToString() == dr["HPOppID"].ToString()
                          //where myRow["Sales Opportunity Id"].ToString() == dr["HPOppID"].ToString()
                          where myRow[excelIndexColumn.ToString()].ToString() == dr["HPOppID"].ToString()
                          select myRow).FirstOrDefault(); 
            foreach (DataRowSharepointMapping map in mappingDHC)
            {
                //Console.WriteLine("Mapping: {0}", map.DataRowColumn);
                //if (map.SharePointColumn != "OppID")
                if (map.SharePointColumn != sharepointIndexColumn.ToString())
                {
                    if (result != null && result[map.DataRowColumn] != null)
                    {
                        //Console.WriteLine("Inside assignment Mapping: {0}", result[map.DataRowColumn]);
                        dr[map.SharePointColumn] = result[map.DataRowColumn];
                    }
                }
                else
                {
                    if (result != null && result[map.DataRowColumn] != null)
                    {
                        //Console.WriteLine("Inside (RE)assignment Mapping: {0}", result[map.DataRowColumn]);
                        dr["HPOppID"] = result[map.DataRowColumn];

                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine(string.Format("Updating DHC Name:{0}  ID:{1}", dr["Title"].ToString(), dr["HPOppID"].ToString())); ;
            Console.WriteLine(string.Format("UPDATE DHC RECORD: {0}  ID:{1}", dr["Title"].ToString(), dr["HPOppID"].ToString())); ;
        }

    }

    internal static class FindFilesPatternToRegex
    {
        private static Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
        private static Regex IllegalCharactersRegex = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
        private static Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
        private static string NonDotCharacters = @"[^.]*";
        public static Regex Convert(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException();
            }
            pattern = pattern.Trim();
            if (pattern.Length == 0)
            {
                throw new ArgumentException("Pattern is empty.");
            }
            if (IllegalCharactersRegex.IsMatch(pattern))
            {
                throw new ArgumentException("Pattern contains illegal characters.");
            }
            bool hasExtension = CatchExtentionRegex.IsMatch(pattern);
            bool matchExact = false;
            if (HasQuestionMarkRegEx.IsMatch(pattern))
            {
                matchExact = true;
            }
            else if (hasExtension)
            {
                matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
            }
            string regexString = Regex.Escape(pattern);
            regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
            regexString = Regex.Replace(regexString, @"\\\?", ".");
            if (!matchExact && hasExtension)
            {
                regexString += NonDotCharacters;
            }
            regexString += "$";
            Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return regex;
        }
    }

}
