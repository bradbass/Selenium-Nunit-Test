using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace eMobileSeleniumNunitTest
{
    class EMobileTest : IDisposable
    {
        private IWebDriver driver;
        private bool existingWO = true;
        const int wait2 = 2000;

        EMobileSeleniumNunitTest ts = new EMobileSeleniumNunitTest();

        public EMobileTest(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void EMobileTestCase()
        {
            var wait2 = 2000;

            EMobileLogin();

            ////eMobile user login
            //driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/Login.aspx");
            ////eMobile admin login
            ////driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/AdminLogin.aspx");

            ////add an implicit wait
            //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));

            ////*/Use with emobile
            ////var loginBtn = driver.FindElement(By.ClassName("ui-btn-hidden"));
            //var userId = driver.FindElement(By.Id("TextUserName"));
            //var userPw = driver.FindElement(By.Id("TextPwd"));
            ////*/

            //userId.SendKeys("chris");
            ////for demo/testing only
            //Thread.Sleep(wait2);
            //userPw.SendKeys("chris");
            ////for demo/testing only
            //Thread.Sleep(wait2);
            //driver.FindElement(By.Id("btnLogin")).Click();

            //take a screenshot for testing
            ts.TakeScreenShot(driver, @"login_screen.png");

            Thread.Sleep(4000);
            //add explicit wait to avoid issues where the browser is a bit slow so the test fails to find the required element
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement pageWorkOrders = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("pageWorkOrders"));
            });

            //needed to slow down test
            Thread.Sleep(wait2);

            //check if needs to download new managed list
            var managedList = "Please download managed list";
            var result = driver.FindElement(By.Id("pageWorkOrders")).Text;
            //IWebElement managedList = driver.FindElementByTagName("body");
            //if (managedList.Text.Contains("Please download managed list"))
            if (result.Contains(managedList))
            {
                //go download the managed list
                ManagedListDownload();
            }

            //syncronize
            SyncWO();

            //needed to slow down test
            Thread.Sleep(wait2);

            //accept and complete existing WO that was sent to device
            CompleteExistingWO();

            //enter new WO
            EnterNewWO();
        }

        public void EMobileLogin()
        {
            //eMobile user login
            driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/Login.aspx");
            //eMobile admin login
            //driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/AdminLogin.aspx");

            //add an implicit wait
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));

            //*/Use with emobile
            //var loginBtn = driver.FindElement(By.ClassName("ui-btn-hidden"));
            var userId = driver.FindElement(By.Id("TextUserName"));
            var userPw = driver.FindElement(By.Id("TextPwd"));
            //*/

            userId.SendKeys("chris");
            userPw.SendKeys("chris");
            try
            {
                driver.FindElement(By.Id("btnLogin")).Click();
            }
            catch (Exception e)
            {
                ts.Log("Login failed due to: "+e);
            }

            WebDriverWait wdw = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement linkNewWO = wdw.Until<IWebElement>((d) =>
                {
                    return d.FindElement(By.Id("linkNewWO"));
                });
        }

        public void EMoibleLoginTest()
        {
             //Test multiple logins
            SqlConn();
        }

        private void SqlConn()
        {
            var iteration = 0;
            SqlConnection conn = new SqlConnection() { ConnectionString = "Server=BRABA-PC1\\SQLEXPRESS;Database=EMobileLoginTest;User Id=Jonas01;Password=Jonas01;" };
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT [username],[password] FROM Users", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            while (iteration <= 2)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    LoginTest(dr["username"].ToString(), dr["password"].ToString());
                }

                iteration++;
            }
        }

        private void LoginTest(string uname, string pword)
        {
            //eMobile user login
            driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/Login.aspx");

            //eMobile admin login
            //driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/AdminLogin.aspx");

            //*/Use with emobile
            driver.FindElement(By.Id("TextUserName")).SendKeys(uname);
            driver.FindElement(By.Id("TextPwd")).SendKeys(pword);
            //*/
            driver.FindElement(By.Id("btnLogin")).Click();

            Thread.Sleep(1000);

            try
            {
                //check if user validation fails
                var authMsg = "Authentication failed";
                //var auth = true;
                var result = "";
                result = driver.FindElement(By.Id("pageLogin")).Text;
                Thread.Sleep(1000);
                if (result.Contains(authMsg))
                {
                    var logMsg = "Authentication failed for user: " + uname;
                    ts.Log(logMsg);
                }
                else
                {
                    var logMsg = "Login failed due to: " + result;
                    ts.Log(logMsg);
                }
            }
            catch (Exception)
            {
                // Login was successful so do nothing..  or write to log the successful login
                var logMsg = "Login successful for user: " + uname;
                ts.Log(logMsg);
            }

            //take a screenshot for testing
            ts.TakeScreenShot(driver, @"C:\Logs\login_screen.png");
        }

        private void CompleteExistingWO()
        {
            //bool existingWO = true;
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement woList = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("woList"));
            });

            try
            {
                //accept existing wo that was sent from Jonas
                driver.FindElement(By.XPath("//a[@href='WorkOrderDet.html']")).Click();
            }
            catch (Exception e)
            {
                //do something with exception
                existingWO = false;
            }

            finally
            {
                if (existingWO == true)
                {
                    FillWODetailsFull();
                }
            }
        }

        private void EnterNewWO()
        {
            //Implicit wait is not alwys long enough depending on browser/system performance
            //Thread.Sleep(4000);
            //add explicit wait to avoid issues where the browser is a bit slow so the test fails to find the required element
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement linkNewWO = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("linkNewWO"));
            });

            //create new WO
            driver.FindElement(By.Id("linkNewWO")).Click();

            FillWODetailsFull();
        }

        public void EnterNewWOMinimal()
        {
            driver.FindElement(By.Id("linkNewWO")).Click();

            FillWODetailsMinimal();
        }

        private void FillWODetailsMinimal()
        {            
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            IWebElement pageWODetails = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("pageWODetails"));
            });

            //input customer name
            driver.FindElement(By.XPath("//a[@href='ManList.html']")).Click();

            IWebElement pageItemList = eWait.Until<IWebElement>((d) =>
                {
                    return driver.FindElement(By.Id("pageItemList"));
                });

            driver.FindElement(By.Id("itemLink")).Click();

            Thread.Sleep(1000);
            // input who called
            driver.FindElement(By.Id("who_called")).SendKeys("QA-TEST");

            // input description of work
            driver.FindElement(By.Id("wo_details_problem")).SendKeys("This is a test description.");

            // save the work order
            saveWO();                                    
        }
        
        private void FillWODetailsFull()
        {
            var wait2 = 2000;
            //implicit wait not long enough
            Thread.Sleep(4000);
            //explicit wait just incase browser is slow
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement pageWODetails = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("pageWODetails"));
            });

            //if existing WO, cannot select customer.  need to skip this.
            if (existingWO != true)
            {
                // fill out work order top details: customer, who called, etc..
                newWODetails();
            }

            if (existingWO == true)
            {
                //Set status to arrived if an existing WO.  Newly created WOs are marked arrived automatically
                SelectElement selectStatus = new SelectElement(driver.FindElement(By.XPath("//select[@id='list_status']")));
                selectStatus.SelectByValue("A");
            }

            //for demo/testing
            Thread.Sleep(wait2);

            // fill out work order bottom details: descrption of work, tech notes, comments, etc.. 
            woDetails();            

            //for demo/testing only
            Thread.Sleep(wait2);

            if (existingWO != true)
            {
                //fill our spare fields, if new work order
                spareFields();
            }

            //goto Details screen
            driver.FindElement(By.XPath("//a[@href='#wo_tables']")).Click();

            //Implicit wait not always long enough
            Thread.Sleep(4000);
            //add explicit wait in case browser is slow
            IWebElement woTablesMats = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("wo_tables_material"));
            });

            //enter equipment/part
            driver.FindElement(By.Id("wo_tables_material")).Click();

            //for demo/testing only
            Thread.Sleep(3000);

            // enter inventory part - non functional
            inventoryPart();

            // enter N/A Part
            naPart();            

            //Implicit wait
            Thread.Sleep(4000);

            // enter labor hours
            woLabor();            

            //for demo/testing only
            Thread.Sleep(wait2);

            //email the summary sheet
            emailSummary();            
            
            /* The print preview is part of Chrome, so do not test this
            //print the summary sheet
            printSummarySheet();
            //*/

            //for demo/testing only
            Thread.Sleep(4000);            

            //var signatureBox = driver.FindElementByXPath("//canvas[@class='pad']");
            //signatureBox.Click();
            //signature();
            Signature();            

            //for demo/testing only
            Thread.Sleep(3000);
            IWebElement backToWODetails = eWait.Until<IWebElement>((d) =>
            {
                //return d.FindElement(By.XPath("//a[@href='WorkOrderDet.html']"));
                return d.FindElement(By.XPath("//*[contains(text(), 'Back')]"));
            });

            //driver.FindElement(By.XPath("//a[@href='WorkOrderDet.html']")).Click();
            driver.FindElement(By.XPath("//*[contains(text(), 'Back')]")).Click();

            //implicit wait, not long enough
            Thread.Sleep(4000);
            //explicit wait incase browser is slow
            IWebElement pageWODetails2 = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("pageWODetails"));
            });

            //var changeStatus = driver.FindElementById("list_status");
            //SelectElement selectStatus = new SelectElement(driver.FindElementById("list_status"));
            SelectElement selectStatusX = new SelectElement(driver.FindElement(By.XPath("//select[@id='list_status']")));
            //select.DeselectAll();
            //selectStatus.SelectByValue("Complete");
            selectStatusX.SelectByValue("X");

            Thread.Sleep(wait2);

            //save W/O
            IWebElement saveWO = eWait.Until<IWebElement>((d) =>
            {
                //return d.FindElement(By.XPath("//a[@onclick='SaveWorkOrder()']"));
                return d.FindElement(By.XPath("//*[contains(text(), 'Save')]"));
            });
            //driver.FindElement(By.XPath("//a[@onclick='SaveWorkOrder()']")).Click();
            driver.FindElement(By.XPath("//*[contains(text(), 'Save')]")).Click();

            //implicit wait, not long enough
            Thread.Sleep(4000);
            //explicit wait incase browser is slow
            IWebElement newWOSubmittedMsg = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("pageInfoMessage"));
            });

            Thread.Sleep(4000);

            //take a screenshot for testing
            ts.TakeScreenShot(driver, @"savedWO_screen.png");

            //click OK on new wo submitted message
            driver.FindElement(By.Id("pageInfoMessageButton")).Click();
        }        

        private void woLabor()
        {
            //explicit wait.  wait until element is visible
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement woTablesLabor = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("wo_tables_labor"));
            });

            //enter labor hours
            driver.FindElement(By.Id("wo_tables_labor")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            driver.FindElement(By.XPath("//a[@href='LaborTimeNew.html']")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            driver.FindElement(By.Id("clear_compl_date")).Click();

            //var completeTimeBtn = driver.FindElement(By.Id("labor_compl_date"));
            //completeTimeBtn.Click();
            var dateWheel = driver.FindElement(By.XPath("//div[@data_val='18']"));
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript("arguments[0].click();", dateWheel);

            //send page down so program can see the button
            driver.FindElement(By.CssSelector("body")).SendKeys(Keys.PageDown);

            //save labor time to session
            driver.FindElement(By.XPath("//a[@data-bind='click: storeToSession']")).Click();
        }

        private void woDetails()
        {
            //enter tech notes
            driver.FindElement(By.Id("btnNotes")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //find wo details tech notes and enter a note
            var techNotesTextBox = driver.FindElement(By.Id("wo_details_notes"));
            techNotesTextBox.SendKeys("This is a test tech note.");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter work performed
            driver.FindElement(By.Id("btnComments")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //find wo details comments and enter work performed
            var workPerformedTextBox = driver.FindElement(By.Id("wo_details_comments"));
            workPerformedTextBox.SendKeys("This is a test message for work performed.");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter recommendations
            driver.FindElement(By.Id("btnRecommendations")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //find wo details recommendations and enter recommendation
            var recommendationsTextBox = driver.FindElement(By.Id("wo_details_recomm"));
            recommendationsTextBox.SendKeys("This is a test recommendation");
        }

        private void newWODetails()
        {
            //input customer name
            driver.FindElement(By.Id("btnSelectCustomer")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            driver.FindElement(By.Id("itemLink")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter who called
            var whoCalled = driver.FindElement(By.Id("who_called"));
            whoCalled.SendKeys("Jonas");

            //for demo/testing only
            Thread.Sleep(wait2);

            //input reference number
            var refNumber = driver.FindElement(By.Id("ref_number"));
            refNumber.SendKeys("test1234");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter problem description
            driver.FindElement(By.Id("btnProblem")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //find the problem description text box and enter description
            var problemDescTextBox = driver.FindElement(By.Id("wo_details_problem"));
            problemDescTextBox.SendKeys("This is a test problem description.");
        }

        private void spareFields()
        {
            //explicit wait just incase browser is slow
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            //enter spare fields
            driver.FindElement(By.XPath("//a[@href='#spare_fields']")).Click();

            //Implicit wait is not always long enough
            Thread.Sleep(4000);
            //explicit wait incase browser is slow
            IWebElement spareFields = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("spare_fields"));
            });

            SelectElement select = new SelectElement(driver.FindElement(By.Id("list_SpareField1")));
            //select.DeselectAll();
            select.SelectByValue("Original W/O");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter second spare field
            var spareField2 = driver.FindElement(By.Id("SpareField2"));
            spareField2.SendKeys("1");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter third spare field
            var spareField3 = driver.FindElement(By.Id("SpareField3"));
            spareField3.SendKeys("1.5 lbs");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter spare field 4
            var spareField4 = driver.FindElement(By.Id("SpareField4"));
            spareField4.SendKeys("test parcel desc");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter spare field 5
            var spareField5 = driver.FindElement(By.Id("SpareField5"));
            spareField5.SendKeys("test status");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter spare field 6
            var spareField6 = driver.FindElement(By.Id("SpareField6"));
            spareField6.SendKeys("truckID 1234");

            //for demo/testing only
            Thread.Sleep(wait2);
        }

        private void inventoryPart()
        {
            //TODO add inventory part
            /*/
            var addMaterialBtn = driver.FindElement(By.XPath("//a[@href='ManList.html']"));
            addMaterialBtn.Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            var addMaterialPartBtn = driver.FindElement(By.XPath("//a[@href='PartDetailsNew1.html']"));
            addMaterialPartBtn.Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //input part reference
            var partReferenceField = driver.FindElement(By.Id("part_reference"));
            partReferenceField.SendKeys("testRef1");

            //for demo/testing only
            Thread.Sleep(wait2);

            //save material/part to session
            var addMaterialPartToSession = driver.FindElement(By.XPath("//a[@data-bind='click: storeToSession']"));
            addMaterialPartToSession.Click();

            //for demo/testing only
            Thread.Sleep(wait2);
            //*/
        }

        private void naPart()
        {
            //explicit wait just incase browser is slow
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            //add n/a part
            driver.FindElement(By.XPath("//a[@href='PartNADetailsNew.html']")).Click();

            //Implicit wait to give time for page to load
            Thread.Sleep(4000);
            //explicit wait.  wait until element is visible
            IWebElement partDesc = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("part_desc"));
            });

            //enter description
            var partDescriptionTextBox = driver.FindElement(By.Id("part_desc"));
            partDescriptionTextBox.SendKeys("Just a test description");

            //for demo/testing only
            Thread.Sleep(wait2);

            //enter part cost
            var partCostTextBox = driver.FindElement(By.Id("part_cost"));
            partCostTextBox.Clear();
            partCostTextBox.SendKeys("13.37");

            //for demo/testing only
            Thread.Sleep(wait2);

            //add part reference
            var partReferenceTextBox = driver.FindElement(By.Id("part_reference"));
            partReferenceTextBox.SendKeys("testRef2");

            //for demo/testing only
            Thread.Sleep(wait2);

            //save n/a part to session
            driver.FindElement(By.XPath("//a[@data-bind='click: storeToSession']")).Click();
        }

        //* This happens outside of Jonas, so not neccessary to test
        private void PrintSummarySheet()
        {
            //get handle of current window
            String curWindowHandle = driver.CurrentWindowHandle;

            //print the summary sheet
            driver.FindElement(By.XPath("//a[@onclick='getSummaryPdfUrl();']")).Click();

            //wait for second window to open
            //Thread.Sleep(4000);
            //*
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            IWebElement printWindow = wait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("print-summary"));
            });
            //*/

            //get popup window handle
            string popupHandle = string.Empty;

            //get window handles
            ReadOnlyCollection<string> handles = driver.WindowHandles;

            foreach (string handle in handles)
            {
                if (handle != curWindowHandle)
                {
                    popupHandle = handle; break;
                }
            }

            //switch to new window
            driver.SwitchTo().Window(popupHandle);

            //find element on new window
            //var newWindow = driver.FindElement(By.Id("print-preview"));

            //have to send keys CTRL+SHIFT+P and then ENTER to print
            driver.FindElement(By.Id("print-preview")).SendKeys(Keys.Control + Keys.Shift + "P");
            /*
            Actions sendPrintKeys = new Actions(driver);
            sendPrintKeys.MoveToElement(newWindow);
            sendPrintKeys.SendKeys(Keys.Control + Keys.Shift + "P");
            sendPrintKeys.Build().Perform();
            //*/

            //wait for print dialog
            Thread.Sleep(2000);

            //switch back to original window
            driver.SwitchTo().Window(curWindowHandle);
        }

        // method functioning!
        private void Signature()
        {
            //get signature
            driver.FindElement(By.XPath("//a[@href='#pageSignature']")).Click();

            //for demo/testing only
            Thread.Sleep(4000);

            var sikuliArgs = @"-r C:\Test\EMobileSignature.skl";
            ts.CallSikuli(sikuliArgs);

            driver.FindElement(By.XPath("//a[@onclick='SaveSignature()']")).Click();
        }

        private void emailSummary()
        {
            //goto summary page
            //driver.FindElement(By.XPath("//a[@onclick='storeWOToSession()']")).Click();
            driver.FindElement(By.XPath("//*[contains(text(), 'Summary')]")).Click();

            //for demo/testing only
            Thread.Sleep(4000);

            //email summary sheet
            driver.FindElement(By.XPath("//a[@href='EmailList.html']")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            var emailField = driver.FindElement(By.Id("newEmailItem"));
            emailField.SendKeys("brad.bass@jonassoftware.com");

            //for demo/testing only
            Thread.Sleep(wait2);

            driver.FindElement(By.XPath("//a[@data-bind='enable: NewEmailToAdd().length > 0, click: AddEmailAddress, clickBubble: false']")).Click();

            //for demo/testing only
            Thread.Sleep(wait2);

            //send email
            driver.FindElement(By.Id("emailListBtnYes")).Click();
        }

        private void saveWO()
        {
            bool saveSuccess = false;
            var messageContent = "";
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            // click the save button
            driver.FindElement(By.Id("btnSaveWorkOrder")).Click();

            //test
            Thread.Sleep(1000);

            // check if the save failed.
            try
            {
                driver.FindElement(By.Id("pageInfoMessage"));
            }
            catch (NoSuchElementException e)
            {
                // do nothing, as an exception means the save was successful.
                saveSuccess = true;
            }

            if (saveSuccess == false)
            {
                // find out why it failed.
                messageContent = driver.FindElement(By.Id("messageContent")).Text;
                ts.Log("Save failed due to: " + messageContent);
            }
            else
            {
                IWebElement woList = eWait.Until<IWebElement>((d) =>
                {
                    return d.FindElement(By.Id("woList"));
                });

                ts.Log("Test Completed Successfully.");
            }
        }
        
        //*
        private void ManagedListDownload()
        {
            //navigate to managed list download page
            driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/ManagedList.aspx");

            //add a wait2 to slow down process for demo
            Thread.Sleep(4000);
            //add implicit wait2
            //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));
            //add explicit wait to avoid issues where the browser is a bit slow so the test fails to find the required element
            WebDriverWait eWait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            IWebElement pageManList = eWait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Id("pageManList"));
            });

            //find download button
            var downloadBtn = driver.FindElement(By.XPath("//a[@data-bind='click: DownloadManListAll']"));
            //click the button and download the full managed list
            //downloadBtn.Click();

            Thread.Sleep(2000);

            //back to home page
            //driver.FindElement(By.XPath("//a[@data-icon='back']")).Click();
            var backBtn = driver.FindElement(By.XPath("//span[@class='ui-btn-text' and contains(text(),'Back')]"));
            backBtn.Click();

            Thread.Sleep(2000);
        }//*/

        private void SyncWO()
        {
            var wait2 = 2000;
            //driver.FindElement(By.ClassName("ui-block-c")).Click();
            driver.Navigate().GoToUrl("https://constqa124.jonasportal.com/emobile/Synch.aspx");

            //needed to slow down test
            Thread.Sleep(4000);

            //download new WOs
            driver.FindElement(By.XPath("//a[@title='Download Work Orders']")).Click();

            //needed to slow down test
            Thread.Sleep(wait2);

            //download updates
            driver.FindElement(By.XPath("//a[@title='Download Updates']")).Click();

            //needed to slow down test
            Thread.Sleep(wait2);

            //go back to main page
            driver.FindElement(By.XPath("//a[@data-rel='back']")).Click();
        }
    }
}
